# Victory condition

The following code demonstrates how to trigger defeat/victory for different map types based on alive players.

```cs
public static void OnStartup()
{
    if (Game.GetMapType() == MapType.Challenge)
    {
        // Challenge maps have AutoVictoryConditionEnabled true by default and will play out like a versus game.
        // Set it to false if you want to control when the user has completed/failed the challenge.
        Game.AutoVictoryConditionEnabled = false;
    }
}

// Example code to trigger victory/defeat for all map types through code if you need more advanced rules than a GameOverTrigger.
public static void CheckGameOver()
{
    switch (Game.GetMapType())
    {
        case MapType.Custom:
        {
            // Custom map types requires you to increase the score of the individual users
            bool victory = false;

            foreach (IUser user in Game.GetActiveUsers())
            {
                IPlayer plr = user.GetPlayer();
                if (plr != null && !plr.IsDead)
                {
                    user.IncreaseScore();
                    victory = true;
                }
            }

            Game.SetGameOver(victory ? "Victory!" : "Defeat");
        }
        break;

        case MapType.Survival:
        {
            // Survival map types are automatic. Just call Game.SetGameOver().
            // If anyone is alive the survival will progress to the next wave.
            // If everyone is dead the survival will restart at the current wave if any retries remains.
            Game.SetGameOver();
        }
        break;

        case MapType.Challenge:
        {
            // For challenges AutoVictoryConditionEnabled is by default true which means once every hostile enemy is dead the challenge is completed.
            // If it's false you need to set Game.ChallengeCompleted and call Game.SetGameOver() manually.
            bool victory = false;

            foreach (IUser user in Game.GetActiveUsers())
            {
                IPlayer plr = user.GetPlayer();

                if (plr != null && !plr.IsDead)
                {
                    victory = true;
                    break;
                }
            }

            Game.ChallengeCompleted = victory;
            Game.SetGameOver();
        }
        break;

        case MapType.Campaign:
        {
            // For campaigns you must decide when to progress to the next part, when the campaign is completed or if the users need to restart the current part.
            bool victory = false;

            foreach (IUser user in Game.GetActiveUsers())
            {
                IPlayer plr = user.GetPlayer();
                if (plr != null && !plr.IsDead)
                {
                    victory = true;
                    break;
                }
            }

            if (!victory)
            {
                // Restart current part again
                Game.SetCampaignMapPart(Game.CampaignCurrentMapPartIndex);
            }
            else
            {
                if ((Game.CampaignCurrentMapPartIndex + 1) == Game.CampaignTotalMapParts)
                {
                    // campaign won!
                    Game.SetGameOver("The End");
                }
                else
                {
                    // Go to next part
                    Game.SetCampaignMapPart(Game.CampaignCurrentMapPartIndex + 1);
                }
            }
        }
        break;

        case MapType.Versus:
        {
            // AutoVictoryConditionEnabled is true by default for versus maps. No need to do anything.
        }
        break;
    }
}
```
