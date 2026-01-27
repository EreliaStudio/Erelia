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
		
	}

    private static void HandleStayInBush(BushTriggerContext context)
	{
		
	}

    private static void HandleExitBush(BushTriggerContext context)
	{
		
	}
}
