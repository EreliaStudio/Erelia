using System;
using UnityEngine;

namespace Utils
{
	public class ServiceLocator
	{
		private static ServiceLocator _instance = null;

		public static ServiceLocator Instance => _instance;

		private readonly Exploration.World.Service worldService = null;
		public Exploration.World.Service WorldService => worldService;
		
		private readonly Core.Voxel.Service voxelService = null;
		public Core.Voxel.Service VoxelService => voxelService;
		
		private readonly Core.Player.Service playerService = null;
		public Core.Player.Service PlayerService => playerService;

		private readonly Core.Encounter.Service encounterService = null;
		public Core.Encounter.Service EncounterService => encounterService;

		private readonly Battle.Board.Service battleBoardService = null;
		public Battle.Board.Service BattleBoardService => battleBoardService;

		private readonly Core.Mask.Service maskService = null;
		public Core.Mask.Service MaskService => maskService;
		
		private readonly SceneLoader sceneLoader = null;
		public SceneLoader SceneLoader => sceneLoader;

		private readonly Core.Camera.Service cameraService = null;
		public Core.Camera.Service CameraService => cameraService;

		private ServiceLocator(ServiceLocatorConfig config)
		{
			worldService = new Exploration.World.Service(config != null ? config.WorldGenerator : null);
			voxelService = new Core.Voxel.Service(config != null ? config.VoxelEntries : null);
			playerService = new Core.Player.Service();
			encounterService = new Core.Encounter.Service(config != null ? config.DefaultEncounterTable : null);
			battleBoardService = new Battle.Board.Service();
			sceneLoader = new SceneLoader();
			maskService = new Core.Mask.Service(config != null ? config.SpriteMappings : new Core.Mask.SpriteMapping());
			cameraService = new Core.Camera.Service();
		}

		public static void Initialize(ServiceLocatorConfig config)
		{
			if (_instance != null)
			{
				Debug.LogWarning("ServiceLocator.Initialize was called more than once. Using existing instance.");
				return;
			}

			_instance = new ServiceLocator(config);
		}
	}
}
