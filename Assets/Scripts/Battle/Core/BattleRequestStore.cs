public static class BattleRequestStore
{
    public static BattleRequest Current { get; private set; }

    public static void Set(BattleRequest request)
    {
        Current = request;
    }

    public static void Clear()
    {
        Current = null;
    }
}
