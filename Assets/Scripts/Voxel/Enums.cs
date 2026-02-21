using UnityEngine;
using System;

namespace Erelia.Voxel
{
	public enum Traversal
	{
		Obstacle,
		Walkable
	}

	public enum Orientation
	{
		PositiveX,
		PositiveZ,
		NegativeX,
		NegativeZ
	}

	public enum FlipOrientation
	{
		PositiveY,
		NegativeY
	}
}

