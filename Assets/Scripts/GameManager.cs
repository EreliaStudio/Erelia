using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private VoxelMap map;

    private void OnEnable()
    {
        if (map == null)
        {
            return;
        }

        map.PlayerEnteredBush += HandleEnterBush;
        map.PlayerStayInBush += HandleStayInBush;
        map.PlayerExitBush += HandleExitBush;
    }

    private void OnDisable() 
    {
        if (map == null)
        {
            return;
        }

        map.PlayerEnteredBush -= HandleEnterBush;
        map.PlayerStayInBush -= HandleStayInBush;
        map.PlayerExitBush -= HandleExitBush;
    }

    private static void HandleEnterBush(BushTriggerContext context)
    {
        Debug.Log($"Player entered bush in chunk {context.Coord}.");
    }

    private static void HandleStayInBush(BushTriggerContext context)
    {
        Debug.Log($"Player moving inside bush in chunk {context.Coord}.");
    }

    private static void HandleExitBush(BushTriggerContext context)
    {
        Debug.Log($"Player left bush in chunk {context.Coord}.");
    }
}
