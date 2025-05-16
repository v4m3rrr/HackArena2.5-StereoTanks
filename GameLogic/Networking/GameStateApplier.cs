using GameLogic.ZoneStates;

namespace GameLogic.Networking;

#if CLIENT || (SERVER && HACKATHON && STEREO)

/// <summary>
/// Applies a <see cref="GameStatePayload"/> to a grid instance.
/// </summary>
internal static class GameStateApplier
{
    /// <summary>
    /// Applies a payload to the given grid, updating all state.
    /// </summary>
    /// <param name="grid">The grid to update.</param>
    /// <param name="payload">The game state payload.</param>
    public static void ApplyToGrid(Grid grid, GameStatePayload payload)
    {
        grid.OnStateUpdating();

        UpdateDimensions(grid, payload);
        UpdateTanks(grid, payload);
        UpdateBullets(grid, payload);
        UpdateLasers(grid, payload);
        UpdateMines(grid, payload);
        UpdateZones(grid, payload);

#if !STEREO
        grid.SecondaryItems.Clear();
        grid.SecondaryItems.AddRange(payload.Map.Tiles.Items);
#endif

        grid.OnStateUpdated();
    }

    private static void UpdateDimensions(Grid grid, GameStatePayload payload)
    {
        var newDim = payload.Map.Tiles.WallGrid.GetLength(0);
        if (grid.Dim != newDim)
        {
            grid.OnDimensionsChanging();
            grid.WallGrid = new Wall?[newDim, newDim];
            grid.Dim = newDim;
            grid.OnDimensionsChanged();
        }

        Array.Copy(payload.Map.Tiles.WallGrid, grid.WallGrid, payload.Map.Tiles.WallGrid.Length);
    }

    private static void UpdateTanks(Grid grid, GameStatePayload payload)
    {
        var visibleTanks = payload.Map.Tiles.Tanks;
        var visibleTankIds = visibleTanks.Select(t => t.OwnerId).ToHashSet();

        _ = grid.Tanks.RemoveAll(tank => !visibleTankIds.Contains(tank.OwnerId));

        foreach (var snapshot in visibleTanks)
        {
            var owner = payload.Players.FirstOrDefault(p => p.Id == snapshot.OwnerId);

            if (owner is null)
            {
                continue;
            }

            var existing = grid.Tanks.FirstOrDefault(t => t.OwnerId == snapshot.OwnerId);

            if (existing is null)
            {
                existing = snapshot;
                grid.Tanks.Add(snapshot);
            }

#if !STEREO
            /* Backwards compatibility */

            if (payload is not GameStatePayload.ForPlayer p || p.PlayerId == snapshot.OwnerId)
            {
                snapshot.VisibilityGrid = owner.VisibilityGrid ?? payload.Map.Visibility?.Grid;

                if (owner.IsUsingRadar is not null)
                {
                    snapshot.Radar = new RadarAbility(null!)
                    {
                        IsActive = owner.IsUsingRadar.Value,
                    };
                }
            }
#endif

            existing.UpdateFrom(snapshot);
            existing.Owner = owner;
            existing.Turret.Tank = existing;
            owner.Tank = existing;
        }
    }

    private static void UpdateBullets(Grid grid, GameStatePayload payload)
    {
        var newBullets = payload.Map.Tiles.Bullets;
        var newIds = new HashSet<int>(newBullets.Select(b => b.Id));

        _ = grid.Bullets.RemoveAll(b => !newIds.Contains(b.Id));

        foreach (var bulletSnapshot in newBullets)
        {
            var existing = grid.Bullets.FirstOrDefault(b => b.Id == bulletSnapshot.Id);

            if (existing is null)
            {
                if (bulletSnapshot.ShooterId is not null)
                {
                    bulletSnapshot.Shooter = payload.Players.FirstOrDefault(p => p.Id == bulletSnapshot.ShooterId);
                }

                grid.Bullets.Add(bulletSnapshot);
            }
            else
            {
                existing.UpdateFrom(bulletSnapshot);

                if (bulletSnapshot.ShooterId is not null)
                {
                    existing.Shooter = payload.Players.FirstOrDefault(p => p.Id == bulletSnapshot.ShooterId);
                }
            }
        }
    }

