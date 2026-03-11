using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	/// <summary>
	/// Serializable data model representing a single creature instance.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A creature instance references its species through <see cref="SpeciesId"/> (resolved via
	/// <see cref="Erelia.Core.Creature.SpeciesRegistry"/>) and optionally stores a nickname.
	/// </para>
	/// <para>
	/// JSON format (Unity <see cref="JsonUtility"/>):
	/// </para>
	/// <code>
	/// {
	///   "speciesId": 12,
	///   "nickname": "Kitsu"
	/// }
	/// </code>
	/// <para>
	/// Notes:
	/// <list type="bullet">
	/// <item><description>Property names match the serialized field names (<c>speciesId</c>, <c>nickname</c>).</description></item>
	/// <item><description>Private fields are serialized because they are marked with <c>[SerializeField]</c>.</description></item>
	/// <item><description>Serialization is handled externally via <see cref="JsonUtility"/>.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	[System.Serializable]
	public sealed class Model : ISerializationCallbackReceiver
	{
		/// <summary>
		/// Species registry id associated with this creature instance.
		/// </summary>
		[SerializeField] private int speciesId = Erelia.Core.Creature.SpeciesRegistry.EmptySpeciesId;

		/// <summary>
		/// Optional nickname assigned to this creature instance.
		/// </summary>
		[SerializeField] private string nickname;

		/// <summary>
		/// Additive stats gained by this specific creature instance.
		/// </summary>
		[SerializeField] private Erelia.Core.Creature.Stats stats = new Erelia.Core.Creature.Stats();

		/// <summary>
		/// Gets the species registry id.
		/// </summary>
		public int SpeciesId => speciesId;

		/// <summary>
		/// Gets the nickname.
		/// </summary>
		public string Nickname => nickname;
		public Erelia.Core.Creature.Stats Stats => stats ??= new Erelia.Core.Creature.Stats();
		public bool IsEmpty => speciesId < 0;

		public string DisplayName
		{
			get
			{
				if (!string.IsNullOrEmpty(nickname))
				{
					return nickname;
				}

				Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
				if (registry != null &&
					registry.TryGet(speciesId, out Erelia.Core.Creature.Species species) &&
					species != null)
				{
					return species.DisplayName;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Creates an empty creature instance model.
		/// </summary>
		public Model()
		{
			// Default constructor required for serialization.
		}

		public Model(int speciesId, string nickname, Erelia.Core.Creature.Stats stats)
		{
			this.speciesId = speciesId;
			this.nickname = nickname;
			this.stats = stats ?? new Erelia.Core.Creature.Stats();
		}

		/// <summary>
		/// Sets the species registry id.
		/// </summary>
		/// <param name="id">New species id.</param>
		public void SetSpeciesId(int id)
		{
			speciesId = id;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			stats ??= new Erelia.Core.Creature.Stats();
		}
	}
}
