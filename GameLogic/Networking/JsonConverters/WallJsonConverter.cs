using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a wall json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class WallJsonConverter(SerializationContext context) : JsonConverter<Wall>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Wall? ReadJson(JsonReader reader, Type objectType, Wall? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        return new Wall()
        {
            X = jObject["x"]?.Value<int>()! ?? -1,
            Y = jObject["y"]?.Value<int>()! ?? -1,
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Wall? value, JsonSerializer serializer)
    {
        var jObject = new JObject();

        if (this.context is SerializationContext.Spectator)
        {
            jObject["x"] = value!.X;
            jObject["y"] = value!.Y;
        }

        jObject.WriteTo(writer);
    }
}
