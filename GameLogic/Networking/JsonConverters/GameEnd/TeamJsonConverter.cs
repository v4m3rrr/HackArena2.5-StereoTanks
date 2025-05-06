using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameEnd;

#if STEREO

/// <summary>
/// Represents a team json converter.
/// </summary>
internal class TeamJsonConverter : JsonConverter<Team>
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

        return team;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Team? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["name"] = value!.Name,
            ["color"] = value.Color,
            ["score"] = value.Score,
            ["players"] = JToken.FromObject(value.Players, serializer),
        };

        jObject.WriteTo(writer);
    }
}

#endif
