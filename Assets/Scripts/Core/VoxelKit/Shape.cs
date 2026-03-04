using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Base class for voxel shapes.
	/// A shape is responsible for producing the geometric faces used for rendering and collision.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Canonical (non-oriented) definition:</b>
	/// This class represents the voxel in its default, non-oriented state.
	/// All geometry produced here must be considered:
	/// </para>
	/// <list type="bullet">
	///   <item>
	///     <description>
	///       Facing the <see cref="Orientation.PositiveX"/> direction.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       Not flipped (no <see cref="FlipOrientation"/> applied).
	///     </description>
	///   </item>
	/// </list>
	/// <para>
	/// Any rotation or flip applied at runtime must be performed as a transformation step
	/// on the faces produced by this shape. Implementations of <see cref="Shape"/> must
	/// therefore define their geometry in this canonical reference orientation only.
	/// </para>
	/// 
	/// <para>
	/// The produced geometry is split into two "channels":
	/// </para>
	/// <list type="bullet">
	///   <item><description><see cref="RenderFaces"/>: faces used to build the render mesh.</description></item>
	///   <item><description><see cref="CollisionFaces"/>: faces used to build collision geometry.</description></item>
	/// </list>
	/// 
	/// <para>
	/// Faces are split into two groups inside a <see cref="FaceSet"/>:
	/// </para>
	/// <list type="bullet">
	///   <item>
	///     <description>
	///       <b>Outer shell</b>:
	///       faces that lie on the unit-cube boundary planes (x=0/1, y=0/1, z=0/1) and are keyed by
	///       <see cref="AxisPlane"/> in <see cref="FaceSet.OuterShell"/>.
	///       These faces can be directly occluded by neighboring voxels and are typically
	///       used for face culling, visibility tests, and stitching with adjacent blocks.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       <b>Inner</b>:
	///       faces that are not part of the axis-aligned boundary surface (e.g. internal polygons, bevels,
	///       sloped/diagonal surfaces, cavities). They are stored in <see cref="FaceSet.Inner"/> and are not
	///       associated with a single <see cref="AxisPlane"/>.
	///       These faces are generally not removed by neighbor-based culling and must be handled by
	///       shape-specific occlusion rules (or always rendered/used for collision).
	///     </description>
	///   </item>
	/// </list>
	/// 
	/// <para>
	/// In short:
	/// <b>Outer shell</b> = boundary faces of the voxel that match one of the 6 axis planes.
	/// <b>Inner</b> = everything else (non-axis-aligned and/or internal surfaces).
	/// </para>
	/// </remarks>
	[Serializable]
	public abstract class Shape
	{
		/// <summary>
		/// Groups faces for a given usage (render or collision), split into:
		/// <list type="bullet">
		/// <item><description><see cref="Inner"/>: faces inside the shape (typically not exposed to neighbors).</description></item>
		/// <item><description><see cref="OuterShell"/>: faces on the voxel outer planes (used for culling/occlusion).</description></item>
		/// </list>
		/// </summary>
		[Serializable]
		public struct FaceSet
		{
			/// <summary>
			/// Faces considered "inner" geometry for the shape.
			/// </summary>
			public List<Erelia.Core.VoxelKit.Face> Inner;

			/// <summary>
			/// Faces forming the outer shell of the shape, keyed by the axis plane they lie on (Relative to a shape non-oriented).
			/// </summary>
			public Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face> OuterShell;

			/// <summary>
			/// Initializes a new <see cref="FaceSet"/> with provided containers.
			/// Null containers are replaced by empty ones.
			/// </summary>
			/// <param name="inner">Inner faces list, or null to create an empty list.</param>
			/// <param name="outerShell">Outer shell dictionary, or null to create an empty dictionary.</param>
			public FaceSet(
				List<Erelia.Core.VoxelKit.Face> inner,
				Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face> outerShell)
			{
				// Ensure containers are always non-null to simplify consumers.
				Inner = inner ?? new List<Erelia.Core.VoxelKit.Face>();
				OuterShell = outerShell ?? new Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face>();
			}
		}

		/// <summary>
		/// Holds both render and collision face sets.
		/// </summary>
		[Serializable]
		public struct Channels
		{
			/// <summary>
			/// Faces used to generate render meshes.
			/// </summary>
			public FaceSet Render;

			/// <summary>
			/// Faces used to generate collision geometry.
			/// </summary>
			public FaceSet Collision;

			/// <summary>
			/// Initializes a new <see cref="Channels"/> value.
			/// </summary>
			/// <param name="render">Render face set.</param>
			/// <param name="collision">Collision face set.</param>
			public Channels(FaceSet render, FaceSet collision)
			{
				Render = render;
				Collision = collision;
			}
		}

		/// <summary>
		/// Cached channels generated by <see cref="Initialize"/>.
		/// </summary>
		private Channels channels;

		/// <summary>
		/// Gets the face set used for rendering.
		/// </summary>
		public FaceSet RenderFaces => channels.Render;

		/// <summary>
		/// Gets the face set used for collision.
		/// </summary>
		public FaceSet CollisionFaces => channels.Collision;

		/// <summary>
		/// Constructs the faces used for rendering.
		/// </summary>
		/// <returns>A face set describing render geometry.</returns>
		/// <remarks>
		/// Implementations must override this method, as rendering geometry is shape-specific.
		/// </remarks>
		protected abstract FaceSet ConstructRenderFaces();

		/// <summary>
		/// Constructs the faces used for collision.
		/// </summary>
		/// <returns>A face set describing collision geometry.</returns>
		/// <remarks>
		/// Default behavior reuses render faces for collision. Override if collision should be simplified
		/// or differ from rendering geometry.
		/// </remarks>
		protected virtual FaceSet ConstructCollisionFaces()
		{
			// By default, collision matches render geometry.
			return ConstructRenderFaces();
		}

		/// <summary>
		/// Initializes the shape by constructing and caching render/collision face sets.
		/// </summary>
		/// <remarks>
		/// Call this once after creating the shape instance (or after changing parameters affecting geometry),
		/// so <see cref="RenderFaces"/> and <see cref="CollisionFaces"/> return valid data.
		/// In a normal program execution, this is called by the <see cref="VoxelKit.Definition"/> class, uppon constructing itself
		/// </remarks>
		public virtual void Initialize()
		{
			// Build both channels and store them in the cached 'channels' field.
			channels = new Channels(
				ConstructRenderFaces(),
				ConstructCollisionFaces());
		}
	}
}