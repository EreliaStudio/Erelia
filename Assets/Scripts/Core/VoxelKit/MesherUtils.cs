using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit.MesherUtils
{
	public static class FaceByOrientationCache
	{
		private struct Key : IEquatable<Key>
		{
			private readonly Erelia.Core.VoxelKit.Face face;
			private readonly Erelia.Core.VoxelKit.Orientation orientation;
			private readonly Erelia.Core.VoxelKit.FlipOrientation flipOrientation;

			public Key(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
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

		private static readonly Dictionary<Key, Erelia.Core.VoxelKit.Face> Collection = new Dictionary<Key, Erelia.Core.VoxelKit.Face>();

		public static bool TryGetValue(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation, out Erelia.Core.VoxelKit.Face output)
		{
			output = null;
			if (face == null)
			{
				return false;
			}

			var key = new Key(face, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out Erelia.Core.VoxelKit.Face cached))
			{
				output = cached;
				return true;
			}

			output = Erelia.Core.VoxelKit.Utils.Geometry.TransformFace(face, orientation, flipOrientation);
			Collection[key] = output;
			return true;
		}	
	}

	public static class FaceVsFaceOcclusionCache
	{
		private struct FacePairKey : IEquatable<FacePairKey>
		{
			private readonly Erelia.Core.VoxelKit.Face face;
			private readonly Erelia.Core.VoxelKit.Face occluder;

			public FacePairKey(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Face occluder)
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

		public static bool TryGetValue(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Face occluder, out bool isOccluded)
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



