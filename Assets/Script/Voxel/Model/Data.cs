using UnityEngine;
using System;

namespace Voxel.Model
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Collision collision = Collision.Solid;
    	[SerializeField] private Traversal traversal = Traversal.Obstacle;

    	public Voxel.Collision Collision => collision;
    	public Voxel.Traversal Traversal => traversal;
	}
}
