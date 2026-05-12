using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class LiquidWaterCleanupSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;

    private float _updateCounter = 0f;
    private const float UpdateInterval = 0.5f;

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
            foreach (var (indices, tile) in gridAtmos.Tiles)
            {
                if (tile.Air == null)
                    continue;

                var liquidWater = tile.Air.GetMoles(Gas.LiquidWater);
                if (liquidWater <= 0)
                    continue;
                if (tile.Space || tile.MapAtmosphere)
                {
                    tile.Air.AdjustMoles(Gas.LiquidWater, -liquidWater);
                    tile.Air.AdjustMoles(Gas.Water, liquidWater);
                    _atmosphere.InvalidateTile(uid, indices);
                }
            }
        }
    }
}
