using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards;

[Pool(typeof(RienSangCardPool))]
public abstract class RienSangCard(int cost, CardType type, CardRarity rarity, TargetType target) : 
    CustomCardModel(cost, type, rarity, target), ILimbusSpCostCard, ILimbusEgoCard, ILimbusSpecialCard
{
    public virtual bool IsEgoCard => false;
    public virtual bool GainsKarma => false;
    public virtual bool IsLCSpecialCard => false;
    public LimbusDamageType CurrentDamageType { get; set; } = LimbusDamageType.None;

    public virtual int SpCost => -1;
    public override int CanonicalStarCost => SpCost;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = base.CanonicalKeywords?.ToList() ?? new List<CardKeyword>();
            if (IsEgoCard && !keywords.Contains(CardKeyword.Exhaust))
            {
                keywords.Add(CardKeyword.Exhaust);
            }
            return keywords;
        }
    }

    public override string CustomPortraitPath => GetPortraitPath(true);
    public override string PortraitPath => GetPortraitPath(false);
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    private string GetPortraitPath(bool big)
    {
        var fileName = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
        var path = big ? fileName.BigCardImagePath() : fileName.CardImagePath();
        
        return ResourceLoader.Exists(path) ? path : (big ? "card.png".BigCardImagePath() : "card.png".CardImagePath());
    }
}