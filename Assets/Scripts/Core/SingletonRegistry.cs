using UnityEngine;

namespace Erelia.Core
{
	public abstract class SingletonRegistry<T> : ScriptableObject where T : SingletonRegistry<T>
	{
		private static T instance;

		protected abstract string ResourcePath { get; }

		protected virtual void Rebuild()
		{
		}

		protected virtual void OnEnable()
		{
			if (instance == null)
			{
				instance = (T)this;
			}

			Rebuild();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			Rebuild();
		}
#endif

		public static T Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				string path = GetResourcePathWithoutAsset();
				instance = Resources.Load<T>(path);

				if (instance == null)
				{
					Debug.LogWarning($"{typeof(T).Name} not found at Resources/{path}.asset");
					return null;
				}

				instance.Rebuild();
				return instance;
			}
		}

		private static string GetResourcePathWithoutAsset()
		{
			T tmp = ScriptableObject.CreateInstance<T>();
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