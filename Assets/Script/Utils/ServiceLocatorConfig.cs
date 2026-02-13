using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	[CreateAssetMenu(menuName = "Config/Service Locator", fileName = "ServiceLocatorConfig")]
	public class ServiceLocatorConfig : ScriptableObject
	{
		private const string ResourcesName = "ServiceLocatorConfig";

		[SerializeField] private Exploration.World.Chunk.Model.IGenerator worldGenerator = null;
		[SerializeField] private List<Core.Voxel.Service.Entry> voxelEntries = new List<Core.Voxel.Service.Entry>();
		[SerializeField] private Core.Mask.SpriteMapping maskMappings = new Core.Mask.SpriteMapping();
		[SerializeField] private Core.Encounter.Table.Model.Data defaultEncounterTable = null;

		public Exploration.World.Chunk.Model.IGenerator WorldGenerator => worldGenerator;
		public List<Core.Voxel.Service.Entry> VoxelEntries => voxelEntries;
		public Core.Mask.SpriteMapping SpriteMappings => maskMappings;
		public Core.Encounter.Table.Model.Data DefaultEncounterTable => defaultEncounterTable;

		public static ServiceLocatorConfig LoadFromResources()
		{
			return Resources.Load<ServiceLocatorConfig>(ResourcesName);
		}
	}
}
