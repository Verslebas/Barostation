using Content.Server.Weather;
using Content.Server.Atmos.Components;
using Content.Shared.GameTicking;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class WaterWeatherSystem : EntitySystem
{
    [Dependency] private WeatherSystem _weather = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WaterWeatherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WaterWeatherComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnMapInit(Entity<WaterWeatherComponent> ent, ref MapInitEvent args)
    {
        _weather.TryAddWeather(ent, ent.Comp.WeatherPrototype, out _, null);
    }

    private void OnComponentRemove(Entity<WaterWeatherComponent> ent, ref ComponentRemove args)
    {
        _weather.TryRemoveWeather(ent, ent.Comp.WeatherPrototype);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
    }
}
