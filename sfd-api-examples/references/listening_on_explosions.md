# Listening on explosions

The following code demonstrates how to listen on triggered explosions.

```cs
Events.ExplosionHitCallback m_explosionHitEvent;

public void OnStartup()
{
    m_explosionHitEvent = Game.Events.StartExplosionHitCallback(OnExplosionHit);
}

public static void OnExplosionHit(ExplosionData explosion, ExplosionHitArg[] args)
{
    // explosion triggered
    foreach (ExplosionHitArg v in args)
    {
        switch (v.HitType)
        {
            case ExplosionHitType.Damage:
            Game.WriteToConsole($"Explosion {explosion.InstanceID} hit {(v.IsPlayer ? "player" : "object")} {v.ObjectID} for {v.Damage} damage");
            break;

            case ExplosionHitType.Shockwave:
            Game.WriteToConsole($"Explosion {explosion.InstanceID} pushed {(v.IsPlayer ? "player" : "object")} {v.ObjectID}");
            break;

            case ExplosionHitType.None:
            Game.WriteToConsole(
            $"Explosion {explosion.InstanceID} overlapped {(v.IsPlayer ? "player" : "object")} {v.ObjectID}");
            break;
        }
    }
}
```