    private static void UpdateLasers(Grid grid, GameStatePayload payload)
    {
        var newLasers = payload.Map.Tiles.Lasers;
        var newIds = new HashSet<int>(newLasers.Select(l => l.Id));

        _ = grid.Lasers.RemoveAll(l => !newIds.Contains(l.Id));

        foreach (var laserSnapshot in newLasers)
        {
            var existing = grid.Lasers.FirstOrDefault(l => l.Id == laserSnapshot.Id);

            if (existing is null)
            {
                if (laserSnapshot.ShooterId is not null)
                {
                    laserSnapshot.Shooter = payload.Players.FirstOrDefault(p => p.Id == laserSnapshot.ShooterId);
                }

                grid.Lasers.Add(laserSnapshot);
            }
            else
            {
                existing.UpdateFrom(laserSnapshot);

                if (laserSnapshot.ShooterId is not null)
                {
                    existing.Shooter = payload.Players.FirstOrDefault(p => p.Id == laserSnapshot.ShooterId);
                }
            }
        }
    }

    private static void UpdateMines(Grid grid, GameStatePayload payload)
    {
        var newMines = payload.Map.Tiles.Mines;
        var newIds = new HashSet<int>(newMines.Select(m => m.Id));

        _ = grid.Mines.RemoveAll(m => !newIds.Contains(m.Id));

        foreach (var mineSnapshot in newMines)
        {
            var existing = grid.Mines.FirstOrDefault(m => m.Id == mineSnapshot.Id);

            if (existing is null)
            {
                if (mineSnapshot.LayerId is not null)
                {
                    mineSnapshot.Layer = payload.Players.FirstOrDefault(p => p.Id == mineSnapshot.LayerId);
                }

                grid.Mines.Add(mineSnapshot);
            }
            else
            {
                existing.UpdateFrom(mineSnapshot);

                if (mineSnapshot.LayerId is not null)
                {
                    existing.Layer = payload.Players.FirstOrDefault(p => p.Id == mineSnapshot.LayerId);
                }
            }
        }
    }

    private static void UpdateZones(Grid grid, GameStatePayload payload)
    {
        var newZones = payload.Map.Zones;
        var newIds = new HashSet<char>(newZones.Select(z => z.Index));

        _ = grid.Zones.RemoveAll(z => !newIds.Contains(z.Index));

        foreach (var zoneSnapshot in newZones)
        {
            var existing = grid.Zones.FirstOrDefault(z => z.Index == zoneSnapshot.Index);

            if (existing is null)
            {
#if STEREO && CLIENT
                zoneSnapshot.Shares.NormalizedByTeam = zoneSnapshot.Shares.NormalizedByTeamName.ToDictionary(
                    kvp => payload.Teams.First(t => t.Name == kvp.Key),
                    kvp => kvp.Value);
#elif !STEREO
                AttachZonePlayers(zoneSnapshot, payload.Players);
#endif
                grid.Zones.Add(zoneSnapshot);
            }
            else
            {
                existing.UpdateFrom(zoneSnapshot);
#if STEREO && CLIENT
                existing.Shares.NormalizedByTeam = zoneSnapshot.Shares.NormalizedByTeamName.ToDictionary(
                    kvp => payload.Teams.First(t => t.Name == kvp.Key),
                    kvp => kvp.Value);
#elif !STEREO
                AttachZonePlayers(existing, payload.Players);
#endif
            }
        }
    }

#if !STEREO

    private static void AttachZonePlayers(Zone zone, IEnumerable<Player> players)
    {
        switch (zone.State)
        {
            case BeingCapturedZoneState beingCaptured:
                beingCaptured.Player = players.First(p => p.Id == beingCaptured.PlayerId);
                break;

            case CapturedZoneState captured:
                captured.Player = players.First(p => p.Id == captured.PlayerId);
                break;

            case BeingContestedZoneState beingContested:
                if (beingContested.CapturedById is not null)
                {
                    beingContested.CapturedBy = players.First(p => p.Id == beingContested.CapturedById);
                }

                break;

            case BeingRetakenZoneState beingRetaken:
                beingRetaken.CapturedBy = players.First(p => p.Id == beingRetaken.CapturedById);
                beingRetaken.RetakenBy = players.First(p => p.Id == beingRetaken.RetakenById);
                break;
        }
    }

#endif
}

#endif
