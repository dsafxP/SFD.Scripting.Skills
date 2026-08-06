# Ammo management

In this part I will illustrate in the code how to manipulate player item ammo.

```cs
IPlayer plr = ... // Any player instance

// Example to set 5 rounds to the current primary weapon.
plr.SetCurrentPrimaryWeaponAmmo(5);

// Example to set primary weapon with a full magazine, loaded and ready.
plr.SetCurrentPrimaryWeaponAmmo(plr.CurrentPrimaryWeapon.MagSize * plr.CurrentPrimaryWeapon.WeaponMagCapacity, 0);

// Example to set one extra set of rounds (one spare mag or one set of extra shells for shotguns) for a weapon.
// The weapon must be reloaded after setting this.
plr.SetCurrentPrimaryWeaponAmmo(0, plr.CurrentPrimaryWeapon.WeaponMagCapacity);

// Example to set 6 bouncing rounds to the weapon. Note: Only one powerup ammo type can be active at any time for one weapon.
plr.SetCurrentPrimaryWeaponAmmo(6, ProjectilePowerup.Bouncing);

// Example to stop powerup rounds to the weapon.
plr.SetCurrentPrimaryWeaponAmmo(0, ProjectilePowerup.Bouncing);

// Example to set 2 thrown items of current throwable item
plr.SetCurrentThrownItemAmmo(2);

// Example to remove current thrown item.
plr.SetCurrentThrownItemAmmo(0);

// Example to restore melee weapon durability to 100%
plr.SetCurrentMeleeDurability(1f);
plr.SetCurrentMeleeMakeshiftDurability(1f);

// Example to set durability to 0 for current melee weapon
plr.SetCurrentMeleeDurability(0f);
```

This also allows you to create some more creative scripts like the last round in every magazine in the weapon will bounce or similar:

```cs
// Example script to give the last round in every magazine the ability to bounce if no powerup is already active.
public static void OnStartup()
{
    Game.Events.StartUpdateCallback(OnUpdate);
}

public static void OnUpdate(float ms)
{
    foreach (IPlayer plr in Game.GetPlayers())
    {
        RifleWeaponItem rifle = plr.CurrentPrimaryWeapon;

        if (rifle.PowerupBouncingRounds == 0 && rifle.PowerupFireRounds == 0)
        {
            // WeaponMagCapacity > 1 indicates it's a shotgun.
            // Let just the very last shell in shotguns be bouncing.
            if (rifle.WeaponMagCapacity == 1 && rifle.CurrentAmmo == 1 || rifle.TotalAmmo == 1)
            {
                plr.SetCurrentPrimaryWeaponAmmo(1, ProjectilePowerup.Bouncing);
            }
        }
        
        // Same logic for secondary weapons
        HandgunWeaponItem handgun = plr.CurrentSecondaryWeapon;

        if (handgun.PowerupBouncingRounds == 0 && handgun.PowerupFireRounds == 0)
        {
            // WeaponMagCapacity > 1 indicates it's a shotgun.
            // Let just the very last shell in shotguns be bouncing.
            if (handgun.WeaponMagCapacity == 1 && handgun.CurrentAmmo == 1 || handgun.TotalAmmo == 1)
            {
                plr.SetCurrentSecondaryWeaponAmmo(1, ProjectilePowerup.Bouncing);
            }
        }
    }
}
```
