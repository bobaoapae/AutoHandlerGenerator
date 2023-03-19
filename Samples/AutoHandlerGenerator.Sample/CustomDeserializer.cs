using System;
using AutoHandlerGenerator.Definitions;
using AutoSerializer.Definitions;

namespace AutoHandlerGenerator.Sample;

public class CustomDeserializer : IDeserializer
{
    public static T Deserialize<T>(T target, in ArraySegment<byte> data) where T : IAutoDeserialize, new()
    {
        var offset = 0;
        target.Deserialize(in data, ref offset);
        return target;
    }
}