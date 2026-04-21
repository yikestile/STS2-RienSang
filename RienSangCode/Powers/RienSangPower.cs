using BaseLib.Abstracts;
using BaseLib.Extensions;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public abstract class RienSangPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}