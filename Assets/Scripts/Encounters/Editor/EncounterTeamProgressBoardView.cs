#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EncounterTeamProgressBoardView
{
	private const float BaseGridSize = 36f;
	private const float MinZoom = 0.6f;
	private const float MaxZoom = 1.8f;
	private static readonly Vector2 NodeGridSize = new Vector2(5f, 4f);

	private FeatNode selectedNode;
	private Vector2 pan = new Vector2(220f, 160f);
	private float zoom = 1f;

	public FeatNode SelectedNode => selectedNode;

	public void ClearSelection()
	{
		selectedNode = null;
	}

	public void Draw(Rect rect, CreatureUnit unit, Action<string, Action> applyChange)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.92f, 0.92f, 0.92f));

		if (unit?.Species?.FeatBoard?.Nodes == null)
		{
			DrawEmptyState(rect, "No species selected.");
			return;
		}

		List<FeatNode> nodes = unit.Species.FeatBoard.Nodes;
		if (nodes.Count == 0)
		{
			DrawEmptyState(rect, "This species has no feat nodes.");
			return;
		}

		if (selectedNode != null)
		{
			selectedNode = unit.Species.FeatBoard.GetNode(selectedNode.Id);
		}

		HandleInput(rect, unit, applyChange);

		GUI.BeginClip(rect);
		DrawGrid(rect.size);
		DrawLinks(unit, nodes);
		DrawNodes(unit, nodes);
		DrawFooter(rect.size, nodes.Count);
		GUI.EndClip();
	}

	public void DrawSelectedNodeActions(CreatureUnit unit, Action<string, Action> applyChange)
	{
		EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

		if (selectedNode == null || unit == null)
		{
			EditorGUILayout.HelpBox("Select a node in the graph to change this creature progression.", MessageType.Info);
			return;
		}

		EditorGUILayout.LabelField("Name", selectedNode.DisplayName);
		EditorGUILayout.LabelField("Kind", selectedNode.Kind.ToString());
		EditorGUILayout.LabelField("Completions", CreatureUnitFeatProgressUtility.GetCompletionCount(unit, selectedNode).ToString());
		EditorGUILayout.LabelField("Reachable", CreatureUnitFeatProgressUtility.IsNodeReachable(unit, selectedNode) ? "Yes" : "No");
		EditorGUILayout.LabelField("Exhausted", CreatureUnitFeatProgressUtility.IsNodeExhausted(unit, selectedNode) ? "Yes" : "No");

		EditorGUILayout.Space(8f);

		using (new EditorGUI.DisabledScope(
			       !CreatureUnitFeatProgressUtility.IsNodeReachable(unit, selectedNode) ||
			       CreatureUnitFeatProgressUtility.IsNodeExhausted(unit, selectedNode)))
		{
			if (GUILayout.Button("Complete Once"))
			{
				applyChange?.Invoke("Complete Encounter Feat Node",
					() => CreatureUnitFeatProgressUtility.CompleteNodeOnce(unit, selectedNode));
			}
		}

		if (GUILayout.Button("Reset Node"))
		{
			applyChange?.Invoke("Reset Encounter Feat Node",
				() => CreatureUnitFeatProgressUtility.ResetNodeProgress(unit, selectedNode));
		}

		if (GUILayout.Button("Clear All Progress"))
		{
			applyChange?.Invoke("Clear Encounter Feat Progress",
				() => CreatureUnitFeatProgressUtility.ClearAllProgress(unit));
		}
	}

	private void DrawEmptyState(Rect rect, string message)
	{
		Rect helpRect = new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, 40f);
		GUI.Label(helpRect, message, EditorStyles.boldLabel);
	}

	private void HandleInput(Rect rect, CreatureUnit unit, Action<string, Action> applyChange)
	{
		Event currentEvent = Event.current;
		if (!rect.Contains(currentEvent.mousePosition))
		{
			return;
		}

		Vector2 localMouse = currentEvent.mousePosition - rect.position;

		if (currentEvent.type == EventType.ScrollWheel)
		{
			float oldZoom = zoom;
			float newZoom = Mathf.Clamp(zoom - currentEvent.delta.y * 0.03f, MinZoom, MaxZoom);
			if (!Mathf.Approximately(oldZoom, newZoom))
			{
				Vector2 boardBeforeZoom = CanvasToBoard(localMouse);
				zoom = newZoom;
				pan = localMouse - boardBeforeZoom * GetScaledGridSize();
				currentEvent.Use();
			}
		}

		if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 2)
		{
			pan += currentEvent.delta;
			currentEvent.Use();
		}

		if (currentEvent.type == EventType.MouseDown && (currentEvent.button == 0 || currentEvent.button == 1))
		{
			FeatNode clickedNode = GetNodeAtLocalPosition(unit, localMouse);
			selectedNode = clickedNode;

			if (clickedNode != null && currentEvent.clickCount >= 2)
			{
				if (currentEvent.button == 0 &&
				    CreatureUnitFeatProgressUtility.IsNodeReachable(unit, clickedNode) &&
				    !CreatureUnitFeatProgressUtility.IsNodeExhausted(unit, clickedNode))
				{
					applyChange?.Invoke("Complete Encounter Feat Node",
						() => CreatureUnitFeatProgressUtility.CompleteNodeOnce(unit, clickedNode));
				}
				else if (currentEvent.button == 1)
				{
					applyChange?.Invoke("Reset Encounter Feat Node",
						() => CreatureUnitFeatProgressUtility.ResetNodeProgress(unit, clickedNode));
				}
			}

			currentEvent.Use();
		}
	}

	private void DrawGrid(Vector2 size)
	{
		float spacing = GetScaledGridSize();

		Handles.BeginGUI();
		Color previousColor = Handles.color;
		Handles.color = new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.06f : 0.12f);

		float offsetX = Mathf.Repeat(pan.x, spacing);
		if (offsetX > 0f)
		{
			offsetX -= spacing;
		}

		float offsetY = Mathf.Repeat(pan.y, spacing);
		if (offsetY > 0f)
		{
			offsetY -= spacing;
		}

		for (float x = offsetX; x <= size.x; x += spacing)
		{
			Handles.DrawLine(new Vector3(x, 0f), new Vector3(x, size.y));
		}

		for (float y = offsetY; y <= size.y; y += spacing)
		{
			Handles.DrawLine(new Vector3(0f, y), new Vector3(size.x, y));
		}

		Handles.color = previousColor;
		Handles.EndGUI();
	}

	private void DrawLinks(CreatureUnit unit, List<FeatNode> nodes)
	{
		FeatBoard featBoard = unit?.Species?.FeatBoard;
		if (featBoard == null)
		{
			return;
		}

		Dictionary<FeatNode, int> indexLookup = new Dictionary<FeatNode, int>();
		for (int index = 0; index < nodes.Count; index++)
		{
			FeatNode node = nodes[index];
			if (node != null && !indexLookup.ContainsKey(node))
			{
				indexLookup.Add(node, index);
			}
		}

		Handles.BeginGUI();

		for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
		{
			FeatNode node = nodes[nodeIndex];
			if (node == null)
			{
				continue;
			}

			List<FeatNode> neighbours = featBoard.GetNeighbourNodes(node);
			for (int neighbourIndex = 0; neighbourIndex < neighbours.Count; neighbourIndex++)
			{
				FeatNode neighbour = neighbours[neighbourIndex];
				if (neighbour == null || !indexLookup.TryGetValue(neighbour, out int mappedIndex) || mappedIndex < nodeIndex)
				{
					continue;
				}

				Handles.color = new Color(1f, 1f, 1f, 0.3f);
				Handles.DrawAAPolyLine(2.5f, GetNodeRect(node).center, GetNodeRect(neighbour).center);
			}
		}

		Handles.EndGUI();
	}

	private void DrawNodes(CreatureUnit unit, List<FeatNode> nodes)
	{
		for (int index = 0; index < nodes.Count; index++)
		{
			FeatNode node = nodes[index];
			if (node == null)
			{
				continue;
			}

			DrawNode(unit, node);
		}
	}

	private void DrawNode(CreatureUnit unit, FeatNode node)
	{
		Rect nodeRect = GetNodeRect(node);
		Color fillColor = GetNodeColor(unit, node);
		Color borderColor = selectedNode == node ? new Color(1f, 0.94f, 0.58f, 1f) : new Color(0f, 0f, 0f, 0.55f);

		EditorGUI.DrawRect(nodeRect, fillColor);
		DrawOutline(nodeRect, borderColor, selectedNode == node ? 3f : 1.5f);
		DrawRepeatBadge(nodeRect, unit, node);

		float padding = 8f * zoom;
		float iconSize = node.Icon != null ? 30f * zoom : 0f;
		float stateHeight = 16f * zoom;
		float rewardLineHeight = 14f * zoom;
		float titleHeight = 20f * zoom;

		Rect contentRect = new Rect(nodeRect.x + padding, nodeRect.y + padding, nodeRect.width - padding * 2f, nodeRect.height - padding * 2f);

		if (node.Icon != null)
		{
			Rect iconRect = new Rect(contentRect.x, contentRect.y, iconSize, iconSize);
			SpriteGuiUtility.DrawSprite(iconRect, node.Icon);
			contentRect.yMin += iconSize + 6f * zoom;
		}

		Rect stateRect = new Rect(contentRect.x, nodeRect.yMax - padding - stateHeight, contentRect.width, stateHeight);
		Rect titleRect = new Rect(contentRect.x, contentRect.y, contentRect.width, titleHeight);
		Rect rewardsRect = new Rect(contentRect.x, titleRect.yMax + 4f * zoom, contentRect.width, Mathf.Max(0f, stateRect.yMin - (titleRect.yMax + 4f * zoom)));

		GUI.Label(titleRect, node.DisplayName, GetTitleStyle());
		DrawRewardPreview(rewardsRect, node, rewardLineHeight);
		GUI.Label(stateRect, GetStateLabel(unit, node), GetStateStyle());
	}

	private void DrawRewardPreview(Rect rect, FeatNode node, float lineHeight)
	{
		if (zoom < 0.75f || node?.Rewards == null || rect.height <= 0f)
		{
			return;
		}

		GUIStyle rewardStyle = GetRewardStyle();
		float y = rect.y;
		int maxVisibleLineCount = Mathf.FloorToInt(rect.height / lineHeight);

		for (int rewardIndex = 0; rewardIndex < node.Rewards.Count && rewardIndex < maxVisibleLineCount; rewardIndex++)
		{
			string rewardLabel = BuildRewardLabel(node.Rewards[rewardIndex]);
			Rect lineRect = new Rect(rect.x, y, rect.width, lineHeight);
			GUI.Label(lineRect, rewardLabel, rewardStyle);
			y += lineHeight;
		}

		if (node.Rewards.Count > maxVisibleLineCount && maxVisibleLineCount > 0)
		{
			Rect ellipsisRect = new Rect(rect.x, rect.yMax - lineHeight, rect.width, lineHeight);
			GUI.Label(ellipsisRect, "...", rewardStyle);
		}
	}

	private void DrawRepeatBadge(Rect nodeRect, CreatureUnit unit, FeatNode node)
	{
		bool shouldShowBadge = CreatureUnitFeatProgressUtility.IsNodeCompleted(unit, node) || node.NumberOfRepeatTime > 1 || node.NumberOfRepeatTime < 0;
		if (!shouldShowBadge)
		{
			return;
		}

		int completionCount = CreatureUnitFeatProgressUtility.GetCompletionCount(unit, node);
		int maxCount = node.NumberOfRepeatTime < 0 ? -1 : Mathf.Max(1, node.NumberOfRepeatTime);
		string maxLabel = maxCount < 0 ? "inf" : maxCount.ToString();
		string badgeText = completionCount + "/" + maxLabel;

		Rect badgeRect = new Rect(nodeRect.xMax - 42f, nodeRect.y + 6f, 36f, 16f);
		EditorGUI.DrawRect(badgeRect, new Color(0f, 0f, 0f, 0.28f));

		GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
		{
			alignment = TextAnchor.MiddleCenter
		};
		badgeStyle.normal.textColor = Color.white;
		GUI.Label(badgeRect, badgeText, badgeStyle);
	}

	private void DrawFooter(Vector2 size, int nodeCount)
	{
		Rect footerRect = new Rect(10f, size.y - 24f, Mathf.Max(200f, size.x - 20f), 18f);
		GUI.Label(footerRect, $"{nodeCount} nodes | Zoom {Mathf.RoundToInt(zoom * 100f)}% | LMB select | MMB drag | Wheel zoom", EditorStyles.miniLabel);
	}

	private Color GetNodeColor(CreatureUnit unit, FeatNode node)
	{
		if (CreatureUnitFeatProgressUtility.IsNodeCompleted(unit, node))
		{
			return new Color(0.31f, 0.62f, 0.33f, 1f);
		}

		if (CreatureUnitFeatProgressUtility.IsNodeReachable(unit, node))
		{
			return new Color(0.80f, 0.66f, 0.26f, 1f);
		}

		return new Color(0.46f, 0.46f, 0.46f, 1f);
	}

	private string GetStateLabel(CreatureUnit unit, FeatNode node)
	{
		if (CreatureUnitFeatProgressUtility.IsNodeExhausted(unit, node))
		{
			return "Exhausted";
		}

		if (CreatureUnitFeatProgressUtility.IsNodeCompleted(unit, node))
		{
			return "Completed";
		}

		if (CreatureUnitFeatProgressUtility.IsNodeReachable(unit, node))
		{
			return "Reachable";
		}

		return "Locked";
	}

	private string BuildRewardLabel(FeatReward reward)
	{
		if (reward == null)
		{
			return "- Missing reward";
		}

		return reward switch
		{
			BonusStatsReward bonusStatsReward => $"- +{bonusStatsReward.Value} {bonusStatsReward.Attribute}",
			AbilityReward abilityReward => abilityReward.Ability != null ? $"- Ability: {abilityReward.Ability.name}" : "- Ability: None",
			PassiveReward passiveReward => passiveReward.Status != null ? $"- Passive: {passiveReward.Status.name}" : "- Passive: None",
			ChangeFormReward changeFormReward => string.IsNullOrEmpty(changeFormReward.FormKey) ? "- Form: None" : $"- Form: {changeFormReward.FormKey}",
			_ => $"- {reward.GetType().Name}"
		};
	}

	private FeatNode GetNodeAtLocalPosition(CreatureUnit unit, Vector2 localPosition)
	{
		if (unit?.Species?.FeatBoard?.Nodes == null)
		{
			return null;
		}

		List<FeatNode> nodes = unit.Species.FeatBoard.Nodes;
		for (int index = nodes.Count - 1; index >= 0; index--)
		{
			FeatNode node = nodes[index];
			if (node != null && GetNodeRect(node).Contains(localPosition))
			{
				return node;
			}
		}

		return null;
	}

	private Rect GetNodeRect(FeatNode node)
	{
		Vector2 canvasPosition = BoardToCanvas(node.Position);
		Vector2 size = NodeGridSize * GetScaledGridSize();
		return new Rect(canvasPosition.x, canvasPosition.y, size.x, size.y);
	}

	private Vector2 BoardToCanvas(Vector2 boardPosition)
	{
		return pan + boardPosition * GetScaledGridSize();
	}

	private Vector2 CanvasToBoard(Vector2 canvasPosition)
	{
		return (canvasPosition - pan) / GetScaledGridSize();
	}

	private float GetScaledGridSize()
	{
		return BaseGridSize * zoom;
	}

	private GUIStyle GetTitleStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
		{
			fontSize = Mathf.Max(9, Mathf.RoundToInt(11f * zoom)),
			wordWrap = true,
			clipping = TextClipping.Clip
		};
		style.normal.textColor = new Color(0.92f, 0.92f, 0.92f);
		return style;
	}

	private GUIStyle GetStateStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
		{
			fontSize = Mathf.Max(8, Mathf.RoundToInt(10f * zoom)),
			wordWrap = false,
			clipping = TextClipping.Clip
		};
		style.normal.textColor = new Color(0.88f, 0.88f, 0.88f);
		return style;
	}

	private GUIStyle GetRewardStyle()
	{
		GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
		{
			fontSize = Mathf.Max(7, Mathf.RoundToInt(9f * zoom)),
			wordWrap = false,
			clipping = TextClipping.Clip
		};
		style.normal.textColor = new Color(0.94f, 0.94f, 0.94f, 0.95f);
		return style;
	}

	private void DrawOutline(Rect rect, Color color, float thickness)
	{
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
		EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
	}
}
#endif
