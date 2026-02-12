using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils
{
	public class MouseRaycastService
	{

		public bool TryGetWorldPosition(
			global::UnityEngine.Camera camera,
			out Vector3 worldPosition,
			RayCastConfiguration configuration = default)
		{
			worldPosition = default;

			if (camera == null)
			{
				return false;
			}

			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return false;
			}

			if (configuration.IsDefault)
			{
				configuration = RayCastConfiguration.Default;
			}

			Vector2 screenPosition = mouse.position.ReadValue();
			Ray ray = camera.ScreenPointToRay(screenPosition);

			if (Physics.Raycast(
				ray,
				out RaycastHit hit,
				configuration.MaxDistance,
				configuration.LayerMask,
				configuration.TriggerInteraction))
			{
				worldPosition = hit.point;
				return true;
			}

			return false;
		}

		public struct RayCastConfiguration
		{
			private const float DefaultMaxDistance = 500f;
			private const QueryTriggerInteraction DefaultTriggerInteraction = QueryTriggerInteraction.Ignore;

			public static readonly RayCastConfiguration Default = new RayCastConfiguration
			{
				LayerMask = Physics.DefaultRaycastLayers,
				MaxDistance = DefaultMaxDistance,
				TriggerInteraction = DefaultTriggerInteraction
			};

			public LayerMask LayerMask;
			public float MaxDistance;
			public QueryTriggerInteraction TriggerInteraction;

			public bool IsDefault =>
				MaxDistance <= 0.0001f
				&& LayerMask.value == 0
				&& TriggerInteraction == QueryTriggerInteraction.UseGlobal;
		}
	}
}
