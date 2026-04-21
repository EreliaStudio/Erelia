using UnityEngine;

[DisallowMultipleComponent]
public class BattleUnitView : MonoBehaviour
{
	private GameObject currentModelInstance;

	private void Awake()
	{
		
	}

	public void SetModel(GameObject p_modelPrefab)
	{
		ClearModel();

		if (p_modelPrefab == null)
		{
			return;
		}

		currentModelInstance = Instantiate(p_modelPrefab, transform, false);
		currentModelInstance.transform.localPosition = Vector3.zero;
		currentModelInstance.transform.localRotation = Quaternion.identity;
		currentModelInstance.transform.localScale = Vector3.one;
	}

	public void ClearModel()
	{
		if (currentModelInstance == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Destroy(currentModelInstance);
		}
		else
		{
			DestroyImmediate(currentModelInstance);
		}

		currentModelInstance = null;
	}
}
