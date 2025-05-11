using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a grid state json converter.
/// </summary>
internal class GridVisibilityJsonConverter() : JsonConverter<VisibilityPayload>
{
    /// <inheritdoc/>
    public override VisibilityPayload? ReadJson(JsonReader reader, Type objectType, VisibilityPayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JArray.Load(reader);

        var height = jObject.Count;
        var width = ((string)jObject[0])!.Length;

        var visibilityGrid = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            var row = (string)jObject[y]!;
            for (int x = 0; x < width; x++)
            {
                visibilityGrid[x, y] = row[x] == '1';
            }
        }

        return new VisibilityPayload(visibilityGrid);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, VisibilityPayload? value, JsonSerializer serializer)
    {
        var jObject = new JArray();

        var width = value!.Grid.GetLength(0);
        var height = value!.Grid.GetLength(1);

        var sb = new StringBuilder(width);

        for (int y = 0; y < height; y++)
        {
            _ = sb.Clear();

            for (int x = 0; x < width; x++)
            {
                _ = sb.Append(value.Grid[x, y] ? '1' : '0');
            }

            jObject.Add(sb.ToString());
        }

        jObject.WriteTo(writer);
    }
}
