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
        if (Random.value > chance)
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
        var shape = OrganicBoardShapeGenerator.BuildCells(profile.Size, seed, profile.NoiseScale, profile.NoiseStrength, profile.MinEdgeChance, profile.MinCells);
        BattleBoardData board = WorldSliceExtractor.BuildBoard(context.Map, context.PlayerPosition, shape, profile);

        var request = new BattleRequest
        {
            PlayerWorldPosition = context.PlayerPosition,
            CameraLocalPosition = cameraLocalPosition,
            Seed = seed,
            AreaProfile = profile,
            Board = board,
            EnemyTableId = settings != null ? settings.EnemyTableId : "default"
        };

        BattleRequestStore.Set(request);
        Debug.Log($"EncounterDirector: Triggering battle at {request.PlayerWorldPosition} with {request.Board?.Cells.Count ?? 0} cells.");
        battleLoading = true;
        AsyncOperation op = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        if (op != null)
        {
            op.completed += _ => battleLoading = false;
        }
    }

    private BattleAreaProfile ResolveBattleAreaProfile(BushTriggerContext context)
    {
        Vector3Int baseCell = Vector3Int.FloorToInt(context.PlayerPosition);
        for (int y = -1; y <= 2; y++)
        {
            Vector3Int cell = new Vector3Int(baseCell.x, baseCell.y + y, baseCell.z);
            if (!VoxelMapQuery.TryGetVoxel(context.Map, cell, out Voxel voxel, out _))
            {
                continue;
            }

            if (voxel.Collision != VoxelCollision.Bush)
            {
                continue;
            }

            if (voxel.BattleAreaProfile != null)
            {
                return voxel.BattleAreaProfile;
            }
        }

        return null;
    }
}
