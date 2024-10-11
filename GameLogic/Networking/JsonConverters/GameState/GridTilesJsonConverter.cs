using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Represents a grid state json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class GridTilesJsonConverter(GameSerializationContext context) : JsonConverter<Grid.TilesPayload>
{
    /// <inheritdoc/>
    public override Grid.TilesPayload? ReadJson(JsonReader reader, Type objectType, Grid.TilesPayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (context)
        {
            case GameSerializationContext.Player:
                return this.ReadPlayerJson(reader, serializer);
            case GameSerializationContext.Spectator:
                return this.ReadSpectatorJson(reader, serializer);
            default:
                Debug.Fail("Unknown serialization context.");
                return null;
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Grid.TilesPayload? value, JsonSerializer serializer)
    {
        switch (context)
        {
            case GameSerializationContext.Player:
                this.WritePlayerJson(writer, value, serializer);
                break;
            case GameSerializationContext.Spectator:
                this.WriteSpectatorJson(writer, value, serializer);
                break;
            default:
                Debug.Fail("Unknown serialization context.");
                break;
        }
    }

    private Grid.TilesPayload ReadPlayerJson(JsonReader reader, JsonSerializer serializer)
    {
        var jObject = JArray.Load(reader);

        var wallGrid = new Wall?[jObject.Count, (jObject[0] as JArray)!.Count];
        List<Tank> tanks = [];
        List<Bullet> bullets = [];
        List<SecondaryMapItem> items = [];

        for (int i = 0; i < jObject.Count; i++)
        {
            var row = jObject[i] as JArray;
            for (int j = 0; j < row!.Count; j++)
            {
                var cell = (row[j] as JArray)!;
                foreach (var item in cell)
                {
                    switch (item["type"]?.Value<string>())
                    {
                        case "wall":
                            var wallPayload = item["payload"] ?? new JObject();
                            wallPayload["x"] = i;
                            wallPayload["y"] = j;
                            var wall = wallPayload.ToObject<Wall>(serializer);
                            wallGrid[i, j] = wall;
                            break;
                        case "tank":
                            var tank = item["payload"]!.ToObject<Tank>(serializer)!;
                            tank.SetPosition(i, j);
                            tanks.Add(tank);
                            break;
                        case "bullet":
                            var bulletPayload = item["payload"] ?? new JObject();
                            bulletPayload["x"] = i;
                            bulletPayload["y"] = j;
                            var bullet = bulletPayload.ToObject<Bullet>(serializer)!;
                            bullets.Add(bullet);
                            break;
                        case "item":
                            var itemPayload = item["payload"]!;
                            itemPayload["x"] = i;
                            itemPayload["y"] = j;
                            var mapItem = itemPayload.ToObject<SecondaryMapItem>(serializer)!;
                            items.Add(mapItem);
                            break;
                    }
                }
            }
        }

        return new Grid.TilesPayload(wallGrid, tanks, bullets, items);
    }

    private Grid.TilesPayload ReadSpectatorJson(JsonReader reader, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var gridDimensions = jObject["gridDimensions"]!.ToObject<int[]>()!;
        var wallGrid = new Wall?[gridDimensions[0], gridDimensions[1]];
        foreach (var wall in jObject["walls"]!.ToObject<List<Wall>>(serializer)!)
        {
            wallGrid[wall.X, wall.Y] = wall;
        }

        var tanks = jObject["tanks"]!.ToObject<List<Tank>>(serializer)!;
        var bullets = jObject["bullets"]!.ToObject<List<Bullet>>(serializer)!;
        var items = jObject["items"]!.ToObject<List<SecondaryMapItem>>(serializer)!;

        return new Grid.TilesPayload(wallGrid, tanks, bullets, items);
    }

    private void WritePlayerJson(JsonWriter writer, Grid.TilesPayload? value, JsonSerializer serializer)
    {
        var visibilityGrid = (context as GameSerializationContext.Player)!.VisibilityGrid!;

        var jObject = new JArray();

        for (int i = 0; i < value!.WallGrid.GetLength(0); i++)
        {
            var row = new JArray();
            for (int j = 0; j < value.WallGrid.GetLength(1); j++)
            {
                var cell = new JArray();

                if (value.WallGrid[i, j] is { } w)
                {
                    var wall = new JObject()
                    {
                        { "type", "wall" },
                    };
                    cell.Add(wall);
                }

                row.Add(cell);
            }

            jObject.Add(row);
        }

        foreach (Tank tank in value.Tanks.Where(x => !x.IsDead))
        {
            if (!FogOfWarManager.IsElementVisible(visibilityGrid, tank.X, tank.Y))
            {
                continue;
            }

            var obj = JObject.FromObject(tank, serializer);
            (jObject[tank.X][tank.Y] as JArray)!.Add(new JObject
            {
                { "type", "tank" },
                { "payload", obj },
            });
        }

        foreach (Bullet bullet in value.Bullets)
        {
            if (!FogOfWarManager.IsElementVisible(visibilityGrid, bullet.X, bullet.Y))
            {
                continue;
            }

            var obj = JObject.FromObject(bullet, serializer);
            (jObject[bullet.X][bullet.Y] as JArray)!.Add(new JObject
            {
                { "type", "bullet" },
                { "payload", obj },
            });
        }

        foreach (SecondaryMapItem item in value.Items)
        {
            if (!FogOfWarManager.IsElementVisible(visibilityGrid, item.X, item.Y))
            {
                continue;
            }

            var obj = JObject.FromObject(item, serializer);
            (jObject[item.X][item.Y] as JArray)!.Add(new JObject
            {
                { "type", "item" },
                { "payload", obj },
            });
        }

        jObject.WriteTo(writer);
    }

    private void WriteSpectatorJson(JsonWriter writer, Grid.TilesPayload? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["bullets"] = JArray.FromObject(value!.Bullets, serializer),
            ["tanks"] = JArray.FromObject(value.Tanks, serializer),
            ["walls"] = JArray.FromObject(value.WallGrid.Cast<Wall?>().Where(w => w is not null), serializer),
            ["items"] = JArray.FromObject(value.Items, serializer),
            ["gridDimensions"] = new JArray(value.WallGrid.GetLength(0), value.WallGrid.GetLength(1)),
        };

        jObject.WriteTo(writer);
    }
}
