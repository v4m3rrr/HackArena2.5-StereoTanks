using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a zone json converter.
/// </summary>
internal class ZoneSharesJsonConverter : JsonConverter<ZoneShares>
{
#if CLIENT

    /// <inheritdoc/>
    public override ZoneShares? ReadJson(JsonReader reader, Type objectType, ZoneShares? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        float neutralControl = 0f;
        if (jObject.TryGetValue("neutral", out var stateToken))
        {
            neutralControl = stateToken.ToObject<float>(serializer)!;
        }

        _ = jObject.Remove("neutral");

        return new ZoneShares()
        {
            NormalizedByTeamName = jObject.ToObject<Dictionary<string, float>>(serializer)!,
            NormalizedNeutral = neutralControl,
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ZoneShares? value, JsonSerializer serializer)
    {
    }

#endif

#if SERVER

    /// <inheritdoc/>
    public override ZoneShares? ReadJson(JsonReader reader, Type objectType, ZoneShares? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return null;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ZoneShares? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["neutral"] = value!.NeutralShare,
        };

        foreach (var team in value.ByTeam.Keys)
        {
            jObject[team.Name] = value.GetNormalized(team);
        }

        jObject.WriteTo(writer);
    }

#endif
}

#endif
