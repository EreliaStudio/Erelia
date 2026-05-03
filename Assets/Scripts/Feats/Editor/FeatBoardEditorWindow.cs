using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class FeatBoardEditorWindow : EditorWindow
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

	private static readonly List<FeatNode> EmptyNodeList = new List<FeatNode>();
	private static readonly Comparison<Type> FeatRewardTypeComparison =
		(left, right) => string.CompareOrdinal(GetFeatRewardLabel(left), GetFeatRewardLabel(right));
	private static readonly Comparison<Type> FeatRequirementTypeComparison =
		(left, right) => string.CompareOrdinal(GetFeatRequirementLabel(left), GetFeatRequirementLabel(right));

	private CreatureSpecies species;
	private FeatNode selectedNode;
	private FeatNode linkSourceNode;

	private Vector2 canvasPan = new Vector2(320f, 220f);
	private Vector2 lastCanvasSize = new Vector2(700f, 500f);
	private Vector2 inspectorScroll;

	private float zoom = DefaultZoom;
	private bool snapToGrid = true;

	private bool isDraggingNode;
	private FeatNode draggedNode;
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
		selectedNode = null;
		linkSourceNode = null;
		draggedNode = null;
		isDraggingNode = false;
		isPanningCanvas = false;
		inspectorScroll = Vector2.zero;
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
				string linkButtonLabel = linkSourceNode == selectedNode ? "Stop Link" : "Link Mode";
				if (GUI.Button(new Rect(x, y, 78f, height), linkButtonLabel, EditorStyles.toolbarButton))
				{
					linkSourceNode = linkSourceNode == selectedNode ? null : selectedNode;
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

		if (linkSourceNode != null)
		{
			string linkLabel = "Linking from: " + GetNodeLabel(linkSourceNode);
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
		if (nodes.Count == 0 || species?.FeatBoard == null)
		{
			return;
		}

		Dictionary<FeatNode, int> indexLookup = BuildNodeIndexLookup(nodes);
		Handles.BeginGUI();

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node == null)
			{
				continue;
			}

			List<FeatNode> neighbours = species.FeatBoard.GetNeighbourNodes(node);
			for (int j = 0; j < neighbours.Count; j++)
			{
				FeatNode neighbour = neighbours[j];
				if (neighbour == null || !indexLookup.TryGetValue(neighbour, out int neighbourIndex))
				{
					continue;
				}

				if (neighbourIndex < i)
				{
					continue;
				}

				Vector3 from = GetNodeRect(node).center;
				Vector3 to = GetNodeRect(neighbour).center;

				bool isHighlighted =
					node == selectedNode ||
					neighbour == selectedNode ||
					node == linkSourceNode ||
					neighbour == linkSourceNode;

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
		bool isSelected = node == selectedNode;
		bool isLinkSource = node == linkSourceNode;

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

		if (IsRootNode(node))
		{
			Rect rootRect = new Rect(rect.x + padding, rect.yMax - 22f * zoom, 52f * zoom, 16f * zoom);
			EditorGUI.DrawRect(rootRect, new Color(1f, 0.85f, 0.25f, 0.9f));
			GUI.Label(rootRect, "ROOT", badgeStyle);
		}
	}

	private void DrawCanvasOverlay(Vector2 canvasSize)
	{
		Rect hintRect = new Rect(14f, 14f, Mathf.Min(canvasSize.x - 28f, 510f), 58f);
		EditorGUI.DrawRect(hintRect, EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.2f) : new Color(1f, 1f, 1f, 0.7f));

		string hintText = linkSourceNode == null
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
		EditorGUILayout.LabelField("Root Node", GetRootNode() != null ? GetNodeLabel(GetRootNode()) : "None");
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

		EditorGUILayout.EndHorizontal();
	}

	private void DrawSelectedNodeInspector()
	{
		EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

		FeatNode node = GetSelectedNode();
		if (node == null)
		{
			EditorGUILayout.HelpBox("Select a node on the grid to edit its contents and position.", MessageType.Info);
			return;
		}

		EditorGUI.BeginChangeCheck();
		string newDisplayName = EditorGUILayout.TextField("Display Name", node.DisplayName);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Rename Feat Node", () => node.DisplayName = newDisplayName);
		}

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.Space(6f);

		EditorGUILayout.BeginHorizontal();

		using (new EditorGUI.DisabledScope(IsRootNode(node)))
		{
			if (GUILayout.Button("Make Root"))
			{
				SetRootNode(node);
			}
		}

		using (new EditorGUI.DisabledScope(!IsRootNode(node)))
		{
			if (GUILayout.Button("Clear Root"))
			{
				SetRootNode(null);
			}
		}

		EditorGUILayout.EndHorizontal();
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

		EditorGUILayout.Space(6f);
		EditorGUILayout.BeginHorizontal();

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
			RemoveNode(node);
			GUIUtility.ExitGUI();
		}

		EditorGUILayout.Space(10f);
		DrawRequirementsInspector(node);
		EditorGUILayout.Space(10f);
		DrawRewardsInspector(node);
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

		DrawRequirementDurationField(requirement);
		DrawRequirementRepeatField(requirement);

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

			case CastAbilityCountRequirement castAbility:
				DrawCastAbilityCountRequirementFields(castAbility);
				break;

			default:
				EditorGUILayout.HelpBox("Unsupported requirement type: " + requirement.GetType().Name, MessageType.Warning);
				break;
		}
	}

	private void DrawRequirementDurationField(FeatRequirement requirement)
	{
		if (requirement == null)
		{
			return;
		}

		EditorGUI.BeginChangeCheck();
		FeatRequirement.Scope scope =
			(FeatRequirement.Scope)EditorGUILayout.EnumPopup("Scope", requirement.RequirementScope);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Edit Feat Requirement Scope", () => requirement.RequirementScope = scope);
		}
	}

	private void DrawRequirementRepeatField(FeatRequirement requirement)
	{
		if (requirement == null)
		{
			return;
		}

		EditorGUI.BeginChangeCheck();
		int repeatCount = EditorGUILayout.IntField("Repeat Count", requirement.RequiredRepeatCount);
		if (EditorGUI.EndChangeCheck())
		{
			ApplySpeciesChange("Edit Feat Requirement Repeat Count", () =>
				requirement.RequiredRepeatCount = Mathf.Max(1, repeatCount));
		}
	}

	private void DrawCastAbilityCountRequirementFields(CastAbilityCountRequirement requirement)
	{
		EditorGUILayout.LabelField("Abilities (leave empty for any)");
		for (int i = 0; i < requirement.Abilities.Count; i++)
		{
			int captured = i;
			EditorGUI.BeginChangeCheck();
			Ability ability = (Ability)EditorGUILayout.ObjectField($"  [{i}]", requirement.Abilities[i], typeof(Ability), false);
			if (EditorGUI.EndChangeCheck())
				ApplySpeciesChange("Edit Feat Requirement", () => requirement.Abilities[captured] = ability);
		}
		if (GUILayout.Button("Add Ability"))
			ApplySpeciesChange("Edit Feat Requirement", () => requirement.Abilities.Add(null));
		if (requirement.Abilities.Count > 0 && GUILayout.Button("Remove Last Ability"))
			ApplySpeciesChange("Edit Feat Requirement", () => requirement.Abilities.RemoveAt(requirement.Abilities.Count - 1));

		EditorGUI.BeginChangeCheck();
		int count = EditorGUILayout.IntField("Required Casts", requirement.RequiredCount);
		if (EditorGUI.EndChangeCheck())
			ApplySpeciesChange("Edit Feat Requirement", () => requirement.RequiredCount = Mathf.Max(1, count));
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
				BonusStatsReward.AttributeType attribute = (BonusStatsReward.AttributeType)EditorGUILayout.EnumPopup("Attribute", bonusStats.Attribute);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => bonusStats.Attribute = attribute);
				}

				EditorGUI.BeginChangeCheck();
				float statValue = EditorGUILayout.FloatField("Value", bonusStats.Value);
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

			case RemoveAbilityReward removeAbilityReward:
				EditorGUI.BeginChangeCheck();
				Ability abilityToRemove = (Ability)EditorGUILayout.ObjectField("Ability", removeAbilityReward.Ability, typeof(Ability), false);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => removeAbilityReward.Ability = abilityToRemove);
				}
				break;

			case PassiveReward passiveReward:
				EditorGUI.BeginChangeCheck();
				Status passiveStatus = (Status)EditorGUILayout.ObjectField("Passive", passiveReward.Status, typeof(Status), false);
				if (EditorGUI.EndChangeCheck())
				{
					ApplySpeciesChange("Edit Feat Reward", () => passiveReward.Status = passiveStatus);
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
		Type[] rewardTypes = ManagedReferenceTypePicker.GetConcreteTypes(typeof(FeatReward), FeatRewardTypeComparison);

		for (int i = 0; i < rewardTypes.Length; i++)
		{
			Type rewardType = rewardTypes[i];
			string label = GetFeatRewardLabel(rewardType);

			menu.AddItem(new GUIContent(label), false, () =>
			{
				ApplySpeciesChange("Add Feat Reward", () => node.Rewards.Add((FeatReward)ManagedReferenceTypePicker.CreateInstance(rewardType)));
			});
		}

		menu.ShowAsContext();
	}

	private void ShowAddRequirementMenu(FeatNode node)
	{
		GenericMenu menu = new GenericMenu();
		Type[] requirementTypes = ManagedReferenceTypePicker.GetConcreteTypes(typeof(FeatRequirement), FeatRequirementTypeComparison);

		for (int i = 0; i < requirementTypes.Length; i++)
		{
			Type requirementType = requirementTypes[i];
			if (IsLegacyRequirementType(requirementType))
			{
				continue;
			}

			string label = GetFeatRequirementLabel(requirementType);

			menu.AddItem(new GUIContent(label), false, () =>
			{
				ApplySpeciesChange("Add Feat Requirement", () => node.Requirements.Add((FeatRequirement)ManagedReferenceTypePicker.CreateInstance(requirementType)));
			});
		}

		menu.ShowAsContext();
	}

	private static string GetFeatRewardLabel(Type rewardType)
	{
		return ManagedReferenceTypePicker.NicifyTypeName(rewardType, suffixToTrim: "Reward");
	}

	private static string GetFeatRequirementLabel(Type requirementType)
	{
		return ManagedReferenceTypePicker.NicifyTypeName(requirementType, suffixToTrim: "Requirement");
	}

	private static bool IsLegacyRequirementType(Type requirementType)
	{
		return false;
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

			if (linkSourceNode != null)
			{
				if (clickedNode == null)
				{
					linkSourceNode = null;
				}
				else if (clickedNode == linkSourceNode)
				{
					linkSourceNode = null;
					selectedNode = clickedNode;
				}
				else
				{
					ToggleLink(linkSourceNode, clickedNode);
					selectedNode = clickedNode;
				}

				evt.Use();
				return;
			}

			if (clickedNode == null)
			{
				selectedNode = null;

				if (evt.clickCount == 2)
				{
					AddNodeAtCanvasPosition(localMouse);
				}

				Repaint();
				return;
			}

			if (evt.shift && selectedNode != null && clickedNode != selectedNode)
			{
				ToggleLink(selectedNode, clickedNode);
				evt.Use();
				return;
			}

			selectedNode = clickedNode;
			draggedNode = clickedNode;
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
				draggedNode = null;
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
				linkSourceNode = null;
				isDraggingNode = false;
				isPanningCanvas = false;
				Repaint();
				evt.Use();
				return;
			}

			if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
			{
				RemoveNode(selectedNode);
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

			FeatNode currentSelectedNode = GetSelectedNode();
			if (currentSelectedNode == null)
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
					currentSelectedNode.Position = snapToGrid
						? SnapToGrid(currentSelectedNode.Position + delta)
						: currentSelectedNode.Position + delta;
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
			menu.AddItem(new GUIContent("Select"), node == selectedNode, () => selectedNode = node);
			menu.AddItem(new GUIContent(linkSourceNode == node ? "Stop Link Mode" : "Start Link Mode"), false, () =>
			{
				selectedNode = node;
				linkSourceNode = linkSourceNode == node ? null : node;
				Repaint();
			});

			if (selectedNode != null && selectedNode != node)
			{
				string toggleLabel = AreLinked(selectedNode, node) ? "Remove Link With Selected" : "Create Link With Selected";
				menu.AddItem(new GUIContent(toggleLabel), false, () => ToggleLink(selectedNode, node));
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Create Link With Selected"));
			}

			menu.AddItem(new GUIContent("Frame Node"), false, () => FrameNode(node));
			menu.AddSeparator(string.Empty);
			menu.AddItem(new GUIContent("Delete Node"), false, () => RemoveNode(node));
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
			Id = Guid.NewGuid().ToString("N"),
			DisplayName = "New Feat",
			Position = boardPosition
		};

		ApplySpeciesChange("Add Feat Node", () =>
		{
			species.FeatBoard.Nodes.Add(newNode);
		});

		selectedNode = newNode;
		linkSourceNode = null;
		Repaint();
	}

	private void RemoveNode(FeatNode node)
{
	if (species == null || node == null)
	{
		return;
	}

	ApplySpeciesChange("Delete Feat Node", () =>
	{
		List<FeatNode> nodes = GetNodes();
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] != null && nodes[i].NeighbourNodeIds != null)
			{
				nodes[i].NeighbourNodeIds.Remove(node.Id);
			}
		}

		if (species.FeatBoard.IsRootNode(node))
		{
			species.FeatBoard.RootNodeId = string.Empty;
		}

		species.FeatBoard.Nodes.Remove(node);
	});

	if (selectedNode == node)
	{
		selectedNode = null;
	}

	if (linkSourceNode == node)
	{
		linkSourceNode = null;
	}
}

	private void ToggleLink(FeatNode first, FeatNode second)
	{
		if (species == null || first == null || second == null || first == second)
		{
			return;
		}

		bool currentlyLinked = AreLinked(first, second);

		ApplySpeciesChange(currentlyLinked ? "Remove Feat Link" : "Create Feat Link", () =>
		{
			if (currentlyLinked)
			{
				first.NeighbourNodeIds.Remove(second.Id);
				second.NeighbourNodeIds.Remove(first.Id);
			}
			else
			{
				if (!first.NeighbourNodeIds.Contains(second.Id))
				{
					first.NeighbourNodeIds.Add(second.Id);
				}

				if (!second.NeighbourNodeIds.Contains(first.Id))
				{
					second.NeighbourNodeIds.Add(first.Id);
				}
			}
		});
	}

	private bool AreLinked(FeatNode first, FeatNode second)
	{
		if (first == null || second == null || first.NeighbourNodeIds == null)
		{
			return false;
		}

		return first.NeighbourNodeIds.Contains(second.Id);
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
}
