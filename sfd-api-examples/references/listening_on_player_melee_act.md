# Listening on player melee action

The following code demonstrates how to listen on player melee action events.

```cs
public static void OnStartup()
{
    Game.Events.StartPlayerMeleeActionCallback(OnPlayerMeleeAction);
}

public static void OnPlayerMeleeAction(IPlayer player, PlayerMeleeHitArg[] args)
{
    // player performed a melee action. args contains all hit objects (if any).
    Game.WriteToConsole($"Player {player.UniqueID} hit {args.Length} objects during melee {(player.IsKicking ? "kick" : "attack")}");

    foreach (PlayerMeleeHitArg arg in args)
    {
        Game.WriteToConsole($"Player {player.UniqueID} hit object {arg.HitObject.UniqueID} for {arg.HitDamage} damage");
    }
}
```
