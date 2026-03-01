using System.IO;
using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	[System.Serializable]
	public sealed class Model
	{
		[SerializeField] private int speciesId = Erelia.Core.Creature.SpeciesRegistry.EmptySpeciesId;
		[SerializeField] private string nickname;
		[SerializeField] private int level = 1;

		public int SpeciesId => speciesId;
		public string Nickname => nickname;
		public int Level => level;

		public Model()
		{
		}

		public Model(int speciesId, string nickname, int level)
		{
			this.speciesId = speciesId;
			this.nickname = nickname;
			this.level = level;
		}

		public void SetSpeciesId(int id)
		{
			speciesId = id;
		}

		public void Save(string path)
		{
			string json = JsonUtility.ToJson(this, true);
			File.WriteAllText(path, json);
		}

		public bool Load(string path)
		{
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				Debug.LogWarning($"[Erelia.Core.Creature.Instance.Model] Save file not found at '{path}'.");
				return false;
			}

			JsonUtility.FromJsonOverwrite(json, this);
			return true;
		}
	}
}
