using System;

namespace AutoHandlerGenerator.Definitions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute : Attribute
    {
        public int Opcode { get; }
        public int Group { get; }

        public HandlerAttribute(int opcode, int group = 0)
        {
            Opcode = opcode;
            Group = group;
        }
    }
}