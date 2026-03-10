using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Timeline
{
	/// <summary>
	/// Runtime battle timeline that advances countdown stamina and reports ready combatants.
	/// </summary>
	[System.Serializable]
	public sealed class Model
	{
		private readonly List<Erelia.Battle.Unit.Presenter> units = new List<Erelia.Battle.Unit.Presenter>();
		private readonly List<Erelia.Battle.Unit.Presenter> aliveUnits = new List<Erelia.Battle.Unit.Presenter>();

		public IReadOnlyList<Erelia.Battle.Unit.Presenter> Units => units;

		public void SetUnits(IEnumerable<Erelia.Battle.Unit.Presenter> presenters)
		{
			units.Clear();
			if (presenters == null)
			{
				return;
			}

			foreach (Erelia.Battle.Unit.Presenter presenter in presenters)
			{
				if (presenter != null)
				{
					units.Add(presenter);
				}
			}
		}

		public bool Tick(float deltaTime, List<Erelia.Battle.Unit.Presenter> readyUnits)
		{
			readyUnits?.Clear();

			aliveUnits.Clear();
			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter presenter = units[i];
				Erelia.Battle.Unit.Model model = presenter?.Model;
				if (model == null || !model.IsAlive || !model.HasCell)
				{
					continue;
				}

				aliveUnits.Add(presenter);
			}

			if (aliveUnits.Count == 0)
			{
				return false;
			}

			aliveUnits.Sort(CompareUnitsByStamina);

			float clampedDeltaTime = Mathf.Max(0f, deltaTime);
			Erelia.Battle.Unit.Model firstModel = aliveUnits[0].Model;
			float nextReadyIn = Mathf.Max(0f, firstModel.CurrentStamina);
			float elapsedTime = Mathf.Min(clampedDeltaTime, nextReadyIn);

			if (elapsedTime > 0f)
			{
				for (int i = 0; i < aliveUnits.Count; i++)
				{
					aliveUnits[i].TickStamina(elapsedTime);
				}
			}

			if (nextReadyIn > clampedDeltaTime)
			{
				return false;
			}

			bool hasReadyUnits = false;
			for (int i = 0; i < aliveUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter presenter = aliveUnits[i];
				if (presenter.Model.IsReady)
				{
					readyUnits?.Add(presenter);
					hasReadyUnits = true;
				}
			}

			return hasReadyUnits;
		}

		private static int CompareUnitsByStamina(Erelia.Battle.Unit.Presenter left, Erelia.Battle.Unit.Presenter right)
		{
			float leftValue = left != null && left.Model != null ? left.Model.CurrentStamina : 0f;
			float rightValue = right != null && right.Model != null ? right.Model.CurrentStamina : 0f;
			return leftValue.CompareTo(rightValue);
		}
	}
}
