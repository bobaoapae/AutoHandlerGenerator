using System;

namespace AutoHandlerGenerator.Definitions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoHandlerDeserializerAttribute<T> : Attribute where T : IDeserializer
    {
    }
}