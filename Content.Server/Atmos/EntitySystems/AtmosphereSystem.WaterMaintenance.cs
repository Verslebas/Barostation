using Content.Server.GameTicking.Events;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    private float _waterMaintenanceTimer = 0f;
    private const float WaterMaintenanceInterval = 5f;
    private const float TargetWaterPressure = 1000f; // kPa
    private const float TargetWaterTemperature = Atmospherics.T0C; // 273.15 K (0°C)

    private void InitializeWaterMaintenance()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStartingForWater);
    }

    private void OnRoundStartingForWater(RoundStartingEvent ev)
    {
        var query = EntityQueryEnumerator<GridAtmosphereComponent>();
        while (query.MoveNext(out var uid, out var gridAtmos))
        {
            MaintainWaterOnGrid((uid, gridAtmos));
        }
    }

    public void MaintainWaterOnTile(GridAtmosphereComponent gridAtmos, TileAtmosphere tile)
    {
        if (tile?.Air == null || !tile.Air.Immutable)
            return;

        var waterMoles = tile.Air.GetMoles(Gas.Water);
        if (waterMoles <= 0)
            return;

        var changed = false;
        if (Math.Abs(tile.Air.Temperature - TargetWaterTemperature) > 0.1f)
        {
            tile.Air.Temperature = TargetWaterTemperature;
            changed = true;
        }

        // n = (P * V) / (R * T)
        var targetMoles = (TargetWaterPressure * tile.Air.Volume) /
                          (Atmospherics.R * TargetWaterTemperature);
        if (Math.Abs(waterMoles - targetMoles) > 0.1f)
        {
            tile.Air.SetMoles(Gas.Water, targetMoles);
            changed = true;
        }
        if (!tile.Air.Immutable)
        {
            tile.Air.MarkImmutable();
            changed = true;
        }

        if (changed && TryComp(tile.GridIndex, out GasTileOverlayComponent? overlay))
        {
            InvalidateVisuals((tile.GridIndex, overlay), tile.GridIndices);
        }
    }

    public void MaintainWaterOnGrid(Entity<GridAtmosphereComponent> ent)
    {
        foreach (var (indices, tile) in ent.Comp.Tiles)
        {
            if (tile.Air?.GetMoles(Gas.Water) > 0)
            {
                MaintainWaterOnTile(ent.Comp, tile);
            }
        }
    }

    private void UpdateWaterMaintenance(float frameTime)
    {
        _waterMaintenanceTimer += frameTime;
        if (_waterMaintenanceTimer < WaterMaintenanceInterval)
            return;

        _waterMaintenanceTimer = 0f;

        var query = EntityQueryEnumerator<GridAtmosphereComponent>();
        while (query.MoveNext(out var uid, out var gridAtmos))
        {
            MaintainWaterOnGrid((uid, gridAtmos));
        }
    }
}
