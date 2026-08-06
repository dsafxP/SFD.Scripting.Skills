# RayCast and HitTest

The following code demonstrates how to perform RayCasts in the world between two points.

```cs
// Example script how to RayCast in the world from a player and show some debugging information about the hit objects while testing the map in the editor.
public static void OnStartup()
{
    Game.Events.StartUpdateCallback(OnUpdate);
}

public static void OnUpdate(float ms)
{
    IUser[] users = Game.GetActiveUsers();
    IPlayer plr = users != null && users.Length > 0 ? users[0].GetPlayer() : null; // any player instance

    if (plr != null)
    {
        Vector2 worldPos = plr.GetWorldPosition() + Vector2.UnitY * 12f;
        Vector2 worldPosEnd = worldPos + plr.AimVector * 100f;

        if (Game.IsEditorTest)
        {
            Game.DrawLine(worldPos, worldPosEnd, Color.Red);
        }

        // RayCastInput offers some filtering capabilities, for now just filter on everything with a set CategoryBit (effectively ignoring background objects).
        RayCastInput rci = new() { IncludeOverlap = true, MaskBits = 0xFFFF, FilterOnMaskBits = true };

        // You can also filter on specific types. If you only want to raycast players you would add the IPlayer type to the Types array property: rci.Types = new Type[1] { typeof(IPlayer) };
        RayCastResult[] results = Game.RayCast(worldPos, worldPosEnd, rci);

        foreach (RayCastResult result in results)
        {

            if (Game.IsEditorTest)
            {
                Game.DrawCircle(result.Position, 1f, Color.Yellow);

                Game.DrawLine(result.Position, result.Position + result.Normal * 5f, Color.Yellow);
                Game.DrawArea(result.HitObject.GetAABB(), Color.Yellow);
                Game.DrawText(result.HitObject.UniqueID.ToString(), result.Position, Color.Yellow);
            }

            // Destroy nearby glass that the player is looking at
            if (result.Fraction < 0.3f && result.HitObject.Name.Contains("glass", StringComparison.OrdinalIgnoreCase))
            {
                result.HitObject.Destroy();
            }
        }
    }
}
```

To perform a HitTest on any `IObject` simply use the `bool` `IObject.HitTest(Vector2 position)` function. You can also perform RayCasts on individual `IObject`s.

```cs
IObject obj = ... // any IObject instance

if (obj.HitTest(Vector2.Zero))
{
    // Object overlaps the center of the world
}

Vector2 pA = Vector2.Zero;
Vector2 pB = Vector2.UnitX * 100f;

if (obj.RayCast(pA, pB, true).Hit)
{
    // Object hit between pA and pB
}
```
