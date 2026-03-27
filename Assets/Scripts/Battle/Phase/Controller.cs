

namespace Erelia.Battle.Phase
{
	[System.Serializable]
	public abstract class Controller
	{
		public virtual void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
		}

		public virtual void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
		}
	}
}
