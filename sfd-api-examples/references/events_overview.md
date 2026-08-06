# Events.ExplosionHitCallback

Registers an explosion hit callback event. Example:

```cs
Events.ExplosionHitCallback m_explosionHitEvent;

public void OnStartup()
{
    m_explosionHitEvent = Game.Events.StartExplosionHitCallback(OnExplosionHit);
}

public static void OnExplosionHit(ExplosionData explosion, ExplosionHitArg[] args)
{
    // explosion triggered
    foreach(ExplosionHitArg v in args)
    {
        Game.WriteToConsole($"Explosion {explosion.InstanceID} hit {(v.IsPlayer ? "player" : "object")} {v.ObjectID} for {v.Damage} damage");
    }
}
```

# Events.ObjectCreatedCallback

Registers object created callback event. Example:

```cs
Events.ObjectCreatedCallback m_objectCreatedEvent;

public void OnStartup()
{
    m_objectCreatedEvent = Game.Events.StartObjectCreatedCallback(OnObjectCreated);
}

public static void OnObjectCreated(IObject[] objs)
{
    // objects created.
    // Note: You can only start listening on objects created AFTER OnStartup is run.

    foreach(IObject obj in objs)
    {
        Game.WriteToConsole($"Object {obj.UniqueID} ({obj.Name}) was created");
    }
}
```

# Events.ObjectDamageCallback

Registers an object damage callback event. Example:

```cs
Events.ObjectDamageCallback m_objectDamageEvent;

public void OnStartup()
{
    m_objectDamageEvent = Game.Events.StartObjectDamageCallback(OnObjectDamage);
}

public static void OnObjectDamage(IObject obj, ObjectDamageArgs args) {
    // object took damage
    if (args.SourceID != 0)
    {
        Game.WriteToConsole($"Object {obj.UniqueID} took {args.Damage} {args.DamageType} damage from {(args.IsPlayer ? "player" : "object")} {args.SourceID}");
    }
    else
    {
        Game.WriteToConsole($"Object {obj.UniqueID} took {args.Damage} {args.DamageType} damage");
    }
}
```

# Events.ObjectTerminatedCallback

Registers an object terminated callback event. Example:

```cs
Events.ObjectTerminatedCallback m_objectTerminatedEvent;

public void OnStartup()
{
    m_objectTerminatedEvent = Game.Events.StartObjectTerminatedCallback(OnObjectTerminated);
}

public static void OnObjectTerminated(IObject[] objs)
{
    // objects terminated. Note: This is run just before the object is about to be destroyed or removed. To see if it was destroyed, check the IObject.DestructionInitiated property.
    foreach(IObject obj in objs)
    {
        Game.WriteToConsole($"Object {obj.UniqueID} was {(obj.DestructionInitiated ? "destroyed" : "removed")}");
    }
}
```

# Events.PlayerCreatedCallback

Registers player created callback event. Example:

```cs
Events.PlayerCreatedCallback m_playerCreatedCallback;

public void OnStartup()
{
    m_playerCreatedCallback = Game.Events.StartPlayerCreatedCallback(OnPlayerCreated);
}

public static void OnPlayerCreated(IPlayer[] players)
{
    // players created.
    // Note: You can only start listening on players created AFTER OnStartup is run.

    foreach(IPlayer ply in players)
    {
        Game.WriteToConsole($"Player {ply.UniqueID} ({ply.Name}) was created");
    }
}
```

# Events.PlayerDamageCallback

Registers a player damage callback event. Example:

```cs
Events.PlayerDamageCallback m_playerDamageEvent;

public void OnStartup()
{
    m_playerDamageEvent = Game.Events.StartPlayerDamageCallback(OnPlayerDamage);
}

public static void OnPlayerDamage(IPlayer player, PlayerDamageArgs args)
{
    // player just took damage.
    if (!args.OverkillDamage)
    {
        if (args.DamageType == PlayerDamageEventType.Melee && args.SourceID != 0)
        {
            Game.WriteToConsole($"Player {player.UniqueID} took {args.Damage} melee damage from player {args.SourceID}");
        }
        else
        {
            Game.WriteToConsole($"Player {player.UniqueID} took {args.Damage} damage (damage type {args.DamageType})");
        }
    }
    else
    {
        // player already dead and takes additional damage.
        Game.WriteToConsole($"Player {player.UniqueID} took {args.Damage} damage overkill damage");
    }
}
```

# Events.PlayerDeathCallback

