# Creating buttons through script

In this tutorial, we'll create a button programmatically that displays a popup message with the player's name and current time when pressed.

You can test this script in an empty map. Copy and paste it into the script window and test run the map.

```cs
// OnStartup is run when the script starts running.
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create button pole
    Game.CreateObject("ButtonPole00", new Vector2(8f * (-2.5f), 8f * (-0.5f)), 0f);

    // Create the actual button - Button00 implements IObjectButtonTrigger and IObjectTrigger
    IObjectButtonTrigger customButton = (IObjectButtonTrigger)Game.CreateObject("Button00",
    new Vector2(8f * (-2.5f), 8f * (0.5f)), 0f);

    // Hook up method to run when the button is pressed
    customButton.SetScriptMethod("ButtonPressed");
    // ! ScriptMethod must be a public void method with param TriggerArgs !
}

// Will be run when the button is pressed
public static void ButtonPressed(TriggerArgs args)
{
    if (args.Sender is IPlayer player)
    {
        string playerName = player.Name;
        Game.ShowPopupMessage($"Button pressed by {playerName} at {DateTime.Now}");
    }
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B",
    new Vector2(8f * (-4.5f), 8f * (-1.5f)), 0f);

    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```
