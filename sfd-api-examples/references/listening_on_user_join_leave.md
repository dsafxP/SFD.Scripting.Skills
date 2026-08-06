# Listening on user join and leave

Scripting in SFD assumes you have a fair knowledge of C#.

The following code demonstrates how to listen on user joining and leaving mid-game.

Code:

```cs
Events.UserJoinCallback m_userJoinCallback;
Events.UserLeaveCallback m_userLeaveCallback;

public void OnStartup()
{
    m_userJoinCallback = Game.Events.StartUserJoinCallback(OnUserJoin);
    m_userLeaveCallback = Game.Events.StartUserLeaveCallback(OnUserLeave);
}

public static void OnUserJoin(IUser[] users)
{
    foreach (IUser user in users)
    {
        Game.WriteToConsole($"User {user.Name} joined");
    }
}

public static void OnUserLeave(IUser[] users, DisconnectionType type)
{
    foreach (IUser user in users)
    {
        Game.WriteToConsole($"User {user.Name} {type}");
    }
}
```
