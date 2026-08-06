# Visual debugging

The following code demonstrates how to draw lines, circles and areas for debugging information on the next drawn frame while testing your map in the map editor. This debug information only works while testing your map in the map editor! It also requires you to enable "Show Script Debug Information" in the map debug options menu (F7).

The visual debug information is cleared after the next drawn frame so if you want to constantly monitor some information you need to keep drawing it in an update loop.

```cs
public void OnStartup()
{
    if (Game.IsEditorTest)
    {
        Game.Events.StartUpdateCallback(DrawDebugOnUpdate);
    }
}

public void DrawDebugOnUpdate(float ms)
{
    Game.DrawLine(Vector2.Zero, Vector2.One * 10f, Color.Red);
    Game.DrawLine(Vector2.Zero, Vector2.One * -10f, Color.Blue);

    Game.DrawCircle(Vector2.Zero, 1f);
    Game.DrawCircle(Vector2.Zero, 2f, Color.Yellow);
    Game.DrawCircle(Vector2.Zero, 4f, Color.Red);
    Game.DrawCircle(Vector2.One * 10f, 4f, Color.Red);

    Game.DrawText("ABC", Vector2.Zero, Color.Yellow);

    Game.DrawArea(Game.GetObject("AT").GetAABB(), Color.Yellow);
}
```
