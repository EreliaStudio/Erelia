using UnityEngine;
using System;

namespace Voxel
{
	public enum Collision
	{
		None,
		Solid,
		Bush
	}

	public enum Traversal
	{
		Obstacle,
		Walkable,
		Air
	}
}