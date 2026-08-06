# Listening on object creation, damage and termination

The following code demonstrates how to listen on objects being created, damaged and terminated.

```cs
// Example script to listen on objects created, damaged and terminated.
public static void OnStartup()
{
    Game.Events.StartObjectCreatedCallback(OnObjectCreated);
    Game.Events.StartObjectDamageCallback(OnObjectDamage);
    Game.Events.StartObjectTerminatedCallback(OnObjectTerminated);

}

public static void OnObjectCreated(IObject[] objs)
{
    foreach (IObject obj in objs)
    {
        Game.WriteToConsole($"Object {obj.UniqueID} ({obj.Name}) created");
    }
}

public static void OnObjectDamage(IObject obj, ObjectDamageArgs args)
{
    // object took damage
    if (args.DamageType != ObjectDamageType.Fire)
    {
        if (args.SourceID != 0)
        {
            Game.WriteToConsole($"Object {obj.UniqueID} took {args.Damage} {args.DamageType} damage from {(args.IsPlayer ? "player" : "object")} {args.SourceID}");
        }
        else
        {
            Game.WriteToConsole($"Object {obj.UniqueID} took {args.Damage} {args.DamageType} damage");
        }
    }
}

public static void OnObjectTerminated(IObject[] objs)
{
    // objects terminated. Note: This is run just before the object is about to be destroyed or removed. To see if it was destroyed, check the IObject.DestructionInitiated property.
    foreach (IObject obj in objs)
    {
        Game.WriteToConsole($"Object {obj.UniqueID} was {(obj.DestructionInitiated ? "destroyed" : "removed")}");
    }
}
```
