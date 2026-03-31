using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EncounterTeamProgressBoardView
{
	private const float BaseGridSize = 36f;
	private static readonly Vector2 NodeGridSize = new Vector2(5f, 4f);

	private FeatNode _selectedNode;
	private Vector2 _pan = new Vector2(220f, 160f);
	private float _zoom = 1f;

	public FeatNode SelectedNode => _selectedNode;

	public void ClearSelection()
	{
		_selectedNode = null;
	}

	public void Draw(Rect p_rect, CreatureUnit p_unit)
	{
		EditorGUI.DrawRect(p_rect, EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.90f, 0.90f, 0.90f));

		if (p_unit == null || p_unit.Species == null || p_unit.Species.FeatBoard == null || p_unit.Species.FeatBoard.Nodes == null)
		{
			DrawEmptyState(p_rect, "No species selected.");
			return;
		}

		List<FeatNode> nodes = p_unit.Species.FeatBoard.Nodes;
		if (nodes.Count == 0)
		{
			DrawEmptyState(p_rect, "This species has no feat nodes.");
			return;
		}

		HandleInput(p_rect, p_unit);

		GUI.BeginClip(p_rect);
		DrawGrid(p_rect.size);
		DrawLinks(p_unit, nodes);
		DrawNodes(p_unit, nodes);
		GUI.EndClip();
	}

	public void DrawSelectedNodeActions(CreatureUnit p_unit)
	{
		EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

		if (_selectedNode == null)
		{
			EditorGUILayout.HelpBox("Select a node in the board.", MessageType.Info);
			return;
		}

		EditorGUILayout.LabelField("Name", _selectedNode.DisplayName);
		EditorGUILayout.LabelField("Kind", _selectedNode.Kind.ToString());
		EditorGUILayout.LabelField("Completions", CreatureUnitFeatProgressUtility.GetCompletionCount(p_unit, _selectedNode).ToString());
		EditorGUILayout.LabelField("Reachable", CreatureUnitFeatProgressUtility.IsNodeReachable(p_unit, _selectedNode) ? "Yes" : "No");
		EditorGUILayout.LabelField("Exhausted", CreatureUnitFeatProgressUtility.IsNodeExhausted(p_unit, _selectedNode) ? "Yes" : "No");

		EditorGUILayout.Space(8f);

		using (new EditorGUI.DisabledScope(
			!CreatureUnitFeatProgressUtility.IsNodeReachable(p_unit, _selectedNode) ||
			CreatureUnitFeatProgressUtility.IsNodeExhausted(p_unit, _selectedNode)))
		{
			if (GUILayout.Button("Complete Once"))
			{
				CreatureUnitFeatProgressUtility.CompleteNodeOnce(p_unit, _selectedNode);
			}
		}

		if (GUILayout.Button("Reset Node"))
		{
			CreatureUnitFeatProgressUtility.ResetNodeProgress(p_unit, _selectedNode);
		}

		if (GUILayout.Button("Clear All Progress"))
		{
			CreatureUnitFeatProgressUtility.ClearAllProgress(p_unit);
		}
	}

	private void DrawEmptyState(Rect p_rect, string p_message)
	{
		GUI.Label(
			new Rect(p_rect.x + 12f, p_rect.y + 12f, p_rect.width - 24f, 22f),
			p_message,
			EditorStyles.boldLabel);
	}

	private void HandleInput(Rect p_rect, CreatureUnit p_unit)
	{
		Event currentEvent = Event.current;
		if (!p_rect.Contains(currentEvent.mousePosition))
		{
			return;
		}

		Vector2 localMouse = currentEvent.mousePosition - p_rect.position;

		if (currentEvent.type == EventType.ScrollWheel)
		{
			float previousZoom = _zoom;
			_zoom = Mathf.Clamp(_zoom - currentEvent.delta.y * 0.03f, 0.5f, 1.8f);

			Vector2 boardBeforeZoom = CanvasToBoard(localMouse);
			_pan = localMouse - boardBeforeZoom * GetScaledGridSize();

			if (!Mathf.Approximately(previousZoom, _zoom))
			{
				currentEvent.Use();
			}
		}

		if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 2)
		{
			_pan += currentEvent.delta;
			currentEvent.Use();
		}

		if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
		{
			FeatNode clickedNode = GetNodeAtLocalPosition(p_unit, localMouse);
			_selectedNode = clickedNode;
			currentEvent.Use();
		}
	}

	private void DrawGrid(Vector2 p_size)
	{
		float spacing = GetScaledGridSize();

		Handles.BeginGUI();
		Color previousColor = Handles.color;
		Handles.color = new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.06f : 0.12f);

		float offsetX = Mathf.Repeat(_pan.x, spacing);
		if (offsetX > 0f)
		{
			offsetX -= spacing;
		}

		float offsetY = Mathf.Repeat(_pan.y, spacing);
		if (offsetY > 0f)
		{
			offsetY -= spacing;
		}

		for (float x = offsetX; x <= p_size.x; x += spacing)
		{
			Handles.DrawLine(new Vector3(x, 0f), new Vector3(x, p_size.y));
		}

		for (float y = offsetY; y <= p_size.y; y += spacing)
		{
			Handles.DrawLine(new Vector3(0f, y), new Vector3(p_size.x, y));
		}

		Handles.color = previousColor;
		Handles.EndGUI();
	}

	private void DrawLinks(CreatureUnit p_unit, List<FeatNode> p_nodes)
	{
		Handles.BeginGUI();

		for (int nodeIndex = 0; nodeIndex < p_nodes.Count; nodeIndex++)
		{
			FeatNode node = p_nodes[nodeIndex];
			if (node == null || node.NeighbourNodes == null)
			{
				continue;
			}

			for (int neighbourIndex = 0; neighbourIndex < node.NeighbourNodes.Count; neighbourIndex++)
			{
				FeatNode neighbour = node.NeighbourNodes[neighbourIndex];
				if (neighbour == null)
				{
					continue;
				}

				if (p_nodes.IndexOf(neighbour) < nodeIndex)
				{
					continue;
				}

				Rect nodeRect = GetNodeRect(node);
				Rect neighbourRect = GetNodeRect(neighbour);

				Handles.color = new Color(1f, 1f, 1f, 0.3f);
				Handles.DrawAAPolyLine(2.5f, nodeRect.center, neighbourRect.center);
			}
		}

		Handles.EndGUI();
	}

	private void DrawNodes(CreatureUnit p_unit, List<FeatNode> p_nodes)
	{
		for (int nodeIndex = 0; nodeIndex < p_nodes.Count; nodeIndex++)
		{
			FeatNode node = p_nodes[nodeIndex];
			if (node == null)
			{
				continue;
			}

			DrawNode(p_unit, node);
		}
	}

	private void DrawNode(CreatureUnit p_unit, FeatNode p_node)
	{
		Rect nodeRect = GetNodeRect(p_node);

		Color fillColor = GetNodeColor(p_unit, p_node);
		Color borderColor = _selectedNode == p_node ? new Color(1f, 0.95f, 0.5f, 1f) : new Color(0f, 0f, 0f, 0.55f);

		EditorGUI.DrawRect(nodeRect, fillColor);
		DrawOutline(nodeRect, borderColor, _selectedNode == p_node ? 3f : 1.5f);

		DrawRepeatBadge(nodeRect, p_unit, p_node);

		float padding = 8f * _zoom;
		float iconSize = p_node.Icon != null ? 30f * _zoom : 0f;
		float stateHeight = 16f * _zoom;
		float rewardLineHeight = 14f * _zoom;
		float titleHeight = 20f * _zoom;

		Rect contentRect = new Rect(
			nodeRect.x + padding,
			nodeRect.y + padding,
			nodeRect.width - padding * 2f,
			nodeRect.height - padding * 2f
		);

		if (p_node.Icon != null)
		{
			Rect iconRect = new Rect(
				contentRect.x,
				contentRect.y,
				iconSize,
				iconSize
			);

			GUI.DrawTexture(iconRect, p_node.Icon.texture, ScaleMode.ScaleToFit);
			contentRect.yMin += iconSize + 6f * _zoom;
		}

		Rect stateRect = new Rect(
			contentRect.x,
			nodeRect.yMax - padding - stateHeight,
			contentRect.width,
			stateHeight
		);

		Rect titleRect = new Rect(
			contentRect.x,
			contentRect.y,
			contentRect.width,
			titleHeight
		);

		Rect rewardsRect = new Rect(
			contentRect.x,
			titleRect.yMax + 4f * _zoom,
			contentRect.width,
			Mathf.Max(0f, stateRect.yMin - (titleRect.yMax + 4f * _zoom))
		);

		GUI.Label(titleRect, p_node.DisplayName, GetNodeTitleStyle());
		DrawRewardPreview(rewardsRect, p_node, rewardLineHeight);
		GUI.Label(stateRect, GetNodeStateLabel(p_unit, p_node), GetNodeStateStyle());
	}

	private void DrawRewardPreview(Rect p_rect, FeatNode p_node, float p_lineHeight)
	{
		if (_zoom < 0.75f)
		{
			return;
		}

		if (p_node == null || p_node.Rewards == null || p_node.Rewards.Count == 0 || p_rect.height <= 0f)
		{
			return;
		}

		GUIStyle rewardStyle = GetNodeRewardStyle();

		float y = p_rect.y;
		int maxVisibleLineCount = Mathf.FloorToInt(p_rect.height / p_lineHeight);

		for (int rewardIndex = 0; rewardIndex < p_node.Rewards.Count && rewardIndex < maxVisibleLineCount; rewardIndex++)
		{
			FeatReward reward = p_node.Rewards[rewardIndex];
			string rewardLabel = BuildRewardLabel(reward);

			Rect lineRect = new Rect(
				p_rect.x,
				y,
				p_rect.width,
				p_lineHeight
			);

			GUI.Label(lineRect, rewardLabel, rewardStyle);
			y += p_lineHeight;
		}

		if (p_node.Rewards.Count > maxVisibleLineCount && maxVisibleLineCount > 0)
		{
			Rect ellipsisRect = new Rect(
				p_rect.x,
				p_rect.yMax - p_lineHeight,
				p_rect.width,
				p_lineHeight
			);

			GUI.Label(ellipsisRect, "...", rewardStyle);
		}
	}

	private string BuildRewardLabel(FeatReward p_reward)
	{
		if (p_reward == null)
		{
			return "• Missing reward";
		}

		switch (p_reward)
		{
			case BonusStatsReward bonusStatsReward:
				return $"• +{bonusStatsReward.Value} {bonusStatsReward.Attribute}";

			case AbilityReward abilityReward:
				return abilityReward.Ability != null
					? $"• Ability: {abilityReward.Ability.name}"
					: "• Ability: None";

			case PassiveReward passiveReward:
				return passiveReward.Status != null
					? $"• Passive: {passiveReward.Status.name}"
					: "• Passive: None";

			case ChangeFormReward changeFormReward:
				return string.IsNullOrEmpty(changeFormReward.FormKey)
					? "• Form: None"
					: $"• Form: {changeFormReward.FormKey}";

			default:
				return $"• {p_reward.GetType().Name}";
		}
	}

	private GUIStyle GetNodeTitleStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
		style.fontSize = Mathf.Max(9, Mathf.RoundToInt(11f * _zoom));
		style.wordWrap = true;
		style.clipping = TextClipping.Clip;
		style.normal.textColor = new Color(0.92f, 0.92f, 0.92f);
		return style;
	}

	private GUIStyle GetNodeStateStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
		style.fontSize = Mathf.Max(8, Mathf.RoundToInt(10f * _zoom));
		style.wordWrap = false;
		style.clipping = TextClipping.Clip;
		style.normal.textColor = new Color(0.88f, 0.88f, 0.88f);
		return style;
	}

	private GUIStyle GetNodeRewardStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
		style.fontSize = Mathf.Max(7, Mathf.RoundToInt(9f * _zoom));
		style.wordWrap = false;
		style.clipping = TextClipping.Clip;
		style.normal.textColor = new Color(0.93f, 0.93f, 0.93f, 0.95f);
		return style;
	}

	private void DrawRepeatBadge(Rect p_nodeRect, CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_node == null)
		{
			return;
		}

		bool shouldShowBadge =
			CreatureUnitFeatProgressUtility.IsNodeCompleted(p_unit, p_node) ||
			p_node.NumberOfRepeatTime > 1 ||
			p_node.NumberOfRepeatTime < 0;

		if (!shouldShowBadge)
		{
			return;
		}

		int completionCount = CreatureUnitFeatProgressUtility.GetCompletionCount(p_unit, p_node);
		int maxCount = p_node.NumberOfRepeatTime < 0 ? -1 : Mathf.Max(1, p_node.NumberOfRepeatTime);
		string maxLabel = maxCount < 0 ? "∞" : maxCount.ToString();
		string badgeText = completionCount + "/" + maxLabel;

		Rect badgeRect = new Rect(
			p_nodeRect.xMax - 42f,
			p_nodeRect.y + 6f,
			36f,
			16f
		);

		EditorGUI.DrawRect(badgeRect, new Color(0f, 0f, 0f, 0.28f));

		GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			normal = { textColor = Color.white }
		};

		GUI.Label(badgeRect, badgeText, badgeStyle);
	}

	private Color GetNodeColor(CreatureUnit p_unit, FeatNode p_node)
	{
		if (CreatureUnitFeatProgressUtility.IsNodeCompleted(p_unit, p_node))
		{
			return new Color(0.32f, 0.62f, 0.34f, 1f);
		}

		if (CreatureUnitFeatProgressUtility.IsNodeReachable(p_unit, p_node))
		{
			return new Color(0.80f, 0.66f, 0.26f, 1f);
		}

		return new Color(0.48f, 0.48f, 0.48f, 1f);
	}

	private string GetNodeStateLabel(CreatureUnit p_unit, FeatNode p_node)
	{
		if (CreatureUnitFeatProgressUtility.IsNodeExhausted(p_unit, p_node))
		{
			return "Exhausted";
		}

		if (CreatureUnitFeatProgressUtility.IsNodeCompleted(p_unit, p_node))
		{
			return "Completed";
		}

		if (CreatureUnitFeatProgressUtility.IsNodeReachable(p_unit, p_node))
		{
			return "Reachable";
		}

		return "Locked";
	}

	private FeatNode GetNodeAtLocalPosition(CreatureUnit p_unit, Vector2 p_localPosition)
	{
		if (p_unit == null || p_unit.Species == null || p_unit.Species.FeatBoard == null || p_unit.Species.FeatBoard.Nodes == null)
		{
			return null;
		}

		List<FeatNode> nodes = p_unit.Species.FeatBoard.Nodes;
		for (int nodeIndex = nodes.Count - 1; nodeIndex >= 0; nodeIndex--)
		{
			FeatNode node = nodes[nodeIndex];
			if (node != null && GetNodeRect(node).Contains(p_localPosition))
			{
				return node;
			}
		}

		return null;
	}

	private Rect GetNodeRect(FeatNode p_node)
	{
		Vector2 canvasPosition = BoardToCanvas(p_node.Position);
		Vector2 size = NodeGridSize * GetScaledGridSize();
		return new Rect(canvasPosition.x, canvasPosition.y, size.x, size.y);
	}

	private Vector2 BoardToCanvas(Vector2 p_boardPosition)
	{
		return _pan + p_boardPosition * GetScaledGridSize();
	}

	private Vector2 CanvasToBoard(Vector2 p_canvasPosition)
	{
		return (p_canvasPosition - _pan) / GetScaledGridSize();
	}

	private float GetScaledGridSize()
	{
		return BaseGridSize * _zoom;
	}

	private void DrawOutline(Rect p_rect, Color p_color, float p_thickness)
	{
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.y, p_rect.width, p_thickness), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.yMax - p_thickness, p_rect.width, p_thickness), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.y, p_thickness, p_rect.height), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.xMax - p_thickness, p_rect.y, p_thickness, p_rect.height), p_color);
	}
}