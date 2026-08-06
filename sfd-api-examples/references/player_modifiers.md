# Player modifiers

The following code demonstrates how to update some modifiers for all players on startup.
See the ScriptAPI documentation for a full list of available modifiers.

```cs
public static void AfterStartup()
{
    foreach(IPlayer plr in Game.GetPlayers())
    {
        PlayerModifiers modifiers = plr.GetModifiers();
        
        modifiers.MaxHealth = 200;
        modifiers.EnergyRechargeModifier = 2;
        modifiers.CanBurn = 0;
        
        plr.SetModifiers(modifiers);
    }
}
```
