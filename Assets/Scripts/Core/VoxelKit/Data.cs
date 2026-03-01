using UnityEngine;
using System;

namespace Erelia.Core.VoxelKit
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Erelia.Core.VoxelKit.Traversal traversal = Erelia.Core.VoxelKit.Traversal.Obstacle;

    	public Erelia.Core.VoxelKit.Traversal Traversal => traversal;
	}
}



