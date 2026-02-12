using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	[CreateAssetMenu(menuName = "Config/Service Locator", fileName = "ServiceLocatorConfig")]
	public class ServiceLocatorConfig : ScriptableObject
	{
		private const string ResourcesName = "ServiceLocatorConfig";

		[SerializeField] private World.Chunk.Model.IGenerator worldGenerator = null;
		[SerializeField] private List<Voxel.Service.Entry> voxelEntries = new List<Voxel.Service.Entry>();
		[SerializeField] private Mask.SpriteMapping maskMappings = new Mask.SpriteMapping();
		[SerializeField] private Battle.EncounterTable.Model.Data defaultEncounterTable = null;

		public World.Chunk.Model.IGenerator WorldGenerator => worldGenerator;
		public List<Voxel.Service.Entry> VoxelEntries => voxelEntries;
		public Mask.SpriteMapping SpriteMappings => maskMappings;
		public Battle.EncounterTable.Model.Data DefaultEncounterTable => defaultEncounterTable;

		public static ServiceLocatorConfig LoadFromResources()
		{
			return Resources.Load<ServiceLocatorConfig>(ResourcesName);
		}
	}
}
