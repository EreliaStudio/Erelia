using UnityEditor;
using UnityEngine;

public static class ReferenceableDatabaseBuilder
{
	[MenuItem("Tools/Erelia/Rebuild Reference Database")]
	private static void Rebuild()
	{
		ReferenceableDatabase database = FindOrCreateDatabase();
		database.Rebuild();
	}

	private static ReferenceableDatabase FindOrCreateDatabase()
	{
		string[] hits = AssetDatabase.FindAssets("t:ReferenceableDatabase");
		if (hits.Length > 0)
		{
			string path = AssetDatabase.GUIDToAssetPath(hits[0]);
			return AssetDatabase.LoadAssetAtPath<ReferenceableDatabase>(path);
		}

		ReferenceableDatabase database = ScriptableObject.CreateInstance<ReferenceableDatabase>();
		AssetDatabase.CreateAsset(database, "Assets/ReferenceRegistry/ReferenceableDatabase.asset");
		AssetDatabase.SaveAssets();

		Debug.Log("[ReferenceableDatabaseBuilder] No database found — created one at Assets/ReferenceRegistry/ReferenceableDatabase.asset.");
		return database;
	}
}
