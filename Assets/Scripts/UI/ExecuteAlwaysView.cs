using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ExecuteAlwaysView : MonoBehaviour
{
#if UNITY_EDITOR
	private bool editorRefreshQueued;

	protected void QueueEditorRefresh()
	{
		if (editorRefreshQueued)
		{
			return;
		}

		editorRefreshQueued = true;
		EditorApplication.delayCall += RunEditorRefresh;
	}

	private void RunEditorRefresh()
	{
		editorRefreshQueued = false;

		if (this == null)
		{
			return;
		}

		OnEditorRefresh();
	}

	protected virtual void OnEditorRefresh() { }
#endif
}
