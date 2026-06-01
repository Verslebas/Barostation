using System.Linq;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared._BaroStation.Achievements;

public sealed partial class EquipItemCondition : EntityConditionBase<EquipItemCondition>
{
    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> Items = new();

    private string? _cachedId;

    public string GetId()
    {
        if (_cachedId != null)
            return _cachedId;

        _cachedId = string.Join("|", Items.OrderBy(x => x.Id).Select(x => x.Id));
        return _cachedId;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}

public sealed partial class KillMobCondition : EntityConditionBase<KillMobCondition>
{
    [DataField]
    public List<string> ExactIds = new();

    [DataField]
    public string? ContainsId = null;

    private string? _cachedId;

    public string GetId()
    {
        if (_cachedId != null)
            return _cachedId;

        if (!string.IsNullOrEmpty(ContainsId))
            _cachedId = $"contains:{ContainsId}";
        else if (ExactIds.Count > 0)
            _cachedId = $"exact:{string.Join(",", ExactIds.OrderBy(x => x))}";
        else
            _cachedId = "empty";

        return _cachedId;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
