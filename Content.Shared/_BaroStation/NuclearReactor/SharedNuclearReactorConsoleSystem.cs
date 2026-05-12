using Robust.Shared.Network;

namespace Content.Shared._BaroStation.NuclearReactor;

public abstract partial class SharedNuclearReactorConsoleSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorConsoleComponent, NuclearReactorConsoleLinkMessage>(OnLinkMessage);
        SubscribeLocalEvent<NuclearReactorConsoleComponent, NuclearReactorConsoleClearLinkMessage>(OnClearLinkMessage);
    }

    protected virtual void PopupLinkFail(EntityUid uid, EntityUid user) { }
    protected virtual void PopupLinkSuccess(EntityUid uid, EntityUid user) { }

    private void OnLinkMessage(EntityUid uid, NuclearReactorConsoleComponent comp, NuclearReactorConsoleLinkMessage args)
    {
        var target = GetEntity(args.Target);
        if (TryComp<NuclearReactorComponent>(target, out _))
        {
            comp.LinkedReactor = target;
            Dirty(uid, comp);
            UpdateConsoleUi(uid, comp);
        }
    }

    private void OnClearLinkMessage(EntityUid uid, NuclearReactorConsoleComponent comp, NuclearReactorConsoleClearLinkMessage args)
    {
        comp.LinkedReactor = null;
        Dirty(uid, comp);
        UpdateConsoleUi(uid, comp);
    }

    protected virtual void UpdateConsoleUi(EntityUid uid, NuclearReactorConsoleComponent comp) { }

    public void UpdateFromReactor(EntityUid consoleUid, NuclearReactorConsoleComponent comp, NuclearReactorUiState state)
    {
        comp.LastReactorState = state;
        Dirty(consoleUid, comp);
        UpdateConsoleUi(consoleUid, comp);
    }
}
