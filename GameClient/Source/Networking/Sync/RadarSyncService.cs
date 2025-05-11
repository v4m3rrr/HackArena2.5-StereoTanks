using GameClient.Sprites;
using GameLogic;

namespace GameClient.Networking;

/// <summary>
/// Synchronizes radar effect sprites based on active radar abilities in tanks.
/// </summary>
/// <param name="parent">The grid component providing access to tanks and all sprites.</param>
/// <param name="radarEffects">The list of radar effect sprites currently rendered.</param>
internal class RadarSyncService(GridComponent parent, List<RadarEffect> radarEffects)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var tank in parent.Logic.Tanks)
        {
            var radar = tank.GetAbility<RadarAbility>();
            if (radar?.IsActive ?? false)
            {
                var effect = new RadarEffect(tank, parent, parent.AllSprites);
                radarEffects.Add(effect);
            }

            _ = radarEffects.RemoveAll(e => e.IsExpired);
        }
    }
}
