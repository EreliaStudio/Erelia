using UnityEngine;

namespace Erelia.Core.Creature
{
	/// <summary>
	/// Serializable container representing a team of creature instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The team is stored as a fixed-size array of slots (<see cref="Slots"/>). Each slot can contain a
	/// <see cref="Erelia.Core.Creature.Instance.Model"/> or be <c>null</c> to represent an empty slot.
	/// </para>
	/// <para>
	/// JSON format (Unity <see cref="JsonUtility"/>):
	/// </para>
	/// <code>
	/// {
	///   "slots": [
	///     { "speciesId": 12, "nickname": "Kitsu" },
	///     null,
	///     { "speciesId": 3, "nickname": "Torty" },
	///     null,
	///     null,
	///     { "speciesId": -1, "nickname": null }
	///   ]
	/// }
	/// </code>
	/// <para>
	/// Notes:
	/// <list type="bullet">
	/// <item><description>The JSON property name is <c>slots</c> because it matches the serialized field name.</description></item>
	/// <item><description>Entries may be <c>null</c> (empty slot) or an object matching the JSON format of the instance model.</description></item>
	/// <item><description>Serialization is handled externally via <see cref="JsonUtility"/>.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	[System.Serializable]
	public sealed class Team : ISerializationCallbackReceiver
	{
		/// <summary>
		/// Default number of slots in a team.
		/// </summary>
		public const int DefaultSize = 6;

		/// <summary>
		/// Backing array storing the team slots.
		/// </summary>
		/// <remarks>
		/// Each element may be <c>null</c> to represent an empty slot.
		/// </remarks>
		[SerializeField] private Erelia.Core.Creature.Instance.Model[] slots =
			new Erelia.Core.Creature.Instance.Model[DefaultSize];

		/// <summary>
		/// Gets the array of team slots.
		/// </summary>
		/// <remarks>
		/// The returned array is the live backing array. Mutating it changes the team contents.
		/// </remarks>
		public Erelia.Core.Creature.Instance.Model[] Slots => slots;


		/// <summary>
		/// Gets the number of slots in this team.
		/// </summary>
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
