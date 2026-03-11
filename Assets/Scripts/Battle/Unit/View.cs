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

		[SerializeField] private Transform pivot;
		[SerializeField] private Transform healthBarAnchor;
		[SerializeField] private Transform visualRoot;
		[SerializeField] private Erelia.Core.Creature.Instance.Presenter creaturePresenter;
		[SerializeField] private Erelia.Core.Creature.Instance.View creatureView;
		[SerializeField] private Erelia.Core.UI.ProgressBarView healthBarView;
		private GameObject healthBarPrefab;
		private GameObject healthBarInstance;
		private Erelia.Core.UI.ProgressBarView runtimeHealthBarView;
		private Renderer[] visualRenderers;
		private Transform visualRenderersRoot;
		private Vector3 healthBarBaseScale = Vector3.one;

		[SerializeField] private float defaultHealthBarScaleMultiplier = 2f;
		[SerializeField] private float hoveredHealthBarScaleMultiplier = 3f;
		[SerializeField] private float hoveredHealthBarScaleLerpSpeed = 12f;

		public Transform Pivot => ResolvePivot();

		private void Awake()
		{
			CacheHierarchyReferences();
			ResolveConfiguredHealthBarView();
			UpdateHealthBarAnchor();
		}

		private void OnEnable()
		{
			CacheHierarchyReferences();
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
			if (ReferenceEquals(healthBarPrefab, prefab) && healthBarInstance != null)
			{
				return;
			}

			healthBarPrefab = prefab;
			RebuildHealthBar();
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
			CacheHierarchyReferences();
			presenter = creaturePresenter;
			return presenter != null;
		}

		private void LateUpdate()
		{
			FaceHealthBarCamera();
			UpdateHealthBarScale();
		}

		private void RefreshHealthBar(int currentHealth, int maxHealth)
		{
			Erelia.Core.UI.ProgressBarView activeHealthBarView = ResolveActiveHealthBarView();
			Transform healthBarRoot = activeHealthBarView != null ? activeHealthBarView.transform : null;
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
				activeHealthBarView.SetLabel(string.Empty);
				return;
			}

			activeHealthBarView.SetProgress((float)currentHealth / maxHealth);
			activeHealthBarView.SetLabel(BuildHealthBarLabel(currentHealth, maxHealth));
		}

		private void RebuildHealthBar()
		{
			DestroyRuntimeHealthBarInstance();
			ResolveConfiguredHealthBarView();
			if (healthBarView != null)
			{
				healthBarBaseScale = healthBarView.transform.localScale;
				UpdateHealthBarAnchor();
				return;
			}

			if (healthBarPrefab == null)
			{
				return;
			}

			Transform healthBarParent = ResolveHealthBarParent();
			healthBarInstance = Object.Instantiate(healthBarPrefab, healthBarParent, false);
			healthBarInstance.name = healthBarPrefab.name;
			runtimeHealthBarView =
				healthBarInstance.GetComponent<Erelia.Core.UI.ProgressBarView>() ??
				healthBarInstance.GetComponentInChildren<Erelia.Core.UI.ProgressBarView>(true);
			if (runtimeHealthBarView == null)
			{
				Debug.LogWarning("[Erelia.Battle.Unit.View] Health bar prefab is missing a ProgressBarView component.");
				DestroyRuntimeHealthBarInstance();
				return;
			}

			healthBarBaseScale = runtimeHealthBarView.transform.localScale;
			UpdateHealthBarAnchor();
		}

		private void DestroyRuntimeHealthBarInstance()
		{
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
			runtimeHealthBarView = null;
		}

		private void UpdateHealthBarAnchor()
		{
			Erelia.Core.UI.ProgressBarView activeHealthBarView = ResolveActiveHealthBarView();
			Transform healthBarRoot = activeHealthBarView != null ? activeHealthBarView.transform : null;
			if (healthBarRoot == null)
			{
				return;
			}

			if (healthBarAnchor != null)
			{
				healthBarRoot.localPosition = Vector3.zero;
				return;
			}

			healthBarRoot.localPosition = ResolveHealthBarLocalPosition();
		}

		private Vector3 ResolveHealthBarLocalPosition()
		{
			CacheHierarchyReferences();
			if (healthBarAnchor != null)
			{
				return Vector3.zero;
			}

			if (!TryGetVisualBounds(out Bounds combinedBounds))
			{
				return new Vector3(0f, DefaultHealthBarLocalHeight, 0f);
			}

			Vector3 topCenterWorld = new Vector3(
				combinedBounds.center.x,
				combinedBounds.max.y + HealthBarVerticalPadding,
				combinedBounds.center.z);
			return transform.InverseTransformPoint(topCenterWorld);
		}

		private void FaceHealthBarCamera()
		{
			Erelia.Core.UI.ProgressBarView activeHealthBarView = ResolveActiveHealthBarView();
			Transform healthBarRoot = activeHealthBarView != null ? activeHealthBarView.transform : null;
			if (healthBarRoot == null || !healthBarRoot.gameObject.activeInHierarchy)
			{
				return;
			}

			Camera targetCamera = Camera.main;
			if (targetCamera == null)
			{
				return;
			}

			Canvas healthBarCanvas = activeHealthBarView.Canvas;
			if (healthBarCanvas != null && healthBarCanvas.renderMode == RenderMode.WorldSpace)
			{
				healthBarCanvas.worldCamera = targetCamera;
			}

			healthBarRoot.transform.rotation = targetCamera.transform.rotation;
		}

		private void UpdateHealthBarScale()
		{
			Erelia.Core.UI.ProgressBarView activeHealthBarView = ResolveActiveHealthBarView();
			Transform healthBarRoot = activeHealthBarView != null ? activeHealthBarView.transform : null;
			if (healthBarRoot == null)
			{
				return;
			}

			UpdateHoveredView();

			float scaleMultiplier = ReferenceEquals(hoveredView, this)
				? Mathf.Max(defaultHealthBarScaleMultiplier, hoveredHealthBarScaleMultiplier)
				: Mathf.Max(1f, defaultHealthBarScaleMultiplier);
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
			CacheHierarchyReferences();
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

		private void CacheHierarchyReferences()
		{
			if (creatureView == null)
			{
				creatureView =
					GetComponent<Erelia.Core.Creature.Instance.View>() ??
					GetComponentInChildren<Erelia.Core.Creature.Instance.View>(true);
			}

			if (creaturePresenter == null)
			{
				creaturePresenter =
					GetComponent<Erelia.Core.Creature.Instance.Presenter>() ??
					GetComponentInChildren<Erelia.Core.Creature.Instance.Presenter>(true);
			}

			visualRoot ??= creatureView != null ? creatureView.transform : transform;
			Transform currentVisualRoot = visualRoot != null ? visualRoot : transform;
			if (visualRenderers == null || visualRenderers.Length == 0 || visualRenderersRoot != currentVisualRoot)
			{
				visualRenderersRoot = currentVisualRoot;
				visualRenderers = ResolveVisualRenderers();
			}
		}

		private Transform ResolvePivot()
		{
			if (pivot != null)
			{
				return pivot;
			}

			CacheHierarchyReferences();
			return creatureView != null ? creatureView.Pivot : transform;
		}

		private Transform ResolveHealthBarParent()
		{
			return healthBarAnchor != null ? healthBarAnchor : transform;
		}

		private void ResolveConfiguredHealthBarView()
		{
			if (healthBarView != null)
			{
				return;
			}

			healthBarView =
				GetComponent<Erelia.Core.UI.ProgressBarView>() ??
				GetComponentInChildren<Erelia.Core.UI.ProgressBarView>(true);
			if (healthBarView != null)
			{
				healthBarBaseScale = healthBarView.transform.localScale;
			}
		}

		private Erelia.Core.UI.ProgressBarView ResolveActiveHealthBarView()
		{
			return runtimeHealthBarView != null ? runtimeHealthBarView : healthBarView;
		}

		private Renderer[] ResolveVisualRenderers()
		{
			Transform root = visualRoot != null ? visualRoot : transform;
			return root != null
				? root.GetComponentsInChildren<Renderer>(true)
				: null;
		}

		private static string BuildHealthBarLabel(int currentHealth, int maxHealth)
		{
			return $"{Mathf.Max(0, currentHealth)} / {Mathf.Max(0, maxHealth)}";
		}
	}
}
