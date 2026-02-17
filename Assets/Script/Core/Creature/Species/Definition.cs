using UnityEngine;

namespace Core.Creature.Species
{
	[CreateAssetMenu(menuName = "Core/Creature/Species", fileName = "CreatureSpecies")]
	public class Definition : ScriptableObject
	{
		[SerializeField] private Core.Creature.Species.Model.Data data = new Core.Creature.Species.Model.Data();
		[SerializeField] private Core.Creature.Species.View.Presenter presenter = new Core.Creature.Species.View.Presenter();
		[SerializeField] private Core.Creature.Species.Controller.BodyCollider bodyCollider = new Core.Creature.Species.Controller.BodyCollider();

		public Core.Creature.Species.Model.Data Data => data;
		public Core.Creature.Species.View.Presenter Presenter => presenter;
		public Core.Creature.Species.Controller.BodyCollider BodyCollider => bodyCollider;

		public string FamilyName => data != null ? data.FamilyName : string.Empty;
	}
}
