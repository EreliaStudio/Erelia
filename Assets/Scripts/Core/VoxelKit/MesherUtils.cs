using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit.MesherUtils
{
	/// <summary>
	/// Caches transformed faces by (source face reference, orientation, flip).
	/// </summary>
	/// <remarks>
	/// Meshing can require transforming the same canonical <see cref="Erelia.Core.VoxelKit.Face"/> many times
	/// (for different voxel instances with different <see cref="Erelia.Core.VoxelKit.Orientation"/> /
	/// <see cref="Erelia.Core.VoxelKit.FlipOrientation"/>).
	/// This cache avoids repeating <see cref="Erelia.Core.VoxelKit.Utils.Geometry.TransformFace"/> for identical inputs.
	/// <para>
	/// Important: the cache key uses <b>reference equality</b> for the input face. If the face object content
	/// is mutated after caching, cached results may become inconsistent.
	/// </para>
	/// </remarks>
	public static class FaceByOrientationCache
	{
		/// <summary>
		/// Key used to store/retrieve transformed faces from the cache.
		/// </summary>
		/// <remarks>
		/// Uses:
		/// <list type="bullet">
		///   <item><description>Reference identity of the input face (not a deep content hash).</description></item>
		///   <item><description>Orientation and flip values.</description></item>
		/// </list>
		/// </remarks>
		private struct Key : IEquatable<Key>
		{
			/// <summary>
			/// The canonical source face reference.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Face face;

			/// <summary>
			/// The rotation to apply (cardinal Y rotation).
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Orientation orientation;

			/// <summary>
			/// The flip to apply (typically mirroring along Y).
			/// </summary>
			private readonly Erelia.Core.VoxelKit.FlipOrientation flipOrientation;

			/// <summary>
			/// Creates a cache key for a given face transform configuration.
			/// </summary>
			/// <param name="face">Source face reference.</param>
			/// <param name="orientation">Rotation around Y.</param>
			/// <param name="flipOrientation">Optional Y flip.</param>
			public Key(
				Erelia.Core.VoxelKit.Face face,
				Erelia.Core.VoxelKit.Orientation orientation,
				Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
			{
				this.face = face;
				this.orientation = orientation;
				this.flipOrientation = flipOrientation;
			}

			/// <summary>
			/// Checks equality against another key (reference equality for faces + exact enum matches).
			/// </summary>
			/// <param name="other">Other key.</param>
			/// <returns><c>true</c> if both keys represent the same transform request; otherwise <c>false</c>.</returns>
			public bool Equals(Key other)
			{
				return ReferenceEquals(face, other.face)
					&& orientation == other.orientation
					&& flipOrientation == other.flipOrientation;
			}

			/// <summary>
			/// Object equality override delegating to <see cref="Equals(Key)"/>.
			/// </summary>
			/// <param name="obj">Object to compare.</param>
			/// <returns><c>true</c> if the object is a matching key; otherwise <c>false</c>.</returns>
			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			/// <summary>
			/// Computes a hash code for use in dictionaries.
			/// </summary>
			/// <returns>Hash code built from face reference hash and enum values.</returns>
			public override int GetHashCode()
			{
				// Start from face reference hash (or 0 if null).
				int hash = face != null ? face.GetHashCode() : 0;

				// Combine with orientation/flip.
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
					hash = (hash * 397) ^ (int)flipOrientation;
				}

				return hash;
			}
		}

		/// <summary>
		/// Internal cache storage mapping keys to transformed faces.
		/// </summary>
		private static readonly Dictionary<Key, Erelia.Core.VoxelKit.Face> Collection
			= new Dictionary<Key, Erelia.Core.VoxelKit.Face>();

		/// <summary>
		/// Gets the transformed version of a face for the given orientation/flip, using a cache.
		/// </summary>
		/// <param name="face">Source face (canonical, non-oriented).</param>
		/// <param name="orientation">Rotation around Y.</param>
		/// <param name="flipOrientation">Optional Y flip.</param>
		/// <param name="output">The cached or newly transformed face.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="face"/> was non-null (output is set);
		/// otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// If the result is not in the cache, this method computes it via
		/// <see cref="Erelia.Core.VoxelKit.Utils.Geometry.TransformFace"/> and stores it.
		/// </remarks>
		public static bool TryGetValue(
			Erelia.Core.VoxelKit.Face face,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation,
			out Erelia.Core.VoxelKit.Face output)
		{
			// Default output.
			output = null;

			// If the input face is missing, nothing can be produced.
			if (face == null)
			{
				return false;
			}

			// Build lookup key and attempt to retrieve cached transform.
			var key = new Key(face, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out Erelia.Core.VoxelKit.Face cached))
			{
				output = cached;
				return true;
			}

			// Cache miss: compute and store.
			output = Erelia.Core.VoxelKit.Utils.Geometry.TransformFace(face, orientation, flipOrientation);
			Collection[key] = output;

			return true;
		}
	}

	/// <summary>
	/// Caches face-vs-face occlusion results.
	/// </summary>
	/// <remarks>
	/// During meshing, occlusion checks such as <see cref="Erelia.Core.VoxelKit.Face.IsOccludedBy"/> can be
	/// called repeatedly for the same pair of face references. This cache stores the boolean result
	/// keyed by the pair (face, occluder) using reference equality.
	/// <para>
	/// Important: this cache assumes faces are immutable for the duration of caching. If face geometry
	/// is modified after a result is cached, the cached value may be invalid.
	/// </para>
	/// </remarks>
	public static class FaceVsFaceOcclusionCache
	{
		/// <summary>
		/// Key identifying an ordered pair of faces (face, occluder).
		/// </summary>
		/// <remarks>
		/// The pair is ordered: (A,B) and (B,A) are different keys, which matches occlusion semantics.
		/// Uses reference equality for both faces.
		/// </remarks>
		private struct FacePairKey : IEquatable<FacePairKey>
		{
			/// <summary>
			/// The face being tested for occlusion.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Face face;

			/// <summary>
			/// The face acting as the occluder.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Face occluder;

			/// <summary>
			/// Creates a key for an occlusion test.
			/// </summary>
			/// <param name="face">Face being tested.</param>
			/// <param name="occluder">Potential occluding face.</param>
			public FacePairKey(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Face occluder)
			{
				this.face = face;
				this.occluder = occluder;
			}

			/// <summary>
			/// Checks equality against another key (reference equality for both faces).
			/// </summary>
			/// <param name="other">Other key.</param>
			/// <returns><c>true</c> if both pairs match; otherwise <c>false</c>.</returns>
			public bool Equals(FacePairKey other)
			{
				return ReferenceEquals(face, other.face) && ReferenceEquals(occluder, other.occluder);
			}

			/// <summary>
			/// Object equality override delegating to <see cref="Equals(FacePairKey)"/>.
			/// </summary>
			/// <param name="obj">Object to compare.</param>
			/// <returns><c>true</c> if the object is a matching key; otherwise <c>false</c>.</returns>
			public override bool Equals(object obj)
			{
				return obj is FacePairKey other && Equals(other);
			}

			/// <summary>
			/// Computes a hash code for use in dictionaries.
			/// </summary>
			/// <returns>Hash code built from both face reference hashes.</returns>
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

		/// <summary>
		/// Internal cache storage mapping face pairs to their occlusion result.
		/// </summary>
		private static readonly Dictionary<FacePairKey, bool> Collection
			= new Dictionary<FacePairKey, bool>();

		/// <summary>
		/// Gets the occlusion result for a face pair, using a cache.
		/// </summary>
		/// <param name="face">Face being tested.</param>
		/// <param name="occluder">Face potentially occluding <paramref name="face"/>.</param>
		/// <param name="isOccluded">The cached or computed occlusion result.</param>
		/// <returns>
		/// <c>true</c> if both input faces are non-null (result is set);
		/// otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// If the result is not in the cache, this method computes it via
		/// <see cref="Erelia.Core.VoxelKit.Face.IsOccludedBy"/> and stores it.
		/// </remarks>
		public static bool TryGetValue(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.Face occluder, out bool isOccluded)
		{
			// Default output.
			isOccluded = false;

			// Cannot test occlusion without both faces.
			if (face == null || occluder == null)
			{
				return false;
			}

			// Cache lookup.
			var key = new FacePairKey(face, occluder);
			if (Collection.TryGetValue(key, out bool cached))
			{
				isOccluded = cached;
				return true;
			}

			// Cache miss: compute and store.
			isOccluded = face.IsOccludedBy(occluder);
			Collection[key] = isOccluded;

			return true;
		}
	}
}