using UnityEngine;

namespace Erelia.Core
{
	public abstract class SingletonRegistry<TRegistryType> : ScriptableObject
		where TRegistryType : SingletonRegistry<TRegistryType>
	{
		private static TRegistryType instance;

		protected abstract string ResourcePath { get; }

		protected virtual void Rebuild()
		{
		}

		protected virtual void OnEnable()
		{
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

		protected virtual void OnDisable()
		{
			if (instance == this)
			{
				instance = null;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (instance == null || instance == this)
			{
				Rebuild();
			}
		}
#endif

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