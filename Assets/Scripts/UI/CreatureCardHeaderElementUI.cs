using System;
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
		if (linkedCreatureUnit == null)
		{
			Apply(emptyCreatureIcon, emptyCreatureHeaderMessage);
			return;
		}

		CreatureForm activeForm = linkedCreatureUnit.GetForm();

		if (activeForm == null)
		{
			throw new InvalidOperationException("CreatureUnit.GetActiveForm returned null.");
		}

		if (activeForm.Icon == null)
		{
			throw new InvalidOperationException(
				$"Creature form [{activeForm.DisplayName}] has no Icon assigned.");
		}

		if (string.IsNullOrEmpty(activeForm.DisplayName))
		{
			throw new InvalidOperationException(
				$"Creature form on species [{linkedCreatureUnit.Species.name}] has an empty DisplayName.");
		}

		Apply(activeForm.Icon, activeForm.DisplayName);
	}

	private void Apply(Sprite p_icon, string p_displayName)
	{
		if (creatureIconImage != null)
		{
			creatureIconImage.sprite = p_icon;
			creatureIconImage.enabled = true;
		}

		if (creatureNameLabel != null)
		{
			creatureNameLabel.text = p_displayName;
		}
	}
}