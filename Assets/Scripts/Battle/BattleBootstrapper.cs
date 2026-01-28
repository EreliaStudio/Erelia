using UnityEngine;
using UnityEngine.InputSystem;

public class BattleBootstrapper : MonoBehaviour
{
	[SerializeField] private GameObject playerObject = null;
    [SerializeField] private PlayerInput battleInput = null;
    private readonly System.Collections.Generic.List<AudioListener> disabledListeners = new System.Collections.Generic.List<AudioListener>();
    private readonly System.Collections.Generic.List<Camera> disabledCameras = new System.Collections.Generic.List<Camera>();
    private readonly System.Collections.Generic.List<PlayerInput> disabledInputs = new System.Collections.Generic.List<PlayerInput>();

    private void Awake()
    {
        if (battleInput != null)
        {
            battleInput.enabled = false;
        }
    }

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null)
        {
            return;
        }

		playerObject.transform.position = request.PlayerWorldPosition;

        DisableOtherListeners();
        DisableOtherCameras();
        DisableOtherPlayerInputs();

        if (battleInput != null)
        {
            battleInput.enabled = true;
        }
    }

    private void OnDestroy()
    {
        RestoreListeners();
        RestoreCameras();
        RestorePlayerInputs();
    }

    private void DisableOtherListeners()
    {
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

    private void DisableOtherCameras()
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null)
            {
                continue;
            }

            if (camera.gameObject.scene == gameObject.scene)
            {
                continue;
            }

            if (camera.enabled)
            {
                camera.enabled = false;
                disabledCameras.Add(camera);
            }
        }
    }

    private void RestoreCameras()
    {
        for (int i = 0; i < disabledCameras.Count; i++)
        {
            Camera camera = disabledCameras[i];
            if (camera != null)
            {
                camera.enabled = true;
            }
        }

        disabledCameras.Clear();
    }

    private void DisableOtherPlayerInputs()
    {
        PlayerInput[] inputs = Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < inputs.Length; i++)
        {
            PlayerInput input = inputs[i];
            if (input == null)
            {
                continue;
            }

            if (input.gameObject.scene == gameObject.scene)
            {
                continue;
            }

            if (input.enabled)
            {
                input.enabled = false;
                disabledInputs.Add(input);
            }
        }
    }

    private void RestorePlayerInputs()
    {
        for (int i = 0; i < disabledInputs.Count; i++)
        {
            PlayerInput input = disabledInputs[i];
            if (input != null)
            {
                input.enabled = true;
            }
        }

        disabledInputs.Clear();
    }
}
