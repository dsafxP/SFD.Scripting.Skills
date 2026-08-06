# Change player outfit

The following code demonstrates how to randomize a player's outfit when pressing a button.

```cs
private static readonly Random _rnd = Random.Shared;

public static void ButtonPressed(TriggerArgs args)
{
    if (args.Sender is IPlayer plr)
    {
        IProfile profile = plr.GetProfile();
        profile.ChestUnder = GetRandomItem(Game.GetClothingItemNamesHands(profile.Gender));
        profile.Legs = GetRandomItem(Game.GetClothingItemNamesLegs(profile.Gender));
        profile.Waist = GetRandomItem(Game.GetClothingItemNamesWaist(profile.Gender));
        profile.Feet = GetRandomItem(Game.GetClothingItemNamesFeet(profile.Gender));
        profile.ChestOver = GetRandomItem(Game.GetClothingItemNamesChestOver(profile.Gender));
        profile.Accessory = GetRandomItem(Game.GetClothingItemNamesAccessory(profile.Gender));
        profile.Hands = GetRandomItem(Game.GetClothingItemNamesHands(profile.Gender));
        profile.Head = GetRandomItem(Game.GetClothingItemNamesHead(profile.Gender));
        plr.SetProfile(profile);
    }
}

public static IProfileClothingItem GetRandomItem(string[] items)
{
    // a 20 % chance for nothing
    if (_rnd.NextDouble() < 0.2f || items.Length == 0)
    {
        return null;
    }

    string itemName = items[_rnd.Next(0, items.Length)];
    string colorPalette = Game.GetClothingItemColorPaletteName(itemName);
    ColorPalette cp = Game.GetColorPalette(colorPalette);
    string colorA = "", colorB = "", colorC = "";

    if (cp != null) {
        colorA = cp.PrimaryColorPackages != null && cp.PrimaryColorPackages.Length > 0 ? cp.PrimaryColorPackages[_rnd.Next(0, cp.PrimaryColorPackages.Length)] : "";
        colorB = cp.SecondaryColorPackages != null && cp.SecondaryColorPackages.Length > 0 ? cp.SecondaryColorPackages[_rnd.Next(0, cp.SecondaryColorPackages.Length)] : "";
        colorC = cp.TertiaryColorPackages != null && cp.TertiaryColorPackages.Length > 0 ? cp.TertiaryColorPackages[_rnd.Next(0, cp.TertiaryColorPackages.Length)] : "";
    }

    return new(itemName, colorA, colorB, colorC);
}
```

You can also get a profile directly from an `IObjectPlayerProfileInfo` tile in your map.

```cs
IObjectPlayerProfileInfo prof = Game.GetObject<IObjectPlayerProfileInfo>("ProfileA");

if (prof != null)
{
    plr.SetProfile(prof.GetProfile());
}
```
