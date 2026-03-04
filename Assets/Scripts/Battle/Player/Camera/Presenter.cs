using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player.Camera
{
	/// <summary>
	/// Presenter controlling the battle camera orbit and zoom.
	/// Resolves input actions, applies orbit/zoom each frame, and keeps the camera looking at the pivot.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View component for the camera.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.Camera.View view;
		/// <summary>
		/// Input action for mouse look.
		/// </summary>
		[SerializeField] private InputActionReference lookAction;
		/// <summary>
		/// Input action for keyboard orbit.
		/// </summary>
		[SerializeField] private InputActionReference orbitAction;
		/// <summary>
		/// Input action for zoom.
		/// </summary>
		[SerializeField] private InputActionReference zoomAction;
		/// <summary>
		/// Camera model data.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.Camera.Model model = new Erelia.Battle.Player.Camera.Model();

		/// <summary>
		/// Resolved input action for mouse look.
		/// </summary>
		private InputAction resolvedLookAction;
		/// <summary>
		/// Resolved input action for keyboard orbit.
		/// </summary>
		private InputAction resolvedOrbitAction;
		/// <summary>
		/// Resolved input action for zoom.
		/// </summary>
		private InputAction resolvedZoomAction;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate references and resolve input actions.
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

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Enable input actions.
			resolvedLookAction.Enable();
			resolvedOrbitAction.Enable();
			resolvedZoomAction.Enable();
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Disable input actions.
			resolvedLookAction.Disable();
			resolvedOrbitAction.Disable();
			resolvedZoomAction.Disable();
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Apply orbit and zoom input.
			ApplyOrbitInput();
		}

		/// <summary>
		/// Unity late update loop.
		/// </summary>
		private void LateUpdate()
		{
			// Ensure the camera keeps looking at the pivot.
			LookAtPivot();
		}

		/// <summary>
		/// Reads input and applies orbit/zoom changes.
		/// </summary>
		private void ApplyOrbitInput()
		{
			// Convert input values into orbit/zoom deltas.
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

		/// <summary>
		/// Resolves input actions from their references.
		/// </summary>
		private void ResolveActions()
		{
			// Validate references and cache resolved actions.
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

		/// <summary>
		/// Rotates the camera around its pivot.
		/// </summary>
		private void RotateAroundPivot(float yawDelta)
		{
			// Apply yaw rotation around the pivot position.
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

		/// <summary>
		/// Adjusts the camera distance to the pivot.
		/// </summary>
		private void ApplyZoom(float zoomDelta)
		{
			// Clamp and apply zoom distance.
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

		/// <summary>
		/// Rotates the camera to look at the pivot.
		/// </summary>
		private void LookAtPivot()
		{
			// Aim the camera toward the pivot.
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
