using System;
using UnityEngine;

public class BattlePlacementController : MonoBehaviour
{
    [SerializeField] private BattleBoard battleBoard;
    [SerializeField] private Camera battleCamera;
    [SerializeField] private LayerMask boardLayerMask = -1;
    [SerializeField] private float maxDistance = 500f;

    public event Action<int, CreatureData, Vector3Int> CreaturePlaced;

    public bool TryPlaceCreature(int teamIndex, CreatureData creature, Vector2 screenPoint)
    {
        if (creature == null)
        {
            return false;
        }

        if (battleCamera == null || battleBoard == null || battleBoard.Data == null)
        {
            return false;
        }

        Ray ray = battleCamera.ScreenPointToRay(screenPoint);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, boardLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (!TryGetPlacementCell(hit.point, out Vector3Int cell))
        {
            return false;
        }

        CreaturePlaced?.Invoke(teamIndex, creature, cell);
        return true;
    }

    private bool TryGetPlacementCell(Vector3 worldPoint, out Vector3Int cell)
    {
        cell = default;
        if (battleBoard == null || battleBoard.Data == null)
        {
            return false;
        }

        Vector3 localPoint = worldPoint - battleBoard.transform.position;
        int cellX = Mathf.FloorToInt(localPoint.x);
        int cellZ = Mathf.FloorToInt(localPoint.z);

        BattleBoardData data = battleBoard.Data;
        if (cellX < 0 || cellX >= data.SizeX || cellZ < 0 || cellZ >= data.SizeZ)
        {
            return false;
        }

        for (int y = data.SizeY - 1; y >= 0; y--)
        {
            if (data.TryGetMaskCell(cellX, y, cellZ, out BattleCell maskCell)
                && maskCell != null
                && maskCell.HasMask(BattleCellMask.Placement))
            {
                cell = new Vector3Int(cellX, y, cellZ);
                return true;
            }
        }

        return false;
    }
}
