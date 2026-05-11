using System.Diagnostics.CodeAnalysis;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences.Loadouts.Effects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._BaroStation.Achievements;

public sealed partial class AchievementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public string AchievementId = string.Empty;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (session == null)
        {
            return true;
        }

        if (SharedAchievementsSystem.AchievementChecker != null)
        {
            var hasAchievement = SharedAchievementsSystem.AchievementChecker.HasAchievement(session.UserId, AchievementId);

            if (hasAchievement)
            {
                return true;
            }
        }
        else
        {
            var entitySystemManager = collection.Resolve<IEntitySystemManager>();
            if (entitySystemManager.TryGetEntitySystem<SharedAchievementsSystem>(out var achievementsSystem))
            {
                var hasAchievement = achievementsSystem.HasAchievement(session.UserId, AchievementId);

                if (hasAchievement)
                {
                    return true;
                }
            }
        }

        var achievementName = GetAchievementName(collection, AchievementId);
        reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-achievement-not-earned",
            ("achievement", achievementName)));
        return false;
    }

    private string GetAchievementName(IDependencyCollection collection, string achievementId)
    {
        var prototypeManager = collection.Resolve<IPrototypeManager>();
        if (prototypeManager.TryIndex<AchievementPrototype>(achievementId, out var achievementProto))
        {
            return Loc.GetString(achievementProto.Name);
        }
        return achievementId;
    }

    public override void Apply(RoleLoadout loadout)
    {
    }
}
