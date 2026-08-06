# Fire

The following code demonstrates how to read and remove fire.

This is a limited instruction set. All you can do is to read a limited set of data from current fire nodes and remove individual fire nodes in the game.

```cs
// Example how to create a "no-fire zone" around the middle of the map
public static void OnStartup()
{
    Game.Events.StartUpdateCallback(OnUpdate);
}

public static void OnUpdate(float ms)
{
    const float size = 32f;

    // No fire zone
    Area area = new(new(-size, -size), new(size, size));

    Game.DrawArea(area, Color.Yellow);

    FireNode[] fireNodes = Game.GetFireNodes(area);

    foreach(FireNode fireNode in fireNodes)
    {
        if (fireNode.AttachedToObjectID != 0)
        {
            Game.EndFireNode(fireNode.InstanceID);
        }
    }

    foreach(IObject obj in Game.GetBurningObjects())
    {
        if (area.Contains(obj.GetWorldPosition()))
        {
            obj.ClearFire();
        }
    }
}
```
