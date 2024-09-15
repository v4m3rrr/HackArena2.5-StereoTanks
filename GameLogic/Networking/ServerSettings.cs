namespace GameLogic.Networking;

public record class ServerSettings(
    int GridDimension,
    int NumberOfPlayers,
    int Seed,
    int BroadcastInterval,
    bool EagerBroadcast);
