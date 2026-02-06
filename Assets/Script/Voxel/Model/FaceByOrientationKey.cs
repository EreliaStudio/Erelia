using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel.Model
{
	public class FaceByOrientationCollection
	{
		public struct Key : IEquatable<Key>
		{
			private readonly Voxel.Model.Face face;
			private readonly Orientation orientation;
			private readonly FlipOrientation flipOrientation;

			public Key(Voxel.Model.Face face, Orientation orientation, FlipOrientation flipOrientation)
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

		private readonly Dictionary<Key, Voxel.Model.Face> _collection = new Dictionary<Key, Voxel.Model.Face>();

		public bool TryGetValue(Voxel.Model.Face face, Voxel.Model.Orientation orientation, Voxel.Model.FlipOrientation flipOrientation, out Voxel.Model.Face output)
		{
			output = null;
			if (face == null)
			{
				return false;
			}

			var key = new Key(face, orientation, flipOrientation);
			if (_collection.TryGetValue(key, out Voxel.Model.Face cached))
			{
				output = cached;
				return true;
			}

			cached = Utils.Geometry.TransformFace(face, orientation, flipOrientation);
			_collection[key] = cached;
			return true;
		}	
	}
}