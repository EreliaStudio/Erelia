using UnityEngine;

namespace Erelia.Battle.Voxel
{
	[CreateAssetMenu(menuName = "BattleVoxel/Definition", fileName = "NewBattleVoxelDefinition")]
	public class Definition : Erelia.Core.VoxelKit.Definition
	{
		[SerializeField] private Erelia.Battle.Voxel.Data battleData = new Erelia.Battle.Voxel.Data();
		[HideInInspector] [SerializeReference] private Erelia.Battle.Voxel.MaskShape maskShape = null;

		public Erelia.Battle.Voxel.Data BattleData => battleData;
		public Erelia.Battle.Voxel.MaskShape MaskShape => maskShape;

		protected override void Initialize()
		{
			base.Initialize();
			maskShape?.Initialize();
		}
	}
}

