using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO
#pragma warning disable CS9113

/// <summary>
/// Represents a team json converter.
/// </summary>
internal class TeamJsonConverter(GameSerializationContext context) : JsonConverter<Team>
{
    /// <inheritdoc/>
    public override Team? ReadJson(JsonReader reader, Type objectType, Team? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var name = jObject["name"]!.Value<string>()!;
        var color = jObject["color"]!.Value<uint>()!;
        var players = jObject["players"]!.ToObject<List<Player>>(serializer)!;

        var team = new Team(name, color, players);

        foreach (var player in players)
        {
            player.Team = team;
        }

        if (context is GameSerializationContext.Spectator || players.Any(x => context.IsPlayerWithId(x.Id)))
        {
            var score = jObject["score"]!.Value<int>()!;
            team.Score = score;
        }

        return team;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Team? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["name"] = value!.Name,
            ["color"] = value.Color,
            ["players"] = JToken.FromObject(value.Players, serializer),
        };

        if (context is GameSerializationContext.Spectator || value.Players.Any(x => context.IsPlayerWithId(x.Id)))
        {
            jObject["score"] = value.Score;
        }

        jObject.WriteTo(writer);
    }
}

#endif
