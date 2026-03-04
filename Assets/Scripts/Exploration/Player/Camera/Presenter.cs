using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Exploration.Player.Camera
{
	/// <summary>
	/// Presenter controlling the exploration camera orbit and zoom.
	/// Resolves input actions, applies orbit/zoom each frame, and keeps the camera looking at the pivot.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View component for the camera.
		/// </summary>
		[SerializeField] private Erelia.Exploration.Player.Camera.View view;

		/// <summary>
		/// Input action for mouse look.
		/// This is connected to the same behaviour as the <see cref="orbitAction"/>
		/// </summary>
		[SerializeField] private InputActionReference lookAction;

		/// <summary>
		/// Input action for keyboard orbit.
		/// This is connected to the same behaviour as the <see cref="lookAction"/>
		/// </summary>
		[SerializeField] private InputActionReference orbitAction;

		/// <summary>
		/// Input action for zoom.
		/// </summary>
		[SerializeField] private InputActionReference zoomAction;

		/// <summary>
		/// Camera model data.
		/// </summary>
		[SerializeField] private Erelia.Exploration.Player.Camera.Model model = new Erelia.Exploration.Player.Camera.Model();

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
			// Ensure required references are assigned.
			if (view == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Camera.Presenter] View is not assigned.");
			}

			// Ensure a model exists.
			if (model == null)
			{
				model = new Erelia.Exploration.Player.Camera.Model();
			}

			// Resolve input actions once at startup.
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
			// Apply orbit and zoom inputs.
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
		/// Applies orbit and zoom input to the camera.
		/// </summary>
		private void ApplyOrbitInput()
		{
			// Read input axes.
			float lookAxis = resolvedLookAction.ReadValue<float>();
			float orbitAxis = resolvedOrbitAction.ReadValue<float>();
			float zoomAxis = resolvedZoomAction.ReadValue<float>();

			// Convert input into yaw deltas.
			float yawFromMouse = lookAxis * model.MouseOrbitSensitivity;
			float yawFromKeys = orbitAxis * model.KeyOrbitSpeed * Time.deltaTime;

			if (Mathf.Abs(yawFromMouse) > 0.0001f || Mathf.Abs(yawFromKeys) > 0.0001f)
			{
				RotateAroundPivot(yawFromMouse + yawFromKeys);
			}

			if (Mathf.Abs(zoomAxis) > 0.01f)
			{
				// Convert scroll ticks into zoom delta.
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
			// Validate action references.
			if (lookAction == null || lookAction.action == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Camera.Presenter] Look action is not assigned.");
			}

			if (orbitAction == null || orbitAction.action == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Camera.Presenter] Orbit action is not assigned.");
			}

			if (zoomAction == null || zoomAction.action == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Camera.Presenter] Zoom action is not assigned.");
			}

			// Store resolved actions.
			resolvedLookAction = lookAction.action;
			resolvedOrbitAction = orbitAction.action;
			resolvedZoomAction = zoomAction.action;
		}

		/// <summary>
		/// Rotates the camera around its pivot by the given yaw delta.
		/// </summary>
		/// <param name="yawDelta">Yaw rotation in degrees.</param>
		private void RotateAroundPivot(float yawDelta)
		{
			// Resolve camera transform.
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			// Use local position as orbit offset.
			Vector3 offset = cameraTransform.localPosition;
			if (offset.sqrMagnitude <= 0.0001f)
			{
				return;
			}

			// Rotate around Y axis and keep looking at pivot.
			Quaternion rotation = Quaternion.AngleAxis(yawDelta, Vector3.up);
			cameraTransform.localPosition = rotation * offset;
			LookAtPivot();
		}

		/// <summary>
		/// Applies zoom by changing the distance to the pivot.
		/// </summary>
		/// <param name="zoomDelta">Delta distance to apply.</param>
		private void ApplyZoom(float zoomDelta)
		{
			// Resolve camera transform.
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			// Compute current distance.
			Vector3 offset = cameraTransform.localPosition;
			float distance = offset.magnitude;
			if (distance <= 0.0001f)
			{
				return;
			}

			// Clamp to allowed orbit range.
			float clamped = Mathf.Clamp(distance + zoomDelta, model.MinOrbitDistance, model.MaxOrbitDistance);
			cameraTransform.localPosition = offset.normalized * clamped;
			LookAtPivot();
		}

		/// <summary>
		/// Rotates the camera to look at the pivot point.
		/// </summary>
		private void LookAtPivot()
		{
			// Resolve camera transform.
			Transform cameraTransform = view.transform;
			if (cameraTransform == null)
			{
				return;
			}

			// Compute direction to pivot from local offset.
			Vector3 toPivot = -cameraTransform.localPosition;
			if (toPivot.sqrMagnitude <= 0.0001f)
			{
				return;
			}

			// Apply rotation toward pivot.
			cameraTransform.localRotation = Quaternion.LookRotation(toPivot.normalized, Vector3.up);
		}
	}
}
