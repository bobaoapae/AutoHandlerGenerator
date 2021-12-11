using System.Runtime.InteropServices;

namespace AutoHandlerGenerator.Definitions
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ServiceHeader
    {
        public static int Size = Marshal.SizeOf<ServiceHeader>();
        public ushort OpCode;
        public ushort Group;
        public ushort TotalSize;

        public ServiceHeader(ushort opCode, ushort group) : this()
        {
            OpCode = opCode;
            Group = group;
        }
    }
}