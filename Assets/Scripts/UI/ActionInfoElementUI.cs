using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActionInfoElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;
	[SerializeField] private TMP_Text actionNameLabel;
	[SerializeField] private TMP_Text costLabel;
	[SerializeField] private TMP_Text rangeLabel;
	[SerializeField] private TMP_Text areaOfEffectLabel;
	[SerializeField] private TMP_Text lineOfSightLabel;
	[SerializeField] private TMP_Text descriptionLabel;
	[SerializeField] private string emptyMessage = "-----";

	private void Awake()
	{
		AutoResolveReferences();
		Clear();
	}

	private void OnValidate()
	{
		AutoResolveReferences();
	}

	[ContextMenu("Auto Resolve")]
	public void AutoResolveReferences()
	{
		TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
		Image[] images = GetComponentsInChildren<Image>(true);

		iconImage ??= FindImage(images, "icon");
		actionNameLabel ??= FindText(texts, "name");
		costLabel ??= FindText(texts, "cost");
		rangeLabel ??= FindText(texts, "range");
		areaOfEffectLabel ??= FindText(texts, "area", "aoe");
		lineOfSightLabel ??= FindText(texts, "sight", "los");
		descriptionLabel ??= FindText(texts, "description", "desc");
	}

	public void Bind(Ability p_ability)
	{
		AutoResolveReferences();

		if (p_ability == null)
		{
			Clear();
			return;
		}

		if (iconImage != null)
		{
			iconImage.sprite = p_ability.Icon;
			iconImage.enabled = p_ability.Icon != null;
		}

		SetLabel(actionNameLabel, AbilityPresentationUtility.GetDisplayName(p_ability));
		SetLabel(costLabel, AbilityPresentationUtility.FormatCost(p_ability));
		SetLabel(rangeLabel, AbilityPresentationUtility.FormatRange(p_ability));
		SetLabel(areaOfEffectLabel, AbilityPresentationUtility.FormatAreaOfEffect(p_ability));
		SetLabel(lineOfSightLabel, AbilityPresentationUtility.FormatLineOfSight(p_ability));
		SetLabel(descriptionLabel, AbilityPresentationUtility.BuildDescription(p_ability));
	}

	public void Bind(BattleStatus p_status)
	{
		AutoResolveReferences();

		if (p_status?.Status == null)
		{
			Clear();
			return;
		}

		if (iconImage != null)
		{
			iconImage.sprite = p_status.Status.Icon;
			iconImage.enabled = p_status.Status.Icon != null;
		}

		SetLabel(actionNameLabel, StatusPresentationUtility.GetDisplayName(p_status.Status));
		SetLabel(costLabel, StatusPresentationUtility.FormatStackSummary(p_status));
		SetLabel(rangeLabel, StatusPresentationUtility.FormatDurationSummary(p_status));
		SetLabel(areaOfEffectLabel, StatusPresentationUtility.FormatTriggerSummary(p_status.Status));
		SetLabel(lineOfSightLabel, StatusPresentationUtility.FormatTagSummary(p_status.Status));
		SetLabel(descriptionLabel, StatusPresentationUtility.BuildDescription(p_status));
	}

	public void Clear()
	{
		if (iconImage != null)
		{
			iconImage.sprite = null;
			iconImage.enabled = false;
		}

		SetLabel(actionNameLabel, emptyMessage);
		SetLabel(costLabel, emptyMessage);
		SetLabel(rangeLabel, emptyMessage);
		SetLabel(areaOfEffectLabel, emptyMessage);
		SetLabel(lineOfSightLabel, emptyMessage);
		SetLabel(descriptionLabel, emptyMessage);
	}

	private static void SetLabel(TMP_Text p_label, string p_value)
	{
		if (p_label == null)
		{
			return;
		}

		p_label.text = string.IsNullOrWhiteSpace(p_value)
			? string.Empty
			: p_value;
	}

	private static TMP_Text FindText(TMP_Text[] p_candidates, params string[] p_tokens)
	{
		for (int index = 0; index < p_candidates.Length; index++)
		{
			TMP_Text candidate = p_candidates[index];
			string candidateName = candidate.name.ToLowerInvariant();
			for (int tokenIndex = 0; tokenIndex < p_tokens.Length; tokenIndex++)
			{
				if (candidateName.Contains(p_tokens[tokenIndex].ToLowerInvariant()))
				{
					return candidate;
				}
			}
		}

		return null;
	}

	private static Image FindImage(Image[] p_candidates, params string[] p_tokens)
	{
		for (int index = 0; index < p_candidates.Length; index++)
		{
			Image candidate = p_candidates[index];
			string candidateName = candidate.name.ToLowerInvariant();
			for (int tokenIndex = 0; tokenIndex < p_tokens.Length; tokenIndex++)
			{
				if (candidateName.Contains(p_tokens[tokenIndex].ToLowerInvariant()))
				{
					return candidate;
				}
			}
		}

		return null;
	}
}
