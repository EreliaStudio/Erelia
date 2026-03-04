using System;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Defines a voxel "type" as a Unity asset (ScriptableObject).
	/// </summary>
	/// <remarks>
	/// A <see cref="Definition"/> groups:
	/// <list type="bullet">
	/// <item><description><see cref="Data"/>: generic voxel parameters (materials, textures, flags, etc.).</description></item>
	/// <item><description><see cref="Type"/>: a high-level shape category used for authoring/selection. This is used only by the unity editor
	/// to be able to output the correct informations corresponding to the shape type selected</description></item>
	/// <item><description><see cref="Shape"/>: the actual shape instance that generates render/collision faces.</description></item>
	/// </list>
	/// The asset auto-initializes its shape when enabled (and when edited in the Unity Editor).
	/// </remarks>
	[CreateAssetMenu(menuName = "Voxel/Definition", fileName = "NewVoxelDefinition")]
	public class Definition : ScriptableObject
	{
		/// <summary>
		/// Enumerates the supported voxel shape categories for this definition.
		/// </summary>
		public enum ShapeType
		{
			/// <summary>Full cube voxel.</summary>
			Cube,

			/// <summary>Half-height voxel (or similar slab variant).</summary>
			Slab,

			/// <summary>Ramp-like voxel shape (slope).</summary>
			Slope,

			/// <summary>Stair voxel shape.</summary>
			Stair,

			/// <summary>Two crossed planes (commonly used for plants/foliage).</summary>
			CrossPlane
		}

		/// <summary>
		/// Serialized voxel data associated with this definition.
		/// </summary>
		[SerializeField] private Erelia.Core.VoxelKit.Data data = new Erelia.Core.VoxelKit.Data();

		/// <summary>
		/// The selected high-level shape type for this voxel.
		/// </summary>
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		/// <summary>
		/// The concrete shape instance responsible for producing geometry for this voxel.
		/// </summary>
		/// <remarks>
		/// Stored as a managed reference so different <see cref="Erelia.Core.VoxelKit.Shape"/> implementations
		/// can be assigned polymorphically in the Inspector.
		/// </remarks>
		[SerializeReference] private Erelia.Core.VoxelKit.Shape shape = null;

		/// <summary>
		/// Gets the voxel data associated with this definition.
		/// </summary>
		public Erelia.Core.VoxelKit.Data Data => data;

		/// <summary>
		/// Gets the high-level shape category for this definition.
		/// </summary>
		public ShapeType Type => shapeType;

		/// <summary>
		/// Gets the concrete <see cref="Erelia.Core.VoxelKit.Shape"/> instance that generates geometry.
		/// </summary>
		public Erelia.Core.VoxelKit.Shape Shape => shape;

		/// <summary>
		/// Initializes this definition and its underlying shape.
		/// </summary>
		/// <remarks>
		/// Default implementation forwards initialization to the assigned <see cref="Shape"/> (if any).
		/// Override if a derived definition needs extra initialization logic.
		/// </remarks>
		protected virtual void Initialize()
		{
			// Ensure the shape has built and cached its render/collision faces.
			shape?.Initialize();
		}

		/// <summary>
		/// Unity callback invoked when the asset is loaded or enabled.
		/// </summary>
		private void OnEnable()
		{
			// Keep the definition ready-to-use at runtime.
			Initialize();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Unity Editor callback invoked when the asset is modified in the Inspector.
		/// </summary>
		private void OnValidate()
		{
			// Re-initialize so any authoring change is reflected immediately.
			Initialize();
		}
#endif
	}
}