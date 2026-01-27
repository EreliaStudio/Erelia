using UnityEngine;

public class BattleBootstrapper : MonoBehaviour
{
    private readonly System.Collections.Generic.List<AudioListener> disabledListeners = new System.Collections.Generic.List<AudioListener>();

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null)
        {
            return;
        }

        DisableOtherListeners();
    }

    private void OnDestroy()
    {
        RestoreListeners();
    }

    private void DisableOtherListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>(true);
        AudioListener keep = GetComponentInChildren<AudioListener>(true);

        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];
            if (listener == null)
            {
                continue;
            }

            if (keep != null && listener == keep)
            {
                continue;
            }

            if (listener.enabled)
            {
                listener.enabled = false;
                disabledListeners.Add(listener);
            }
        }
    }

    private void RestoreListeners()
    {
        for (int i = 0; i < disabledListeners.Count; i++)
        {
            AudioListener listener = disabledListeners[i];
            if (listener != null)
            {
                listener.enabled = true;
            }
        }

        disabledListeners.Clear();
    }
}
