using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BattlePhaseController : MonoBehaviour
{
	protected BattleOrchestrator Orchestrator { get; private set; }
	protected BattleMode BattleMode => Orchestrator?.BattleMode;
	protected BattleContext BattleContext => Orchestrator?.BattleContext;
	protected TurnContext TurnContext => BattleContext?.CurrentTurn;
	protected BattleCoordinator Coordinator => Orchestrator?.Coordinator;

	private InputAction confirmAction;
	private InputAction cancelAction;
	private bool isActive;

	public abstract BattlePhaseType PhaseType { get; }

	public void Bind(BattleOrchestrator orchestrator)
	{
		Orchestrator = orchestrator;
		OnBind();
	}

	public void ConfigureInput(InputAction confirm, InputAction cancel)
	{
		UnsubscribeInputCallbacks();
		confirmAction = confirm;
		cancelAction = cancel;

		if (isActive)
		{
			SubscribeInputCallbacks();
		}
	}

	public virtual void SetActive(bool isActive)
	{
		this.isActive = isActive;

		if (isActive)
		{
			SubscribeInputCallbacks();
		}
		else
		{
			UnsubscribeInputCallbacks();
		}

		gameObject.SetActive(isActive);
	}

	protected virtual void OnBind()
	{
	}

	protected virtual void OnConfirmAction(InputAction.CallbackContext context)
	{
	}

	protected virtual void OnCancelAction(InputAction.CallbackContext context)
	{
	}

	protected virtual void OnConfirmCanceled(InputAction.CallbackContext context)
	{
	}

	protected virtual void OnCancelCanceled(InputAction.CallbackContext context)
	{
	}

	private void HandleConfirmPerformed(InputAction.CallbackContext context)
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction(context))
		{
			OnConfirmCanceled(context);
			return;
		}

		OnConfirmAction(context);
	}

	private void HandleCancelPerformed(InputAction.CallbackContext context)
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction(context))
		{
			OnCancelCanceled(context);
			return;
		}

		OnCancelAction(context);
	}

	private void SubscribeInputCallbacks()
	{
		if (confirmAction != null)
		{
			confirmAction.performed -= HandleConfirmPerformed;
			confirmAction.performed += HandleConfirmPerformed;
			EnableAction(confirmAction);
		}

		if (cancelAction != null)
		{
			cancelAction.performed -= HandleCancelPerformed;
			cancelAction.performed += HandleCancelPerformed;
			EnableAction(cancelAction);
		}
	}

	private void UnsubscribeInputCallbacks()
	{
		if (confirmAction != null)
		{
			confirmAction.performed -= HandleConfirmPerformed;
			DisableAction(confirmAction);
		}

		if (cancelAction != null)
		{
			cancelAction.performed -= HandleCancelPerformed;
			DisableAction(cancelAction);
		}
	}

	private static void EnableAction(InputAction action)
	{
		if (action != null && !action.enabled)
		{
			action.Enable();
		}
	}

	private static void DisableAction(InputAction action)
	{
		if (action != null && action.enabled)
		{
			action.Disable();
		}
	}
}
