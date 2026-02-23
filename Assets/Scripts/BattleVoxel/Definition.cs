using UnityEngine;

namespace Erelia.BattleVoxel
{
	[CreateAssetMenu(menuName = "BattleVoxel/Definition", fileName = "NewBattleVoxelDefinition")]
	public class Definition : VoxelKit.Definition
	{
		[SerializeField] private Erelia.BattleVoxel.Cell battleData = new Erelia.BattleVoxel.Cell();
		[HideInInspector] [SerializeReference] private Erelia.BattleVoxel.MaskShape maskShape = null;

		public Erelia.BattleVoxel.Cell BattleData => battleData;
		public Erelia.BattleVoxel.MaskShape MaskShape => maskShape;

		protected override void Initialize()
		{
			base.Initialize();
			maskShape?.Initialize();
		}
	}
}

