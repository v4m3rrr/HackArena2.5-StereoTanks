using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if !STEREO

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents an item json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class ItemJsonConverter(GameSerializationContext context) : JsonConverter<SecondaryItem>
{
    /// <inheritdoc/>
    public override SecondaryItem? ReadJson(JsonReader reader, Type objectType, SecondaryItem? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var x = jObject["x"]?.Value<int>() ?? -1;
        var y = jObject["y"]?.Value<int>() ?? -1;
        var type = JsonConverterUtils.ReadEnum<SecondaryItemType>(jObject["type"]!);

        return new SecondaryItem(x, y, type);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, SecondaryItem? value, JsonSerializer serializer)
    {
        var jObject = new JObject()
        {
            ["type"] = JsonConverterUtils.WriteEnum(value!.Type, context.EnumSerialization),
        };

        if (context is GameSerializationContext.Spectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
        }

        jObject.WriteTo(writer);
    }
}

#endif
