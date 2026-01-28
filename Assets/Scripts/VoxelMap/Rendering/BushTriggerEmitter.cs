using System.Collections.Generic;
using UnityEngine;

public readonly struct BushTriggerContext
{
    public VoxelMap Map { get; }
    public ChunkCoord Coord { get; }
    public Transform Player { get; }
    public Collider PlayerCollider { get; }
    public Vector3 PlayerPosition { get; }

    public BushTriggerContext(VoxelMap map, ChunkCoord coord, Transform player, Collider playerCollider, Vector3 playerPosition)
    {
        Map = map;
        Coord = coord;
        Player = player;
        PlayerCollider = playerCollider;
        PlayerPosition = playerPosition;
    }
}

[RequireComponent(typeof(Collider))]
public class BushTriggerEmitter : MonoBehaviour
{
    private VoxelMap ownerMap;
    private ChunkCoord coord;
    private Rigidbody cachedBody;
    private readonly Dictionary<int, Vector3Int> lastPlayerCells = new Dictionary<int, Vector3Int>();

    public void Configure(VoxelMap owner, ChunkCoord chunkCoord)
    {
        ownerMap = owner;
        coord = chunkCoord;
        EnsureKinematicRigidbody();
    }

    private void EnsureKinematicRigidbody()
    {
        if (cachedBody == null)
        {
            cachedBody = GetComponent<Rigidbody>();
        }

        if (cachedBody == null)
        {
            cachedBody = gameObject.AddComponent<Rigidbody>();
        }

        cachedBody.isKinematic = true;
        cachedBody.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ownerMap == null || other == null)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3Int cell = Vector3Int.FloorToInt(playerPosition);
        int playerId = player.gameObject.GetInstanceID();
        lastPlayerCells[playerId] = cell;
        var context = new BushTriggerContext(ownerMap, coord, player.transform, other, playerPosition);
        ownerMap.NotifyPlayerEnteredBush(context);
    }

    private void OnTriggerStay(Collider other)
    {
        if (ownerMap == null || other == null)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }
		Debug.Log("Coucou this is the OnTriggerStay");

        Vector3 playerPosition = player.transform.position;
        Vector3Int cell = Vector3Int.FloorToInt(playerPosition);
        int playerId = player.gameObject.GetInstanceID();
        if (lastPlayerCells.TryGetValue(playerId, out Vector3Int lastCell))
        {
            if (cell == lastCell)
            {
                return;
            }
        }
        lastPlayerCells[playerId] = cell;
        var context = new BushTriggerContext(ownerMap, coord, player.transform, other, playerPosition);
        ownerMap.NotifyPlayerStayInBush(context);
    }

    private void OnTriggerExit(Collider other)
    {
        if (ownerMap == null || other == null)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        int playerId = player.gameObject.GetInstanceID();
        lastPlayerCells.Remove(playerId);
        var context = new BushTriggerContext(ownerMap, coord, player.transform, other, player.transform.position);
        ownerMap.NotifyPlayerExitBush(context);
    }
}