Registers a player death callback event. This is run when the player dies (reach 0 HP) and when the player is removed (falling outside the map, gibbed or remove() is called on the player). Example:

```cs
Events.PlayerDeathCallback m_playerDeathEvent;

public void OnStartup()
{
    m_playerDeathEvent = Game.Events.StartPlayerDeathCallback(OnPlayerDeath);
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
        Game.WriteToConsole($"Player {player.UniqueID} was removed");
    }
}
```

# Events.PlayerKeyInputCallback

Registers a player key callback event. This registers key input from players as long as the players exist in the game. This will register input from dead and disabled players too (but not removed players)! This event is run at the end of the player update cycle. Note: Key input over network can drop late packages if the data package gets lost. This could result in an input not registering in extreme cases. Example:

```cs
Events.PlayerKeyInputCallback m_playerKeyInputEvent;

public void OnStartup()
{
    m_playerKeyInputEvent = Game.Events.StartPlayerKeyInputCallback(OnPlayerKeyInput);
}

public void OnPlayerKeyInput(IPlayer player, VirtualKeyInfo[] keyEvents) {
    // player key event registered
    foreach (VirtualKeyInfo v in keyEvents)
    {
        if (v.Event == VirtualKeyEvent.Pressed && v.Key == VirtualKey.BLOCK)
        {
            // Player just pressed the block button.

            // Call one of these to stop the event from being called again:
            Game.Events.Stop(m_playerKeyInputEvent);
            m_playerKeyInputEvent.Stop();
        }

        Game.WriteToConsole($"Player {player.UniqueID} keyevent: {v.ToString()}");
    }
}
```

# Events.PlayerMeleeActionCallback

```cs
Events.PlayerMeleeActionCallback m_playerMeleeActionEvent;

public void OnStartup()
{
    m_playerMeleeActionEvent = Game.Events.StartPlayerMeleeActionCallback(OnPlayerMeleeAction);
}

public static void OnPlayerMeleeAction(IPlayer player, PlayerMeleeHitArg[] args)
{
    // player performed a melee action
    Game.WriteToConsole($"Player {player.UniqueID} hit {args.Length} objects during melee {(player.IsKicking ? "kick" : "attack")}");

    foreach (PlayerMeleeHitArg arg in args)
    {
        Game.WriteToConsole($"Player {player.UniqueID} hit object {arg.HitObject.UniqueID} for {arg.HitDamage} damage");
    }
}
```

# Events.PlayerWeaponAddedActionCallback

Registers a player weapon added callback event (added from any source, grabbed, scripts, thrown count changed etc...). This is run at the end of the player update. Example:

```cs
Events.PlayerWeaponAddedActionCallback m_playerWeaponAddedActionEvent;

public void OnStartup()
{
    m_playerWeaponAddedActionEvent = Game.Events.StartPlayerWeaponAddedActionCallback(OnPlayerWeaponAddedAction);
}

public static void OnPlayerWeaponAddedAction(IPlayer player, PlayerWeaponAddedArg arg)
{
    // player got a weapon added
    Game.WriteToConsole(player.UniqueID, "WpnAdded", arg.WeaponItemType, arg.WeaponItem, arg.SourceObjectID);

    if (arg.SourceObjectID != 0)
    {
        // player grabbed this item from an ObjectWeaponItem in the world with ID arg.SourceObjectID
        // (NOTE: this object no longer exist in the world as the player has grabbed the item!).
    }
}
```

# Events.PlayerWeaponRemovedActionCallback

Registers a player weapon Removed callback event (removed from any source, dropped, scripts, thrown count changed etc...). This is run at the end of the player update. Example:

```cs
Events.PlayerWeaponRemovedActionCallback m_playerWeaponRemovedActionEvent;

public void OnStartup()
{
    m_playerWeaponRemovedActionEvent = Game.Events.StartPlayerWeaponRemovedActionCallback(OnPlayerWeaponRemovedAction);
}

public static void OnPlayerWeaponRemovedAction(IPlayer player, PlayerWeaponRemovedArg arg)
{
    // player got a weapon removed
    Game.WriteToConsole("WpnRemoved", player.UniqueID, arg.Dropped, arg.Thrown, arg.WeaponItemType, arg.WeaponItem, arg.TargetObjectID);

    if (arg.TargetObjectID != 0)
    {
        IObject item = Game.GetObject(arg.TargetObjectID);

        if (item != null)
        {
            // The object spawned in the world after the weapon got removed from the player's equipment (maybe the player dropped the object or threw it). Do stuff...
        }
    }
}
```

