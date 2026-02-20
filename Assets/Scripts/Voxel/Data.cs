using UnityEngine;
using System;

namespace Erelia.Voxel
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Erelia.Voxel.Collision collision = Erelia.Voxel.Collision.Solid;
    	[SerializeField] private Erelia.Voxel.Traversal traversal = Erelia.Voxel.Traversal.Obstacle;

    	public Erelia.Voxel.Collision Collision => collision;
    	public Erelia.Voxel.Traversal Traversal => traversal;
	}
}


