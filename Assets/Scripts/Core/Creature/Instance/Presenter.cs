using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	/// <summary>
	/// Unity component responsible for holding and exposing a creature instance <see cref="Erelia.Core.Creature.Instance.Model"/>
	/// and linking it to a scene <see cref="View"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This presenter acts as the runtime “owner” of the model for a creature GameObject. It is typically used by
	/// spawning/initialization systems to assign a <see cref="Model"/> and then let the <see cref="View"/> read it
	/// to display visuals (3D models, name, HP, level, etc.).
	/// </para>
	/// </remarks>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// Scene view component associated with this creature instance.
		/// </summary>
		/// <remarks>
		/// Typically assigned in the inspector. The presenter can later use it to apply model data to visuals.
		/// </remarks>
		[SerializeField] private View view;

		/// <summary>
		/// Backing field for the current creature model.
		/// </summary>
		private Model model;

		/// <summary>
		/// Gets the currently assigned creature instance model.
		/// </summary>
		public Model Model => model;

		/// <summary>
		/// Assigns the model used by this presenter.
		/// </summary>
		/// <param name="newModel">Model instance to assign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown when <paramref name="newModel"/> is <c>null</c>.
		/// </exception>
		public void SetModel(Model newModel)
		{
			// Enforce a non-null model: the creature instance must always have valid data.
			if (newModel == null)
			{
				throw new System.ArgumentNullException(nameof(newModel), "[Erelia.Core.Creature.Instance.Presenter] Model cannot be null.");
			}

			// Store the model reference for later use.
			model = newModel;

			// TODO : update the view when changing the model
		}

		/// <summary>
		/// Unity callback invoked when the component is initialized.
		/// </summary>
		/// <remarks>
		/// Validates that the <see cref="view"/> reference is assigned in the inspector.
		/// </remarks>
		private void Awake()
		{
			// Warn if the view is missing so the prefab/scene setup issue is visible early.
			if (view == null)
			{
				Debug.LogWarning("[Erelia.Core.Creature.Instance.Presenter] View is not assigned.");
			}
		}
	}
}