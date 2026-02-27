using UnityEngine;

namespace Erelia.BattleVoxel
{
	[CreateAssetMenu(menuName = "BattleVoxel/Definition", fileName = "NewBattleVoxelDefinition")]
	public class Definition : VoxelKit.Definition
	{
		[SerializeField] private Erelia.BattleVoxel.Data battleData = new Erelia.BattleVoxel.Data();
		[HideInInspector] [SerializeReference] private Erelia.BattleVoxel.MaskShape maskShape = null;

		public Erelia.BattleVoxel.Data BattleData => battleData;
		public Erelia.BattleVoxel.MaskShape MaskShape => maskShape;

		protected override void Initialize()
		{
			base.Initialize();
			maskShape?.Initialize();
		}
	}
}

