# Give/Remove player items

The following code demonstrates how to give a pistol and remove any katana from a player when the player push a button:

```cs
public static void ButtonPressed(TriggerArgs args)
{
    if (args.Sender is IPlayer plr)
    {
        if (plr.CurrentMeleeWeapon.WeaponItem == WeaponItem.KATANA)
        {
            plr.RemoveWeaponItemType(WeaponItemType.Melee);
        }

        plr.GiveWeaponItem(WeaponItem.PISTOL);
    }
}
```
