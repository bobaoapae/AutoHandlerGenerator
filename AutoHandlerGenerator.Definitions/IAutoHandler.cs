using System;
using System.Threading.Tasks;

namespace AutoHandlerGenerator.Definitions
{
    public interface IAutoHandler
    {
        bool Contains(int opcode);
        bool Contains(int opcode, int group);
        ValueTask Handle(int opcode, ArraySegment<byte> buffer);
        ValueTask Handle(int opcode, int group, ArraySegment<byte> buffer);
    }

    public interface IAutoHandler<in T>
    {
        bool Contains(int opcode);
        bool Contains(int opcode, int group);
        ValueTask Handle(int opcode, ArraySegment<byte> buffer, T session);
        ValueTask Handle(int opcode, int group, ArraySegment<byte> buffer, T session);
    }
}