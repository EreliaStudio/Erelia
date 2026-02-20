using System.Collections.Generic;
using UnityEngine;

namespace Erelia.World
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.World.View worldView;

		private Erelia.World.Model worldModel;
		private readonly Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter> presenters =
			new Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter>();

		private void Awake()
		{
			Erelia.Logger.Log("[Erelia.World.Presenter] Awake - initializing world model.");
			worldModel = new Erelia.World.Model();
		}

		public Erelia.World.Chunk.Model CreateChunk(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (worldModel == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] World model was null. Recreating.");
				worldModel = new Erelia.World.Model();
			}

			Erelia.World.Chunk.Model model = worldModel.GetOrCreateChunk(coordinates);

			if (presenters.ContainsKey(coordinates))
			{
				Erelia.Logger.Log("[Erelia.World.Presenter] Chunk presenter already exists for coordinates " + coordinates + ".");
				return model;
			}

			Erelia.World.Chunk.View view = worldView != null ? worldView.CreateChunkView(coordinates) : null;
			if (worldView == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] World view is not assigned. Chunk view will be null for " + coordinates + ".");
			}
			else if (view == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] Chunk view could not be created for " + coordinates + ".");
			}
			var presenter = new Erelia.World.Chunk.Presenter(model, view);
			presenter.Bind();
			presenter.ForceRebuild();

			presenters.Add(coordinates, presenter);
			Erelia.Logger.Log("[Erelia.World.Presenter] Chunk presenter created for " + coordinates + ".");
			return model;
		}

		private void OnDestroy()
		{
			Erelia.Logger.Log("[Erelia.World.Presenter] OnDestroy - unbinding chunk presenters.");
			foreach (var pair in presenters)
			{
				pair.Value.Unbind();
			}
			presenters.Clear();
		}
	}
}


