using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class EncounterDirector : MonoBehaviour
{
    [SerializeField] private VoxelMap map;
    [SerializeField] private EncounterSettings settings;
    [SerializeField] private BattleAreaProfile defaultAreaProfile;
    [SerializeField] private string battleSceneName = "Battle";
    [SerializeField] private bool triggerOnStay = true;

    private float lastRollTime;
    private bool battleLoading;

    private void OnEnable()
    {
        if (map == null)
        {
            return;
        }

        map.PlayerEnteredBush += HandleEnterBush;
        map.PlayerStayInBush += HandleStayInBush;
    }

    private void OnDisable()
    {
        if (map == null)
        {
            return;
        }

        map.PlayerEnteredBush -= HandleEnterBush;
        map.PlayerStayInBush -= HandleStayInBush;
    }

    private void HandleEnterBush(BushTriggerContext context)
    {
        TryRoll(context, settings != null ? settings.ChanceOnEnter : 0f);
    }

    private void HandleStayInBush(BushTriggerContext context)
    {
        if (!triggerOnStay)
        {
            return;
        }

        TryRoll(context, settings != null ? settings.ChanceOnMove : 0f);
    }

    private void TryRoll(BushTriggerContext context, float chance)
    {
        if (chance <= 0f)
        {
            return;
        }

        if (settings != null && Time.time - lastRollTime < settings.RollCooldownSeconds)
        {
            return;
        }

        lastRollTime = Time.time;
        if (UnityEngine.Random.value > chance)
        {
            return;
        }

        StartBattle(context);
    }

    private void StartBattle(BushTriggerContext context)
    {
        if (battleLoading || SceneManager.GetSceneByName(battleSceneName).isLoaded)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        Vector3 cameraLocalPosition = Vector3.zero;
        if (mainCamera != null)
        {
            cameraLocalPosition = mainCamera.transform.localPosition;
        }

        BattleAreaProfile profile = ResolveBattleAreaProfile(context) ?? defaultAreaProfile;
        if (profile == null)
        {
            Debug.LogWarning("EncounterDirector: No BattleAreaProfile assigned.");
            return;
        }

        int seed = Mathf.Abs((int)(Time.time * 1000f)) ^ context.Player.GetInstanceID();
        int radius = Mathf.Max(1, profile.Size);
        int cornerRadius = Mathf.Max(1, Mathf.CeilToInt(radius / 3f));
        var shape = BuildShape(profile, radius, cornerRadius);
        BattleBoardData battleBoard = WorldSliceExtractor.BuildBattleBoard(context.Map, context.PlayerPosition, shape, profile);

        var request = new BattleRequest
        {
            PlayerWorldPosition = context.PlayerPosition,
            CameraLocalPosition = cameraLocalPosition,
            Seed = seed,
            AreaProfile = profile,
            BattleBoard = battleBoard,
            Registry = context.Map != null ? context.Map.Registry : null,
            EnemyTableId = settings != null ? settings.EnemyTableId : "default"
        };

        BattleRequestStore.Set(request);
        
		battleLoading = true;
        AsyncOperation op = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        if (op != null)
        {
            op.completed += _ => battleLoading = false;
        }
    }

    private BattleAreaProfile ResolveBattleAreaProfile(BushTriggerContext context)
    {
        if (context.BushIslandConfiguration == null)
        {
            Debug.LogWarning("EncounterDirector: Missing BushIslandConfiguration on bush collider.");
            return null;
        }

        BattleAreaProfile profile = context.BushIslandConfiguration.Configuration?.AreaProfile;
        if (profile == null)
        {
            Debug.LogWarning("EncounterDirector: BushIslandConfiguration has no BattleAreaProfile assigned.");
        }

        return profile;
    }

    private static HashSet<Vector2Int> BuildShape(BattleAreaProfile profile, int radius, int cornerRadius)
    {
        return RoundedSquareShapeGenerator.BuildCells(radius, cornerRadius);
    }
}
