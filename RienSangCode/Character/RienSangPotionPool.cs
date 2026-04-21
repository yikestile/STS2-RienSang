using BaseLib.Abstracts;
using Godot;

namespace RienSang.RienSangCode.Character;

public class RienSangPotionPool : CustomPotionPoolModel
{
    public override string EnergyColorName => RienSang.CharacterId;
    public override Color LabOutlineColor => RienSang.Color;
}