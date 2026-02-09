using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player
{
	[Serializable]
	public class Service
	{			
		public event Action<World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;

		public void Init()
		{
			
		}
	}
}
