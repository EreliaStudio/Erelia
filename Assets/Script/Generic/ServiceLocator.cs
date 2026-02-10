using Unity;
using System;
using UnityEngine;

namespace Utils
{
	public class ServiceLocator : MonoBehaviour
	{
		private static ServiceLocator _instance = null;

		public static ServiceLocator Instance => _instance;

		[SerializeField] private World.Service worldService = new World.Service();
		public World.Service WorldService => worldService;
		
		[SerializeField] private Voxel.Service voxelService = new Voxel.Service();
		public Voxel.Service VoxelService => voxelService;
		
		[SerializeField] private Player.Service playerService = new Player.Service();
		public Player.Service PlayerService => playerService;

		[SerializeField] private Exploration.Encounter.Service encounterService = new Exploration.Encounter.Service();
		public Exploration.Encounter.Service EncounterService => encounterService;

		[SerializeField] private Battle.Board.Service battleBoardService = new Battle.Board.Service();
		public Battle.Board.Service BattleBoardService => battleBoardService;
		
		[SerializeField] private SceneLoader sceneLoader = new SceneLoader();
		public SceneLoader SceneLoader => sceneLoader;

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return ;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);
			
			worldService.Init();
			voxelService.Init();
			playerService.Init();
			encounterService.Init();
			battleBoardService.Init();
		}
	}
}
