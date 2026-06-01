using Robust.Shared.GameStates;

namespace Content.Shared._BaroStation.Achievements;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PlayerAchievementsComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<string> EarnedAchievements = new();
}
