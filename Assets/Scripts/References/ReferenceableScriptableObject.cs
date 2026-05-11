using UnityEngine;

public abstract class ReferenceableScriptableObject : ScriptableObject
{
	[SerializeField, HideInInspector] private string guid;

	public string Guid => guid;

#if UNITY_EDITOR
	protected virtual void OnValidate()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return;
		}

		string path = UnityEditor.AssetDatabase.GetAssetPath(this);
		if (string.IsNullOrEmpty(path))
		{
			return;
		}

		guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
		UnityEditor.EditorUtility.SetDirty(this);
	}
#endif
}
