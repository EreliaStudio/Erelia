using UnityEngine;
using System;

namespace Voxel.Model
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Voxel.Model.Collision collision = Voxel.Model.Collision.Solid;
    	[SerializeField] private Voxel.Model.Traversal traversal = Voxel.Model.Traversal.Obstacle;

    	public Voxel.Model.Collision Collision => collision;
    	public Voxel.Model.Traversal Traversal => traversal;
	}
}
