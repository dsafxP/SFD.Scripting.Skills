# Storage testbed

The following code demonstrates how to use the local storage feature.

```cs
int timesStarted = 0;

public void OnStartup()
{
    if (!Game.LocalStorage.TryGetItemInt("timesStarted", out timesStarted))
    {
        timesStarted = 0;
    }

    timesStarted += 1;

    Game.LocalStorage.SetItem("timesStarted", timesStarted);
    Game.ShowPopupMessage("Times started: " + timesStarted.ToString());
}

public void OnShutdown()
{
    Game.LocalStorage.SetItem("timesEnded", timesStarted);
}
```

There's 3 different types of storages:
- `Game.LocalStorage` - This storage is permanent between server restarts and between different played levels. Data is stored based on the current map/script's file path.
- `Game.SessionStorage` - This storage is maintained between games for individual level scripts. This is reset when the server changes to a different map or the server restart.
- `Game.GetSharedStorage("KEY")` - Like local storage but globally shared between scripts based on the specified key. Use this if you want to be able to store and read data between different scripts. Beware that any script can manipulate data contained within this storage!
