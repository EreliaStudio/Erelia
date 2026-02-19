using UnityEngine;
using System;

namespace Voxel
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Voxel.Collision collision = Voxel.Collision.Solid;
    	[SerializeField] private Voxel.Traversal traversal = Voxel.Traversal.Obstacle;

    	public Voxel.Collision Collision => collision;
    	public Voxel.Traversal Traversal => traversal;
	}
}
