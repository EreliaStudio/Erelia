using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace Erelia.Battle.Unit
{
	public sealed class View :
		MonoBehaviour,
		Erelia.Battle.Unit.UIView
	{
		private const float DefaultHealthBarLocalHeight = 1.5f;
		private const float HealthBarVerticalPadding = 0.2f;

		private static readonly List<Erelia.Battle.Unit.View> ActiveViews = new List<Erelia.Battle.Unit.View>();
		private static Erelia.Battle.Unit.View hoveredView;
		private static int lastHoverRefreshFrame = -1;

		private GameObject visualInstance;
		private GameObject healthBarPrefab;
		private GameObject healthBarInstance;
		private Transform pivot;
		private Erelia.Core.UI.ProgressBarView healthBarView;
		private Renderer[] visualRenderers;
		private Vector3 healthBarBaseScale = Vector3.one;

		[SerializeField] private float hoveredHealthBarScaleMultiplier = 3f;
		[SerializeField] private float hoveredHealthBarScaleLerpSpeed = 12f;

		public Transform Pivot => pivot != null ? pivot : transform;

		private void OnEnable()
		{
			if (!ActiveViews.Contains(this))
			{
				ActiveViews.Add(this);
			}
		}

		private void OnDisable()
		{
			ActiveViews.Remove(this);
			if (ReferenceEquals(hoveredView, this))
			{
				hoveredView = null;
			}
		}

		public void SetHealthBarPrefab(GameObject prefab)
		{
			healthBarPrefab = prefab;
			RebuildHealthBar();
		}

		public void SetVisualPrefab(GameObject prefab)
		{
			DestroyVisualInstance();

			if (prefab == null)
			{
				pivot = transform;
				RebuildHealthBar();
				UpdateHealthBarAnchor();
				return;
			}

			visualInstance = Object.Instantiate(prefab, transform);
			visualInstance.name = prefab.name;
			visualRenderers = visualInstance.GetComponentsInChildren<Renderer>(true);
			Erelia.Core.Creature.Instance.View creatureView = ResolveCreatureView(visualInstance);
			pivot = creatureView != null ? creatureView.Pivot : visualInstance.transform;
			RebuildHealthBar();
			UpdateHealthBarAnchor();
		}

		public void SetVisible(bool value)
		{
			if (gameObject.activeSelf == value)
			{
				return;
			}

			gameObject.SetActive(value);
		}

		public void SetWorldPosition(Vector3 worldPosition)
		{
			Transform root = transform;
			Transform currentPivot = Pivot;
			Vector3 pivotOffset = currentPivot.position - root.position;
			root.position = worldPosition - pivotOffset;
		}

		public void ApplySnapshot(Erelia.Battle.Unit.Snapshot snapshot)
		{
			RefreshHealthBar(snapshot.CurrentHealth, snapshot.MaxHealth);
		}

		public bool TryGetCreaturePresenter(out Erelia.Core.Creature.Instance.Presenter presenter)
		{
			if (visualInstance == null)
			{
				presenter = null;
				return false;
			}

			presenter =
				visualInstance.GetComponent<Erelia.Core.Creature.Instance.Presenter>() ??
				visualInstance.GetComponentInChildren<Erelia.Core.Creature.Instance.Presenter>(true);
			return presenter != null;
		}

		private void LateUpdate()
		{
			FaceHealthBarCamera();
			UpdateHealthBarScale();
		}

		private void DestroyVisualInstance()
		{
			if (visualInstance == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				Object.Destroy(visualInstance);
			}
			else
			{
				Object.DestroyImmediate(visualInstance);
			}

			visualInstance = null;
			visualRenderers = null;
			pivot = transform;
			DestroyHealthBarInstance();
		}

		private void RefreshHealthBar(int currentHealth, int maxHealth)
		{
			Transform healthBarRoot = healthBarView != null ? healthBarView.transform : null;
			if (healthBarRoot == null)
			{
				return;
			}

			bool showHealthBar = maxHealth > 0;
			if (healthBarRoot.gameObject.activeSelf != showHealthBar)
			{
				healthBarRoot.gameObject.SetActive(showHealthBar);
			}

			if (!showHealthBar)
			{
				healthBarView.SetLabel(string.Empty);
				return;
			}

			healthBarView.SetProgress((float)currentHealth / maxHealth);
			healthBarView.SetLabel(BuildHealthBarLabel(currentHealth, maxHealth));
		}

		private void RebuildHealthBar()
		{
			DestroyHealthBarInstance();
			if (healthBarPrefab == null)
			{
				return;
			}

			healthBarInstance = Object.Instantiate(healthBarPrefab, transform);
			healthBarInstance.name = healthBarPrefab.name;
			healthBarView =
				healthBarInstance.GetComponent<Erelia.Core.UI.ProgressBarView>() ??
				healthBarInstance.GetComponentInChildren<Erelia.Core.UI.ProgressBarView>(true);
			if (healthBarView == null)
			{
				Debug.LogWarning("[Erelia.Battle.Unit.View] Health bar prefab is missing a ProgressBarView component.");
				DestroyHealthBarInstance();
				return;
			}

			healthBarBaseScale = healthBarView.transform.localScale;
			UpdateHealthBarAnchor();
		}

		private void DestroyHealthBarInstance()
		{
			healthBarView = null;
			if (healthBarInstance == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				Object.Destroy(healthBarInstance);
			}
			else
			{
				Object.DestroyImmediate(healthBarInstance);
			}

			healthBarInstance = null;
		}

		private void UpdateHealthBarAnchor()
		{
			Transform healthBarRoot = healthBarView != null ? healthBarView.transform : null;
			if (healthBarRoot == null)
			{
				return;
			}

			healthBarRoot.localPosition = ResolveHealthBarLocalPosition();
		}

		private Vector3 ResolveHealthBarLocalPosition()
		{
			if (visualInstance == null)
			{
				return new Vector3(0f, DefaultHealthBarLocalHeight, 0f);
			}

			Renderer[] renderers = visualInstance.GetComponentsInChildren<Renderer>(true);
			if (renderers == null || renderers.Length == 0)
			{
				return pivot != null
					? pivot.localPosition + (Vector3.up * DefaultHealthBarLocalHeight)
					: new Vector3(0f, DefaultHealthBarLocalHeight, 0f);
			}

			Bounds combinedBounds = renderers[0].bounds;
			for (int i = 1; i < renderers.Length; i++)
			{
				combinedBounds.Encapsulate(renderers[i].bounds);
			}

			Vector3 topCenterWorld = new Vector3(
				combinedBounds.center.x,
				combinedBounds.max.y + HealthBarVerticalPadding,
				combinedBounds.center.z);
			return transform.InverseTransformPoint(topCenterWorld);
		}

		private void FaceHealthBarCamera()
		{
			Transform healthBarRoot = healthBarView != null ? healthBarView.transform : null;
			if (healthBarRoot == null || !healthBarRoot.gameObject.activeInHierarchy)
			{
				return;
			}

			Camera targetCamera = Camera.main;
			if (targetCamera == null)
			{
				return;
			}

			Canvas healthBarCanvas = healthBarView.Canvas;
			if (healthBarCanvas != null && healthBarCanvas.renderMode == RenderMode.WorldSpace)
			{
				healthBarCanvas.worldCamera = targetCamera;
			}

			healthBarRoot.transform.rotation = targetCamera.transform.rotation;
		}

		private void UpdateHealthBarScale()
		{
			Transform healthBarRoot = healthBarView != null ? healthBarView.transform : null;
			if (healthBarRoot == null)
			{
				return;
			}

			UpdateHoveredView();

			float scaleMultiplier = ReferenceEquals(hoveredView, this)
				? Mathf.Max(1f, hoveredHealthBarScaleMultiplier)
				: 1f;
			Vector3 targetScale = healthBarBaseScale * scaleMultiplier;
			if (hoveredHealthBarScaleLerpSpeed <= 0f)
			{
				healthBarRoot.localScale = targetScale;
				return;
			}

			healthBarRoot.localScale = Vector3.Lerp(
				healthBarRoot.localScale,
				targetScale,
				1f - Mathf.Exp(-hoveredHealthBarScaleLerpSpeed * Time.deltaTime));
		}

		private static void UpdateHoveredView()
		{
			if (lastHoverRefreshFrame == Time.frameCount)
			{
				return;
			}

			lastHoverRefreshFrame = Time.frameCount;
			hoveredView = null;

			Camera targetCamera = Camera.main;
			if (targetCamera == null || Mouse.current == null || IsPointerOverUi())
			{
				return;
			}

			Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			float nearestDistance = float.PositiveInfinity;
			for (int i = 0; i < ActiveViews.Count; i++)
			{
				Erelia.Battle.Unit.View candidate = ActiveViews[i];
				if (candidate == null || !candidate.isActiveAndEnabled)
				{
					continue;
				}

				if (!candidate.TryIntersectVisual(ray, out float hitDistance) || hitDistance >= nearestDistance)
				{
					continue;
				}

				nearestDistance = hitDistance;
				hoveredView = candidate;
			}
		}

		private bool TryIntersectVisual(Ray ray, out float hitDistance)
		{
			hitDistance = float.PositiveInfinity;
			if (!TryGetVisualBounds(out Bounds bounds))
			{
				return false;
			}

			return bounds.IntersectRay(ray, out hitDistance);
		}

		private bool TryGetVisualBounds(out Bounds bounds)
		{
			bounds = default;
			if (visualRenderers == null || visualRenderers.Length == 0)
			{
				return false;
			}

			bool hasBounds = false;
			for (int i = 0; i < visualRenderers.Length; i++)
			{
				Renderer renderer = visualRenderers[i];
				if (renderer == null || !renderer.enabled)
				{
					continue;
				}

				if (!hasBounds)
				{
					bounds = renderer.bounds;
					hasBounds = true;
					continue;
				}

				bounds.Encapsulate(renderer.bounds);
			}

			return hasBounds;
		}

		private static bool IsPointerOverUi()
		{
			return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		}

		private static Erelia.Core.Creature.Instance.View ResolveCreatureView(GameObject viewObject)
		{
			if (viewObject == null)
			{
				return null;
			}

			Erelia.Core.Creature.Instance.View creatureView =
				viewObject.GetComponent<Erelia.Core.Creature.Instance.View>() ??
				viewObject.GetComponentInChildren<Erelia.Core.Creature.Instance.View>(true);
			return creatureView;
		}

		private static string BuildHealthBarLabel(int currentHealth, int maxHealth)
		{
			return $"{Mathf.Max(0, currentHealth)} / {Mathf.Max(0, maxHealth)}";
		}
	}
}
