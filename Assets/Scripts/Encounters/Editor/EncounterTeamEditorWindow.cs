using UnityEditor;
using UnityEngine;

public class EncounterTeamEditorWindow : EditorWindow
{
	private const float TopBarHeight = 72f;
	private const float OuterPadding = 8f;
	private const float SectionSpacing = 8f;
	private const float RightPanelWidthRatio = 0.27f;

	private SerializedObject _serializedObject;
	private SerializedProperty _teamProperty;

	private int _selectedUnitIndex = 0;

	private readonly EncounterTeamProgressBoardView _boardView = new EncounterTeamProgressBoardView();
	private readonly EncounterTeamUnitInspectorView _inspectorView = new EncounterTeamUnitInspectorView();

	public static void Open(SerializedObject p_serializedObject, SerializedProperty p_teamProperty)
	{
		if (p_serializedObject == null || p_teamProperty == null)
		{
			Debug.LogError("Cannot open EncounterTeamEditorWindow with null data.");
			return;
		}

		EncounterTeamEditorWindow window = GetWindow<EncounterTeamEditorWindow>("Encounter Team");
		window.Initialize(p_serializedObject, p_teamProperty);
	}

	private void Initialize(SerializedObject p_serializedObject, SerializedProperty p_teamProperty)
	{
		_serializedObject = p_serializedObject;
		_teamProperty = p_teamProperty;

		EnsureArraySize();
		ClearInvalidSelection();
	}

