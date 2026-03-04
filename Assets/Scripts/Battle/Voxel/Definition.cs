using UnityEngine;

namespace Erelia.Battle.Voxel
{
	/// <summary>
	/// Battle-specific voxel definition that adds mask shape and battle data.
	/// Initializes the mask shape so it can provide faces and entry points.
	/// </summary>
	[CreateAssetMenu(menuName = "BattleVoxel/Definition", fileName = "NewBattleVoxelDefinition")]
	public class Definition : Erelia.Core.VoxelKit.Definition
	{
		/// <summary>
		/// Battle-specific data attached to this voxel definition.
		/// </summary>
		[SerializeField] private Erelia.Battle.Voxel.Data battleData = new Erelia.Battle.Voxel.Data();
		/// <summary>
		/// Mask shape instance used for overlay rendering.
		/// </summary>
		[HideInInspector] [SerializeReference] private Erelia.Battle.Voxel.MaskShape maskShape = null;

		/// <summary>
		/// Gets the battle-specific data.
		/// </summary>
		public Erelia.Battle.Voxel.Data BattleData => battleData;
		/// <summary>
		/// Gets the mask shape instance.
		/// </summary>
		public Erelia.Battle.Voxel.MaskShape MaskShape => maskShape;

		/// <summary>
		/// Initializes the base definition and the battle mask shape.
		/// </summary>
		protected override void Initialize()
		{
			// Run base initialization and prepare the mask shape.
			base.Initialize();
			maskShape?.Initialize();
		}
	}
}

