using UnityEngine;

public abstract class Mode : MonoBehaviour
{
	public bool IsActive => gameObject.activeSelf;

	public void Enter()
	{
		gameObject.SetActive(true);
		OnEnter();
	}

	public void Exit()
	{
		OnExit();
		gameObject.SetActive(false);
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnExit()
	{
	}
}