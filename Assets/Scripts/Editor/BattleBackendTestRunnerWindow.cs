using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public sealed class BattleBackendTestRunnerWindow : EditorWindow
{
	private const string TestAssemblyName = "Erelia.Tests.EditMode";

	private static readonly SuiteDefinition[] Suites =
	{
		new SuiteDefinition("All Backend Battle Tests", new[]
		{
			"SetupPhaseTests",
			"PlacementPhaseTests",
			"IdlePhaseTests",
			"PlayerTurnPhaseQueryTests",
			"PlayerTurnPhaseCommandTests",
			"EnemyTurnPhaseTests",
			"ResolutionPhaseTests",
			"EndPhaseTests",
			"BattleCombatFlowTests"
		}),
		new SuiteDefinition("Setup Phase", new[] { "SetupPhaseTests" }),
		new SuiteDefinition("Placement Phase", new[] { "PlacementPhaseTests" }),
		new SuiteDefinition("Idle Phase", new[] { "IdlePhaseTests" }),
		new SuiteDefinition("Player Turn Queries", new[] { "PlayerTurnPhaseQueryTests" }),
		new SuiteDefinition("Player Turn Commands", new[] { "PlayerTurnPhaseCommandTests" }),
		new SuiteDefinition("Enemy Turn", new[] { "EnemyTurnPhaseTests" }),
		new SuiteDefinition("Resolution Phase", new[] { "ResolutionPhaseTests" }),
		new SuiteDefinition("End Phase", new[] { "EndPhaseTests" }),
		new SuiteDefinition("Whole Combat", new[] { "BattleCombatFlowTests" })
	};

	private Vector2 scrollPosition;

	[MenuItem("Tools/Tests/Battle Backend Runner")]
	public static void Open()
	{
		GetWindow<BattleBackendTestRunnerWindow>("Battle Tests");
	}

	[MenuItem("Tools/Tests/Run All Battle Backend Tests")]
	public static void RunAll()
	{
		RunSuite(Suites[0]);
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Battle Backend Test Runner", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox(
			"Runs the backend battle edit-mode tests programmatically through Unity's TestRunner API.",
			MessageType.Info);

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		for (int index = 0; index < Suites.Length; index++)
		{
			DrawSuiteButton(Suites[index]);
		}
		EditorGUILayout.EndScrollView();
	}

	private static void DrawSuiteButton(SuiteDefinition suite)
	{
		using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
		{
			EditorGUILayout.LabelField(suite.DisplayName, EditorStyles.boldLabel);
			EditorGUILayout.LabelField(string.Join(", ", suite.FixtureNames), EditorStyles.wordWrappedMiniLabel);

			if (GUILayout.Button($"Run {suite.DisplayName}"))
			{
				RunSuite(suite);
			}
		}
	}

	private static void RunSuite(SuiteDefinition suite)
	{
		TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
		BattleBackendTestRunCallbacks callbacks = ScriptableObject.CreateInstance<BattleBackendTestRunCallbacks>();
		callbacks.Initialize(suite.DisplayName);
		api.RegisterCallbacks(callbacks);

		api.Execute(new ExecutionSettings(new Filter
		{
			testMode = TestMode.EditMode,
			assemblyNames = new[] { TestAssemblyName },
			groupNames = BuildFixtureRegexes(suite.FixtureNames)
		}));
	}

	private static string[] BuildFixtureRegexes(IReadOnlyList<string> fixtureNames)
	{
		string[] regexes = new string[fixtureNames.Count];
		for (int index = 0; index < fixtureNames.Count; index++)
		{
			regexes[index] = $"^{fixtureNames[index]}$";
		}

		return regexes;
	}

	private readonly struct SuiteDefinition
	{
		public SuiteDefinition(string displayName, IReadOnlyList<string> fixtureNames)
		{
			DisplayName = displayName;
			FixtureNames = fixtureNames ?? Array.Empty<string>();
		}

		public string DisplayName { get; }
		public IReadOnlyList<string> FixtureNames { get; }
	}
}

public sealed class BattleBackendTestRunCallbacks : ScriptableObject, ICallbacks
{
	private string suiteName = "Battle Backend Tests";
	private readonly List<string> failures = new List<string>();

	public void Initialize(string suiteName)
	{
		this.suiteName = string.IsNullOrWhiteSpace(suiteName) ? "Battle Backend Tests" : suiteName;
		failures.Clear();
	}

	public void RunStarted(ITestAdaptor testsToRun)
	{
		Debug.Log($"[BattleBackendTestRunner] Running suite: {suiteName}");
	}

	public void RunFinished(ITestResultAdaptor result)
	{
		if (failures.Count == 0)
		{
			Debug.Log($"[BattleBackendTestRunner] Finished suite: {suiteName}. Result: {result.TestStatus}");
		}
		else
		{
			Debug.LogError(
				$"[BattleBackendTestRunner] Finished suite: {suiteName}. Result: {result.TestStatus}\n" +
				string.Join("\n", failures));
		}

		DestroyImmediate(this);
	}

	public void TestStarted(ITestAdaptor test)
	{
	}

	public void TestFinished(ITestResultAdaptor result)
	{
		if (result.TestStatus == TestStatus.Failed)
		{
			failures.Add($"FAILED {result.FullName}: {result.Message}");
		}
	}
}
