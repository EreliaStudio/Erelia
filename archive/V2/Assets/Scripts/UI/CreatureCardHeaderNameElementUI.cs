using TMPro;
using UnityEngine;

public sealed class CreatureCardHeaderNameElementUI : MonoBehaviour
{
	[SerializeField] private TMP_Text label;
	[SerializeField] private string emptyMessage = "-----";

	private void Awake()
	{
		label ??= GetComponent<TMP_Text>();
	}

	public void Bind(CreatureUnit p_value)
	{
		label ??= GetComponent<TMP_Text>();

		if (p_value?.Species == null)
		{
			label.text = emptyMessage;
			return;
		}

		CreatureForm creatureForm = p_value.GetForm();
		label.text = string.IsNullOrWhiteSpace(creatureForm.DisplayName)
			? emptyMessage
			: creatureForm.DisplayName;
	}

	public void Clear()
	{
		label ??= GetComponent<TMP_Text>();
		label.text = emptyMessage;
	}
}
