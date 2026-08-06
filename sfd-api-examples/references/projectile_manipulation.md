# Projectile manipulation

The following code demonstrates how to manipulate projectiles and listen on projectile hit events.

```cs
// Example script to manipulate projectiles
public static void OnStartup()
{
    Game.Events.StartUpdateCallback(OnUpdate);
    Game.Events.StartProjectileCreatedCallback(OnProjectileCreated);
    Game.Events.StartProjectileHitCallback(OnProjectileHit);
}

public static void OnProjectileCreated(IProjectile[] projectiles)
{
    // Created projectiles, not yet run their first update cycle.
    foreach(IProjectile projectile in projectiles) {
        Game.WriteToConsole($"Projectile {projectile.InstanceID} created");
    }
}

public static void OnProjectileHit(IProjectile projectile, ProjectileHitArgs args)
{
    Game.WriteToConsole($"Projectile {projectile.InstanceID} hit {(args.IsPlayer ? "player" : "object")} {args.HitObjectID} for {args.Damage} damage");
}

public static void OnUpdate(float ms)
{
    foreach (IProjectile proj in Game.GetProjectiles())
    {
        // lower velocity for bazooka rockets to 300
        if (proj.ProjectileItem == ProjectileItem.BAZOOKA)
        {
            if (proj.Velocity.Length() > 301f)
            {
                proj.Velocity = proj.Direction * 300f;
            }
        }

        // shotguns can only reach 200 world units
        if (proj.ProjectileItem == ProjectileItem.SHOTGUN || proj.ProjectileItem == ProjectileItem.DARK_SHOTGUN)
        {
            if (proj.TotalDistanceTraveled > 200f)
            {
                proj.FlagForRemoval();
            }
        }

        // pistols rounds affected by gravity
        if (proj.ProjectileItem == ProjectileItem.PISTOL)
        {
            proj.Velocity = new Vector2(proj.Velocity.X, proj.Velocity.Y - 0.3f * ms);
        }
    }
}
```

You can do all kinds of interesting things with projectiles. But changing position and velocity too often and too suddenly can make the projectiles look jittery and buggy on clients if the client have a fluctuating ping (which makes the client-side prediction fail - it can't predict what you want to do in your code :P). It's just how it is. Keep that in mind when testing your code in the editor vs a public game.

> **Note:** If you only want to listen on player damage you could use the `PlayerDamageCallback` event instead.
