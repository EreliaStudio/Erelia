using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public sealed class BattlePhaseInputRouter : IDisposable
{
	[SerializeField] private InputActionReference confirmAction;
	[SerializeField] private InputActionReference cancelAction;
	[SerializeField] private InputActionReference selectAbility1Action;
	[SerializeField] private InputActionReference selectAbility2Action;
	[SerializeField] private InputActionReference selectAbility3Action;
	[SerializeField] private InputActionReference selectAbility4Action;
	[SerializeField] private InputActionReference selectAbility5Action;
	[SerializeField] private InputActionReference selectAbility6Action;
	[SerializeField] private InputActionReference selectAbility7Action;
	[SerializeField] private InputActionReference selectAbility8Action;

	private readonly Action<InputAction.CallbackContext> shortcut0Callback;
	private readonly Action<InputAction.CallbackContext> shortcut1Callback;
	private readonly Action<InputAction.CallbackContext> shortcut2Callback;
	private readonly Action<InputAction.CallbackContext> shortcut3Callback;
	private readonly Action<InputAction.CallbackContext> shortcut4Callback;
	private readonly Action<InputAction.CallbackContext> shortcut5Callback;
	private readonly Action<InputAction.CallbackContext> shortcut6Callback;
	private readonly Action<InputAction.CallbackContext> shortcut7Callback;

	[NonSerialized] private BattleOrchestrator orchestrator;

	public BattlePhaseInputRouter()
	{
		shortcut0Callback = _ => HandleAbilityShortcutPerformed(0);
		shortcut1Callback = _ => HandleAbilityShortcutPerformed(1);
		shortcut2Callback = _ => HandleAbilityShortcutPerformed(2);
		shortcut3Callback = _ => HandleAbilityShortcutPerformed(3);
		shortcut4Callback = _ => HandleAbilityShortcutPerformed(4);
		shortcut5Callback = _ => HandleAbilityShortcutPerformed(5);
		shortcut6Callback = _ => HandleAbilityShortcutPerformed(6);
		shortcut7Callback = _ => HandleAbilityShortcutPerformed(7);
	}

	public void Validate(UnityEngine.Object context)
	{
		if (confirmAction == null)
			Logger.LogError("BattlePhaseInputRouter: confirmAction is not assigned.", Logger.Severity.Critical, context);
		if (cancelAction == null)
			Logger.LogError("BattlePhaseInputRouter: cancelAction is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility1Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility1Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility2Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility2Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility3Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility3Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility4Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility4Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility5Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility5Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility6Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility6Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility7Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility7Action is not assigned.", Logger.Severity.Critical, context);
		if (selectAbility8Action == null)
			Logger.LogError("BattlePhaseInputRouter: selectAbility8Action is not assigned.", Logger.Severity.Critical, context);
	}

	public void Configure(BattleOrchestrator battleOrchestrator)
	{
		Dispose();
		orchestrator = battleOrchestrator;
		Subscribe();
	}

	public void Dispose()
	{
		Unsubscribe();
		orchestrator = null;
	}

	private void Subscribe()
	{
		SubscribeAction(confirmAction, HandleConfirmPerformed);
		SubscribeAction(cancelAction, HandleCancelPerformed);
		SubscribeAction(selectAbility1Action, shortcut0Callback);
		SubscribeAction(selectAbility2Action, shortcut1Callback);
		SubscribeAction(selectAbility3Action, shortcut2Callback);
		SubscribeAction(selectAbility4Action, shortcut3Callback);
		SubscribeAction(selectAbility5Action, shortcut4Callback);
		SubscribeAction(selectAbility6Action, shortcut5Callback);
		SubscribeAction(selectAbility7Action, shortcut6Callback);
		SubscribeAction(selectAbility8Action, shortcut7Callback);
	}

	private void Unsubscribe()
	{
		UnsubscribeAction(confirmAction, HandleConfirmPerformed);
		UnsubscribeAction(cancelAction, HandleCancelPerformed);
		UnsubscribeAction(selectAbility1Action, shortcut0Callback);
		UnsubscribeAction(selectAbility2Action, shortcut1Callback);
		UnsubscribeAction(selectAbility3Action, shortcut2Callback);
		UnsubscribeAction(selectAbility4Action, shortcut3Callback);
		UnsubscribeAction(selectAbility5Action, shortcut4Callback);
		UnsubscribeAction(selectAbility6Action, shortcut5Callback);
		UnsubscribeAction(selectAbility7Action, shortcut6Callback);
		UnsubscribeAction(selectAbility8Action, shortcut7Callback);
	}

	private static void SubscribeAction(InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
	{
		if (actionRef == null) return;
		InputAction action = actionRef.action;
		action.performed -= callback;
		action.performed += callback;
		if (!action.enabled) action.Enable();
	}

	private static void UnsubscribeAction(InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
	{
		if (actionRef == null) return;
		InputAction action = actionRef.action;
		action.performed -= callback;
		if (action.enabled) action.Disable();
	}

	private void HandleConfirmPerformed(InputAction.CallbackContext context)
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction(context)) return;
		if (TryGetActiveInputHandler(out IBattlePhaseInputHandler handler))
		{
			handler.Confirm();
		}
	}

	private void HandleCancelPerformed(InputAction.CallbackContext context)
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction(context)) return;
		if (TryGetActiveInputHandler(out IBattlePhaseInputHandler handler))
		{
			handler.Cancel();
		}
	}

	private void HandleAbilityShortcutPerformed(int shortcutIndex)
	{
		if (orchestrator == null ||
			!orchestrator.TryGetActiveController(out BattlePhaseController controller) ||
			controller is not IBattlePhaseAbilityShortcutHandler handler)
		{
			return;
		}

		handler.SelectAbilityShortcut(shortcutIndex);
	}

	private bool TryGetActiveInputHandler(out IBattlePhaseInputHandler handler)
	{
		handler = null;
		if (orchestrator == null || !orchestrator.TryGetActiveController(out BattlePhaseController controller))
		{
			return false;
		}

		handler = controller as IBattlePhaseInputHandler;
		return handler != null;
	}
}
