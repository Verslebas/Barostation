using Robust.Shared.Network;

namespace Content.Shared._BaroStation.Achievements;

public interface IAchievementChecker
{
    bool HasAchievement(NetUserId userId, string achievementId);
}
