using UnityEngine;

public abstract class Mode : MonoBehaviour
{
	[SerializeField] private GameObject root;
	[SerializeField] private bool debugLogging;

	public abstract ModeKind Kind { get; }
	public GameObject Root => root;
	public bool IsActive => root != null && root.activeSelf;

	protected virtual void Reset()
	{
		if (root == null)
		{
			root = gameObject;
		}
	}

	public void Enter(ModeContext context = null)
	{
		SetRootActive(true);
		OnEnter(context ?? ModeContext.Empty);
		LogDebug($"{Kind} mode entered.");
	}

	public void Exit(ModeContext context = null)
	{
		OnExit(context ?? ModeContext.Empty);
		SetRootActive(false);
		LogDebug($"{Kind} mode exited.");
	}

	protected virtual void OnEnter(ModeContext context)
	{
	}

	protected virtual void OnExit(ModeContext context)
	{
	}

	protected void SetRoot(GameObject targetRoot)
	{
		root = targetRoot;
	}

	protected void LogDebug(string message)
	{
		if (!debugLogging)
		{
			return;
		}

		Debug.Log($"[{GetType().Name}] {message}", this);
	}

	private void SetRootActive(bool value)
	{
		if (root != null)
		{
			root.SetActive(value);
		}
	}
}
