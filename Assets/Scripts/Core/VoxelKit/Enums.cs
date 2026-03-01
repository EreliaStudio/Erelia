using UnityEngine;
using System;

namespace Erelia.Core.VoxelKit
{
	public enum Traversal
	{
		Obstacle,
		Walkable
	}

	public enum Orientation
	{
		PositiveX = 0,
		PositiveZ = 1,
		NegativeX = 2,
		NegativeZ = 3
	}

	public enum FlipOrientation
	{
		PositiveY,
		NegativeY
	}
}


