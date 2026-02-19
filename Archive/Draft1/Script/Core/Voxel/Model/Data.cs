using UnityEngine;
using System;

namespace Core.Voxel.Model
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Core.Voxel.Model.Collision collision = Core.Voxel.Model.Collision.Solid;
    	[SerializeField] private Core.Voxel.Model.Traversal traversal = Core.Voxel.Model.Traversal.Obstacle;

    	public Core.Voxel.Model.Collision Collision => collision;
    	public Core.Voxel.Model.Traversal Traversal => traversal;
	}
}
