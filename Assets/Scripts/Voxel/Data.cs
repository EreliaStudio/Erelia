using UnityEngine;
using System;

namespace VoxelKit
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private VoxelKit.Traversal traversal = VoxelKit.Traversal.Obstacle;

    	public VoxelKit.Traversal Traversal => traversal;
	}
}



