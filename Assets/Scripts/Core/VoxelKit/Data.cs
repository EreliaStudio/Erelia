using UnityEngine;
using System;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Stores gameplay and authoring data associated with a voxel definition.
	/// </summary>
	/// <remarks>
	/// This class is intended to be serialized by Unity and embedded in voxel assets
	/// (for example inside a <c>Definition</c> ScriptableObject).
	/// </remarks>
	[Serializable]
	public class Data
	{
		/// <summary>
		/// Determines how this voxel can be traversed (e.g., blocks movement vs walkable).
		/// </summary>
		[SerializeField]
		private Erelia.Core.VoxelKit.Traversal traversal = Erelia.Core.VoxelKit.Traversal.Obstacle;

		/// <summary>
		/// Gets the traversal rule for this voxel.
		/// </summary>
		public Erelia.Core.VoxelKit.Traversal Traversal => traversal;
	}
}