using Content.Shared._BaroStation.Achievements;
using Robust.Client.UserInterface;
using Robust.Shared.Network;

namespace Content.Client._BaroStation.Achievements;

[Virtual]
public partial class ClientAchievementsSystem : SharedAchievementsSystem, IAchievementChecker
{
    private AchievementsUIController? _uiController;

    public override void Initialize()
    {
        base.Initialize();

        AchievementChecker = this;

        var uiManager = IoCManager.Resolve<IUserInterfaceManager>();
        _uiController = uiManager.GetUIController<AchievementsUIController>();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        if (AchievementChecker == this)
            AchievementChecker = null;
    }

    public override bool HasAchievement(NetUserId userId, string achievementId)
    {
        if (_uiController != null)
        {
            return _uiController.HasAchievement(achievementId);
        }

        return false;
    }

    bool IAchievementChecker.HasAchievement(NetUserId userId, string achievementId)
    {
        return HasAchievement(userId, achievementId);
    }
}
