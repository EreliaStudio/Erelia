using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.MesherUtils
{
	/// <summary>
	/// Caches transformed cardinal point sets by (source set reference, orientation).
	/// </summary>
	/// <remarks>
	/// This mirrors the role of <see cref="Erelia.Core.VoxelKit.MesherUtils.FaceByOrientationCache"/>,
	/// but for battle voxel cardinal point sets instead of faces.
	/// The source set is already selected per flip orientation before entering the cache,
	/// so only the cardinal Y rotation remains to be applied here.
	/// </remarks>
	public static class CardinalPointSetByOrientationCache
	{
		/// <summary>
		/// Cache key for transformed cardinal point sets.
		/// </summary>
		private readonly struct Key : IEquatable<Key>
		{
			/// <summary>
			/// Source cardinal point set.
			/// </summary>
			private readonly CardinalPointSet source;
			/// <summary>
			/// Orientation applied to the point set.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Orientation orientation;
			/// <summary>
			/// Creates a cache key from a point set and orientations.
			/// </summary>
			public Key(
				CardinalPointSet source,
				Erelia.Core.VoxelKit.Orientation orientation)
			{
				this.source = source;
				this.orientation = orientation;
			}

			/// <summary>
			/// Tests equality against another key.
			/// </summary>
			public bool Equals(Key other)
			{
				return ReferenceEquals(source, other.source)
					&& orientation == other.orientation;
			}

			/// <summary>
			/// Object equality override.
			/// </summary>
			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			/// <summary>
			/// Computes a hash code for dictionary usage.
			/// </summary>
			public override int GetHashCode()
			{
				int hash = source != null ? source.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
				}
				return hash;
			}
		}

		private static readonly Dictionary<Key, CardinalPointSet> Collection
			= new Dictionary<Key, CardinalPointSet>();

		/// <summary>
		/// Gets or computes a transformed cardinal point set.
		/// </summary>
		public static bool TryGetValue(
			CardinalPointSet source,
			Erelia.Core.VoxelKit.Orientation orientation,
			out CardinalPointSet output)
		{
			output = null;
			if (source == null)
			{
				return false;
			}

			var key = new Key(source, orientation);
			if (Collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(source, orientation);
			Collection[key] = output;
			return true;
		}

		/// <summary>
		/// Transforms a cardinal point set by orientation and flip.
		/// </summary>
		private static CardinalPointSet TransformCardinalPoints(
			CardinalPointSet source,
			Erelia.Core.VoxelKit.Orientation orientation)
		{
			Vector3 posX = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.PositiveX, orientation);
			Vector3 negX = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.NegativeX, orientation);
			Vector3 posZ = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.PositiveZ, orientation);
			Vector3 negZ = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.NegativeZ, orientation);
			Vector3 stationary = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(
				source.Stationary,
				orientation,
				Erelia.Core.VoxelKit.FlipOrientation.PositiveY);
			return new CardinalPointSet(posX, negX, posZ, negZ, stationary);
		}

		private static Vector3 TransformPointForWorldDirection(
			CardinalPointSet source,
			Erelia.Battle.Voxel.CardinalPoint worldDirection,
			Erelia.Core.VoxelKit.Orientation orientation)
		{
			Erelia.Battle.Voxel.CardinalPoint localDirection = ResolveLocalDirection(worldDirection, orientation);
			return Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(
				source.Get(localDirection),
				orientation,
				Erelia.Core.VoxelKit.FlipOrientation.PositiveY);
		}

		private static Erelia.Battle.Voxel.CardinalPoint ResolveLocalDirection(
			Erelia.Battle.Voxel.CardinalPoint worldDirection,
			Erelia.Core.VoxelKit.Orientation orientation)
		{
			switch (orientation)
			{
				case Erelia.Core.VoxelKit.Orientation.PositiveX:
					return worldDirection;
				case Erelia.Core.VoxelKit.Orientation.PositiveZ:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				case Erelia.Core.VoxelKit.Orientation.NegativeX:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				case Erelia.Core.VoxelKit.Orientation.NegativeZ:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				default:
					return worldDirection;
			}
		}
	}
}
