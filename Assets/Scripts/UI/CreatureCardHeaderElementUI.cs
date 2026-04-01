using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureCardHeaderElementUI : MonoBehaviour
{
	[SerializeField] private Image creatureIconImage;
	[SerializeField] private TMP_Text creatureNameLabel;

	[SerializeField] private Sprite emptyCreatureIcon;
	[SerializeField] private string emptyCreatureHeaderMessage = "-----";

	private CreatureUnit linkedCreatureUnit;

	public void Bind(CreatureUnit p_creatureUnit)
	{
		linkedCreatureUnit = p_creatureUnit;
		Refresh();
	}

	public void Clear()
	{
		linkedCreatureUnit = null;
		Refresh();
	}

	public void Refresh()
	{
		if (linkedCreatureUnit == null || linkedCreatureUnit.Species == null)
		{
			Apply(emptyCreatureIcon, emptyCreatureHeaderMessage);
			return;
		}

		CreatureForm creatureForm = linkedCreatureUnit.GetForm();

		if (creatureForm == null)
		{
			Apply(emptyCreatureIcon, emptyCreatureHeaderMessage);
			return;
		}

		Sprite icon = creatureForm.Icon != null ? creatureForm.Icon : emptyCreatureIcon;
		string displayName = string.IsNullOrWhiteSpace(creatureForm.DisplayName)
			? emptyCreatureHeaderMessage
			: creatureForm.DisplayName;

		Apply(icon, displayName);
	}

	private void Apply(Sprite p_icon, string p_displayName)
	{
		if (creatureIconImage != null)
		{
			creatureIconImage.sprite = p_icon;
			creatureIconImage.enabled = p_icon != null;
		}

		if (creatureNameLabel != null)
		{
			creatureNameLabel.text = p_displayName;
		}
	}
}