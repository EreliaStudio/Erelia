using System.IO;
using UnityEngine;

namespace Erelia.Core.Encounter
{
	[System.Serializable]
	public sealed class EncounterTable
	{
		[System.Serializable]
		public struct TeamEntry
		{
			public string TeamPath;
			[Min(0)]
			public int Weight;
		}

		[Range(0f, 1f)]
		public float EncounterChance;
		public int BaseRadius = 10;
		public int NoiseAmplitude = 4;
		public float NoiseScale = 0.15f;
		public int NoiseSeed = 1337;
		[Min(0)]
		public int PlacementRadius = 3;
		public TeamEntry[] Teams;

		public void Save(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new System.ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			string json = JsonUtility.ToJson(this, true);
			File.WriteAllText(path, json);
		}

		public bool Load(string path)
		{
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				Debug.LogWarning($"[Erelia.Core.Encounter.EncounterTable] Save file not found at '{path}'.");
				return false;
			}

			JsonUtility.FromJsonOverwrite(json, this);
			return true;
		}

		public static EncounterTable LoadFromPath(string path)
		{
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			return JsonUtility.FromJson<EncounterTable>(json);
		}
	}
}
