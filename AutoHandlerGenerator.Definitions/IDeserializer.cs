using System;
using AutoSerializer.Definitions;

namespace AutoHandlerGenerator.Definitions;

public interface IDeserializer
{
    public static abstract T Deserialize<T>(in ArraySegment<byte> data) where T : IAutoDeserialize, new();
}