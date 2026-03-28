using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FeatBoardEditorWindow : EditorWindow
{
	private const float ToolbarHeight = 38f;
	private const float InspectorWidth = 360f;
	private const float BaseGridSize = 40f;
	private const float MinZoom = 0.7f;
	private const float MaxZoom = 1.75f;
	private const float DefaultZoom = 1f;
	private const float InspectorPadding = 10f;

	private static readonly Vector2 NodeGridSize = new Vector2(5f, 4f);

	private enum RepeatTimeMode
	{
		Unique,
		Repeatable,
		Infinite
	}

	private static readonly Type[] RewardTypes =
	{
		typeof(BonusStatsReward),
		typeof(AbilityReward),
		typeof(PassiveReward),
		typeof(ChangeFormReward)
	};

	private static readonly Type[] RequirementTypes =
	{
		typeof(DealDamageRequirement),
		typeof(HealHealthRequirement)
	};

	private static readonly List<FeatNode> EmptyNodeList = new List<FeatNode>();

	private CreatureSpecies species;
	private string selectedNodeId;
	private string linkSourceNodeId;

	private Vector2 canvasPan = new Vector2(320f, 220f);
	private Vector2 lastCanvasSize = new Vector2(700f, 500f);
	private Vector2 inspectorScroll;

	private float zoom = DefaultZoom;
	private bool snapToGrid = true;

	private bool isDraggingNode;
	private string draggedNodeId;
	private Vector2 dragNodeStart;
	private Vector2 dragMouseStart;

	private bool isPanningCanvas;
	private Vector2 panStart;
	private Vector2 panMouseStart;

	private GUIStyle nodeTitleStyle;
	private GUIStyle nodeBodyStyle;
	private GUIStyle nodeBadgeStyle;
	private GUIStyle canvasHintStyle;
	private GUIStyle canvasEmptyTitleStyle;
	private GUIStyle canvasEmptyBodyStyle;

	[MenuItem("Tools/Feat Board Editor")]
	public static void OpenWindow()
	{
		GetWindow<FeatBoardEditorWindow>("Feat Board");
	}

	public static void Open(CreatureSpecies targetSpecies)
	{
		FeatBoardEditorWindow window = GetWindow<FeatBoardEditorWindow>("Feat Board");
		window.SetSpecies(targetSpecies);
		window.Focus();
	}

	private void OnEnable()
	{
		titleContent = new GUIContent("Feat Board");
		minSize = new Vector2(1100f, 650f);
		wantsMouseMove = true;

		Undo.undoRedoPerformed += Repaint;
		Selection.selectionChanged += HandleSelectionChanged;

		if (species == null && Selection.activeObject is CreatureSpecies selectedSpecies)
		{
			SetSpecies(selectedSpecies);
		}
	}

	private void OnDisable()
	{
		Undo.undoRedoPerformed -= Repaint;
		Selection.selectionChanged -= HandleSelectionChanged;
	}

	private void OnGUI()
	{
		EnsureStyles();
		EnsureBoardIntegrity(false, string.Empty);

		Rect toolbarRect = new Rect(0f, 0f, position.width, ToolbarHeight);
		Rect bodyRect = new Rect(0f, ToolbarHeight, position.width, Mathf.Max(0f, position.height - ToolbarHeight));
		float resolvedInspectorWidth = Mathf.Min(InspectorWidth, Mathf.Max(280f, bodyRect.width * 0.4f));
		Rect canvasRect = new Rect(bodyRect.x, bodyRect.y, Mathf.Max(0f, bodyRect.width - resolvedInspectorWidth), bodyRect.height);
		Rect inspectorRect = new Rect(canvasRect.xMax, bodyRect.y, resolvedInspectorWidth, bodyRect.height);

		lastCanvasSize = canvasRect.size;

		DrawToolbar(toolbarRect);
		DrawCanvas(canvasRect);
		DrawInspector(inspectorRect);
	}

	private void HandleSelectionChanged()
	{
		if (species != null)
		{
			return;
		}

		if (Selection.activeObject is CreatureSpecies selectedSpecies)
		{
			SetSpecies(selectedSpecies);
		}
	}

	private void SetSpecies(CreatureSpecies targetSpecies)
	{
		species = targetSpecies;
		selectedNodeId = null;
		linkSourceNodeId = null;
		draggedNodeId = null;
		isDraggingNode = false;
		isPanningCanvas = false;
		inspectorScroll = Vector2.zero;

		EnsureBoardIntegrity(false, string.Empty);
		Repaint();
	}

	private void DrawToolbar(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.86f, 0.86f, 0.86f));

		float x = rect.x + 8f;
		float y = rect.y + 7f;
		float height = rect.height - 14f;

		Rect speciesLabelRect = new Rect(x, y + 1f, 48f, height);
		GUI.Label(speciesLabelRect, "Species", EditorStyles.miniBoldLabel);
		x = speciesLabelRect.xMax + 6f;

		Rect speciesFieldRect = new Rect(x, y, 240f, height);
		CreatureSpecies newSpecies = (CreatureSpecies)EditorGUI.ObjectField(speciesFieldRect, species, typeof(CreatureSpecies), false);
		if (newSpecies != species)
		{
			SetSpecies(newSpecies);
		}

		x = speciesFieldRect.xMax + 6f;

		if (GUI.Button(new Rect(x, y, 94f, height), "Use Selection", EditorStyles.toolbarButton))
		{
			if (Selection.activeObject is CreatureSpecies selectedSpecies)
			{
				SetSpecies(selectedSpecies);
			}
		}

		x += 100f;

		using (new EditorGUI.DisabledScope(species == null))
		{
			if (GUI.Button(new Rect(x, y, 74f, height), "Add Node", EditorStyles.toolbarButton))
			{
				AddNodeAtViewportCenter();
			}

			x += 80f;

			if (GUI.Button(new Rect(x, y, 76f, height), "Frame All", EditorStyles.toolbarButton))
			{
				FrameAllNodes();
			}

			x += 82f;
			snapToGrid = GUI.Toggle(new Rect(x, y, 58f, height), snapToGrid, "Snap", EditorStyles.toolbarButton);
			x += 64f;

			FeatNode selectedNode = GetSelectedNode();
			using (new EditorGUI.DisabledScope(selectedNode == null))
			{
				string linkButtonLabel = linkSourceNodeId == selectedNodeId ? "Stop Link" : "Link Mode";
				if (GUI.Button(new Rect(x, y, 78f, height), linkButtonLabel, EditorStyles.toolbarButton))
				{
					linkSourceNodeId = linkSourceNodeId == selectedNodeId ? null : selectedNodeId;
					Repaint();
				}
			}

			x += 84f;

			using (new EditorGUI.DisabledScope(selectedNode == null))
			{
				if (GUI.Button(new Rect(x, y, 106f, height), "Frame Selected", EditorStyles.toolbarButton))
				{
					FrameSelectedNode();
				}
			}
		}

		if (!string.IsNullOrEmpty(linkSourceNodeId))
		{
			FeatNode linkNode = GetNodeById(linkSourceNodeId);
			string linkLabel = linkNode != null ? "Linking from: " + GetNodeLabel(linkNode) : "Link mode active";
			Vector2 size = EditorStyles.miniLabel.CalcSize(new GUIContent(linkLabel));
			Rect infoRect = new Rect(rect.xMax - size.x - 18f, y + 1f, size.x + 10f, height);
			GUI.Label(infoRect, linkLabel, EditorStyles.miniLabel);
		}
	}

	private void DrawCanvas(Rect rect)
	{
		EditorGUI.DrawRect(rect, GetCanvasBackgroundColor());

		if (rect.width <= 0f || rect.height <= 0f)
		{
			return;
		}

		if (species == null)
		{
			DrawCanvasEmptyState(rect, "Pick a species to start editing its feat board.", "You can open the window from a species inspector or choose one in the toolbar.");
			return;
		}

		HandleCanvasInput(rect);

		GUI.BeginClip(rect);
		DrawGrid(rect.size, 1, GetMinorGridColor());
		DrawGrid(rect.size, 5, GetMajorGridColor());
		DrawLinks();
		DrawNodes(rect.size);
		DrawCanvasOverlay(rect.size);
		GUI.EndClip();
	}

	private void DrawCanvasEmptyState(Rect rect, string title, string body)
	{
		GUI.BeginClip(rect);

		Rect panelRect = new Rect(
			Mathf.Max(24f, rect.width * 0.5f - 220f),
			Mathf.Max(24f, rect.height * 0.5f - 56f),
			440f,
			112f);

		EditorGUI.DrawRect(panelRect, EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.18f) : new Color(1f, 1f, 1f, 0.55f));
		GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 14f, panelRect.width - 32f, 24f), title, canvasEmptyTitleStyle);
		GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 42f, panelRect.width - 32f, 52f), body, canvasEmptyBodyStyle);

		GUI.EndClip();
	}

	private void DrawGrid(Vector2 size, int multiplier, Color color)
	{
		float spacing = GetScaledGridSize() * multiplier;
		if (spacing < 8f)
		{
			return;
		}

		float offsetX = Mathf.Repeat(canvasPan.x, spacing);
		if (offsetX > 0f)
		{
			offsetX -= spacing;
		}

		float offsetY = Mathf.Repeat(canvasPan.y, spacing);
		if (offsetY > 0f)
		{
			offsetY -= spacing;
		}

		Handles.BeginGUI();
		Color previousColor = Handles.color;
		Handles.color = color;

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

	private void DrawLinks()
	{
		List<FeatNode> nodes = GetNodes();
		if (nodes.Count == 0)
		{
			return;
		}

		Dictionary<string, FeatNode> lookup = BuildNodeLookup(nodes);
		Handles.BeginGUI();

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null || node.NeighbourNodeIds == null)
			{
				continue;
			}

			for (int j = 0; j < node.NeighbourNodeIds.Count; j++)
			{
				string neighbourId = node.NeighbourNodeIds[j];
				if (!lookup.TryGetValue(neighbourId, out FeatNode neighbour))
				{
					continue;
				}

				if (string.CompareOrdinal(node.Id, neighbour.Id) > 0)
				{
					continue;
				}

				Vector3 from = GetNodeRect(node).center;
				Vector3 to = GetNodeRect(neighbour).center;

				bool isHighlighted =
					node.Id == selectedNodeId ||
					neighbour.Id == selectedNodeId ||
					node.Id == linkSourceNodeId ||
					neighbour.Id == linkSourceNodeId;

				Handles.color = isHighlighted
					? new Color(1f, 0.94f, 0.5f, 0.95f)
					: new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.55f : 0.7f);

				Handles.DrawAAPolyLine(isHighlighted ? 4f : 2.5f, from, to);
			}
		}

		Handles.EndGUI();
	}

	private void DrawNodes(Vector2 canvasSize)
	{
		List<FeatNode> drawOrder = GetDrawOrder();
		Rect visibleRect = new Rect(-120f, -120f, canvasSize.x + 240f, canvasSize.y + 240f);

		for (int i = 0; i < drawOrder.Count; i++)
		{
			FeatNode node = drawOrder[i];
			if (node == null)
			{
				continue;
			}

			Rect rect = GetNodeRect(node);
			if (!rect.Overlaps(visibleRect))
			{
				continue;
			}

			DrawNodeCard(node, rect);
		}

		if (drawOrder.Count == 0)
		{
			DrawCanvasEmptyState(new Rect(0f, 0f, canvasSize.x, canvasSize.y), "No nodes yet.", "Double-click the grid or use Add Node to create the first feat node.");
		}
	}

	private void DrawNodeCard(FeatNode node, Rect rect)
	{
		bool isSelected = node.Id == selectedNodeId;
		bool isLinkSource = node.Id == linkSourceNodeId;

		Color kindColor = GetNodeColor(node.Kind);
		Color fillColor = Color.Lerp(kindColor, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : Color.white, EditorGUIUtility.isProSkin ? 0.78f : 0.74f);
		Color headerColor = Color.Lerp(kindColor, Color.white, EditorGUIUtility.isProSkin ? 0.12f : 0.18f);
		Color borderColor = isSelected
			? new Color(1f, 0.96f, 0.6f, 1f)
			: isLinkSource
				? new Color(0.68f, 1f, 0.76f, 1f)
				: Color.Lerp(kindColor, Color.black, EditorGUIUtility.isProSkin ? 0.15f : 0.35f);
		Color titleTextColor = GetReadableTextColor(headerColor);
		Color bodyTextColor = GetReadableTextColor(fillColor);
		Color badgeBackgroundColor = ShouldUseDarkText(headerColor)
			? new Color(1f, 1f, 1f, 0.22f)
			: new Color(0f, 0f, 0f, 0.18f);
		Color badgeTextColor = GetReadableTextColor(Color.Lerp(headerColor, badgeBackgroundColor, badgeBackgroundColor.a));
		GUIStyle titleStyle = CreateColoredStyle(nodeTitleStyle, titleTextColor);
		GUIStyle bodyStyle = CreateColoredStyle(nodeBodyStyle, bodyTextColor);
		GUIStyle badgeStyle = CreateColoredStyle(nodeBadgeStyle, badgeTextColor);

		float padding = 10f * zoom;
		float headerHeight = 28f * zoom;
		float borderThickness = isSelected || isLinkSource ? 3f : 1.5f;
		float shadowOffset = 4f;

		EditorGUI.DrawRect(new Rect(rect.x + shadowOffset, rect.y + shadowOffset, rect.width, rect.height), new Color(0f, 0f, 0f, 0.18f));
		EditorGUI.DrawRect(rect, fillColor);
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, headerHeight), headerColor);
		DrawRectOutline(rect, borderColor, borderThickness);

		Rect titleRect = new Rect(rect.x + padding, rect.y + 5f * zoom, rect.width - padding * 2f - 76f * zoom, headerHeight - 10f * zoom);
		GUI.Label(titleRect, string.IsNullOrWhiteSpace(node.DisplayName) ? "Unnamed Node" : node.DisplayName, titleStyle);

		float badgeWidth = Mathf.Min(rect.width * 0.42f, 88f * zoom);
		Rect badgeRect = new Rect(rect.xMax - badgeWidth - padding * 0.7f, rect.y + 6f * zoom, badgeWidth, 18f * zoom);
		EditorGUI.DrawRect(badgeRect, badgeBackgroundColor);
		GUI.Label(badgeRect, GetKindLabel(node.Kind), badgeStyle);

		float iconSize = node.Icon != null ? 44f * zoom : 0f;
		Rect contentRect = new Rect(rect.x + padding, rect.y + headerHeight + padding, rect.width - padding * 2f, rect.height - headerHeight - padding * 2f);
		Rect bodyRect = contentRect;

		if (iconSize > 0f)
		{
			Rect iconRect = new Rect(contentRect.x, contentRect.y, iconSize, iconSize);
			DrawSpritePreview(iconRect, node.Icon);
			DrawRectOutline(iconRect, new Color(1f, 1f, 1f, 0.2f), 1f);
			bodyRect.x += iconSize + 8f * zoom;
			bodyRect.width -= iconSize + 8f * zoom;
		}

		GUI.Label(bodyRect, BuildNodeBody(node), bodyStyle);
		EditorGUIUtility.AddCursorRect(ToScreenRect(rect), MouseCursor.MoveArrow);
	}

	private void DrawCanvasOverlay(Vector2 canvasSize)
	{
		Rect hintRect = new Rect(14f, 14f, Mathf.Min(canvasSize.x - 28f, 510f), 58f);
		EditorGUI.DrawRect(hintRect, EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.2f) : new Color(1f, 1f, 1f, 0.7f));

		string hintText = string.IsNullOrEmpty(linkSourceNodeId)
			? "LMB select and drag  |  Shift+Click toggle link  |  Double-click empty space to add  |  Alt+LMB or MMB pan  |  Mouse wheel zoom"
			: "Link mode: click another node to toggle a link, or click the source node again to cancel.";

		GUI.Label(new Rect(hintRect.x + 12f, hintRect.y + 9f, hintRect.width - 24f, 38f), hintText, canvasHintStyle);

		Rect footerRect = new Rect(14f, canvasSize.y - 30f, 220f, 18f);
		string footerText = species != null ? GetNodes().Count + " nodes  |  Zoom " + Mathf.RoundToInt(zoom * 100f) + "%" : string.Empty;
		GUI.Label(footerRect, footerText, EditorStyles.miniLabel);
	}

	private void DrawInspector(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.93f, 0.93f, 0.93f));

		Rect areaRect = new Rect(
			rect.x + InspectorPadding,
			rect.y + InspectorPadding,
			rect.width - InspectorPadding * 2f,
			rect.height - InspectorPadding * 2f);

		GUILayout.BeginArea(areaRect);
		inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

		DrawBoardInspector();
		EditorGUILayout.Space(12f);
		DrawSelectedNodeInspector();

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawBoardInspector()
	{
		EditorGUILayout.LabelField("Board", EditorStyles.boldLabel);

		if (species == null)
		{
			EditorGUILayout.HelpBox("Select a creature species to edit its feat board.", MessageType.Info);
			return;
		}

		EditorGUILayout.ObjectField("Species", species, typeof(CreatureSpecies), false);
		EditorGUILayout.LabelField("Node Count", GetNodes().Count.ToString());
		EditorGUILayout.LabelField("Selected Node", GetSelectedNode() != null ? GetNodeLabel(GetSelectedNode()) : "None");

		EditorGUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Add Node"))
		{
			AddNodeAtViewportCenter();
		}

		if (GUILayout.Button("Frame All"))
		{
			FrameAllNodes();
		}

		if (GUILayout.Button("Repair"))
		{
			EnsureBoardIntegrity(true, "Repair Feat Board");
		}

		EditorGUILayout.EndHorizontal();
	}

	private void DrawSelectedNodeInspector()
	{
		EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

		FeatNode node = GetSelectedNode();
		if (node == null)
		{
			EditorGUILayout.HelpBox("Select a node on the grid to edit its contents, position and links.", MessageType.Info);
			return;
		}

		EditorGUILayout.HelpBox("Shift+Click another node on the grid to toggle a link. Link Mode keeps the link source active for multiple connections.", MessageType.Info);

		EditorGUI.BeginChangeCheck();
		string newDisplayName = EditorGUILayout.TextField("Display Name", node.DisplayName);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Rename Feat Node", () => node.DisplayName = newDisplayName);
		}

		EditorGUI.BeginChangeCheck();
		FeatNodeKind newKind = (FeatNodeKind)EditorGUILayout.EnumPopup("Kind", node.Kind);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Change Feat Node Kind", () =>
			{
				node.Kind = newKind;
				if (node.Kind != FeatNodeKind.StatsBonus)
				{
					node.NumberOfRepeatTime = 0;
				}
			});
		}

		DrawRepeatTimeField(node);

		EditorGUI.BeginChangeCheck();
		Vector2 newPosition = EditorGUILayout.Vector2Field("Grid Position", node.Position);
		if (EditorGUI.EndChangeCheck())
		{
			if (snapToGrid)
			{
				newPosition = SnapToGrid(newPosition);
			}

			ApplySpeciesChange("Move Feat Node", () => node.Position = newPosition);
		}

		EditorGUI.BeginChangeCheck();
		Sprite newIcon = (Sprite)EditorGUILayout.ObjectField("Icon", node.Icon, typeof(Sprite), false);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Change Feat Node Icon", () => node.Icon = newIcon);
		}

		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.TextField("Id", node.Id);
		}

		EditorGUILayout.Space(6f);
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button(linkSourceNodeId == node.Id ? "Stop Link Mode" : "Start Link Mode"))
		{
			linkSourceNodeId = linkSourceNodeId == node.Id ? null : node.Id;
			Repaint();
		}

		if (GUILayout.Button("Snap Position"))
		{
			ApplySpeciesChange("Snap Feat Node Position", () => node.Position = SnapToGrid(node.Position));
		}

		if (GUILayout.Button("Frame"))
		{
			FrameNode(node);
		}

		EditorGUILayout.EndHorizontal();

		if (GUILayout.Button("Delete Node"))
		{
			RemoveNode(node.Id);
			GUIUtility.ExitGUI();
		}

		EditorGUILayout.Space(10f);
		DrawLinksInspector(node);
		EditorGUILayout.Space(10f);
		DrawRequirementsInspector(node);
		EditorGUILayout.Space(10f);
		DrawRewardsInspector(node);
	}

	private void DrawLinksInspector(FeatNode node)
	{
		EditorGUILayout.LabelField("Links", EditorStyles.boldLabel);

		if (node.NeighbourNodeIds == null || node.NeighbourNodeIds.Count == 0)
		{
			EditorGUILayout.HelpBox("No links yet.", MessageType.None);
			return;
		}

		string linkToRemove = null;

		for (int i = 0; i < node.NeighbourNodeIds.Count; i++)
		{
			string neighbourId = node.NeighbourNodeIds[i];
			FeatNode neighbour = GetNodeById(neighbourId);
			string label = neighbour != null ? GetNodeLabel(neighbour) : neighbourId;

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(label, EditorStyles.miniButtonLeft))
			{
				selectedNodeId = neighbourId;
				if (neighbour != null)
				{
					FrameNode(neighbour);
				}
			}

			if (GUILayout.Button("Unlink", EditorStyles.miniButtonRight, GUILayout.Width(58f)))
			{
				linkToRemove = neighbourId;
			}

			EditorGUILayout.EndHorizontal();
		}

		if (!string.IsNullOrEmpty(linkToRemove))
		{
			ToggleLink(node.Id, linkToRemove);
			GUIUtility.ExitGUI();
		}
	}

	private void DrawRepeatTimeField(FeatNode node)
	{
		if (node.Kind != FeatNodeKind.StatsBonus)
		{
			return;
		}

		RepeatTimeMode currentMode = GetRepeatTimeMode(node.NumberOfRepeatTime);
		int newRepeatTime = node.NumberOfRepeatTime;

		Rect rect = EditorGUILayout.GetControlRect();
		rect = EditorGUI.PrefixLabel(rect, new GUIContent("Repeat time"));

		Rect modeRect = rect;
		Rect valueRect = Rect.zero;
		if (currentMode == RepeatTimeMode.Repeatable)
		{
			const float spacing = 4f;
			float halfWidth = (rect.width - spacing) * 0.5f;
			modeRect.width = halfWidth;
			valueRect = new Rect(modeRect.xMax + spacing, rect.y, rect.width - halfWidth - spacing, rect.height);
		}

		EditorGUI.BeginChangeCheck();
		RepeatTimeMode newMode = (RepeatTimeMode)EditorGUI.EnumPopup(modeRect, currentMode);

		if (newMode != currentMode)
		{
			switch (newMode)
			{
				case RepeatTimeMode.Unique:
					newRepeatTime = 0;
					break;

				case RepeatTimeMode.Repeatable:
					newRepeatTime = node.NumberOfRepeatTime > 0 ? node.NumberOfRepeatTime : 1;
					break;

				case RepeatTimeMode.Infinite:
					newRepeatTime = -1;
					break;
			}
		}

		if (newMode == RepeatTimeMode.Repeatable)
		{
			int displayedValue = newRepeatTime > 0 ? newRepeatTime : 1;
			newRepeatTime = Mathf.Max(1, EditorGUI.IntField(valueRect, displayedValue));
		}

		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Edit Feat Repeat Time", () => node.NumberOfRepeatTime = newRepeatTime);
		}
	}

	private void DrawRequirementsInspector(FeatNode node)
	{
		EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);

		if (node.Requirements == null)
		{
			node.Requirements = new List<FeatRequirement>();
		}

		int removeIndex = -1;

		for (int i = 0; i < node.Requirements.Count; i++)
		{
			FeatRequirement requirement = node.Requirements[i];

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(requirement != null ? ObjectNames.NicifyVariableName(requirement.GetType().Name.Replace("Requirement", string.Empty)) : "Missing Requirement", EditorStyles.miniBoldLabel);

			if (GUILayout.Button("Remove", GUILayout.Width(68f)))
			{
				removeIndex = i;
			}

			EditorGUILayout.EndHorizontal();
			DrawRequirementFields(requirement);
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(4f);
		}

		if (removeIndex >= 0)
		{
			ApplySpeciesChange("Remove Feat Requirement", () => node.Requirements.RemoveAt(removeIndex));
			GUIUtility.ExitGUI();
		}

		if (GUILayout.Button("Add Requirement"))
		{
			ShowAddRequirementMenu(node);
		}
	}

	private void DrawRequirementFields(FeatRequirement requirement)
	{
		if (requirement == null)
		{
			EditorGUILayout.HelpBox("This requirement reference is missing.", MessageType.Warning);
			return;
		}

		switch (requirement)
		{
			case DealDamageRequirement dealDamage:
				EditorGUI.BeginChangeCheck();
				int dealAmount = EditorGUILayout.IntField("Required Damage", dealDamage.RequiredAmount);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Requirement", () => dealDamage.RequiredAmount = Mathf.Max(0, dealAmount));
				}
				break;

			case HealHealthRequirement healHealth:
				EditorGUI.BeginChangeCheck();
				int healAmount = EditorGUILayout.IntField("Required Healing", healHealth.RequiredAmount);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Requirement", () => healHealth.RequiredAmount = Mathf.Max(0, healAmount));
				}
				break;

			default:
				EditorGUILayout.HelpBox("Unsupported requirement type: " + requirement.GetType().Name, MessageType.Warning);
				break;
		}
	}

	private void DrawRewardsInspector(FeatNode node)
	{
		EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);

		if (node.Rewards == null)
		{
			node.Rewards = new List<FeatReward>();
		}

		int removeIndex = -1;

		for (int i = 0; i < node.Rewards.Count; i++)
		{
			FeatReward reward = node.Rewards[i];

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(reward != null ? ObjectNames.NicifyVariableName(reward.GetType().Name.Replace("Reward", string.Empty)) : "Missing Reward", EditorStyles.miniBoldLabel);

			if (GUILayout.Button("Remove", GUILayout.Width(68f)))
			{
				removeIndex = i;
			}

			EditorGUILayout.EndHorizontal();
			DrawRewardFields(reward);
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(4f);
		}

		if (removeIndex >= 0)
		{
			ApplySpeciesChange("Remove Feat Reward", () => node.Rewards.RemoveAt(removeIndex));
			GUIUtility.ExitGUI();
		}

		if (GUILayout.Button("Add Reward"))
		{
			ShowAddRewardMenu(node);
		}
	}

	private void DrawRewardFields(FeatReward reward)
	{
		if (reward == null)
		{
			EditorGUILayout.HelpBox("This reward reference is missing.", MessageType.Warning);
			return;
		}

		switch (reward)
		{
			case BonusStatsReward bonusStats:
				EditorGUI.BeginChangeCheck();
				AttributeType attribute = (AttributeType)EditorGUILayout.EnumPopup("Attribute", bonusStats.Attribute);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => bonusStats.Attribute = attribute);
				}

				EditorGUI.BeginChangeCheck();
				int statValue = EditorGUILayout.IntField("Value", bonusStats.Value);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => bonusStats.Value = statValue);
				}
				break;

			case AbilityReward abilityReward:
				EditorGUI.BeginChangeCheck();
				Ability ability = (Ability)EditorGUILayout.ObjectField("Ability", abilityReward.Ability, typeof(Ability), false);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => abilityReward.Ability = ability);
				}
				break;

			case PassiveReward passiveReward:
				EditorGUI.BeginChangeCheck();
				string passiveName = EditorGUILayout.TextField("Passive Name", passiveReward.PassiveName);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => passiveReward.PassiveName = passiveName);
				}
				break;

			case ChangeFormReward changeFormReward:
				DrawFormRewardField(changeFormReward);
				break;

			default:
				EditorGUILayout.HelpBox("Unsupported reward type: " + reward.GetType().Name, MessageType.Warning);
				break;
		}
	}

	private void DrawFormRewardField(ChangeFormReward reward)
	{
		List<string> formKeys = GetSpeciesFormKeys();

		if (formKeys.Count == 0)
		{
			EditorGUILayout.HelpBox("This species has no forms configured yet. Enter the form key manually or add forms on the species asset.", MessageType.Info);

			EditorGUI.BeginChangeCheck();
			string manualKey = EditorGUILayout.TextField("Form Key", reward.FormKey);
			if (EditorGUI.EndChangeCheck())
			{
				ApplySpeciesChange("Edit Feat Reward", () => reward.FormKey = manualKey);
			}

			return;
		}

		List<string> options = new List<string> { "<None>" };
		options.AddRange(formKeys);

		int selectedIndex = 0;
		if (!string.IsNullOrEmpty(reward.FormKey))
		{
			int optionIndex = formKeys.IndexOf(reward.FormKey);
			selectedIndex = optionIndex >= 0 ? optionIndex + 1 : 0;
		}

		EditorGUI.BeginChangeCheck();
		int newIndex = EditorGUILayout.Popup("Form", selectedIndex, options.ToArray());
		if (EditorGUI.EndChangeCheck())
		{
			string newFormKey = newIndex <= 0 ? string.Empty : formKeys[newIndex - 1];
			ApplySpeciesChange("Edit Feat Reward", () => reward.FormKey = newFormKey);
		}
	}

	private void ShowAddRewardMenu(FeatNode node)
	{
		GenericMenu menu = new GenericMenu();

		for (int i = 0; i < RewardTypes.Length; i++)
		{
			Type rewardType = RewardTypes[i];
			string label = ObjectNames.NicifyVariableName(rewardType.Name.Replace("Reward", string.Empty));

			menu.AddItem(new GUIContent(label), false, () =>
			{
				ApplySpeciesChange("Add Feat Reward", () => node.Rewards.Add((FeatReward)Activator.CreateInstance(rewardType)));
			});
		}

		menu.ShowAsContext();
	}

	private void ShowAddRequirementMenu(FeatNode node)
	{
		GenericMenu menu = new GenericMenu();

		for (int i = 0; i < RequirementTypes.Length; i++)
		{
			Type requirementType = RequirementTypes[i];
			string label = ObjectNames.NicifyVariableName(requirementType.Name.Replace("Requirement", string.Empty));

			menu.AddItem(new GUIContent(label), false, () =>
			{
				ApplySpeciesChange("Add Feat Requirement", () => node.Requirements.Add((FeatRequirement)Activator.CreateInstance(requirementType)));
			});
		}

		menu.ShowAsContext();
	}

	private void HandleCanvasInput(Rect canvasRect)
	{
		Event evt = Event.current;
		bool mouseInCanvas = canvasRect.Contains(evt.mousePosition);
		Vector2 localMouse = evt.mousePosition - canvasRect.position;

		if (evt.type == EventType.ScrollWheel && mouseInCanvas)
		{
			float oldZoom = zoom;
			float newZoom = Mathf.Clamp(zoom - evt.delta.y * 0.03f, MinZoom, MaxZoom);

			if (!Mathf.Approximately(oldZoom, newZoom))
			{
				Vector2 boardBeforeZoom = CanvasToBoard(localMouse);
				zoom = newZoom;
				canvasPan = localMouse - boardBeforeZoom * GetScaledGridSize();
				Repaint();
			}

			evt.Use();
			return;
		}

		if (evt.type == EventType.MouseDown && mouseInCanvas)
		{
			if (evt.button == 2 || (evt.button == 0 && evt.alt))
			{
				isPanningCanvas = true;
				panStart = canvasPan;
				panMouseStart = localMouse;
				evt.Use();
				return;
			}

			if (evt.button == 1)
			{
				ShowCanvasContextMenu(localMouse);
				evt.Use();
				return;
			}

			if (evt.button != 0)
			{
				return;
			}

			FeatNode clickedNode = GetNodeAtCanvasPosition(localMouse);

			if (!string.IsNullOrEmpty(linkSourceNodeId))
			{
				if (clickedNode == null)
				{
					linkSourceNodeId = null;
				}
				else if (clickedNode.Id == linkSourceNodeId)
				{
					linkSourceNodeId = null;
					selectedNodeId = clickedNode.Id;
				}
				else
				{
					ToggleLink(linkSourceNodeId, clickedNode.Id);
					selectedNodeId = clickedNode.Id;
				}

				evt.Use();
				return;
			}

			if (clickedNode == null)
			{
				selectedNodeId = null;

				if (evt.clickCount == 2)
				{
					AddNodeAtCanvasPosition(localMouse);
				}

				Repaint();
				return;
			}

			if (evt.shift && !string.IsNullOrEmpty(selectedNodeId) && clickedNode.Id != selectedNodeId)
			{
				ToggleLink(selectedNodeId, clickedNode.Id);
				evt.Use();
				return;
			}

			selectedNodeId = clickedNode.Id;
			draggedNodeId = clickedNode.Id;
			isDraggingNode = true;
			dragNodeStart = clickedNode.Position;
			dragMouseStart = localMouse;

			Undo.RecordObject(species, "Move Feat Node");
			evt.Use();
			Repaint();
			return;
		}

		if (evt.type == EventType.MouseDrag)
		{
			if (isPanningCanvas)
			{
				canvasPan = panStart + (localMouse - panMouseStart);
				evt.Use();
				Repaint();
				return;
			}

			if (isDraggingNode && evt.button == 0)
			{
				FeatNode draggedNode = GetNodeById(draggedNodeId);
				if (draggedNode != null)
				{
					Vector2 deltaBoard = (localMouse - dragMouseStart) / GetScaledGridSize();
					Vector2 newPosition = dragNodeStart + deltaBoard;
					if (snapToGrid)
					{
						newPosition = SnapToGrid(newPosition);
					}

					draggedNode.Position = newPosition;
					MarkSpeciesDirty();
				}

				evt.Use();
				return;
			}
		}

		if (evt.type == EventType.MouseUp)
		{
			if (evt.button == 0 && isDraggingNode)
			{
				isDraggingNode = false;
				draggedNodeId = null;
				evt.Use();
				return;
			}

			if ((evt.button == 0 || evt.button == 2) && isPanningCanvas)
			{
				isPanningCanvas = false;
				evt.Use();
				return;
			}
		}

		if (evt.type == EventType.KeyDown && !EditorGUIUtility.editingTextField)
		{
			if (evt.keyCode == KeyCode.Escape)
			{
				linkSourceNodeId = null;
				isDraggingNode = false;
				isPanningCanvas = false;
				Repaint();
				evt.Use();
				return;
			}

			if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
			{
				RemoveNode(selectedNodeId);
				evt.Use();
				return;
			}

			if (evt.keyCode == KeyCode.F)
			{
				if (evt.shift)
				{
					FrameAllNodes();
				}
				else
				{
					FrameSelectedNode();
				}

				evt.Use();
				return;
			}

			FeatNode selectedNode = GetSelectedNode();
			if (selectedNode == null)
			{
				return;
			}

			Vector2 delta = Vector2.zero;
			if (evt.keyCode == KeyCode.LeftArrow)
			{
				delta = Vector2.left;
			}
			else if (evt.keyCode == KeyCode.RightArrow)
			{
				delta = Vector2.right;
			}
			else if (evt.keyCode == KeyCode.UpArrow)
			{
				delta = Vector2.up;
			}
			else if (evt.keyCode == KeyCode.DownArrow)
			{
				delta = Vector2.down;
			}

			if (delta != Vector2.zero)
			{
				ApplySpeciesChange("Move Feat Node", () =>
				{
					selectedNode.Position = snapToGrid
						? SnapToGrid(selectedNode.Position + delta)
						: selectedNode.Position + delta;
				});
				evt.Use();
			}
		}
	}

	private void ShowCanvasContextMenu(Vector2 localMouse)
	{
		GenericMenu menu = new GenericMenu();
		FeatNode node = GetNodeAtCanvasPosition(localMouse);

		if (node != null)
		{
			menu.AddItem(new GUIContent("Select"), node.Id == selectedNodeId, () => selectedNodeId = node.Id);
			menu.AddItem(new GUIContent(linkSourceNodeId == node.Id ? "Stop Link Mode" : "Start Link Mode"), false, () =>
			{
				selectedNodeId = node.Id;
				linkSourceNodeId = linkSourceNodeId == node.Id ? null : node.Id;
				Repaint();
			});

			if (!string.IsNullOrEmpty(selectedNodeId) && selectedNodeId != node.Id)
			{
				string toggleLabel = AreLinked(selectedNodeId, node.Id) ? "Remove Link With Selected" : "Create Link With Selected";
				menu.AddItem(new GUIContent(toggleLabel), false, () => ToggleLink(selectedNodeId, node.Id));
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Create Link With Selected"));
			}

			menu.AddItem(new GUIContent("Frame Node"), false, () => FrameNode(node));
			menu.AddSeparator(string.Empty);
			menu.AddItem(new GUIContent("Delete Node"), false, () => RemoveNode(node.Id));
		}
		else
		{
			menu.AddItem(new GUIContent("Add Node Here"), false, () => AddNodeAtCanvasPosition(localMouse));
			menu.AddItem(new GUIContent("Frame All"), false, FrameAllNodes);
			menu.AddItem(new GUIContent("Center View"), false, CenterView);
		}

		menu.ShowAsContext();
	}

	private void AddNodeAtViewportCenter()
	{
		AddNodeAtCanvasPosition(lastCanvasSize * 0.5f);
	}

	private void AddNodeAtCanvasPosition(Vector2 localPosition)
	{
		if (species == null)
		{
			return;
		}

		Vector2 boardPosition = CanvasToBoard(localPosition) - NodeGridSize * 0.5f;
		if (snapToGrid)
		{
			boardPosition = SnapToGrid(boardPosition);
		}

		FeatNode newNode = new FeatNode
		{
			DisplayName = "New Feat",
			Position = boardPosition
		};

		ApplySpeciesChange("Add Feat Node", () => GetNodes().Add(newNode));

		selectedNodeId = newNode.Id;
		linkSourceNodeId = null;
		FrameNode(newNode);
	}

	private void RemoveNode(string nodeId)
	{
		if (species == null || string.IsNullOrEmpty(nodeId))
		{
			return;
		}

		List<FeatNode> nodes = GetNodes();
		FeatNode node = GetNodeById(nodeId);
		if (node == null)
		{
			return;
		}

		ApplySpeciesChange("Delete Feat Node", () =>
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] != null && nodes[i].NeighbourNodeIds != null)
				{
					nodes[i].NeighbourNodeIds.Remove(nodeId);
				}
			}

			nodes.Remove(node);
		});

		if (selectedNodeId == nodeId)
		{
			selectedNodeId = null;
		}

		if (linkSourceNodeId == nodeId)
		{
			linkSourceNodeId = null;
		}
	}

	private void ToggleLink(string firstNodeId, string secondNodeId)
	{
		if (species == null || string.IsNullOrEmpty(firstNodeId) || string.IsNullOrEmpty(secondNodeId) || firstNodeId == secondNodeId)
		{
			return;
		}

		FeatNode first = GetNodeById(firstNodeId);
		FeatNode second = GetNodeById(secondNodeId);
		if (first == null || second == null)
		{
			return;
		}

		bool currentlyLinked = AreLinked(firstNodeId, secondNodeId);

		ApplySpeciesChange(currentlyLinked ? "Remove Feat Link" : "Create Feat Link", () =>
		{
			if (currentlyLinked)
			{
				first.NeighbourNodeIds.Remove(secondNodeId);
				second.NeighbourNodeIds.Remove(firstNodeId);
			}
			else
			{
				if (!first.NeighbourNodeIds.Contains(secondNodeId))
				{
					first.NeighbourNodeIds.Add(secondNodeId);
				}

				if (!second.NeighbourNodeIds.Contains(firstNodeId))
				{
					second.NeighbourNodeIds.Add(firstNodeId);
				}
			}
		});
	}

	private bool AreLinked(string firstNodeId, string secondNodeId)
	{
		FeatNode first = GetNodeById(firstNodeId);
		if (first == null || first.NeighbourNodeIds == null)
		{
			return false;
		}

		return first.NeighbourNodeIds.Contains(secondNodeId);
	}

	private void FrameSelectedNode()
	{
		FeatNode selectedNode = GetSelectedNode();
		if (selectedNode != null)
		{
			FrameNode(selectedNode);
		}
	}

	private void FrameNode(FeatNode node)
	{
		if (node == null || lastCanvasSize.x <= 0f || lastCanvasSize.y <= 0f)
		{
			return;
		}

		Rect rect = GetNodeRect(node);
		canvasPan += lastCanvasSize * 0.5f - rect.center;
		Repaint();
	}

	private void FrameAllNodes()
	{
		List<FeatNode> nodes = GetNodes();
		if (nodes.Count == 0)
		{
			CenterView();
			return;
		}

		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null)
			{
				continue;
			}

			minX = Mathf.Min(minX, node.Position.x);
			minY = Mathf.Min(minY, node.Position.y);
			maxX = Mathf.Max(maxX, node.Position.x + NodeGridSize.x);
			maxY = Mathf.Max(maxY, node.Position.y + NodeGridSize.y);
		}

		if (minX == float.MaxValue)
		{
			CenterView();
			return;
		}

		Rect boardBounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
		float availableWidth = Mathf.Max(120f, lastCanvasSize.x - 140f);
		float availableHeight = Mathf.Max(120f, lastCanvasSize.y - 140f);

		float widthZoom = availableWidth / (boardBounds.width * BaseGridSize);
		float heightZoom = availableHeight / (boardBounds.height * BaseGridSize);
		zoom = Mathf.Clamp(Mathf.Min(widthZoom, heightZoom, MaxZoom), MinZoom, MaxZoom);

		Vector2 boardCenter = boardBounds.center;
		canvasPan = lastCanvasSize * 0.5f - boardCenter * GetScaledGridSize();
		Repaint();
	}

	private void CenterView()
	{
		canvasPan = lastCanvasSize * 0.5f;
		Repaint();
	}

	private void ApplySpeciesChange(string undoLabel, Action mutation)
	{
		if (species == null || mutation == null)
		{
			return;
		}

		Undo.RecordObject(species, undoLabel);
		mutation();
		MarkSpeciesDirty();
	}

	private void MarkSpeciesDirty()
	{
		if (species == null)
		{
			return;
		}

		EditorUtility.SetDirty(species);
		Repaint();
	}

	private void EnsureBoardIntegrity(bool recordUndo, string undoLabel)
	{
		if (species == null)
		{
			return;
		}

		bool changed = false;
		bool undoRecorded = false;

		void RecordUndoIfNeeded()
		{
			if (!recordUndo || undoRecorded)
			{
				return;
			}

			Undo.RecordObject(species, undoLabel);
			undoRecorded = true;
		}

		if (species.FeatBoard == null)
		{
			RecordUndoIfNeeded();
			species.FeatBoard = new FeatBoard();
			changed = true;
		}

		if (species.FeatBoard.Nodes == null)
		{
			RecordUndoIfNeeded();
			species.FeatBoard.Nodes = new List<FeatNode>();
			changed = true;
		}

		List<FeatNode> nodes = species.FeatBoard.Nodes;

		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			if (nodes[i] == null)
			{
				RecordUndoIfNeeded();
				nodes.RemoveAt(i);
				changed = true;
			}
		}

		HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);
		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null)
			{
				continue;
			}

			if (string.IsNullOrWhiteSpace(node.Id) || usedIds.Contains(node.Id))
			{
				RecordUndoIfNeeded();
				node.Id = Guid.NewGuid().ToString();
				changed = true;
			}

			usedIds.Add(node.Id);

			if (node.NeighbourNodeIds == null)
			{
				RecordUndoIfNeeded();
				node.NeighbourNodeIds = new List<string>();
				changed = true;
			}

			if (node.Requirements == null)
			{
				RecordUndoIfNeeded();
				node.Requirements = new List<FeatRequirement>();
				changed = true;
			}

			if (node.Rewards == null)
			{
				RecordUndoIfNeeded();
				node.Rewards = new List<FeatReward>();
				changed = true;
			}

			if (node.Kind != FeatNodeKind.StatsBonus && node.NumberOfRepeatTime != 0)
			{
				RecordUndoIfNeeded();
				node.NumberOfRepeatTime = 0;
				changed = true;
			}
		}

		Dictionary<string, FeatNode> lookup = BuildNodeLookup(nodes);

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null || node.NeighbourNodeIds == null)
			{
				continue;
			}

			HashSet<string> uniqueNeighbours = new HashSet<string>(StringComparer.Ordinal);
			for (int j = node.NeighbourNodeIds.Count - 1; j >= 0; j--)
			{
				string neighbourId = node.NeighbourNodeIds[j];
				bool isInvalid =
					string.IsNullOrWhiteSpace(neighbourId) ||
					neighbourId == node.Id ||
					!lookup.ContainsKey(neighbourId) ||
					!uniqueNeighbours.Add(neighbourId);

				if (!isInvalid)
				{
					continue;
				}

				RecordUndoIfNeeded();
				node.NeighbourNodeIds.RemoveAt(j);
				changed = true;
			}
		}

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null || node.NeighbourNodeIds == null)
			{
				continue;
			}

			for (int j = 0; j < node.NeighbourNodeIds.Count; j++)
			{
				string neighbourId = node.NeighbourNodeIds[j];
				if (!lookup.TryGetValue(neighbourId, out FeatNode neighbour))
				{
					continue;
				}

				if (neighbour.NeighbourNodeIds.Contains(node.Id))
				{
					continue;
				}

				RecordUndoIfNeeded();
				neighbour.NeighbourNodeIds.Add(node.Id);
				changed = true;
			}
		}

		if (changed)
		{
			MarkSpeciesDirty();
		}
	}

	private List<FeatNode> GetNodes()
	{
		if (species == null)
		{
			return EmptyNodeList;
		}

		if (species.FeatBoard == null)
		{
			species.FeatBoard = new FeatBoard();
		}

		if (species.FeatBoard.Nodes == null)
		{
			species.FeatBoard.Nodes = new List<FeatNode>();
		}

		return species.FeatBoard.Nodes;
	}

	private FeatNode GetSelectedNode()
	{
		return GetNodeById(selectedNodeId);
	}

	private FeatNode GetNodeById(string nodeId)
	{
		if (string.IsNullOrEmpty(nodeId))
		{
			return null;
		}

		List<FeatNode> nodes = GetNodes();
		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node != null && node.Id == nodeId)
			{
				return node;
			}
		}

		return null;
	}

	private FeatNode GetNodeAtCanvasPosition(Vector2 localPosition)
	{
		List<FeatNode> drawOrder = GetDrawOrder();

		for (int i = drawOrder.Count - 1; i >= 0; i--)
		{
			FeatNode node = drawOrder[i];
			if (node != null && GetNodeRect(node).Contains(localPosition))
			{
				return node;
			}
		}

		return null;
	}

	private List<FeatNode> GetDrawOrder()
	{
		List<FeatNode> nodes = GetNodes();
		List<FeatNode> drawOrder = new List<FeatNode>(nodes.Count);
		FeatNode selectedNode = GetSelectedNode();

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node != null && node != selectedNode)
			{
				drawOrder.Add(node);
			}
		}

		if (selectedNode != null)
		{
			drawOrder.Add(selectedNode);
		}

		return drawOrder;
	}

	private Rect GetNodeRect(FeatNode node)
	{
		Vector2 size = NodeGridSize * GetScaledGridSize();
		Vector2 position = BoardToCanvas(node.Position);
		return new Rect(position, size);
	}

	private Vector2 BoardToCanvas(Vector2 boardPosition)
	{
		return canvasPan + boardPosition * GetScaledGridSize();
	}

	private Vector2 CanvasToBoard(Vector2 canvasPosition)
	{
		return (canvasPosition - canvasPan) / GetScaledGridSize();
	}

	private float GetScaledGridSize()
	{
		return BaseGridSize * zoom;
	}

	private Vector2 SnapToGrid(Vector2 positionToSnap)
	{
		return new Vector2(Mathf.Round(positionToSnap.x), Mathf.Round(positionToSnap.y));
	}

	private string BuildNodeBody(FeatNode node)
	{
		List<string> lines = new List<string>
		{
			FormatGridLine(node),
			string.Empty,
			"Requirement"
		};

		AppendSummaryLines(lines, node.Requirements, FormatRequirementSummary);
		lines.Add(string.Empty);
		lines.Add("Reward");
		AppendSummaryLines(lines, node.Rewards, FormatRewardSummary);

		return string.Join("\n", lines);
	}

	private void AppendSummaryLines<T>(List<string> lines, List<T> entries, Func<T, string> formatter) where T : class
	{
		bool hasEntries = false;

		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				T entry = entries[i];
				if (entry == null)
				{
					continue;
				}

				lines.Add(formatter(entry));
				hasEntries = true;
			}
		}

		if (!hasEntries)
		{
			lines.Add("None");
		}
	}

	private RepeatTimeMode GetRepeatTimeMode(int numberOfRepeatTime)
	{
		if (numberOfRepeatTime < 0)
		{
			return RepeatTimeMode.Infinite;
		}

		return numberOfRepeatTime > 0 ? RepeatTimeMode.Repeatable : RepeatTimeMode.Unique;
	}

	private string FormatGridLine(FeatNode node)
	{
		string gridText = "Grid " + FormatVector2(node.Position);
		if (node == null || node.Kind != FeatNodeKind.StatsBonus)
		{
			return gridText;
		}

		switch (GetRepeatTimeMode(node.NumberOfRepeatTime))
		{
			case RepeatTimeMode.Repeatable:
				return gridText + " | Repeat " + node.NumberOfRepeatTime + " times";

			case RepeatTimeMode.Infinite:
				return gridText + " | Infinite";

			default:
				return gridText + " | Unique";
		}
	}

	private string FormatRewardSummary(FeatReward reward)
	{
		switch (reward)
		{
			case BonusStatsReward bonusStats:
				return (bonusStats.Value >= 0 ? "+" : string.Empty) + bonusStats.Value + " " + ObjectNames.NicifyVariableName(bonusStats.Attribute.ToString());

			case AbilityReward abilityReward:
				return "Ability: " + (abilityReward.Ability != null ? abilityReward.Ability.Name : "Unassigned");

			case PassiveReward passiveReward:
				return "Passive: " + (string.IsNullOrWhiteSpace(passiveReward.PassiveName) ? "Unnamed" : passiveReward.PassiveName);

			case ChangeFormReward changeFormReward:
				return "Form: " + (string.IsNullOrWhiteSpace(changeFormReward.FormKey) ? "Unassigned" : changeFormReward.FormKey);

			default:
				return "Unknown reward";
		}
	}

	private string FormatRequirementSummary(FeatRequirement requirement)
	{
		switch (requirement)
		{
			case DealDamageRequirement dealDamage:
				return "Deal " + dealDamage.RequiredAmount + " damage";

			case HealHealthRequirement healHealth:
				return "Heal " + healHealth.RequiredAmount;

			default:
				return "Unknown requirement";
		}
	}

	private string GetNodeLabel(FeatNode node)
	{
		if (node == null)
		{
			return "Missing Node";
		}

		string name = string.IsNullOrWhiteSpace(node.DisplayName) ? "Unnamed Node" : node.DisplayName;
		return name + " [" + GetKindLabel(node.Kind) + "]";
	}

	private string GetKindLabel(FeatNodeKind kind)
	{
		return ObjectNames.NicifyVariableName(kind.ToString());
	}

	private string FormatVector2(Vector2 value)
	{
		return Mathf.RoundToInt(value.x) + ", " + Mathf.RoundToInt(value.y);
	}

	private List<string> GetSpeciesFormKeys()
	{
		List<string> keys = new List<string>();
		if (species == null || species.Forms == null)
		{
			return keys;
		}

		foreach (var entry in species.Forms)
		{
			if (!string.IsNullOrWhiteSpace(entry.Key))
			{
				keys.Add(entry.Key);
			}
		}

		keys.Sort(StringComparer.OrdinalIgnoreCase);
		return keys;
	}

	private Dictionary<string, FeatNode> BuildNodeLookup(List<FeatNode> nodes)
	{
		Dictionary<string, FeatNode> lookup = new Dictionary<string, FeatNode>(StringComparer.Ordinal);

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node != null && !string.IsNullOrEmpty(node.Id) && !lookup.ContainsKey(node.Id))
			{
				lookup.Add(node.Id, node);
			}
		}

		return lookup;
	}

	private void EnsureStyles()
	{
		if (nodeTitleStyle == null)
		{
			nodeTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				wordWrap = true,
				clipping = TextClipping.Clip,
				alignment = TextAnchor.MiddleLeft
			};
		}

		if (nodeBodyStyle == null)
		{
			nodeBodyStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				wordWrap = true,
				clipping = TextClipping.Clip
			};
		}

		if (nodeBadgeStyle == null)
		{
			nodeBadgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};
		}

		if (canvasHintStyle == null)
		{
			canvasHintStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				wordWrap = true
			};
		}

		if (canvasEmptyTitleStyle == null)
		{
			canvasEmptyTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 15,
				alignment = TextAnchor.MiddleLeft
			};
		}

		if (canvasEmptyBodyStyle == null)
		{
			canvasEmptyBodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
			{
				alignment = TextAnchor.UpperLeft
			};
		}

		float zoomT = Mathf.InverseLerp(MinZoom, MaxZoom, zoom);
		nodeTitleStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(11f, 16f, zoomT));
		nodeBodyStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(9f, 12f, zoomT));
		nodeBadgeStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(8f, 10f, zoomT));
	}

	private Color GetCanvasBackgroundColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(0.16f, 0.17f, 0.18f) : new Color(0.89f, 0.9f, 0.92f);
	}

	private Color GetMinorGridColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.045f);
	}

	private Color GetMajorGridColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.11f) : new Color(0f, 0f, 0f, 0.1f);
	}

	private Color GetNodeColor(FeatNodeKind kind)
	{
		switch (kind)
		{
			case FeatNodeKind.StatsBonus:
				return new Color(0.18f, 0.56f, 0.96f);

			case FeatNodeKind.Ability:
				return new Color(0.9f, 0.33f, 0.3f);

			case FeatNodeKind.Passive:
				return new Color(0.2f, 0.76f, 0.48f);

			case FeatNodeKind.Form:
				return new Color(0.96f, 0.7f, 0.22f);

			default:
				return new Color(0.6f, 0.6f, 0.6f);
		}
	}

	private bool ShouldUseDarkText(Color backgroundColor)
	{
		float luminance =
			backgroundColor.r * 0.299f +
			backgroundColor.g * 0.587f +
			backgroundColor.b * 0.114f;

		return luminance >= 0.6f;
	}

	private Color GetReadableTextColor(Color backgroundColor)
	{
		return ShouldUseDarkText(backgroundColor)
			? new Color(0.14f, 0.14f, 0.14f, 0.98f)
			: new Color(0.96f, 0.96f, 0.96f, 0.98f);
	}

	private GUIStyle CreateColoredStyle(GUIStyle baseStyle, Color textColor)
	{
		GUIStyle style = new GUIStyle(baseStyle);
		style.normal.textColor = textColor;
		style.hover.textColor = textColor;
		style.active.textColor = textColor;
		style.focused.textColor = textColor;
		style.onNormal.textColor = textColor;
		style.onHover.textColor = textColor;
		style.onActive.textColor = textColor;
		style.onFocused.textColor = textColor;
		return style;
	}

	private void DrawRectOutline(Rect rect, Color color, float thickness)
	{
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
		EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
	}

	private void DrawSpritePreview(Rect rect, Sprite sprite)
	{
		if (sprite == null || sprite.texture == null)
		{
			return;
		}

		Texture2D texture = sprite.texture;
		Rect spriteRect = sprite.rect;
		Rect uv = new Rect(
			spriteRect.x / texture.width,
			spriteRect.y / texture.height,
			spriteRect.width / texture.width,
			spriteRect.height / texture.height);

		GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
	}

	private Rect ToScreenRect(Rect localRect)
	{
		return new Rect(localRect.x, localRect.y + ToolbarHeight, localRect.width, localRect.height);
	}

	private int CountNonNull<T>(List<T> list) where T : class
	{
		int count = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null)
			{
				count++;
			}
		}

		return count;
	}
}
