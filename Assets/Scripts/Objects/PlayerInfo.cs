using System;

[System.Serializable]
public class PlayerInfo
{

    public string playerID;
    public string playerName;

    public long timestamp;
    public long gameTime;

    public PlayerInfo()
    {
    }

    public PlayerInfo(string playerID, string playerName, long timestamp, long gameTime)
    {
        this.playerID = playerID;
        this.playerName = playerName;
        this.timestamp = timestamp;
        this.gameTime = gameTime;
    }

}