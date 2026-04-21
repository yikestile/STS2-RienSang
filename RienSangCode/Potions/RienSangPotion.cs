using BaseLib.Abstracts;
using BaseLib.Utils;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Potions;

[Pool(typeof(RienSangPotionPool))]
public abstract class RienSangPotion : CustomPotionModel;