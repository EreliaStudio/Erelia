using System.IO;
using UnityEngine;

namespace Erelia.Core.Creature
{
	[System.Serializable]
	public sealed class Team
	{
		public const int DefaultSize = 6;

		[SerializeField] private Erelia.Core.Creature.Instance.Model[] slots = new Erelia.Core.Creature.Instance.Model[DefaultSize];

		public Erelia.Core.Creature.Instance.Model[] Slots => slots;

		public Team()
		{
		}

		public Team(int size)
		{
			slots = new Erelia.Core.Creature.Instance.Model[Mathf.Max(0, size)];
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
				Debug.LogWarning($"[Erelia.Core.Creature.Team] Save file not found at '{path}'.");
				return false;
			}

			JsonUtility.FromJsonOverwrite(json, this);
			return true;
		}

		public int SlotCount => slots != null ? slots.Length : 0;
	}
}
