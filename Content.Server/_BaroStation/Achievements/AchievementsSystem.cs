using Content.Server.Database;
using Content.Shared._BaroStation.Achievements;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server._BaroStation.Achievements;

public sealed partial class AchievementsSystem : SharedAchievementsSystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IServerDbManager _dbManager = default!;

    private Dictionary<string, HashSet<string>> _playerAchievements = new();
    public static AchievementsSystem? Instance { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        Instance = this;

        SubscribeNetworkEvent<RequestAchievementsMessage>(OnRequestAchievements);
        SubscribeLocalEvent<InventoryComponent, DidEquipEvent>(OnDidEquip);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Instance = null;
        _playerAchievements.Clear();
    }

    private async void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        await LoadAndApplyAchievementsAsync(ev.Player, ev.Mob, ev.Player.UserId);

        await SendAchievementsToClient(ev.Player);
    }

    private async Task LoadAndApplyAchievementsAsync(ICommonSession session, EntityUid mob, NetUserId userId)
    {
        try
        {
            var earnedIds = await _dbManager.GetPlayerAchievementsAsync(userId);
            var earnedSet = new HashSet<string>(earnedIds);

            _playerAchievements[userId.ToString()] = earnedSet;

            var achievementComp = EnsureComp<PlayerAchievementsComponent>(mob);
            achievementComp.EarnedAchievements = earnedSet;
            Dirty(mob, achievementComp);
        }
        catch (Exception)
        {
        }
    }

    public override bool HasAchievement(NetUserId userId, string achievementId)
    {
        if (!_playerAchievements.TryGetValue(userId.ToString(), out var achievements))
            return false;

        return achievements.Contains(achievementId);
    }

    private void OnDidEquip(EntityUid uid, InventoryComponent component, DidEquipEvent args)
    {
        if (!TryComp<PlayerAchievementsComponent>(uid, out var achievementComp))
            return;

        var prototypeId = MetaData(args.Equipment).EntityPrototype?.ID;

        if (string.IsNullOrEmpty(prototypeId))
            return;

        if (prototypeId == "ClothingMaskClown")
        {
            GrantAchievement(uid, achievementComp, "ClownMaskAchievement");
        }
    }

    private void GrantAchievement(EntityUid player, PlayerAchievementsComponent achievementComp, string achievementId)
    {
        if (achievementComp.EarnedAchievements.Contains(achievementId))
            return;

        if (!_prototypeManager.HasIndex<AchievementPrototype>(achievementId))
        {
            return;
        }

        achievementComp.EarnedAchievements.Add(achievementId);
        Dirty(player, achievementComp);

        if (TryComp<ActorComponent>(player, out var actor))
        {
            var userId = actor.PlayerSession.UserId;

            _ = _dbManager.AddPlayerAchievementAsync(userId, achievementId);

            if (!_playerAchievements.ContainsKey(userId.ToString()))
                _playerAchievements[userId.ToString()] = new HashSet<string>();

            _playerAchievements[userId.ToString()].Add(achievementId);

            var msg = new AchievementEarnedMessage { AchievementId = achievementId };
            RaiseNetworkEvent(msg, actor.PlayerSession);

            var stateMsg = new AchievementsStateMessage { EarnedIds = achievementComp.EarnedAchievements.ToList() };
            RaiseNetworkEvent(stateMsg, actor.PlayerSession);
        }
    }

    private void OnRequestAchievements(RequestAchievementsMessage msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession;
        _ = SendAchievementsToClient(player);
    }

    private async Task SendAchievementsToClient(ICommonSession session)
    {
        try
        {
            var earnedIds = await _dbManager.GetPlayerAchievementsAsync(session.UserId);
            var earnedList = earnedIds.ToList();

            var stateMsg = new AchievementsStateMessage { EarnedIds = earnedList };
            RaiseNetworkEvent(stateMsg, session);
        }
        catch (Exception)
        {
            var stateMsg = new AchievementsStateMessage { EarnedIds = new List<string>() };
            RaiseNetworkEvent(stateMsg, session);
        }
    }
}