	private void OnGUI()
	{
		if (_serializedObject == null || _teamProperty == null)
		{
			EditorGUILayout.HelpBox("No team selected.", MessageType.Info);
			return;
		}

		_serializedObject.Update();
		EnsureArraySize();
		ClearInvalidSelection();

		CreatureUnit selectedUnit = GetSelectedUnit();
		if (selectedUnit != null)
		{
			selectedUnit.EnsureInitialized();
		}

		Rect fullRect = new Rect(0f, 0f, position.width, position.height);
		Rect topRect = new Rect(OuterPadding, OuterPadding, fullRect.width - OuterPadding * 2f, TopBarHeight);
		Rect contentRect = new Rect(
			OuterPadding,
			topRect.yMax + SectionSpacing,
			fullRect.width - OuterPadding * 2f,
			fullRect.height - topRect.height - SectionSpacing - OuterPadding * 2f
		);

		float rightPanelWidth = Mathf.Max(280f, contentRect.width * RightPanelWidthRatio);
		Rect boardRect = new Rect(contentRect.x, contentRect.y, contentRect.width - rightPanelWidth - SectionSpacing, contentRect.height);
		Rect inspectorRect = new Rect(boardRect.xMax + SectionSpacing, contentRect.y, rightPanelWidth, contentRect.height);

		DrawTopUnitBar(topRect);
		DrawSelectedUnitBoard(boardRect);
		DrawSelectedUnitInspector(inspectorRect);

		_serializedObject.ApplyModifiedProperties();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(_serializedObject.targetObject);
			Repaint();
		}
	}

	private void DrawTopUnitBar(Rect p_rect)
	{
		EditorGUI.DrawRect(p_rect, EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.88f, 0.88f, 0.88f));

		float spacing = 6f;
		float buttonWidth = (p_rect.width - (GameRule.TeamMemberCount - 1) * spacing) / GameRule.TeamMemberCount;

		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			Rect buttonRect = new Rect(p_rect.x + index * (buttonWidth + spacing), p_rect.y, buttonWidth, p_rect.height);
			DrawUnitTab(buttonRect, index);
		}
	}

	private void DrawUnitTab(Rect p_rect, int p_index)
	{
		SerializedProperty unitProperty = _teamProperty.GetArrayElementAtIndex(p_index);
		CreatureUnit unit = SerializedPropertyObjectResolver.GetTargetObjectOfProperty<CreatureUnit>(unitProperty);

		Color backgroundColor = _selectedUnitIndex == p_index
			? new Color(0.82f, 0.70f, 0.28f, 1f)
			: (EditorGUIUtility.isProSkin ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.96f, 0.96f, 0.96f));

		EditorGUI.DrawRect(p_rect, backgroundColor);
		DrawOutline(p_rect, new Color(0f, 0f, 0f, 0.55f), 1f);

		const float padding = 6f;

		float iconSize = Mathf.Max(16f, p_rect.height - padding * 2f);
		Rect iconRect = new Rect(
			p_rect.x + padding,
			p_rect.y + (p_rect.height - iconSize) * 0.5f,
			iconSize,
			iconSize
		);

		Rect labelAreaRect = new Rect(
			iconRect.xMax + padding,
			p_rect.y + padding,
			p_rect.xMax - iconRect.xMax - padding * 2f,
			p_rect.height - padding * 2f
		);

		DrawUnitTabIcon(iconRect, unit);
		DrawUnitTabLabel(labelAreaRect, GetUnitTabLabel(unit));

		if (GUI.Button(p_rect, GUIContent.none, GUIStyle.none))
		{
			_selectedUnitIndex = p_index;
			_boardView.ClearSelection();
		}
	}

	private void DrawUnitTabIcon(Rect p_rect, CreatureUnit p_unit)
	{
		EditorGUI.DrawRect(p_rect, new Color(0f, 0f, 0f, 0.18f));

		Sprite sprite = null;

		try
		{
			if (p_unit != null)
			{
				CreatureForm form = p_unit.GetForm();
				if (form != null)
				{
					sprite = form.Icon;
				}
			}
		}
		catch
		{
		}

		if (sprite == null || sprite.texture == null)
		{
			return;
		}

		Rect fittedRect = GetAspectFittedRect(p_rect, sprite.texture.width, sprite.texture.height);
		GUI.DrawTexture(fittedRect, sprite.texture, ScaleMode.StretchToFill, true);
	}

	private void DrawUnitTabLabel(Rect p_rect, string p_label)
	{
		GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontSize = 13,
			wordWrap = true,
			clipping = TextClipping.Clip
		};

		GUI.Label(p_rect, p_label, labelStyle);
	}

	private Rect GetAspectFittedRect(Rect p_rect, float p_contentWidth, float p_contentHeight)
	{
		if (p_contentWidth <= 0f || p_contentHeight <= 0f)
		{
			return p_rect;
		}

		float targetAspect = p_contentWidth / p_contentHeight;
		float rectAspect = p_rect.width / p_rect.height;

		if (targetAspect > rectAspect)
		{
			float fittedHeight = p_rect.width / targetAspect;
			return new Rect(
				p_rect.x,
				p_rect.y + (p_rect.height - fittedHeight) * 0.5f,
				p_rect.width,
				fittedHeight
			);
		}

		float fittedWidth = p_rect.height * targetAspect;
		return new Rect(
			p_rect.x + (p_rect.width - fittedWidth) * 0.5f,
			p_rect.y,
			fittedWidth,
			p_rect.height
		);
	}

	private string GetUnitTabLabel(CreatureUnit p_unit)
	{
		if (p_unit == null || p_unit.Species == null)
		{
			return "-----";
		}

		if (!string.IsNullOrEmpty(p_unit.CurrentFormID) &&
			p_unit.Species.Forms != null &&
			p_unit.Species.Forms.TryGetValue(p_unit.CurrentFormID, out CreatureForm form) &&
			form != null &&
			!string.IsNullOrEmpty(form.DisplayName))
		{
			return form.DisplayName;
		}

		return p_unit.Species.name;
	}

	private void DrawSelectedUnitBoard(Rect p_rect)
	{
		CreatureUnit unit = GetSelectedUnit();

		_boardView.Draw(p_rect, unit);

		DrawEditFeatBoardButton(p_rect, unit);
	}

	private void DrawEditFeatBoardButton(Rect p_rect, CreatureUnit p_unit)
	{
		const float buttonWidth = 120f;
		const float buttonHeight = 26f;
		const float padding = 8f;

		Rect buttonRect = new Rect(
			p_rect.xMax - buttonWidth - padding,
			p_rect.y + padding,
			buttonWidth,
			buttonHeight
		);

		using (new EditorGUI.DisabledScope(p_unit == null || p_unit.Species == null))
		{
			if (GUI.Button(buttonRect, "Edit Feat Board"))
			{
				FeatBoardEditorWindow.Open(p_unit.Species);
			}
		}
	}

	private void DrawSelectedUnitInspector(Rect p_rect)
	{
		EditorGUI.DrawRect(p_rect, EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.94f, 0.94f, 0.94f));

		Rect contentRect = new Rect(p_rect.x + 8f, p_rect.y + 8f, p_rect.width - 16f, p_rect.height - 16f);
		_inspectorView.Draw(contentRect, GetSelectedUnitProperty(), GetSelectedUnit(), _boardView);
	}

	private SerializedProperty GetSelectedUnitProperty()
	{
		if (_teamProperty == null || _selectedUnitIndex < 0 || _selectedUnitIndex >= _teamProperty.arraySize)
		{
			return null;
		}

		return _teamProperty.GetArrayElementAtIndex(_selectedUnitIndex);
	}

	private CreatureUnit GetSelectedUnit()
	{
		SerializedProperty unitProperty = GetSelectedUnitProperty();
		if (unitProperty == null)
		{
			return null;
		}

		return SerializedPropertyObjectResolver.GetTargetObjectOfProperty<CreatureUnit>(unitProperty);
	}

	private void EnsureArraySize()
	{
		if (_teamProperty.arraySize != GameRule.TeamMemberCount)
		{
			_teamProperty.arraySize = GameRule.TeamMemberCount;
		}
	}

	private void ClearInvalidSelection()
	{
		if (_selectedUnitIndex < 0)
		{
			_selectedUnitIndex = 0;
		}

		if (_teamProperty != null && _selectedUnitIndex >= _teamProperty.arraySize)
		{
			_selectedUnitIndex = Mathf.Max(0, _teamProperty.arraySize - 1);
		}
	}

	private void DrawOutline(Rect p_rect, Color p_color, float p_thickness)
	{
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.y, p_rect.width, p_thickness), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.yMax - p_thickness, p_rect.width, p_thickness), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.x, p_rect.y, p_thickness, p_rect.height), p_color);
		EditorGUI.DrawRect(new Rect(p_rect.xMax - p_thickness, p_rect.y, p_thickness, p_rect.height), p_color);
	}
}