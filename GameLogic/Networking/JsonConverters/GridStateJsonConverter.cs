using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Represents a grid state json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class GridStateJsonConverter(SerializationContext context) : JsonConverter<Grid.StatePayload>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Grid.StatePayload? ReadJson(JsonReader reader, Type objectType, Grid.StatePayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (this.context)
        {
            case SerializationContext.Player:
                return this.ReadPlayerJson(reader, serializer);
            case SerializationContext.Spectator:
                return this.ReadSpectatorJson(reader, serializer);
            default:
                Debug.Fail("Unknown serialization context.");
                return null;
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Grid.StatePayload? value, JsonSerializer serializer)
    {
        switch (this.context)
        {
            case SerializationContext.Player:
                this.WritePlayerJson(writer, value, serializer);
                break;
            case SerializationContext.Spectator:
                this.WriteSpectatorJson(writer, value, serializer);
                break;
            default:
                Debug.Fail("Unknown serialization context.");
                break;
        }
    }

    private Grid.StatePayload ReadPlayerJson(JsonReader reader, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var grid = (JArray)jObject["grid"]!;

        var wallGrid = new Wall?[grid.Count, (grid[0] as JArray)!.Count];
        List<Tank> tanks = [];
        List<Bullet> bullets = [];

        for (int i = 0; i < grid.Count; i++)
        {
            var row = grid[i] as JArray;
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
                    }
                }
            }
        }

        var zones = new List<Zone>();
        foreach (var zone in jObject["zones"]!)
        {
            zones.Add(zone.ToObject<Zone>(serializer)!);
        }

        return new Grid.StatePayload
        {
            WallGrid = wallGrid,
            Tanks = tanks,
            Bullets = bullets,
            Zones = zones,
        };
    }

    private Grid.StatePayload ReadSpectatorJson(JsonReader reader, JsonSerializer serializer)
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
        var zones = jObject["zones"]!.ToObject<List<Zone>>(serializer)!;

        return new Grid.StatePayload
        {
            WallGrid = wallGrid,
            Tanks = tanks,
            Bullets = bullets,
            Zones = zones,
        };
    }

    private void WritePlayerJson(JsonWriter writer, Grid.StatePayload? value, JsonSerializer serializer)
    {
        var visibilityGrid = (this.context as SerializationContext.Player)!.VisibilityGrid!;

        var jObject = new JObject
        {
            ["grid"] = new JArray(),
            ["zones"] = JArray.FromObject(value!.Zones, serializer),
        };

        var grid = (JArray)jObject["grid"]!;
        for (int i = 0; i < value!.WallGrid.GetLength(0); i++)
        {
            var row = new JArray();
            for (int j = 0; j < value.WallGrid.GetLength(1); j++)
            {
                var cell = new JArray();

                if (value.WallGrid[i, j] is { } w)
                {
                    /* var obj = JObject.FromObject(w, serializer); */
                    var wall = new JObject()
                    {
                        { "type", "wall" },
                        /* { "payload", obj }, */
                    };
                    cell.Add(wall);
                }

                row.Add(cell);
            }

            grid.Add(row);
        }

        foreach (Tank tank in value.Tanks.Where(x => !x.IsDead))
        {
            if (!FogOfWarManager.IsElementVisible(visibilityGrid, tank.X, tank.Y))
            {
                continue;
            }

            var obj = JObject.FromObject(tank, serializer);
            (grid[tank.X][tank.Y] as JArray)!.Add(new JObject
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
            (grid[bullet.X][bullet.Y] as JArray)!.Add(new JObject
            {
                { "type", "bullet" },
                { "payload", obj },
            });
        }

        jObject.WriteTo(writer);
    }

    private void WriteSpectatorJson(JsonWriter writer, Grid.StatePayload? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["bullets"] = JArray.FromObject(value!.Bullets, serializer),
            ["tanks"] = JArray.FromObject(value.Tanks, serializer),
            ["walls"] = JArray.FromObject(value.WallGrid.Cast<Wall?>().Where(w => w is not null), serializer),
            ["zones"] = JArray.FromObject(value.Zones, serializer),
            ["gridDimensions"] = new JArray(value.WallGrid.GetLength(0), value.WallGrid.GetLength(1)),
        };

        jObject.WriteTo(writer);
    }
}
