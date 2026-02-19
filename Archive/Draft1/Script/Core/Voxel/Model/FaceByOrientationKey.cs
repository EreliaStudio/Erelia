using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Voxel.Model
{
	public class FaceByOrientationCollection
	{
		public struct Key : IEquatable<Key>
		{
			private readonly Core.Voxel.Model.Face face;
			private readonly Orientation orientation;
			private readonly FlipOrientation flipOrientation;

			public Key(Core.Voxel.Model.Face face, Orientation orientation, FlipOrientation flipOrientation)
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

		private readonly Dictionary<Key, Core.Voxel.Model.Face> _collection = new Dictionary<Key, Core.Voxel.Model.Face>();

		public bool TryGetValue(Core.Voxel.Model.Face face, Core.Voxel.Model.Orientation orientation, Core.Voxel.Model.FlipOrientation flipOrientation, out Core.Voxel.Model.Face output)
		{
			output = null;
			if (face == null)
			{
				return false;
			}

			var key = new Key(face, orientation, flipOrientation);
			if (_collection.TryGetValue(key, out Core.Voxel.Model.Face cached))
			{
				output = cached;
				return true;
			}

			output = Core.Utils.Geometry.TransformFace(face, orientation, flipOrientation);
			_collection[key] = output;
			return true;
		}	
	}
}
