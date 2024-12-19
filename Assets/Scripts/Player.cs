using System;
public class Player
{
    public uint playerId { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public int AdminLevel { get; set; }

    public Player(uint playerID, string username, string displayName, int adminLevel)
    {
        playerId = playerID;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        AdminLevel = adminLevel;
    }
}