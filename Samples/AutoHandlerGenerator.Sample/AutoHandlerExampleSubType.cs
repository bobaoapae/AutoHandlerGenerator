﻿using System.Threading.Tasks;
using AutoHandlerGenerator.Definitions;

namespace AutoHandlerGenerator.Sample;

public class AutoHandlerExampleSubType : AutoHandlerExample
{
    [Handler(0)]
    internal static ValueTask Test(PayloadExample payloadExample)
    {
        return ValueTask.CompletedTask;
    }
}