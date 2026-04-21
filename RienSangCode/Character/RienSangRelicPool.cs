using BaseLib.Abstracts;
using Godot;

namespace RienSang.RienSangCode.Character;

public class RienSangRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => RienSang.CharacterId;
    public override Color LabOutlineColor => RienSang.Color;
}