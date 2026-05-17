using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences;
using Robust.Shared.Player;
using Content.Shared._BaroStation.Achievements;

namespace Content.Server._BaroStation.Achievements;

public static class LoadoutValidation
{
    public static bool ValidateAchievementLoadoutEffect(
        AchievementLoadoutEffect effect,
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session)
    {
        if (session == null)
        {
            return true;
        }

        var system = AchievementsServerSystem.Instance;

        if (system == null)
        {
            return false;
        }

        var hasAchievement = system.HasAchievement(session.UserId, effect.AchievementId);

        return hasAchievement;
    }
}
