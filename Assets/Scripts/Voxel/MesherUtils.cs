using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mesherutils
{
	public static class FaceByOrientationCache
	{
		private struct Key : IEquatable<Key>
		{
			private readonly Voxel.Face face;
			private readonly Voxel.Orientation orientation;
			private readonly Voxel.FlipOrientation flipOrientation;

			public Key(Voxel.Face face, Voxel.Orientation orientation, Voxel.FlipOrientation flipOrientation)
			{
				this.face = face;
				this.orientation = orientation;
				this.flipOrientation = flipOrientation;
			}

			public bool Equals(Key other)
			{
				return ReferenceEquals(face, other.face)
					&& orientation == other.orientation
					&& flipOrientation == other.flipOrientation;
			}

			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = face != null ? face.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
					hash = (hash * 397) ^ (int)flipOrientation;
				}
				return hash;
			}
		}

		private static readonly Dictionary<Key, Voxel.Face> Collection = new Dictionary<Key, Voxel.Face>();

		public static bool TryGetValue(Voxel.Face face, Voxel.Orientation orientation, Voxel.FlipOrientation flipOrientation, out Voxel.Face output)
		{
			output = null;
			if (face == null)
			{
				return false;
			}

			var key = new Key(face, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out Voxel.Face cached))
			{
				output = cached;
				return true;
			}

			output = Utils.Geometry.TransformFace(face, orientation, flipOrientation);
			Collection[key] = output;
			return true;
		}	
	}

	public static class FaceVsFaceOcclusionCache
	{
		private struct FacePairKey : IEquatable<FacePairKey>
		{
			private readonly Voxel.Face face;
			private readonly Voxel.Face occluder;

			public FacePairKey(Voxel.Face face, Voxel.Face occluder)
			{
				this.face = face;
				this.occluder = occluder;
			}

			public bool Equals(FacePairKey other)
			{
				return ReferenceEquals(face, other.face) && ReferenceEquals(occluder, other.occluder);
			}

			public override bool Equals(object obj)
			{
				return obj is FacePairKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = face != null ? face.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (occluder != null ? occluder.GetHashCode() : 0);
				}
				return hash;
			}
		}

		private static readonly Dictionary<FacePairKey, bool> Collection = new Dictionary<FacePairKey, bool>();

		public static bool TryGetValue(Voxel.Face face, Voxel.Face occluder, out bool isOccluded)
		{
			isOccluded = false;
			if (face == null || occluder == null)
			{
				return false;
			}

			var key = new FacePairKey(face, occluder);
			if (Collection.TryGetValue(key, out bool cached))
			{
				isOccluded = cached;
				return true;
			}

			isOccluded = face.IsOccludedBy(occluder);
			Collection[key] = isOccluded;
			return true;
		}
	}
}
