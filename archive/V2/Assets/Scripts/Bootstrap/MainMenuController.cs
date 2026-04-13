using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
	private const string DefaultGameplaySceneName = "MainScene";

	[SerializeField] private string gameplaySceneName = DefaultGameplaySceneName;
	[SerializeField] private GameObject mainMenuPanel;
	[SerializeField] private GameObject loadPanel;
	[SerializeField] private Button newGameButton;
	[SerializeField] private Button loadGameButton;
	[SerializeField] private Button backButton;
	[SerializeField] private TMP_Text loadStatusLabel;
	[SerializeField] private RectTransform saveListRoot;
	[SerializeField] private Button saveSlotTemplate;

	private readonly List<Button> runtimeSaveButtons = new List<Button>();

	public void Configure(
		GameObject p_mainMenuPanel,
		GameObject p_loadPanel,
		Button p_newGameButton,
		Button p_loadGameButton,
		Button p_backButton,
		TMP_Text p_loadStatusLabel,
		RectTransform p_saveListRoot,
		Button p_saveSlotTemplate,
		string p_gameplaySceneName)
	{
		mainMenuPanel = p_mainMenuPanel;
		loadPanel = p_loadPanel;
		newGameButton = p_newGameButton;
		loadGameButton = p_loadGameButton;
		backButton = p_backButton;
		loadStatusLabel = p_loadStatusLabel;
		saveListRoot = p_saveListRoot;
		saveSlotTemplate = p_saveSlotTemplate;
		gameplaySceneName = string.IsNullOrWhiteSpace(p_gameplaySceneName) ? DefaultGameplaySceneName : p_gameplaySceneName;
	}

	private void Awake()
	{
		AutoBindIfNeeded();
		SetLoadPanelVisible(false);

		if (saveSlotTemplate != null)
		{
			saveSlotTemplate.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if (newGameButton != null)
		{
			newGameButton.onClick.AddListener(HandleNewGameClicked);
		}

		if (loadGameButton != null)
		{
			loadGameButton.onClick.AddListener(HandleLoadGameClicked);
		}

		if (backButton != null)
		{
			backButton.onClick.AddListener(HandleBackClicked);
		}
	}

	private void OnDisable()
	{
		if (newGameButton != null)
		{
			newGameButton.onClick.RemoveListener(HandleNewGameClicked);
		}

		if (loadGameButton != null)
		{
			loadGameButton.onClick.RemoveListener(HandleLoadGameClicked);
		}

		if (backButton != null)
		{
			backButton.onClick.RemoveListener(HandleBackClicked);
		}

		ClearRuntimeSaveButtons();
	}

	private void HandleNewGameClicked()
	{
		GameData gameData = GameSaveService.CreateNewGame(out string saveId);
		if (gameData == null)
		{
			UpdateStatus("Failed to create a save file.");
			return;
		}

		GameSession.BeginLoad(saveId, gameData);
		SceneManager.LoadScene(gameplaySceneName);
	}

	private void HandleLoadGameClicked()
	{
		RefreshSaveButtons();
		SetLoadPanelVisible(true);
	}

	private void HandleBackClicked()
	{
		SetLoadPanelVisible(false);
	}

	private void HandleSaveSelected(string saveId)
	{
		if (!GameSaveService.TryLoad(saveId, out GameData gameData))
		{
			UpdateStatus($"Failed to load {GameSaveService.GetDisplayName(saveId)}.");
			RefreshSaveButtons();
			return;
		}

		GameSession.BeginLoad(saveId, gameData);
		SceneManager.LoadScene(gameplaySceneName);
	}

	private void RefreshSaveButtons()
	{
		ClearRuntimeSaveButtons();

		if (saveSlotTemplate == null || saveListRoot == null)
		{
			UpdateStatus("The save menu is not configured.");
			return;
		}

		string[] saveIds = GameSaveService.GetAvailableSaveIds();
		if (saveIds.Length == 0)
		{
			UpdateStatus("No saves available yet. Start a new game first.");
			return;
		}

		UpdateStatus("Select a save to load.");

		for (int index = 0; index < saveIds.Length; index++)
		{
			string capturedSaveId = saveIds[index];
			Button button = Instantiate(saveSlotTemplate, saveListRoot);
			button.gameObject.name = $"SaveSlot_{capturedSaveId}";
			button.gameObject.SetActive(true);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate { HandleSaveSelected(capturedSaveId); });

			TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);
			if (buttonLabel != null)
			{
				buttonLabel.text = GameSaveService.GetDisplayName(capturedSaveId);
			}

			runtimeSaveButtons.Add(button);
		}
	}

	private void ClearRuntimeSaveButtons()
	{
		for (int index = 0; index < runtimeSaveButtons.Count; index++)
		{
			Button button = runtimeSaveButtons[index];
			if (button == null)
			{
				continue;
			}

			Destroy(button.gameObject);
		}

		runtimeSaveButtons.Clear();
	}

	private void SetLoadPanelVisible(bool visible)
	{
		if (mainMenuPanel != null)
		{
			mainMenuPanel.SetActive(!visible);
		}

		if (loadPanel != null)
		{
			loadPanel.SetActive(visible);
		}
	}

	private void UpdateStatus(string message)
	{
		if (loadStatusLabel != null)
		{
			loadStatusLabel.text = message;
		}
	}

	private void AutoBindIfNeeded()
	{
		mainMenuPanel ??= FindGameObject("MainMenuPanel");
		loadPanel ??= FindGameObject("LoadPanel");
		newGameButton ??= FindComponent<Button>("NewGameButton");
		loadGameButton ??= FindComponent<Button>("LoadGameButton");
		backButton ??= FindComponent<Button>("BackButton");
		loadStatusLabel ??= FindComponent<TMP_Text>("LoadStatusLabel");
		saveListRoot ??= FindComponent<RectTransform>("SaveListRoot");
		saveSlotTemplate ??= FindComponent<Button>("SaveSlotTemplate");
	}

	private GameObject FindGameObject(string objectName)
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (!activeScene.IsValid())
		{
			return null;
		}

		GameObject[] rootObjects = activeScene.GetRootGameObjects();
		for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
		{
			Transform[] transforms = rootObjects[rootIndex].GetComponentsInChildren<Transform>(true);
			for (int index = 0; index < transforms.Length; index++)
			{
				if (string.Equals(transforms[index].name, objectName, StringComparison.Ordinal))
				{
					return transforms[index].gameObject;
				}
			}
		}

		return null;
	}

	private T FindComponent<T>(string objectName) where T : Component
	{
		GameObject gameObject = FindGameObject(objectName);
		return gameObject != null ? gameObject.GetComponent<T>() : null;
	}
}
