using System;

namespace AutoHandlerGenerator.Definitions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoHandlerSerializerAttribute : Attribute
    {
        public Type Deserializer { get; }

        public AutoHandlerSerializerAttribute(Type deserializer)
        {
            Deserializer = deserializer;
        }
    }
}