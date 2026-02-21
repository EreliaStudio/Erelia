using UnityEngine;
using System;

namespace Erelia.Voxel
{
	[Serializable]
	public class Data
	{
    	[SerializeField] private Erelia.Voxel.Traversal traversal = Erelia.Voxel.Traversal.Obstacle;

    	public Erelia.Voxel.Traversal Traversal => traversal;
	}
}


