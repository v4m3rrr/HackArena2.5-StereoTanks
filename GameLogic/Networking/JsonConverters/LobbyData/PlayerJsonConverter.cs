using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.LobbyData;

#pragma warning disable CS9113

/// <summary>
/// Represents a player json converter.
/// </summary>
internal class PlayerJsonConverter(SerializationContext context) : JsonConverter<Player>
{
    /// <inheritdoc/>
    public override Player? ReadJson(JsonReader reader, Type objectType, Player? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<string>()!;

#if STEREO
        var tankType = JsonConverterUtils.ReadEnum<TankType>(jObject["tankType"]!);
#else
        var color = jObject["color"]!.Value<uint>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
#endif

        var player = new Player(id)
        {
#if !STEREO
            Color = color,
            Nickname = nickname,
#endif
        };

#if STEREO
        player.Tank = new DeclaredTankStub(player, tankType);
#endif

        return player;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Player? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
#if STEREO
            ["tankType"] = JsonConverterUtils.WriteEnum(value.Tank.Type, context.EnumSerialization),
#else
            ["color"] = value.Color,
            ["nickname"] = value.Nickname,
#endif
        };

        jObject.WriteTo(writer);
    }
}
