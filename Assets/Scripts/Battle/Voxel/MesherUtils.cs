using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.MesherUtils
{
	/// <summary>
	/// Cache for transformed cardinal point sets by orientation and flip.
	/// Avoids recomputing rotated offsets during mask placement.
	/// </summary>
	public static class CardinalPointSetCache
	{
		/// <summary>
		/// Cache key for transformed cardinal point sets.
		/// Combines the source set with orientation and flip.
		/// </summary>
		private readonly struct Key : IEquatable<Key>
		{
			/// <summary>
			/// Source cardinal point set.
			/// </summary>
			private readonly CardinalPointSet entryPoints;
			/// <summary>
			/// Orientation applied to the point set.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Orientation orientation;
			/// <summary>
			/// Flip orientation applied to the point set.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.FlipOrientation flipOrientation;

			/// <summary>
			/// Creates a cache key from a point set and orientations.
			/// </summary>
			public Key(CardinalPointSet entryPoints, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
			{
				// Store key components.
				this.entryPoints = entryPoints;
				this.orientation = orientation;
				this.flipOrientation = flipOrientation;
			}

			/// <summary>
			/// Tests equality against another key.
			/// </summary>
			public bool Equals(Key other)
			{
				// Compare key components by reference/value.
				return ReferenceEquals(entryPoints, other.entryPoints)
					&& orientation == other.orientation
					&& flipOrientation == other.flipOrientation;
			}

			/// <summary>
			/// Object equality override.
			/// </summary>
			public override bool Equals(object obj)
			{
				// Delegate to typed equality.
				return obj is Key other && Equals(other);
			}

			/// <summary>
			/// Computes a hash code for dictionary usage.
			/// </summary>
			public override int GetHashCode()
			{
				// Combine entry point and orientation hashes.
				int hash = entryPoints != null ? entryPoints.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
					hash = (hash * 397) ^ (int)flipOrientation;
				}
				return hash;
			}
		}

		private static readonly Dictionary<Key, CardinalPointSet> Collection = new Dictionary<Key, CardinalPointSet>();

		/// <summary>
		/// Gets or computes a transformed cardinal point set.
		/// </summary>
		public static bool TryGetValue(
			CardinalPointSet entryPoints,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation,
			out CardinalPointSet output)
		{
			// Return cached transform or compute and cache a new one.
			output = null;
			if (entryPoints == null)
			{
				return false;
			}

			var key = new Key(entryPoints, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(entryPoints, orientation, flipOrientation);
			Collection[key] = output;
			return true;
		}

		/// <summary>
		/// Transforms a cardinal point set by orientation and flip.
		/// </summary>
		private static CardinalPointSet TransformCardinalPoints(
			CardinalPointSet source,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// Transform each cardinal point using voxel geometry utilities.
			Vector3 posX = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.PositiveX, orientation, flipOrientation);
			Vector3 negX = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.NegativeX, orientation, flipOrientation);
			Vector3 posZ = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.PositiveZ, orientation, flipOrientation);
			Vector3 negZ = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.NegativeZ, orientation, flipOrientation);
			Vector3 stationary = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.Stationary, orientation, flipOrientation);
			return new CardinalPointSet(posX, negX, posZ, negZ, stationary);
		}
	}
}
