using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class ObservableResourceBarElementUI : ObservableValue<ObservableResource>.Listener
{
	[SerializeField] private ProgressBarElementUI progressBarElementUI;
	[SerializeField] private string labelPrefix = string.Empty;

	private void Awake()
	{
		ResolveReferences();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		ResolveReferences();
	}
#endif

	protected override void ReactToEdition(ObservableResource p_value)
	{
		ResolveReferences();

		if (p_value == null)
		{
			progressBarElementUI.Clear();
			return;
		}

		progressBarElementUI.SetProgress(p_value.Ratio);
		progressBarElementUI.SetLabel(FormatLabel(p_value));
	}

	protected override void ClearRenderedValue()
	{
		ResolveReferences();
		progressBarElementUI.Clear();
	}

	private void ResolveReferences()
	{
		progressBarElementUI ??= GetComponent<ProgressBarElementUI>();
	}

	private string FormatLabel(ObservableResource p_resource)
	{
		if (string.IsNullOrWhiteSpace(labelPrefix))
		{
			return $"{p_resource.Current} / {p_resource.Max}";
		}

		return $"{labelPrefix} {p_resource.Current} / {p_resource.Max}";
	}
}
