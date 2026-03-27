using UnityEngine;

namespace Erelia.Core.Creature
{
	[System.Serializable]
	public sealed class Team : ISerializationCallbackReceiver
	{
		public const int DefaultSize = 6;

		[SerializeField] private Erelia.Core.Creature.Instance.Model[] slots =
			new Erelia.Core.Creature.Instance.Model[DefaultSize];

		public Erelia.Core.Creature.Instance.Model[] Slots => slots;

		public int SlotCount => slots != null ? slots.Length : 0;

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			NormalizeSlots();
		}

		public void NormalizeSlots()
		{
			if (slots == null)
			{
				return;
			}

			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i] != null && slots[i].IsEmpty)
				{
					slots[i] = null;
				}
			}
		}
	}
}
