using System;

namespace Tizsoft.Treenet
{
    [Flags]
    public enum PacketFlags : byte
    {
        None            = 0x0000,
        Compressed      = 0x0001,
    }
}