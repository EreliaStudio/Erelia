using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Phase
{
	public class Manager : MonoBehaviour
	{
		private Battle.Phase.AbstractPhase activePhase = null;

		private Dictionary<Battle.Phase.Mode, Battle.Phase.AbstractPhase> phaseDictionary = new Dictionary<Battle.Phase.Mode, Battle.Phase.AbstractPhase>();

		void Awake()
		{
			phaseDictionary[Mode.Initialization] = new InitializationPhase();
			phaseDictionary[Mode.Placement] = new PlacementPhase();

			SetPhase(Mode.Initialization);
		}

		public void SetPhase(Battle.Phase.Mode mode)
		{
			if (activePhase != null)
			{
				activePhase.OnExit();
			}

			if (phaseDictionary.TryGetValue(mode, out AbstractPhase nextPhase) == false)
			{
				Debug.LogError($"Phase not registered: {mode}");
				activePhase = null;
				return;
			}

			Debug.Log("Set phase to [" + mode + "]");
			activePhase = nextPhase;

			if (activePhase != null)
			{
				activePhase.SetManager(this);
				activePhase.OnEnter();
			}
		}

		void Update()
		{
			if (activePhase != null)
			{
				activePhase.OnUpdate();
			}
		}
	}
}