using UnityEngine;

[DisallowMultipleComponent]
public class BattleUnitView : MonoBehaviour
{
	[SerializeField] private Transform modelRoot;

	private GameObject currentModelInstance;

	public Transform ModelRoot => modelRoot != null ? modelRoot : transform;

	private void Awake()
	{
		if (modelRoot == null)
		{
			modelRoot = transform;
		}
	}

	public void SetModel(CreatureModel p_creatureModel)
	{
		ClearModel();

		if (p_creatureModel == null || p_creatureModel.ModelPrefab == null)
		{
			return;
		}

		currentModelInstance = Instantiate(p_creatureModel.ModelPrefab, ModelRoot, false);
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
