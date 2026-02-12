using System;
using UnityEngine;

namespace Utils
{
	public class ServiceLocator
	{
		private static ServiceLocator _instance = null;

		public static ServiceLocator Instance => _instance;

		private readonly World.Service worldService = null;
		public World.Service WorldService => worldService;
		
		private readonly Voxel.Service voxelService = null;
		public Voxel.Service VoxelService => voxelService;
		
		private readonly Player.Service playerService = null;
		public Player.Service PlayerService => playerService;

		private readonly Exploration.Encounter.Service encounterService = null;
		public Exploration.Encounter.Service EncounterService => encounterService;

		private readonly Battle.Board.Service battleBoardService = null;
		public Battle.Board.Service BattleBoardService => battleBoardService;

		private readonly Mask.Service maskService = null;
		public Mask.Service MaskService => maskService;
		
		private readonly SceneLoader sceneLoader = null;
		public SceneLoader SceneLoader => sceneLoader;

		private readonly global::Camera.Service cameraService = null;
		public global::Camera.Service CameraService => cameraService;

		private ServiceLocator(ServiceLocatorConfig config)
		{
			worldService = new World.Service(config != null ? config.WorldGenerator : null);
			voxelService = new Voxel.Service(config != null ? config.VoxelEntries : null);
			playerService = new Player.Service();
			encounterService = new Exploration.Encounter.Service(config != null ? config.DefaultEncounterTable : null);
			battleBoardService = new Battle.Board.Service();
			sceneLoader = new SceneLoader();
			maskService = new Mask.Service(config != null ? config.SpriteMappings : new Mask.SpriteMapping());
			cameraService = new global::Camera.Service();
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
