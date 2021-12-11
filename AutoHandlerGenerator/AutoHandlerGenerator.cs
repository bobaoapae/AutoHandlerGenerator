using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace AutoHandlerGenerator
{
    [Generator]
    public class AutoHandlerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var autoSerializerAssembly = Assembly.GetExecutingAssembly();

            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            var candidateClasses = new List<INamedTypeSymbol>();

            var compilation = context.Compilation;

            foreach (var cls in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(cls.SyntaxTree);

                var classSymbol = model.GetDeclaredSymbol(cls);
                candidateClasses.Add(classSymbol);
            }

            if (candidateClasses.Count == 0)
                return;

            var syncServiceServerBaseAttribute = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.SyncServiceServerAttribute");
            var syncServiceClientBaseAttribute = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.SyncServiceClientAttribute");
            var autoHandlerBaseAttribute = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.AutoHandlerAttribute");
            var serializerBaseAttribute = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.AutoHandlerSerializerAttribute");
            var handlerBaseAttribute = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.HandlerAttribute");
            var interfaceBase = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.IAutoHandler");
            var interfaceGenericBase = compilation.GetTypeByMetadataName("AutoHandlerGenerator.Definitions.IAutoHandler`1");

            if (syncServiceServerBaseAttribute == null || syncServiceClientBaseAttribute == null || autoHandlerBaseAttribute == null || serializerBaseAttribute == null || handlerBaseAttribute == null || interfaceBase == null || interfaceGenericBase == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "AG0004",
                        "Missing reference to AutoHandlerGenerator.Definitions",
                        "Missing reference to AutoHandlerGenerator.Definitions",
                        "",
                        DiagnosticSeverity.Error,
                        true),
                    null));
                return;
            }

            var autoHandlerClassResource = GetResource(autoSerializerAssembly, context, "AutoHandlerGenerator.Resources.AutoHandler.cs");
            var methodAutoHandlerResource = GetResource(autoSerializerAssembly, context, "AutoHandlerGenerator.Resources.MethodAutoHandler.cs");

            if (autoHandlerClassResource == "" || methodAutoHandlerResource == "")
                return;

            #region AutoHandler

            var autoHandlerCollections = GetAllClassAndSubTypesWithAttribute(candidateClasses, autoHandlerBaseAttribute);

            foreach (var keyValuePair in autoHandlerCollections)
            {
                var baseType = keyValuePair.Key;
                var subTypes = keyValuePair.Value;

                var namespaceBase = baseType.ContainingNamespace.ToDisplayString();
                var collectionName = baseType.Name;
                var collectionNameCapitalize = collectionName.First().ToString().ToUpper() + collectionName.Substring(1);

                var initializeHandlers = new StringBuilder();
                var methods = new StringBuilder();

                var autoHandlerInterface = baseType.Interfaces.First(symbol => symbol.Name == interfaceBase.Name || symbol.Name == interfaceGenericBase.Name);

                var isAutoHandlerWithGeneric = autoHandlerInterface.MetadataName == interfaceGenericBase.MetadataName;


                var optionalSessionParameter = "";
                var optionalSessionParameterPassToHandler = "";
                var optionalSessionParameterPassToTargetMethod = "";

                if (isAutoHandlerWithGeneric)
                {
                    var genericTypeName = autoHandlerInterface.TypeArguments.First().ToString();
                    optionalSessionParameter = $", {genericTypeName} session";
                    optionalSessionParameterPassToHandler = ", session";
                    optionalSessionParameterPassToTargetMethod = "session, ";
                }

                var serializerAttribute = baseType.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == serializerBaseAttribute.Name);

                var hashOpcodes = new List<string>();

                foreach (var subType in subTypes)
                {
                    var className = subType.Name;
                    var namespaceName = subType.ContainingNamespace.ToDisplayString();
                    foreach (var member in subType.GetMembers())
                    {
                        if (!(member is IMethodSymbol methodSymbol))
                            continue;

                        var methodName = methodSymbol.Name;
                        var handlerType = methodSymbol.GetAttributes().FirstOrDefault(data => data.AttributeClass?.Name == handlerBaseAttribute.Name);

                        if (handlerType == null)
                            continue;

                        var handlerOpcode = handlerType.ConstructorArguments[0].Value.ToString();
                        var handlerGroup = handlerType.ConstructorArguments[1].Value.ToString();

                        var methodLogic = new StringBuilder();

                        var targetMethod = $"{namespaceName}.{className}.{methodName}";

                        if (methodSymbol.Parameters.Length > (isAutoHandlerWithGeneric ? 1 : 0))
                        {
                            var methodParameters = new StringBuilder();

                            foreach (var methodSymbolParameter in methodSymbol.Parameters.Skip(isAutoHandlerWithGeneric ? 1 : 0))
                            {
                                var targetType = methodSymbolParameter.Type;
                                var targetTypeName = $"{targetType.ContainingNamespace}.{targetType.Name}";

                                if (targetType.Name == "ArraySegment")
                                {
                                    methodParameters.Append(" buffer,");
                                }
                                else
                                {
                                    if (serializerAttribute != null)
                                    {
                                        methodLogic.Append('\t', 3).AppendLine($"var packet = {serializerAttribute.ConstructorArguments.First().Value}.Deserialize<{targetTypeName}>(buffer);");
                                    }
                                    else
                                    {
                                        methodLogic.Append('\t', 3).AppendLine("var offset = buffer.Offset;");
                                        methodLogic.Append('\t', 3).AppendLine($"var packet = new {targetTypeName}();");
                                        methodLogic.Append('\t', 3).AppendLine("packet.Deserialize(buffer, ref offset);");
                                    }

                                    methodParameters.Append(" packet,");
                                }
                            }

                            methodParameters.Length--;
                            methodLogic.Append('\t', 3).Append($"return {targetMethod}({optionalSessionParameterPassToTargetMethod}{methodParameters});");
                        }
                        else
                        {
                            methodLogic.Append('\t', 3).Append($"return {targetMethod}({(optionalSessionParameterPassToTargetMethod.Replace(", ", ""))});");
                        }

                        var hashOpCode = $"{handlerGroup} - {handlerOpcode}";

                        if (hashOpcodes.Contains(hashOpCode))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "AG0003",
                                    "Duplicate OpCode found",
                                    $"Duplicate OpCode found",
                                    "",
                                    DiagnosticSeverity.Error,
                                    true),
                                methodSymbol.Locations.First()));
                        }
                        else
                        {
                            hashOpcodes.Add(hashOpCode);

                            var methodSource = string.Format(methodAutoHandlerResource, handlerGroup, handlerOpcode, optionalSessionParameter, methodLogic);

                            initializeHandlers.Append('\t', 3).AppendLine($"if (!_handlers.ContainsKey({handlerGroup}))");
                            initializeHandlers.Append('\t', 4).AppendLine($"_handlers.Add({handlerGroup}, new Dictionary<int, InternalHandle>());");
                            initializeHandlers.Append('\t', 3).AppendLine($"_handlers[{handlerGroup}].Add({handlerOpcode}, Internal_Handle{handlerGroup}_{handlerOpcode});");

                            methods.Append('\t', 2).AppendLine(methodSource);
                        }
                    }
                }

                var classSource = string.Format(autoHandlerClassResource, namespaceBase, collectionNameCapitalize, initializeHandlers, methods, optionalSessionParameter, optionalSessionParameterPassToHandler);

                context.AddSource($"{namespaceBase}.{collectionNameCapitalize}.AutoHandler.g.cs", SourceText.From(classSource, Encoding.UTF8));
            }

            #endregion
        }

        private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> GetAllClassAndSubTypesWithAttribute(List<INamedTypeSymbol> candidateClasses, INamedTypeSymbol attribute)
        {
            var result = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
            foreach (var classSymbol in candidateClasses)
            {
                var handlerCollection = classSymbol?.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == attribute.Name);
                if (handlerCollection != null)
                {
                    result.Add(classSymbol, new List<INamedTypeSymbol>());
                }
            }

            foreach (var classSymbol in candidateClasses)
            {
                var baseType = classSymbol?.BaseType;
                if (baseType != null && result.ContainsKey(baseType))
                    result[baseType].Add(classSymbol);
            }

            return result;
        }

        private static string GetResource(Assembly assembly, GeneratorExecutionContext context, string resourceName)
        {
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream != null)
                    return new StreamReader(resourceStream).ReadToEnd();

                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "AG0001",
                        "Invalid Resource",
                        $"Cannot find {resourceName} resource",
                        "",
                        DiagnosticSeverity.Error,
                        true),
                    null));
                return "";
            }
        }

        private static string Capitalize(string source)
        {
            return source.First().ToString().ToUpper() + source.Substring(1);
        }

        private static string DeCapitalize(string source)
        {
            return source.First().ToString().ToLower() + source.Substring(1);
        }
    }
}