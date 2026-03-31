using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SquareLayoutElement : MonoBehaviour, ILayoutSelfController
{
	private RectTransform _rectTransform;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
	}

	public void SetLayoutHorizontal()
	{
		EnsureSquare();
	}

	public void SetLayoutVertical()
	{
		EnsureSquare();
	}

	private void EnsureSquare()
	{
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		float width = _rectTransform.rect.width;
		float height = _rectTransform.rect.height;
		float size = Mathf.Min(width, height);

		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
	}
}