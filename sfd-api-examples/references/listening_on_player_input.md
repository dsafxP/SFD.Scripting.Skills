# Listening on player input

The following code demonstrates how to listen on player key input.

```cs
// Example script to listen on player key input
public static void OnStartup()
{
    // This registers key input from players as long as the players exist in the game. This will register input from dead and disabled players too (but not removed players)!
    Game.Events.StartPlayerKeyInputCallback(OnPlayerKeyInput);
}

public static void OnPlayerKeyInput(IPlayer player, VirtualKeyInfo[] keyEvents)
{
    foreach (VirtualKeyInfo v in keyEvents)
    {
        Game.WriteToConsole($"Player {player.UniqueID} keyevent: {v}");

        // Spawn a chair at the player if the player pressed the block button while holding WALK
        if (v.Event == VirtualKeyEvent.Pressed && v.Key == VirtualKey.BLOCK && player.KeyPressed(VirtualKey.WALKING))
        {
            Game.CreateObject("Chair00", player.GetWorldPosition(), 0f, Vector2.Zero, 0f, player.FacingDirection);
        }

        // Example code to move a tile named "ABC" around by 8 units using the movement keys from any player:
        if (v.Event == VirtualKeyEvent.Pressed)
        {
            Vector2 posChange = Vector2.Zero;

            switch (v.Key)
            {
                case VirtualKey.AIM_RUN_RIGHT: posChange = Vector2.UnitX; break;
                case VirtualKey.AIM_RUN_LEFT: posChange = -Vector2.UnitX; break;
                case VirtualKey.AIM_CLIMB_UP: posChange = Vector2.UnitY; break;
                case VirtualKey.AIM_CLIMB_DOWN: posChange = -Vector2.UnitY; break;
            }

            if (posChange != Vector2.Zero)
            {
                IObject obj = Game.GetObject("ABC");

                obj?.SetWorldPosition(obj.GetWorldPosition() + posChange * 8f);
            }
        }
    }
}
```

To listen on key input you must have a player instance to listen to (a technical limitation from the very first Pre-Alpha of the game).
If you don't care about the player you could set input mode to ReadOnly for the player, disable the nametags and move the player outside the camera area.

```cs
foreach(IPlayer plr in Game.GetPlayers())
{
    //plr.SetInputMode(PlayerInputMode.ReadOnly);
    //plr.SetNametagVisible(false);
}
```

You can also check pressed keys yourself using the `IPlayer.KeyPressed(..)` function without using `Events.PlayerKeyInputCallback` depending on your needs.

```cs
IPlayer plr = ...

if (plr.KeyPressed(VirtualKey.SPRINT))
{
    ...
}
```

> **Note:** You can only read the pressed player action key. You can NOT read which physical button was pressed (e.g. if it's a keyboard/gamepad or which button it is). So if you're doing any sort of text prompt always refer to the action (e.g. "Attack", "Kick", "Block") instead of the physical buttons ("A", "F", "P") which are different depending on the user's key bindings.
