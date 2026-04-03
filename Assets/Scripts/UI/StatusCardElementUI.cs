using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StatusCardElementUI :
	ObservableValue<BattleStatus>.Listener,
	IPointerEnterHandler,
	IPointerExitHandler
{
	[SerializeField] private Image iconImage;
	[SerializeField] private TMP_Text stackLabel;
	[SerializeField] private TMP_Text durationLabel;

	private BattleStatus linkedStatus;

	public event Action<StatusCardElementUI, BattleStatus> Hovered;
	public event Action<StatusCardElementUI> HoverEnded;

	public void Bind(ObservableValue<BattleStatus> p_status)
	{
		SubscribeTo(p_status);
	}

	public void Clear()
	{
		ClearBinding();
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if (linkedStatus?.Status == null)
		{
			return;
		}

		Hovered?.Invoke(this, linkedStatus);
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		HoverEnded?.Invoke(this);
	}

	protected override void ReactToEdition(BattleStatus p_value)
	{
		linkedStatus = p_value;
		Apply(p_value);
	}

	protected override void ClearRenderedValue()
	{
		linkedStatus = null;
		Apply(null);
	}

	private void Apply(BattleStatus p_status)
	{
		ResolveReferences();

		Sprite icon = p_status?.Status != null ? p_status.Status.Icon : null;
		if (iconImage != null)
		{
			iconImage.sprite = icon;
			iconImage.enabled = icon != null;
		}

		SetLabel(stackLabel, StatusPresentationUtility.FormatStackLabel(p_status));
		SetLabel(durationLabel, StatusPresentationUtility.FormatDurationLabel(p_status));
	}

	private void ResolveReferences()
	{
		if (iconImage == null)
		{
			Image[] images = GetComponentsInChildren<Image>(true);
			iconImage = FindImage(images, "icon");

			if (iconImage == null)
			{
				for (int index = 0; index < images.Length; index++)
				{
					if (images[index].gameObject == gameObject)
					{
						continue;
					}

					iconImage = images[index];
					break;
				}
			}

			iconImage ??= GetComponent<Image>();
		}

		if (stackLabel == null || durationLabel == null)
		{
			TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
			stackLabel ??= FindText(texts, "stack");
			durationLabel ??= FindText(texts, "duration", "time");
		}
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
		p_label.gameObject.SetActive(string.IsNullOrWhiteSpace(p_label.text) == false);
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
}
