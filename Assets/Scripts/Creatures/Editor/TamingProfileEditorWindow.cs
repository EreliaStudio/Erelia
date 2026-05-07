using System;
using UnityEditor;
using UnityEngine;

public sealed class TamingProfileEditorWindow : EditorWindow
{
	private static readonly Comparison<Type> RequirementTypeComparison =
		(left, right) => string.CompareOrdinal(GetRequirementLabel(left), GetRequirementLabel(right));

	private CreatureSpecies species;
	private SerializedObject serializedSpecies;
	private Vector2 scrollPosition;

	[MenuItem("Tools/Taming Requirement Editor")]
	public static void OpenWindow()
	{
		TamingProfileEditorWindow window = GetWindow<TamingProfileEditorWindow>("Taming Requirements");
		if (Selection.activeObject is CreatureSpecies selectedSpecies)
		{
			window.SetSpecies(selectedSpecies);
		}
	}

	public static void Open(CreatureSpecies targetSpecies)
	{
		TamingProfileEditorWindow window = GetWindow<TamingProfileEditorWindow>("Taming Requirements");
		window.SetSpecies(targetSpecies);
		window.Focus();
	}

	private void OnEnable()
	{
		titleContent = new GUIContent("Taming Requirements");
		minSize = new Vector2(420f, 360f);

		if (species == null && Selection.activeObject is CreatureSpecies selectedSpecies)
		{
			SetSpecies(selectedSpecies);
		}
	}

	private void OnGUI()
	{
		DrawSpeciesField();

		if (species == null)
		{
			EditorGUILayout.HelpBox("Select a creature species to edit its taming requirements.", MessageType.Info);
			return;
		}

		EnsureSerializedSpecies();
		EnsureTamingProfile();

		serializedSpecies.Update();
		SerializedProperty conditionsProperty = serializedSpecies.FindProperty("TamingProfile.Conditions");
		if (conditionsProperty == null)
		{
			EditorGUILayout.HelpBox("Could not find TamingProfile.Conditions on this species.", MessageType.Error);
			return;
		}

		EditorGUILayout.Space(8f);
		EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("All listed requirements must be completed in battle to impress and recruit this wild species.", MessageType.Info);

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		DrawConditions(conditionsProperty);
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space(6f);
		if (GUILayout.Button("Add Requirement", GUILayout.Height(28f)))
		{
			ShowAddRequirementMenu();
		}

		serializedSpecies.ApplyModifiedProperties();
	}

	private void DrawSpeciesField()
	{
		EditorGUI.BeginChangeCheck();
		CreatureSpecies selectedSpecies = (CreatureSpecies)EditorGUILayout.ObjectField("Species", species, typeof(CreatureSpecies), false);
		if (EditorGUI.EndChangeCheck())
		{
			SetSpecies(selectedSpecies);
		}
	}

	private void DrawConditions(SerializedProperty conditionsProperty)
	{
		int removeIndex = -1;
		int moveFromIndex = -1;
		int moveToIndex = -1;

		if (conditionsProperty.arraySize == 0)
		{
			EditorGUILayout.HelpBox("No taming requirements configured.", MessageType.Warning);
		}

		for (int index = 0; index < conditionsProperty.arraySize; index++)
		{
			SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(index);
			object managedReference = conditionProperty.managedReferenceValue;
			string label = managedReference != null
				? GetRequirementLabel(managedReference.GetType())
				: "Missing Requirement";

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

			using (new EditorGUI.DisabledScope(index == 0))
			{
				if (GUILayout.Button("Up", GUILayout.Width(42f)))
				{
					moveFromIndex = index;
					moveToIndex = index - 1;
				}
			}

			using (new EditorGUI.DisabledScope(index >= conditionsProperty.arraySize - 1))
			{
				if (GUILayout.Button("Down", GUILayout.Width(52f)))
				{
					moveFromIndex = index;
					moveToIndex = index + 1;
				}
			}

			if (GUILayout.Button("Remove", GUILayout.Width(68f)))
			{
				removeIndex = index;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.PropertyField(conditionProperty, GUIContent.none, true);
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(4f);
		}

		if (moveFromIndex >= 0 && moveToIndex >= 0)
		{
			Undo.RecordObject(species, "Move Taming Requirement");
			conditionsProperty.MoveArrayElement(moveFromIndex, moveToIndex);
			EditorUtility.SetDirty(species);
			return;
		}

		if (removeIndex >= 0)
		{
			Undo.RecordObject(species, "Remove Taming Requirement");
			conditionsProperty.DeleteArrayElementAtIndex(removeIndex);
			EditorUtility.SetDirty(species);
		}
	}

	private void ShowAddRequirementMenu()
	{
		GenericMenu menu = new GenericMenu();
		Type[] requirementTypes = ManagedReferenceTypePicker.GetConcreteTypes(typeof(FeatRequirement), RequirementTypeComparison);

		for (int index = 0; index < requirementTypes.Length; index++)
		{
			Type requirementType = requirementTypes[index];
			string label = GetRequirementLabel(requirementType);
			menu.AddItem(new GUIContent(label), false, () => AddRequirement(requirementType));
		}

		menu.ShowAsContext();
	}

	private void AddRequirement(Type requirementType)
	{
		if (requirementType == null || species == null)
		{
			return;
		}

		EnsureSerializedSpecies();
		EnsureTamingProfile();

		serializedSpecies.Update();
		SerializedProperty conditionsProperty = serializedSpecies.FindProperty("TamingProfile.Conditions");
		if (conditionsProperty == null)
		{
			return;
		}

		Undo.RecordObject(species, "Add Taming Requirement");
		int newIndex = conditionsProperty.arraySize;
		conditionsProperty.InsertArrayElementAtIndex(newIndex);
		SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(newIndex);
		conditionProperty.managedReferenceValue = ManagedReferenceTypePicker.CreateInstance(requirementType);
		serializedSpecies.ApplyModifiedProperties();
		EditorUtility.SetDirty(species);
		Repaint();
	}

	private void SetSpecies(CreatureSpecies targetSpecies)
	{
		species = targetSpecies;
		serializedSpecies = species != null ? new SerializedObject(species) : null;
		scrollPosition = Vector2.zero;
		Repaint();
	}

	private void EnsureSerializedSpecies()
	{
		if (serializedSpecies == null && species != null)
		{
			serializedSpecies = new SerializedObject(species);
		}
	}

	private void EnsureTamingProfile()
	{
		if (species == null)
		{
			return;
		}

		if (species.TamingProfile != null)
		{
			species.TamingProfile.EnsureInitialized();
			return;
		}

		Undo.RecordObject(species, "Initialize Taming Profile");
		species.TamingProfile = new TamingProfile();
		species.TamingProfile.EnsureInitialized();
		EditorUtility.SetDirty(species);
		serializedSpecies = new SerializedObject(species);
	}

	private static string GetRequirementLabel(Type requirementType)
	{
		return ManagedReferenceTypePicker.NicifyTypeName(requirementType, suffixToTrim: "Requirement");
	}
}
