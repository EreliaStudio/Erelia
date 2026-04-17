using UnityEngine;

public abstract class Mode : MonoBehaviour
{
	private const bool DebugLogging = false;

	public abstract ModeKind Kind { get; }
	public bool IsActive => gameObject.activeSelf;

	public void Enter(ModeContext context = null)
	{
		gameObject.SetActive(true);
		OnEnter(context ?? ModeContext.Empty);
		LogDebug($"{Kind} mode entered.");
	}

	public void Exit(ModeContext context = null)
	{
		OnExit(context ?? ModeContext.Empty);
		gameObject.SetActive(false);
		LogDebug($"{Kind} mode exited.");
	}

	protected virtual void OnEnter(ModeContext context)
	{
	}

	protected virtual void OnExit(ModeContext context)
	{
	}

	protected void LogDebug(string message)
	{
		if (!DebugLogging)
		{
			return;
		}

		Debug.Log($"[{GetType().Name}] {message}", this);
	}
}
