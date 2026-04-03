using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BattleActionBarPrefabComposer
{
	private const string PrefabFolderPath = "Assets/Resources/Prefab";
	private const string HeaderPrefabPath = PrefabFolderPath + "/PlayerHeaderElementUI.prefab";
	private const string AbilityListPrefabPath = PrefabFolderPath + "/AbilityListElementUI.prefab";
	private const string SlotPrefabPath = PrefabFolderPath + "/BattleActionSlotElementUI.prefab";
	private const string StatusCardPrefabPath = PrefabFolderPath + "/BattleStatusCardElementUIPrefab.prefab";
	private const string StatusListPrefabPath = PrefabFolderPath + "/BattleStatusListElementUI.prefab";
	private const string ResourceBarPrefabPath = PrefabFolderPath + "/BattleResourceBarElementUI.prefab";
	private const string ActionInfoPrefabPath = PrefabFolderPath + "/BattleActionInfoElementUI.prefab";
	private const string ActionBarPrefabPath = PrefabFolderPath + "/BattleActionBarElementUI.prefab";

	[MenuItem("Tools/UI/Rebuild Battle Action Bar Prefabs")]
	public static void RebuildMenu()
	{
		Rebuild();
	}

	public static void Rebuild()
	{
		Directory.CreateDirectory(PrefabFolderPath);

		GameObject resourceBarPrefab = BuildResourceBarPrefab();
		GameObject statusCardPrefab = BuildStatusCardPrefab();
		GameObject statusListPrefab = BuildStatusListPrefab(statusCardPrefab);
		GameObject actionInfoPrefab = BuildActionInfoPrefab();
		BuildActionBarPrefab(resourceBarPrefab, statusListPrefab, actionInfoPrefab);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("Battle action bar prefabs rebuilt.");
	}

	private static GameObject BuildResourceBarPrefab()
	{
		GameObject root = CreateUIObject("BattleResourceBarElementUI", null);

		try
		{
			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.sizeDelta = new Vector2(196f, 20f);

			Image backgroundImage = root.AddComponent<Image>();
			ConfigurePanelImage(backgroundImage, new Color(0f, 0f, 0f, 0.4f));

			LayoutElement layoutElement = root.AddComponent<LayoutElement>();
			layoutElement.minHeight = 20f;
			layoutElement.preferredHeight = 20f;
			layoutElement.flexibleWidth = 1f;

			ProgressBarElementUI progressBar = root.AddComponent<ProgressBarElementUI>();
			ObservableResourceBarElementUI resourceBar = root.AddComponent<ObservableResourceBarElementUI>();

			GameObject fillRoot = CreateUIObject("Fill", root.transform);
			RectTransform fillRect = fillRoot.GetComponent<RectTransform>();
			fillRect.anchorMin = new Vector2(0f, 0f);
			fillRect.anchorMax = new Vector2(0f, 1f);
			fillRect.pivot = new Vector2(0f, 0.5f);
			fillRect.anchoredPosition = new Vector2(2f, 0f);
			fillRect.sizeDelta = new Vector2(144f, -4f);

			Image fillImage = fillRoot.AddComponent<Image>();
			ConfigurePanelImage(fillImage, Color.white);

			GameObject labelRoot = CreateUIObject("Label", root.transform);
			RectTransform labelRect = labelRoot.GetComponent<RectTransform>();
			SetStretch(labelRect);

			TextMeshProUGUI labelText = labelRoot.AddComponent<TextMeshProUGUI>();
			ConfigureText(labelText, "PreviewState", 7f, TextAlignmentOptions.Center, true, true);

			SetObjectReference(progressBar, "backgroundImage", backgroundImage);
			SetObjectReference(progressBar, "fillImage", fillImage);
			SetObjectReference(progressBar, "labelText", labelText);
			SetColor(progressBar, "backgroundColor", new Color(0f, 0f, 0f, 0.4f));
			SetColor(progressBar, "fillColor", Color.white);
			SetVector2(progressBar, "fillPadding", new Vector2(2f, 2f));

			SetObjectReference(resourceBar, "progressBarElementUI", progressBar);
			SetString(resourceBar, "labelPrefix", string.Empty);

			return SavePrefab(root, ResourceBarPrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	private static GameObject BuildStatusCardPrefab()
	{
		GameObject root = CreateUIObject("BattleStatusCardElementUIPrefab", null);

		try
		{
			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.sizeDelta = new Vector2(24f, 24f);

			Image hitboxImage = root.AddComponent<Image>();
			hitboxImage.sprite = LoadDefaultSprite();
			hitboxImage.type = Image.Type.Sliced;
			hitboxImage.color = new Color(1f, 1f, 1f, 0f);
			hitboxImage.raycastTarget = true;

			StatusCardElementUI statusCard = root.AddComponent<StatusCardElementUI>();

			LayoutElement layoutElement = root.AddComponent<LayoutElement>();
			layoutElement.preferredWidth = 24f;
			layoutElement.preferredHeight = 24f;

			GameObject iconRoot = CreateUIObject("IconImage", root.transform);
			RectTransform iconRect = iconRoot.GetComponent<RectTransform>();
			SetStretch(iconRect);

			Image iconImage = iconRoot.AddComponent<Image>();
			iconImage.raycastTarget = false;
			iconImage.preserveAspect = true;

			TextMeshProUGUI stackLabel = CreateText("StackLabel", root.transform, "9", 7f, TextAlignmentOptions.TopRight, true, true);
			RectTransform stackRect = stackLabel.rectTransform;
			stackRect.anchorMin = new Vector2(1f, 1f);
			stackRect.anchorMax = new Vector2(1f, 1f);
			stackRect.pivot = new Vector2(1f, 1f);
			stackRect.anchoredPosition = new Vector2(-1f, -1f);
			stackRect.sizeDelta = new Vector2(16f, 8f);
			stackLabel.color = new Color(1f, 0.96f, 0.82f, 1f);
			stackLabel.gameObject.SetActive(false);

			TextMeshProUGUI durationLabel = CreateText("DurationLabel", root.transform, "2T", 6f, TextAlignmentOptions.BottomRight, true, true);
			RectTransform durationRect = durationLabel.rectTransform;
			durationRect.anchorMin = new Vector2(1f, 0f);
			durationRect.anchorMax = new Vector2(1f, 0f);
			durationRect.pivot = new Vector2(1f, 0f);
			durationRect.anchoredPosition = new Vector2(-1f, 1f);
			durationRect.sizeDelta = new Vector2(18f, 8f);
			durationLabel.color = new Color(0.76f, 0.9f, 1f, 1f);
			durationLabel.gameObject.SetActive(false);

			SetObjectReference(statusCard, "iconImage", iconImage);
			SetObjectReference(statusCard, "stackLabel", stackLabel);
			SetObjectReference(statusCard, "durationLabel", durationLabel);

			return SavePrefab(root, StatusCardPrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	private static GameObject BuildStatusListPrefab(GameObject statusCardPrefab)
	{
		GameObject root = CreateUIObject("BattleStatusListElementUI", null);

		try
		{
			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.sizeDelta = new Vector2(220f, 24f);
			rootRect.pivot = new Vector2(0f, 0f);

			LayoutElement rootLayoutElement = root.AddComponent<LayoutElement>();
			rootLayoutElement.preferredHeight = 24f;
			rootLayoutElement.flexibleWidth = 1f;

			ContentSizeFitter contentSizeFitter = root.AddComponent<ContentSizeFitter>();
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			StatusListElementUI statusList = root.AddComponent<StatusListElementUI>();

			GameObject containerRoot = CreateUIObject("StatusCardContainerRoot", root.transform);
			RectTransform containerRect = containerRoot.GetComponent<RectTransform>();
			containerRect.anchorMin = new Vector2(0f, 0f);
			containerRect.anchorMax = new Vector2(1f, 0f);
			containerRect.pivot = new Vector2(0f, 0f);
			containerRect.anchoredPosition = Vector2.zero;
			containerRect.sizeDelta = Vector2.zero;

			GridLayoutGroup gridLayout = containerRoot.AddComponent<GridLayoutGroup>();
			gridLayout.cellSize = new Vector2(24f, 24f);
			gridLayout.spacing = new Vector2(4f, 4f);
			gridLayout.startCorner = GridLayoutGroup.Corner.LowerLeft;
			gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
			gridLayout.childAlignment = TextAnchor.LowerLeft;
			gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			gridLayout.constraintCount = 8;

			ContentSizeFitter gridFitter = containerRoot.AddComponent<ContentSizeFitter>();
			gridFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			SetObjectReference(statusList, "statusCardPrefab", statusCardPrefab.GetComponent<StatusCardElementUI>());
			SetInt(statusList, "minimumVisibleSlotCount", 0);
			SetObjectReference(statusList, "cardContainerRoot", containerRect);

			return SavePrefab(root, StatusListPrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	private static GameObject BuildActionInfoPrefab()
	{
		GameObject root = CreateUIObject("BattleActionInfoElementUI", null);

		try
		{
			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.sizeDelta = new Vector2(520f, 104f);

			Image backgroundImage = root.AddComponent<Image>();
			ConfigurePanelImage(backgroundImage, new Color(0.05f, 0.06f, 0.08f, 0.72f));

			VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
			rootLayout.padding = new RectOffset(10, 10, 10, 10);
			rootLayout.spacing = 6f;
			rootLayout.childAlignment = TextAnchor.UpperLeft;
			rootLayout.childControlWidth = true;
			rootLayout.childControlHeight = true;
			rootLayout.childForceExpandWidth = true;
			rootLayout.childForceExpandHeight = false;

			LayoutElement rootLayoutElement = root.AddComponent<LayoutElement>();
			rootLayoutElement.preferredHeight = 104f;
			rootLayoutElement.flexibleWidth = 1f;

			ActionInfoElementUI actionInfo = root.AddComponent<ActionInfoElementUI>();

			GameObject headerRow = CreateUIObject("InfoHeaderRow", root.transform);
			HorizontalLayoutGroup headerLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
			headerLayout.spacing = 8f;
			headerLayout.childAlignment = TextAnchor.MiddleLeft;
			headerLayout.childControlWidth = true;
			headerLayout.childControlHeight = true;
			headerLayout.childForceExpandWidth = false;
			headerLayout.childForceExpandHeight = false;

			LayoutElement headerLayoutElement = headerRow.AddComponent<LayoutElement>();
			headerLayoutElement.preferredHeight = 44f;
			headerLayoutElement.flexibleWidth = 1f;

			GameObject previewFrame = CreateUIObject("PreviewFrame", headerRow.transform);
			RectTransform previewRect = previewFrame.GetComponent<RectTransform>();
			previewRect.sizeDelta = new Vector2(44f, 44f);

			Image previewImage = previewFrame.AddComponent<Image>();
			ConfigurePanelImage(previewImage, new Color(1f, 1f, 1f, 0.12f));

			LayoutElement previewLayoutElement = previewFrame.AddComponent<LayoutElement>();
			previewLayoutElement.minWidth = 44f;
			previewLayoutElement.minHeight = 44f;
			previewLayoutElement.preferredWidth = 44f;
			previewLayoutElement.preferredHeight = 44f;

			GameObject iconRoot = CreateUIObject("ActionIconImage", previewFrame.transform);
			RectTransform iconRect = iconRoot.GetComponent<RectTransform>();
			SetStretch(iconRect, 5f);

			Image iconImage = iconRoot.AddComponent<Image>();
			iconImage.raycastTarget = false;
			iconImage.preserveAspect = true;
			iconImage.enabled = false;

			GameObject textColumn = CreateUIObject("InfoTextColumn", headerRow.transform);
			VerticalLayoutGroup textColumnLayout = textColumn.AddComponent<VerticalLayoutGroup>();
			textColumnLayout.spacing = 2f;
			textColumnLayout.childAlignment = TextAnchor.MiddleLeft;
			textColumnLayout.childControlWidth = true;
			textColumnLayout.childControlHeight = true;
			textColumnLayout.childForceExpandWidth = true;
			textColumnLayout.childForceExpandHeight = false;

			LayoutElement textColumnLayoutElement = textColumn.AddComponent<LayoutElement>();
			textColumnLayoutElement.flexibleWidth = 1f;
			textColumnLayoutElement.preferredHeight = 40f;

			TextMeshProUGUI actionNameLabel = CreateText("ActionNameLabel", textColumn.transform, "Action Name", 14f, TextAlignmentOptions.Left, false, true);
			TextMeshProUGUI costLabel = CreateText("CostLabel", textColumn.transform, "Cost", 11f, TextAlignmentOptions.Left, false, true);

			GameObject metaRow = CreateUIObject("InfoMetaRow", root.transform);
			HorizontalLayoutGroup metaLayout = metaRow.AddComponent<HorizontalLayoutGroup>();
			metaLayout.spacing = 6f;
			metaLayout.childAlignment = TextAnchor.MiddleLeft;
			metaLayout.childControlWidth = true;
			metaLayout.childControlHeight = true;
			metaLayout.childForceExpandWidth = false;
			metaLayout.childForceExpandHeight = false;

			LayoutElement metaLayoutElement = metaRow.AddComponent<LayoutElement>();
			metaLayoutElement.preferredHeight = 18f;
			metaLayoutElement.flexibleWidth = 1f;

			TextMeshProUGUI rangeLabel = CreateText("RangeLabel", metaRow.transform, "Range", 10f, TextAlignmentOptions.Left, false, true);
			rangeLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

			TextMeshProUGUI areaLabel = CreateText("AreaLabel", metaRow.transform, "Area", 10f, TextAlignmentOptions.Left, false, true);
			areaLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

			TextMeshProUGUI lineOfSightLabel = CreateText("LineOfSightLabel", metaRow.transform, "Line Of Sight", 10f, TextAlignmentOptions.Left, false, true);
			lineOfSightLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

			TextMeshProUGUI descriptionLabel = CreateText("DescriptionLabel", root.transform, "Description", 10f, TextAlignmentOptions.TopLeft, false, false);
			LayoutElement descriptionLayoutElement = descriptionLabel.gameObject.AddComponent<LayoutElement>();
			descriptionLayoutElement.minHeight = 28f;
			descriptionLayoutElement.preferredHeight = 28f;
			descriptionLayoutElement.flexibleHeight = 1f;
			descriptionLayoutElement.flexibleWidth = 1f;

			SetObjectReference(actionInfo, "iconImage", iconImage);
			SetObjectReference(actionInfo, "actionNameLabel", actionNameLabel);
			SetObjectReference(actionInfo, "costLabel", costLabel);
			SetObjectReference(actionInfo, "rangeLabel", rangeLabel);
			SetObjectReference(actionInfo, "areaOfEffectLabel", areaLabel);
			SetObjectReference(actionInfo, "lineOfSightLabel", lineOfSightLabel);
			SetObjectReference(actionInfo, "descriptionLabel", descriptionLabel);
			SetString(actionInfo, "emptyMessage", "-----");

			return SavePrefab(root, ActionInfoPrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	private static GameObject BuildActionBarPrefab(GameObject resourceBarPrefab, GameObject statusListPrefab, GameObject actionInfoPrefab)
	{
		GameObject headerPrefab = LoadPrefab(HeaderPrefabPath);
		GameObject abilityListPrefab = LoadPrefab(AbilityListPrefabPath);
		GameObject slotPrefab = LoadPrefab(SlotPrefabPath);

		GameObject root = CreateUIObject("BattleActionBarElementUI", null);

		try
		{
			RectTransform rootRect = root.GetComponent<RectTransform>();
			rootRect.sizeDelta = new Vector2(980f, 176f);
			rootRect.pivot = new Vector2(0.5f, 0f);

			Image rootImage = root.AddComponent<Image>();
			ConfigurePanelImage(rootImage, new Color(0.07f, 0.08f, 0.1f, 0.85f));

			HorizontalLayoutGroup rootLayout = root.AddComponent<HorizontalLayoutGroup>();
			rootLayout.padding = new RectOffset(12, 12, 12, 12);
			rootLayout.spacing = 12f;
			rootLayout.childAlignment = TextAnchor.MiddleLeft;
			rootLayout.childControlWidth = true;
			rootLayout.childControlHeight = true;
			rootLayout.childForceExpandWidth = false;
			rootLayout.childForceExpandHeight = false;

			ContentSizeFitter rootFitter = root.AddComponent<ContentSizeFitter>();
			rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			LayoutElement rootLayoutElement = root.AddComponent<LayoutElement>();
			rootLayoutElement.minHeight = 176f;
			rootLayoutElement.flexibleWidth = 1f;

			BattleActionBarElementUI battleActionBar = root.AddComponent<BattleActionBarElementUI>();

			GameObject summaryColumn = CreateUIObject("TurnSummaryColumn", root.transform);
			VerticalLayoutGroup summaryLayout = summaryColumn.AddComponent<VerticalLayoutGroup>();
			summaryLayout.spacing = 4f;
			summaryLayout.childAlignment = TextAnchor.UpperLeft;
			summaryLayout.childControlWidth = true;
			summaryLayout.childControlHeight = true;
			summaryLayout.childForceExpandWidth = true;
			summaryLayout.childForceExpandHeight = false;

			LayoutElement summaryLayoutElement = summaryColumn.AddComponent<LayoutElement>();
			summaryLayoutElement.preferredWidth = 220f;
			summaryLayoutElement.flexibleWidth = 0f;

			GameObject headerInstance = (GameObject) PrefabUtility.InstantiatePrefab(headerPrefab, summaryColumn.transform);
			headerInstance.name = "ActiveCreatureHeaderElementUI";

			GameObject statusListInstance = (GameObject) PrefabUtility.InstantiatePrefab(statusListPrefab, summaryColumn.transform);
			statusListInstance.name = "StatusListElementUI";
			StatusListElementUI statusList = statusListInstance.GetComponent<StatusListElementUI>();
			ObservableResourceBarElementUI healthBar = InstantiateResourceBar(resourceBarPrefab, summaryColumn.transform, "HealthBarElementUI", "HP", new Color(0.78f, 0.24f, 0.24f, 1f));
			ObservableResourceBarElementUI actionPointsBar = InstantiateResourceBar(resourceBarPrefab, summaryColumn.transform, "ActionPointsBarElementUI", "AP", new Color(0.85f, 0.69f, 0.26f, 1f));
			ObservableResourceBarElementUI movementPointsBar = InstantiateResourceBar(resourceBarPrefab, summaryColumn.transform, "MovementPointsBarElementUI", "MP", new Color(0.28f, 0.7f, 0.46f, 1f));

			GameObject actionsColumn = CreateUIObject("ActionsColumn", root.transform);
			VerticalLayoutGroup actionsLayout = actionsColumn.AddComponent<VerticalLayoutGroup>();
			actionsLayout.spacing = 8f;
			actionsLayout.childAlignment = TextAnchor.UpperLeft;
			actionsLayout.childControlWidth = true;
			actionsLayout.childControlHeight = true;
			actionsLayout.childForceExpandWidth = true;
			actionsLayout.childForceExpandHeight = false;

			LayoutElement actionsLayoutElement = actionsColumn.AddComponent<LayoutElement>();
			actionsLayoutElement.flexibleWidth = 1f;

			GameObject abilityListInstance = (GameObject) PrefabUtility.InstantiatePrefab(abilityListPrefab, actionsColumn.transform);
			abilityListInstance.name = "AbilityListElementUI";
			AbilityListElementUI abilityList = abilityListInstance.GetComponent<AbilityListElementUI>();
			LayoutElement abilityListLayoutElement = abilityListInstance.GetComponent<LayoutElement>();
			if (abilityListLayoutElement == null)
			{
				abilityListLayoutElement = abilityListInstance.AddComponent<LayoutElement>();
			}

			abilityListLayoutElement.preferredHeight = 56f;
			abilityListLayoutElement.flexibleWidth = 1f;

			SetObjectReference(abilityList, "abilityCardPrefab", slotPrefab.GetComponent<AbilityCardElementUI>());
			SetInt(abilityList, "minimumVisibleSlotCount", 8);
			SetBool(abilityList, "displayShortcutLabels", true);

			Transform cardContainerRoot = abilityListInstance.transform.Find("AbilityCardContainerRoot");
			if (cardContainerRoot == null)
			{
				throw new IOException("Ability list prefab is missing AbilityCardContainerRoot.");
			}

			for (int index = 0; index < 8; index++)
			{
				GameObject slotInstance = (GameObject) PrefabUtility.InstantiatePrefab(slotPrefab, cardContainerRoot);
				slotInstance.name = $"ActionSlot{index + 1:00}";
			}

			GameObject actionInfoInstance = (GameObject) PrefabUtility.InstantiatePrefab(actionInfoPrefab, actionsColumn.transform);
			actionInfoInstance.name = "ActionInfoElementUI";

			GameObject controlsColumn = CreateUIObject("ControlsColumn", root.transform);
			VerticalLayoutGroup controlsLayout = controlsColumn.AddComponent<VerticalLayoutGroup>();
			controlsLayout.childAlignment = TextAnchor.MiddleCenter;
			controlsLayout.childControlWidth = true;
			controlsLayout.childControlHeight = true;
			controlsLayout.childForceExpandWidth = true;
			controlsLayout.childForceExpandHeight = true;

			LayoutElement controlsLayoutElement = controlsColumn.AddComponent<LayoutElement>();
			controlsLayoutElement.preferredWidth = 128f;

			Button endTurnButton = CreateButton("EndTurnButton", controlsColumn.transform, "End Turn");

			SetObjectReference(battleActionBar, "activeCreatureHeaderElementUI", headerInstance.GetComponent<CreatureCardHeaderElementUI>());
			SetObjectReference(battleActionBar, "statusListElementUI", statusList);
			SetObjectReference(battleActionBar, "healthBarElementUI", healthBar);
			SetObjectReference(battleActionBar, "actionPointsBarElementUI", actionPointsBar);
			SetObjectReference(battleActionBar, "movementPointsBarElementUI", movementPointsBar);
			SetObjectReference(battleActionBar, "abilityListElementUI", abilityList);
			SetObjectReference(battleActionBar, "actionInfoElementUI", actionInfoInstance.GetComponent<ActionInfoElementUI>());
			SetBool(battleActionBar, "displayAbilityShortcuts", true);
			SetInt(battleActionBar, "defaultFocusedAbilityIndex", 0);
			SetObjectReference(battleActionBar, "endTurnButton", endTurnButton);

			return SavePrefab(root, ActionBarPrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	private static ObservableResourceBarElementUI InstantiateResourceBar(GameObject prefab, Transform parent, string name, string labelPrefix, Color fillColor)
	{
		GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(prefab, parent);
		instance.name = name;

		ObservableResourceBarElementUI resourceBar = instance.GetComponent<ObservableResourceBarElementUI>();
		ProgressBarElementUI progressBar = instance.GetComponent<ProgressBarElementUI>();

		SetString(resourceBar, "labelPrefix", labelPrefix);
		SetColor(progressBar, "fillColor", fillColor);

		return resourceBar;
	}

	private static Button CreateButton(string name, Transform parent, string label)
	{
		GameObject root = CreateUIObject(name, parent);
		RectTransform rect = root.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(120f, 44f);

		Image image = root.AddComponent<Image>();
		ConfigurePanelImage(image, new Color(0.84f, 0.73f, 0.38f, 0.95f));

		Button button = root.AddComponent<Button>();
		button.targetGraphic = image;
		ColorBlock colors = button.colors;
		colors.normalColor = new Color(0.84f, 0.73f, 0.38f, 0.95f);
		colors.highlightedColor = new Color(0.92f, 0.82f, 0.46f, 1f);
		colors.pressedColor = new Color(0.7f, 0.58f, 0.28f, 1f);
		colors.selectedColor = colors.highlightedColor;
		colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
		button.colors = colors;

		LayoutElement layoutElement = root.AddComponent<LayoutElement>();
		layoutElement.preferredWidth = 120f;
		layoutElement.preferredHeight = 44f;

		TextMeshProUGUI labelText = CreateText("EndTurnLabel", root.transform, label, 14f, TextAlignmentOptions.Center, false, true);
		SetStretch(labelText.rectTransform, 0f);

		return button;
	}

	private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float fontSize, TextAlignmentOptions alignment, bool autoSize, bool singleLine)
	{
		GameObject root = CreateUIObject(name, parent);
		TextMeshProUGUI text = root.AddComponent<TextMeshProUGUI>();
		ConfigureText(text, value, fontSize, alignment, autoSize, singleLine);
		return text;
	}

	private static void ConfigureText(TextMeshProUGUI text, string value, float fontSize, TextAlignmentOptions alignment, bool autoSize, bool singleLine)
	{
		text.font = TMP_Settings.defaultFontAsset;
		text.fontSharedMaterial = TMP_Settings.defaultFontAsset != null
			? TMP_Settings.defaultFontAsset.material
			: null;
		text.text = value;
		text.fontSize = fontSize;
		text.alignment = alignment;
		text.color = Color.white;
		text.enableAutoSizing = autoSize;
		text.fontSizeMin = Mathf.Min(fontSize, 10f);
		text.fontSizeMax = Mathf.Max(fontSize, 14f);
		text.textWrappingMode = singleLine
			? TextWrappingModes.NoWrap
			: TextWrappingModes.Normal;
		text.raycastTarget = false;
	}

	private static void ConfigurePanelImage(Image image, Color color)
	{
		image.sprite = LoadDefaultSprite();
		image.type = Image.Type.Sliced;
		image.color = color;
		image.raycastTarget = false;
	}

	private static GameObject CreateUIObject(string name, Transform parent)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		gameObject.layer = 5;

		if (parent != null)
		{
			gameObject.transform.SetParent(parent, false);
		}

		return gameObject;
	}

	private static void SetStretch(RectTransform rectTransform, float inset = 0f)
	{
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = new Vector2(inset, inset);
		rectTransform.offsetMax = new Vector2(-inset, -inset);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.localScale = Vector3.one;
	}

	private static GameObject SavePrefab(GameObject root, string path)
	{
		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
		if (prefab == null)
		{
			throw new IOException($"Failed to save prefab at {path}.");
		}

		return AssetDatabase.LoadAssetAtPath<GameObject>(path);
	}

	private static GameObject LoadPrefab(string path)
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		if (prefab == null)
		{
			throw new IOException($"Missing prefab dependency at {path}.");
		}

		return prefab;
	}

	private static Sprite LoadDefaultSprite()
	{
		Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
		if (sprite == null)
		{
			throw new IOException("Unable to load the built-in UI sprite.");
		}

		return sprite;
	}

	private static void SetObjectReference(Object target, string propertyName, Object value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).objectReferenceValue = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void SetString(Object target, string propertyName, string value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).stringValue = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void SetInt(Object target, string propertyName, int value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).intValue = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void SetBool(Object target, string propertyName, bool value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).boolValue = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void SetColor(Object target, string propertyName, Color value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).colorValue = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void SetVector2(Object target, string propertyName, Vector2 value)
	{
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.FindProperty(propertyName).vector2Value = value;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}
}
