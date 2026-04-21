using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using LimbusCore.LimbusCoreCode.Cards;
using System.Linq;

namespace RienSang.RienSangCode.Cards;

[Pool(typeof(RienSangCardPool))]
public abstract class RienSangCard : CustomCardModel, ILimbusSpCostCard
{
    protected RienSangCard() : this(0, CardType.Attack, CardRarity.Basic, TargetType.None)
    {
    }

    protected RienSangCard(int cost, CardType type, CardRarity rarity, TargetType target) : base(cost, type, rarity, target)
    {
        _ = DynamicVars; 
    }
    
    public virtual bool IsEgoCard { get; } = false;
    public virtual bool GainsKarma { get; } = false;

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

    public override string CustomPortraitPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
            return ResourceLoader.Exists(path) ? path : "card.png".BigCardImagePath();
        }
    }

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.ToLowerInvariant()}.png".CardImagePath();
}
