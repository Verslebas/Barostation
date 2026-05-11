using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._BaroStation.Achievements;

[Virtual]
public partial class SharedAchievementsSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;

    public static IAchievementChecker? AchievementChecker { get; set; }

    public virtual bool HasAchievement(NetUserId userId, string achievementId)
    {
        if (AchievementChecker != null)
            return AchievementChecker.HasAchievement(userId, achievementId);

        return false;
    }

    protected bool IsValidAchievement(string achievementId)
    {
        return PrototypeManager.HasIndex<AchievementPrototype>(achievementId);
    }
}
