using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReferenceableDatabase", menuName = "Game/Referenceable Database")]
public sealed class ReferenceableDatabase : ScriptableObject
{
	[SerializeField] private List<ReferenceableScriptableObject> entries = new();

	public IReadOnlyList<ReferenceableScriptableObject> Entries => entries;

#if UNITY_EDITOR
	public void Rebuild()
	{
		entries.Clear();

		string[] assetGuids = UnityEditor.AssetDatabase.FindAssets("t:ReferenceableScriptableObject");
		for (int index = 0; index < assetGuids.Length; index++)
		{
			string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuids[index]);
			ReferenceableScriptableObject asset =
				UnityEditor.AssetDatabase.LoadAssetAtPath<ReferenceableScriptableObject>(path);

			if (asset != null)
			{
				entries.Add(asset);
			}
		}

		UnityEditor.EditorUtility.SetDirty(this);
		UnityEditor.AssetDatabase.SaveAssets();

		Debug.Log($"[ReferenceableDatabase] Rebuilt — {entries.Count} entries registered.");
	}
#endif
}
