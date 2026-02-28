using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player.Camera
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Player.Camera.View view;
		[SerializeField] private InputActionReference lookAction;
		[SerializeField] private InputActionReference orbitAction;
		[SerializeField] private InputActionReference zoomAction;
		[SerializeField] private Erelia.Battle.Player.Camera.Model model = new Erelia.Battle.Player.Camera.Model();

		private InputAction resolvedLookAction;
		private InputAction resolvedOrbitAction;
		private InputAction resolvedZoomAction;

		private void Awake()
		{
			if (view == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Camera.Presenter] View is not assigned.");
			}

			if (model == null)
			{
				model = new Erelia.Battle.Player.Camera.Model();
			}

			ResolveActions();
		}

		private void OnEnable()
		{
			resolvedLookAction.Enable();
			resolvedOrbitAction.Enable();
			resolvedZoomAction.Enable();
		}

		private void OnDisable()
		{
			resolvedLookAction.Disable();
			resolvedOrbitAction.Disable();
			resolvedZoomAction.Disable();
		}

		private void Update()
		{
			ApplyOrbitInput();
		}

		private void LateUpdate()
		{
			LookAtPivot();
		}

		private void ApplyOrbitInput()
		{
			float lookAxis = resolvedLookAction.ReadValue<float>();
			float orbitAxis = resolvedOrbitAction.ReadValue<float>();
			float zoomAxis = resolvedZoomAction.ReadValue<float>();

			float yawFromMouse = lookAxis * model.MouseOrbitSensitivity;
			float yawFromKeys = orbitAxis * model.KeyOrbitSpeed * Time.deltaTime;

			if (Mathf.Abs(yawFromMouse) > 0.0001f || Mathf.Abs(yawFromKeys) > 0.0001f)
			{
				RotateAroundPivot(yawFromMouse + yawFromKeys);
			}

			if (Mathf.Abs(zoomAxis) > 0.01f)
			{
				float scrollSteps = zoomAxis / 120f;
				float zoomDelta = -scrollSteps * model.ZoomSpeed;
				ApplyZoom(zoomDelta);
			}
		}

		private void ResolveActions()
		{
			if (lookAction == null || lookAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Camera.Presenter] Look action is not assigned.");
			}

			if (orbitAction == null || orbitAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Camera.Presenter] Orbit action is not assigned.");
			}

			if (zoomAction == null || zoomAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Camera.Presenter] Zoom action is not assigned.");
			}

			resolvedLookAction = lookAction.action;
			resolvedOrbitAction = orbitAction.action;
			resolvedZoomAction = zoomAction.action;
		}

		private void RotateAroundPivot(float yawDelta)
		{
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			Vector3 offset = cameraTransform.localPosition;
			if (offset.sqrMagnitude <= 0.0001f)
			{
				return;
			}

			Quaternion rotation = Quaternion.AngleAxis(yawDelta, Vector3.up);
			cameraTransform.localPosition = rotation * offset;
			LookAtPivot();
		}

		private void ApplyZoom(float zoomDelta)
		{
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			Vector3 offset = cameraTransform.localPosition;
			float distance = offset.magnitude;
			if (distance <= 0.0001f)
			{
				return;
			}

			float clamped = Mathf.Clamp(distance + zoomDelta, model.MinOrbitDistance, model.MaxOrbitDistance);
			cameraTransform.localPosition = offset.normalized * clamped;
			LookAtPivot();
		}

		private void LookAtPivot()
		{
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			Vector3 toPivot = -cameraTransform.localPosition;
			if (toPivot.sqrMagnitude <= 0.0001f)
			{
				return;
			}

			cameraTransform.localRotation = Quaternion.LookRotation(toPivot.normalized, Vector3.up);
		}
	}
}
