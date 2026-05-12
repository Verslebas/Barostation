using Content.Client.Lobby;
using Content.Shared._BaroStation.Achievements;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client._BaroStation.Achievements;

public sealed partial class AchievementsUIController : UIController,
    IOnStateEntered<LobbyState>,
    IOnStateExited<LobbyState>
{
    [Dependency] private IClientNetManager _netManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private static AchievementsUIController? _instance;
    private AchievementsWindow? _window;
    public static event Action<HashSet<string>>? OnAchievementsUpdated;
    private List<AchievementPrototype> _allAchievements = new();
    private HashSet<string> _earnedAchievements = new();
    private bool _hasCachedData;
    private NetUserId? _lastUserId;

    public override void Initialize()
    {
        _instance = this;
        base.Initialize();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<AchievementPrototype>())
        {
            _allAchievements.Add(proto);
        }

        _allAchievements.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

        SubscribeNetworkEvent<AchievementEarnedMessage>(OnAchievementEarned);
        SubscribeNetworkEvent<AchievementsStateMessage>(OnAchievementsState);

        _netManager.ClientConnectStateChanged += OnClientConnectStateChanged;
        _netManager.Disconnect += OnDisconnect;

        _playerManager.LocalSessionChanged += OnLocalSessionChanged;
    }

    public static bool HasAchievementStatic(string achievementId)
    {
        return _instance?._earnedAchievements.Contains(achievementId) ?? false;
    }

    private void OnLocalSessionChanged((ICommonSession? Old, ICommonSession? New) args)
    {
        var newUserId = args.New?.UserId;

        if (_lastUserId != newUserId)
        {
            ClearCache();

            if (_netManager.ClientConnectState == ClientConnectionState.Connected)
            {
                RequestAchievements();
            }
        }

        _lastUserId = newUserId;
    }

    private void ClearCache()
    {
        _earnedAchievements.Clear();
        _hasCachedData = false;

        if (_window != null && !_window.Disposed)
        {
            _window.ClearAchievements();
        }
    }

    private void OnClientConnectStateChanged(ClientConnectionState obj)
    {
        if (obj == ClientConnectionState.Connected)
        {
            RequestAchievements();
        }
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        ClearCache();
    }

    public void OnStateEntered(LobbyState state)
    {
        EnsureWindow();
        RequestAchievements();
    }

    public void OnStateExited(LobbyState state)
    {
    }

    private void EnsureWindow()
    {
        if (_window != null && !_window.Disposed)
            return;

        _window = UIManager.CreateWindow<AchievementsWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.Center);

        _window.OnOpen += OnWindowOpened;

        if (_hasCachedData)
        {
            _window.CacheAchievements(_allAchievements, _earnedAchievements);
        }
    }

    private void OnWindowOpened()
    {
        RequestAchievements();
    }

    public void ToggleWindow()
    {
        if (_window == null || _window.Disposed)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.Open();
        }
    }

    public void RequestAchievements()
    {
        if (_netManager.ClientConnectState != ClientConnectionState.Connected)
        {
            return;
        }

        var msg = new RequestAchievementsMessage();
        _entityManager.EventBus.RaiseEvent(EventSource.Network, msg);
    }

    private void OnAchievementsState(AchievementsStateMessage msg, EntitySessionEventArgs args)
    {
        _earnedAchievements = new HashSet<string>(msg.EarnedIds);
        _hasCachedData = true;

        if (_window != null && !_window.Disposed)
        {
            _window.CacheAchievements(_allAchievements, _earnedAchievements);

            if (_window.IsOpen)
            {
                _window.UpdateAchievements(_allAchievements, _earnedAchievements);
            }
        }

        OnAchievementsUpdated?.Invoke(_earnedAchievements);
    }

    public bool HasAchievement(string achievementId)
    {
        return _earnedAchievements.Contains(achievementId);
    }

    private void OnAchievementEarned(AchievementEarnedMessage msg, EntitySessionEventArgs args)
    {
        if (!_allAchievements.Any(a => a.ID == msg.AchievementId))
        {
            return;
        }

        _earnedAchievements.Add(msg.AchievementId);
        _hasCachedData = true;

        if (_window != null && !_window.Disposed)
        {
            _window.CacheAchievements(_allAchievements, _earnedAchievements);

            if (_window.IsOpen)
            {
                _window.UpdateAchievements(_allAchievements, _earnedAchievements);
            }
        }

        var proto = _allAchievements.FirstOrDefault(a => a.ID == msg.AchievementId);
        if (proto != null)
        {
            ShowAchievementToast(proto);
        }
    }

    private void ShowAchievementToast(AchievementPrototype proto)
    {
        var toast = new ToastNotification(proto);
        toast.OnClosed += () => toast.Dispose();
        toast.Show();
    }
}
