using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Uncommon
{
    [Pool(typeof(RienSangCardPool))]
    public class Faith : RienSangCard
    {
        public Faith() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0, ValueProp.Move)];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
        {
            if (play.Target != null)
            {
                int count = 0;
                if (CombatManager.Instance?.History != null)
                {
                    count = CombatManager.Instance.History.CardPlaysFinished
                        .Count(e => e.CardPlay.Card.Tags.Contains(RienSangTags.BlindFaith));
                }

                if (count > 0)
                {
                    await CaduceusManager.Execute(this, play.Target, (decimal)count, choiceContext, 0);
                }
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }

        [HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview))]
        public static class AbsoluteFaithPreviewPatch
        {
            [HarmonyPostfix]
            public static void Postfix(CardModel __instance, DynamicVarSet dynamicVarSet)
            {
                if (__instance is Faith && __instance.IsMutable)
                {
                    int count = 0;
                    if (CombatManager.Instance?.History != null)
                    {
                        count = CombatManager.Instance.History.CardPlaysFinished
                            .Count(e => e.CardPlay.Card.Tags.Contains(RienSangTags.BlindFaith));
                    }
                    dynamicVarSet.Damage.BaseValue = count;
                }
            }
        }
    }
}