# Events.ProjectileCreatedCallback

Registers a projectile created callback event. This is run at the end of the game's update cycle. Example:

```cs
Events.ProjectileCreatedCallback m_projectileCreatedEvent;

public void OnStartup()
{
    m_projectileCreatedEvent = Game.Events.StartProjectileCreatedCallback(OnProjectileCreated);
}

public static void OnProjectileCreated(IProjectile[] projectiles)
{
    // Created projectiles, not yet run their first update cycle.
    foreach (IProjectile projectile in projectiles)
    {
        Game.WriteToConsole($"Projectile {projectile.InstanceID} created");
    }
}
```

# Events.ProjectileHitCallback

```cs
Events.ProjectileHitCallback m_projectileHitEvent;

public void OnStartup()
{
    m_projectileHitEvent = Game.Events.StartProjectileHitCallback(OnProjectileHit);
}

public static void OnProjectileHit(IProjectile projectile, ProjectileHitArgs args)
{
    // projectile hit something
    Game.WriteToConsole($"Projectile {projectile.InstanceID} hit {(args.IsPlayer ? "player" : "object")} {args.HitObjectID} for {args.Damage} damage {(args.IsDeflection ? "and was deflected" : "")} {(args.RemoveFlag ? "and will be removed" : "")}");
}
```

# Events.UpdateCallback

Registers an update callback event. Example:

```cs
Events.UpdateCallback m_updateEvent;
float m_totalElapsed = 0f;

public void OnStartup()
{
    m_updateEvent = Game.Events.StartUpdateCallback(OnUpdate, 0);
}

public void OnUpdate(float elapsed)
{
    m_totalElapsed += elapsed;

    if (m_totalElapsed > 5000)
    {
        m_updateEvent.Stop();
        m_updateEvent = null;
    }
}
```

Example 2 - fire and forget code style to run some code once after some arbitary time:

```cs
public static void OnStartup()
{
    float a = 1f;
    
    Game.Events.StartUpdateCallback(e =>
    {
        if (a > 0f)
        {
            Game.WriteToConsole("Fire-and-forget style of code");
        }
    }, 10000, 1); // after 10 seconds, run code once
}
```

# Events.UserJoinCallback

Registers UserJoin callback event. Example:

```cs
Events.UserJoinCallback m_userJoinCallback;

public void OnStartup()
{
    m_userJoinCallback = Game.Events.StartUserJoinCallback(OnUserJoin);
}

public static void OnUserJoin(IUser[] users)
{
    foreach (IUser user in users)
    {
        Game.WriteToConsole($"User {user.Name} joined");
    }
}
```

# Events.UserLeaveCallback

Registers UserLeave callback event. Example:

```cs
Events.UserLeaveCallback m_userLeaveCallback;

public void OnStartup()
{
    m_userLeaveCallback = Game.Events.StartUserLeaveCallback(OnUserLeave);
}

public static void OnUserLeave(IUser[] users, DisconnectionType type)
{
    foreach (IUser user in users)
    {
        Game.WriteToConsole($"User {user.Name} {type}");
    }
}
```

# Events.UserMessageCallback

Registers a user message callback event. This allows you to listen to user chat messages. You are not be able to modify the actual content of the message nor listen to whispers/team chats. You will be able to read messages with or without a leading "/" (commands). Messages with a leading "/" (commands) will first be handled as SFD commands. If none SFD command is found then the command will be available in your script for processing.

If you plan to use commands in your script you should consider using a unique prefix to all your commands so you know which commands your script should parse and handle. If you plan to add a command "showmap" (by typing "/showmap") it's unwise to do so in case SFD implements its own command "/showmap" in the future. Instead use a prefix like "/wm showmap" ("wm" could stand for "wizard mod" or some other acronym for example). Example:

```cs
Events.UserMessageCallback m_userMessageCallback;

public void OnStartup()
{
    m_userMessageCallback = Game.Events.StartUserMessageCallback(OnUserMessage);
}

public static void OnUserMessage(UserMessageCallbackArgs args)
{
    // user just said something in the chat.
    if (args.IsCommand && args.Command == "ABC")
    {
        // Do something specific for ABC
        if (args.CommandArguments == "1")
        {
            // Do stuff when user types "/ABC 1"
        }
        else if (args.CommandArguments == "2")
        {
            // Do stuff when user types "/ABC 2"
        }
    }
}
```
