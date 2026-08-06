# Listening on player creation and termination

Scripting in SFD assumes you have a fair knowledge of C#.

The following code demonstrates how to listen on player creation (when a player is created) and termination (when a player is killed/destroyed/removed).

Code:

```cs
Events.PlayerCreatedCallback m_playerCreatedCallback;
Events.ObjectTerminatedCallback m_objectTerminatedEvent;

public void OnStartup()
{
    foreach (IPlayer player in Game.GetPlayers())
    {
        Game.WriteToConsole($"Player {player.UniqueID} ({player.Name}) was created at startup.");
    }

    m_playerCreatedCallback = Game.Events.StartPlayerCreatedCallback(OnPlayerCreated);
    m_objectTerminatedEvent = Game.Events.StartObjectTerminatedCallback(OnObjectTerminated);
}

public static void OnPlayerCreated(IPlayer[] players)
{
    foreach (IPlayer player in players)
    {
        Game.WriteToConsole($"Player {player.UniqueID} ({player.Name}) was created post startup.");
    }
}

public static void OnObjectTerminated(IObject[] objs)
{
    foreach (IObject obj in objs)
    {
        IPlayer player = Game.GetPlayer(obj.UniqueID);

        if (player == null)
        continue;

        Game.WriteToConsole($"Player {player.UniqueID} ({player.Name}) was terminated.");
    }
}
```
