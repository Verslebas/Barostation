using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class LiquidWaterConversionSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;

    private float _updateCounter = 0f;
    private const float UpdateInterval = 0.5f;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateCounter += frameTime;
        if (_updateCounter < UpdateInterval)
            return;

        _updateCounter = 0f;

        var query = EntityQueryEnumerator<GridAtmosphereComponent>();
        while (query.MoveNext(out var uid, out var gridAtmos))
        {
            ConvertWaterToLiquidOnGrid((uid, gridAtmos));
        }
    }

    private void ConvertWaterToLiquidOnGrid(Entity<GridAtmosphereComponent> ent)
    {
        foreach (var (indices, tile) in ent.Comp.Tiles)
        {
            if (tile.Air == null || tile.MapAtmosphere)
                continue;

            var waterMoles = tile.Air.GetMoles(Gas.Water);
            var liquidWaterMoles = tile.Air.GetMoles(Gas.LiquidWater);

            if (waterMoles > 0.01f)
            {
                tile.Air.AdjustMoles(Gas.Water, -waterMoles);
                tile.Air.AdjustMoles(Gas.LiquidWater, waterMoles);
                _atmosphere.InvalidateTile(ent.Owner, indices);
            }
        }
    }
}
