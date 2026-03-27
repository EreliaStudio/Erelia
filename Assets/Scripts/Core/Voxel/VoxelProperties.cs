using UnityEngine;
using System;

namespace Erelia.Core.Voxel
{
	[Serializable]
	public class VoxelProperties
	{
		[SerializeField]
		private Erelia.Core.Voxel.Traversal traversal = Erelia.Core.Voxel.Traversal.Obstacle;

		public Erelia.Core.Voxel.Traversal Traversal => traversal;
	}
}

