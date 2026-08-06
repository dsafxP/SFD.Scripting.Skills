# CampingMines

Let's create a simple time-limit extension script that spawns mines under players who stand still for too long.

```cs
// CampingMines by Gurt.
// Create mines at the feet of any stale/non-moving player after a certain delay.

// OnStartup is run when the script starts running.
public void OnStartup()
{
    Game.Events.StartUpdateCallback(CheckStalePlayers, 300); // Update stale players and see what they are doing now-and-then every 300ms
}

public void CheckStalePlayers(float ms)
{
    // Don't spawn mines the first 7 seconds during startup
    if (Game.TotalElapsedGameTime < 7000)
    {
        return;
    }

    // for each ALIVE player
    IPlayer[] players = Game.GetPlayers();

    foreach (IPlayer player in players)
    {
        if (!player.IsDead && !player.IsRemoved) // failsafe
        {
            PlayerMineTracker tracker = GetPlayerMineTracker(player);
            tracker.Update();
        }
    }
}

private readonly Dictionary<int, PlayerMineTracker> m_mineTrackers = [];

private PlayerMineTracker GetPlayerMineTracker(IPlayer player)
{
    if (!m_mineTrackers.TryGetValue(player.UniqueID, out PlayerMineTracker playerMineTracker))
    {
        playerMineTracker = new PlayerMineTracker(player);
        m_mineTrackers.Add(player.UniqueID, playerMineTracker);
    }
    return playerMineTracker;
}

private class PlayerMineTracker(IPlayer player)
{
    private const float MOVE_DISTANCE_TRESHOLD = 10f;
    private const float SPAWN_MINE_TIME_MS = 3000f;

    private float m_lastMoveTime = Game.TotalElapsedGameTime;
    private Vector2 m_lastPlayerMovePosition = player.GetWorldPosition();

    public void Update()
    {
        Vector2 currentWorldPosition = player.GetWorldPosition();
        if (InSamePosition(currentWorldPosition))
        {
            if (Game.TotalElapsedGameTime - m_lastMoveTime > SPAWN_MINE_TIME_MS)
            {
                SpawnMine();
                // reset variables to prepare for next mine to spawn
                m_lastMoveTime = Game.TotalElapsedGameTime;
                m_lastPlayerMovePosition = currentWorldPosition;
            }
        }
        else
        {
            m_lastMoveTime = Game.TotalElapsedGameTime;
            m_lastPlayerMovePosition = currentWorldPosition;
        }
    }

    /// <summary>
    /// Checks if the player is within bounds of m_lastPlayerMovePosition
    /// </summary>
    private bool InSamePosition(Vector2 newPosition)
    {
        return (m_lastPlayerMovePosition - newPosition).Length() < MOVE_DISTANCE_TRESHOLD;
    }

    /// <summary>
    /// Spawns a new mine at the player's feet
    /// </summary>
    public void SpawnMine()
    {
        if (!player.IsDead && !player.IsRemoved) // failsafe
        {
            Game.CreateObject("WpnMineThrown", player.GetWorldPosition());
        }
    }
}
```
