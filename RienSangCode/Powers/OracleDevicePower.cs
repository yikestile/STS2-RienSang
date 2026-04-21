using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class OracleDevicePower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public OracleDevicePower() : base() { }

    public OracleDevicePower(int amount) : this()
    {
        SetAmount(amount);
    }
}