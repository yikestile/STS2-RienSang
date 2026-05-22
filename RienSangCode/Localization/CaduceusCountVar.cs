using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Localization;

public class CaduceusCountVar : DynamicVar
{
    public CaduceusCountVar() : base("CaduceusCount", 0) { }
    
    public override string ToString() => PreviewValue.ToString();

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (card.Owner == null)
        {
            PreviewValue = 0;
            return;
        }

        var player = card.Owner; 
        
        var uniqueCaduceusCount = player.Piles.SelectMany(p => p.Cards)
            .Where(c => c.CanonicalKeywords.Contains(RienSangKeywords.Caduceus))
            .Select(c => c.Id)
            .Distinct()
            .Count();

        PreviewValue = uniqueCaduceusCount;
    }
}
