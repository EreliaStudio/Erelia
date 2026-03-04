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
	public sealed class Model
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
		/// Gets the species registry id.
		/// </summary>
		public int SpeciesId => speciesId;

		/// <summary>
		/// Gets the nickname.
		/// </summary>
		public string Nickname => nickname;

		/// <summary>
		/// Creates an empty creature instance model.
		/// </summary>
		public Model()
		{
			// Default constructor required for serialization.
		}

		/// <summary>
		/// Creates a creature instance model with explicit values.
		/// </summary>
		/// <param name="speciesId">Species registry id.</param>
		/// <param name="nickname">Optional nickname.</param>
		public Model(int speciesId, string nickname)
		{
			this.speciesId = speciesId;
			this.nickname = nickname;
		}

		/// <summary>
		/// Sets the species registry id.
		/// </summary>
		/// <param name="id">New species id.</param>
		public void SetSpeciesId(int id)
		{
			speciesId = id;
		}

	}
}
