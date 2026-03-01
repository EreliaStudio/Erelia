using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private View view;

		private Model model;

		public Model Model => model;

		public void SetModel(Model newModel)
		{
			if (newModel == null)
			{
				throw new System.ArgumentNullException(nameof(newModel), "[Erelia.Core.Creature.Instance.Presenter] Model cannot be null.");
			}

			model = newModel;
		}

		private void Awake()
		{
			if (view == null)
			{
				Debug.LogWarning("[Erelia.Core.Creature.Instance.Presenter] View is not assigned.");
			}
		}
	}
}
