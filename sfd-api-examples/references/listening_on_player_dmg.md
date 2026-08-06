# Listening on player damage

The following code demonstrates how to listen on player damage events.

```cs
// Example script to read player damage
public static void OnStartup()
{
    Game.Events.StartPlayerDeathCallback(OnPlayerDeath);
    Game.Events.StartPlayerDamageCallback(OnPlayerDamage);
}

public static void OnPlayerDamage(IPlayer player, PlayerDamageArgs args)
{
    if (args.DamageType == PlayerDamageEventType.Melee && args.SourceID != 0)
    {
        IPlayer hitBy = Game.GetPlayer(args.SourceID);
        Game.WriteToConsole($"Player {player.UniqueID} took {args.Damage} melee damage from player {hitBy.UniqueID}");
    }
    else
    {
        Game.WriteToConsole($"Player {player.UniqueID} took {args.Damage} {args.DamageType} damage");
    }
}

public static void OnPlayerDeath(IPlayer player, PlayerDeathArgs args)
{
    // player just died or was removed (or both if falling outside the map while alive or gibbed while alive).
    if (args.Killed)
    {
        Game.WriteToConsole($"Player {player.UniqueID} died");
    }

    if (args.Removed)
    {
        Game.WriteToConsole(string.Format($"Player {{0}} removed", player.UniqueID));
    }
}
```
