using UnityEngine;

namespace Erelia.Core
{
	/// <summary>
	/// Base class for registries implemented as Unity <see cref="ScriptableObject"/> singletons loaded from the
	/// <c>Resources</c> folder.
	/// </summary>
	/// <typeparam name="TRegistryType">
	/// Concrete registry type using the CRTP pattern (i.e. <c>MyRegistry : SingletonRegistry&lt;MyRegistry&gt;</c>).
	/// </typeparam>
	/// <remarks>
	/// <para>
	/// This pattern provides a global <see cref="Instance"/> accessor that:
	/// <list type="number">
	/// <item><description>Returns the cached instance when available.</description></item>
	/// <item><description>Otherwise loads the asset from <c>Resources</c> using the path returned by <see cref="ResourcePath"/>.</description></item>
	/// <item><description>Calls <see cref="Rebuild"/> after loading (and when the asset is enabled/validated).</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The registry asset must be located under a <c>Resources</c> folder. <see cref="ResourcePath"/> must be the
	/// Resources-relative path <b>without</b> the <c>.asset</c> extension
	/// (e.g. <c>"Voxel/VoxelRegistry"</c> for <c>"Assets/Resources/Voxel/VoxelRegistry.asset"</c>).
	/// </para>
	/// <para>
	/// Duplicate assets of the same registry type are detected at runtime: the first enabled instance wins and
	/// later enabled duplicates are ignored (they will not call <see cref="Rebuild"/>).
	/// </para>
	/// <para>
	/// This singleton is not thread-safe and assumes Unity main-thread usage.
	/// </para>
	/// </remarks>
	public abstract class SingletonRegistry<TRegistryType> : ScriptableObject
		where TRegistryType : SingletonRegistry<TRegistryType>
	{
		/// <summary>
		/// Cached singleton instance.
		/// </summary>
		private static TRegistryType instance;

		/// <summary>
		/// Resources-relative path to the registry asset, without the <c>.asset</c> extension.
		/// </summary>
		/// <remarks>
		/// Implement this in derived registries to indicate where the registry asset is located in a <c>Resources</c> folder.
		/// <para>
		/// Example: if the asset is at <c>Assets/Resources/Registries/MyRegistry.asset</c>, return <c>"Registries/MyRegistry"</c>.
		/// </para>
		/// </remarks>
		protected abstract string ResourcePath { get; }

		/// <summary>
		/// Rebuilds any runtime lookup structures derived from serialized fields.
		/// </summary>
		/// <remarks>
		/// Override this to populate dictionaries, validate entries, compute caches, etc.
		/// Called from <see cref="OnEnable"/> and after <see cref="Instance"/> loads the asset.
		/// In the editor, it may also be called by <see cref="OnValidate"/> on the active instance.
		/// </remarks>
		protected virtual void Rebuild()
		{
			// Default: nothing to rebuild.
		}

		/// <summary>
		/// Unity callback invoked when the ScriptableObject is loaded/enabled.
		/// </summary>
		/// <remarks>
		/// The first enabled instance becomes the singleton. Later enabled duplicates are ignored.
		/// </remarks>
		protected virtual void OnEnable()
		{
			// First instance wins. Ignore duplicates to keep behavior deterministic.
			if (instance != null && instance != this)
			{
				Debug.LogError(
					$"Duplicate {typeof(TRegistryType).Name} detected. Keeping '{instance.name}', ignoring '{name}'.",
					this);

				return;
			}

			instance = (TRegistryType)this;
			Rebuild();
		}

		/// <summary>
		/// Unity callback invoked when the ScriptableObject is disabled/unloaded.
		/// </summary>
		/// <remarks>
		/// Clears the singleton reference if this instance is the active one.
		/// </remarks>
		protected virtual void OnDisable()
		{
			if (instance == this)
			{
				instance = null;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Unity editor callback invoked when values are changed in the inspector.
		/// </summary>
		/// <remarks>
		/// Rebuilds runtime lookup structures for the active singleton instance only.
		/// </remarks>
		protected virtual void OnValidate()
		{
			// Avoid rebuilding duplicates; keep editor behavior consistent with runtime behavior.
			if (instance == null || instance == this)
			{
				Rebuild();
			}
		}
#endif

		/// <summary>
		/// Gets the singleton instance of the registry.
		/// </summary>
		/// <remarks>
		/// If no instance is cached yet, this loads it from <c>Resources</c> using <see cref="ResourcePath"/>,
		/// then calls <see cref="Rebuild"/> before returning it.
		/// </remarks>
		public static TRegistryType Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				string path = GetResourcePathWithoutAsset();
				instance = Resources.Load<TRegistryType>(path);

				if (instance == null)
				{
					Debug.LogWarning($"{typeof(TRegistryType).Name} not found at Resources/{path}.asset");
					return null;
				}

				instance.Rebuild();
				return instance;
			}
		}

		/// <summary>
		/// Resolves the Resources-relative path for <typeparamref name="TRegistryType"/> without requiring an existing asset instance.
		/// </summary>
		/// <returns>The Resources-relative path without the <c>.asset</c> extension.</returns>
		/// <remarks>
		/// This uses a temporary <see cref="ScriptableObject"/> instance solely to read <see cref="ResourcePath"/>.
		/// Keep <see cref="ResourcePath"/> implementation free of dependencies on serialized data.
		/// </remarks>
		private static string GetResourcePathWithoutAsset()
		{
			TRegistryType tmp = ScriptableObject.CreateInstance<TRegistryType>();
			string path = tmp.ResourcePath;

#if UNITY_EDITOR
			Object.DestroyImmediate(tmp);
#else
			Object.Destroy(tmp);
#endif

			return path;
		}
	}
}