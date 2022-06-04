using AutoSerializer.Definitions;

namespace AutoHandlerGenerator.Sample;

[AutoDeserialize]
public partial class PayloadExample : IAutoDeserialize
{
    public int Data { get; set; }
}