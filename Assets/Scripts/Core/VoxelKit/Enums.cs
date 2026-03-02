using UnityEngine;
using System;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Describes how a voxel can be traversed by an entity (pathfinding / movement rules).
	/// </summary>
	public enum Traversal
	{
		/// <summary>
		/// The voxel blocks movement (cannot be walked through).
		/// </summary>
		Obstacle,

		/// <summary>
		/// The voxel can be walked on / through.
		/// </summary>
		Walkable
	}

	/// <summary>
	/// Cardinal orientation around the Y axis for voxel instances.
	/// </summary>
	/// <remarks>
	/// Values are explicitly set so this enum can be cast to an integer number of 90° steps:
	/// <c>(int)orientation</c> yields a value in the range 0..3.
	/// </remarks>
	public enum Orientation
	{
		/// <summary>Default orientation (0 steps): facing +X.</summary>
		PositiveX = 0,

		/// <summary>1 step (90°): facing +Z.</summary>
		PositiveZ = 1,

		/// <summary>2 steps (180°): facing -X.</summary>
		NegativeX = 2,

		/// <summary>3 steps (270°): facing -Z.</summary>
		NegativeZ = 3
	}

	/// <summary>
	/// Mirror state applied to a voxel instance along the Y axis.
	/// </summary>
	public enum FlipOrientation
	{
		/// <summary>
		/// Default (not flipped) orientation along Y.
		/// </summary>
		PositiveY,

		/// <summary>
		/// Flipped along Y (mirrored vertically, around the plane at Y = 0.5).
		/// </summary>
		NegativeY
	}

	/// <summary>
	/// Identifies the six axis-aligned planes of a unit voxel cube.
	/// </summary>
	/// <remarks>
	/// This is typically used to:
	/// <list type="bullet">
	/// <item><description>Address outer faces of a voxel for meshing/culling.</description></item>
	/// <item><description>Map between face direction and offsets/normals.</description></item>
	/// <item><description>Index per-face data structures (textures, occlusion, etc.).</description></item>
	/// </list>
	/// </remarks>
	[Serializable]
	public enum AxisPlane
	{
		/// <summary>Positive X face (x = 1).</summary>
		PosX,

		/// <summary>Negative X face (x = 0).</summary>
		NegX,

		/// <summary>Positive Y face (y = 1).</summary>
		PosY,

		/// <summary>Negative Y face (y = 0).</summary>
		NegY,

		/// <summary>Positive Z face (z = 1).</summary>
		PosZ,

		/// <summary>Negative Z face (z = 0).</summary>
		NegZ
	}
}