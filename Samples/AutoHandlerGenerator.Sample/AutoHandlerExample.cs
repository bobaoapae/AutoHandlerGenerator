using AutoHandlerGenerator.Definitions;

namespace AutoHandlerGenerator.Sample;

[AutoHandler]
[AutoHandlerDeserializer<CustomDeserializer>]
public partial class AutoHandlerExample : IAutoHandler
{
}