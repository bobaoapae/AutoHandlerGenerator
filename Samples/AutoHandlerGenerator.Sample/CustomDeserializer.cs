using System;
using AutoHandlerGenerator.Definitions;
using AutoSerializer.Definitions;

namespace AutoHandlerGenerator.Sample;

public class CustomDeserializer : IDeserializer
{
    public static T Deserialize<T>(in ArraySegment<byte> data) where T : IAutoDeserialize, new()
    {
        var offset = 0;
        var obj = new T();
        obj.Deserialize(in data, ref offset);
        return obj;
    }
}