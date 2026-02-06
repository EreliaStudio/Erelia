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
		}
	}
}
