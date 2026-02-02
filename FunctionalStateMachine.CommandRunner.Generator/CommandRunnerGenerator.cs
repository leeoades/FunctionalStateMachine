using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FunctionalStateMachine.CommandRunner.Generator;

[Generator]
public sealed class CommandRunnerGenerator : IIncrementalGenerator
{
    private const string ExtensionTypeName = "FunctionalStateMachine.CommandRunner.ServiceCollectionExtensions";
    private const string AddCommandRunnersName = "AddCommandRunners";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var baseTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsAddCommandRunnerInvocation(node),
                static (ctx, _) => GetCommandBaseType(ctx))
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!)
            .Collect();

        var combined = baseTypes.Combine(context.CompilationProvider);
        context.RegisterSourceOutput(combined, static (ctx, data) =>
        {
            var (baseTypeSymbols, compilation) = data;
            if (baseTypeSymbols.IsDefaultOrEmpty)
            {
                return;
            }

            Generate(ctx, compilation, baseTypeSymbols);
        });
    }

    private static bool IsAddCommandRunnerInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax { Name: GenericNameSyntax { Identifier.ValueText: AddCommandRunnersName } } => true,
            GenericNameSyntax { Identifier.ValueText: AddCommandRunnersName } => true,
            _ => false
        };
    }

    private static INamedTypeSymbol? GetCommandBaseType(GeneratorSyntaxContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        if (!string.Equals(methodSymbol.Name, AddCommandRunnersName, StringComparison.Ordinal))
        {
            return null;
        }

        if (!string.Equals(methodSymbol.ContainingType?.ToDisplayString(), ExtensionTypeName, StringComparison.Ordinal))
        {
            return null;
        }

        if (methodSymbol.TypeArguments.Length != 1)
        {
            return null;
        }

        return methodSymbol.TypeArguments[0] as INamedTypeSymbol;
    }

    private static void Generate(SourceProductionContext context, Compilation compilation, IReadOnlyList<INamedTypeSymbol> baseTypes)
    {
        var syncRunnerInterface = compilation.GetTypeByMetadataName("FunctionalStateMachine.CommandRunner.ICommandRunner`1");
        var asyncRunnerInterface = compilation.GetTypeByMetadataName("FunctionalStateMachine.CommandRunner.IAsyncCommandRunner`1");
        if (syncRunnerInterface is null && asyncRunnerInterface is null)
        {
            return;
        }

        var uniqueBaseTypes = new List<INamedTypeSymbol>();
        var seenBaseTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var baseType in baseTypes)
        {
            if (seenBaseTypes.Add(baseType))
            {
                uniqueBaseTypes.Add(baseType);
            }
        }

        var allTypes = new List<INamedTypeSymbol>();
        CollectTypes(compilation.Assembly.GlobalNamespace, allTypes);

        var syncRunnerMap = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var asyncRunnerMap = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var type in allTypes)
        {
            if (type.IsAbstract || type.TypeParameters.Length > 0)
            {
                continue;
            }

            foreach (var iface in type.AllInterfaces)
            {
                if (syncRunnerInterface is not null && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, syncRunnerInterface))
                {
                    if (iface.TypeArguments.Length == 1 && iface.TypeArguments[0] is INamedTypeSymbol commandType)
                    {
                        if (!syncRunnerMap.ContainsKey(commandType))
                        {
                            syncRunnerMap.Add(commandType, type);
                        }
                    }
                }

                if (asyncRunnerInterface is not null && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, asyncRunnerInterface))
                {
                    if (iface.TypeArguments.Length == 1 && iface.TypeArguments[0] is INamedTypeSymbol commandType)
                    {
                        if (!asyncRunnerMap.ContainsKey(commandType))
                        {
                            asyncRunnerMap.Add(commandType, type);
                        }
                    }
                }
            }
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("using System;");
        source.AppendLine("using System.Collections.Generic;");
        source.AppendLine("using System.Runtime.CompilerServices;");
        source.AppendLine("using System.Threading.Tasks;");
        source.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        source.AppendLine("using FunctionalStateMachine.CommandRunner;");
        source.AppendLine();
        source.AppendLine("namespace FunctionalStateMachine.CommandRunner.Generated");
        source.AppendLine("{");
        source.AppendLine("    internal static class CommandRunnerRegistrationBootstrap");
        source.AppendLine("    {");
        source.AppendLine("        [ModuleInitializer]");
        source.AppendLine("        internal static void Initialize()");
        source.AppendLine("        {");

        foreach (var baseType in uniqueBaseTypes)
        {
            var commandInfos = GetCommandInfos(baseType, allTypes, syncRunnerMap, asyncRunnerMap);
            var providerId = ToSafeIdentifier(baseType);
            var baseTypeName = baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var hasAsync = commandInfos.Any(info => info.AsyncRunner is not null);

            source.AppendLine($"            CommandRunnerRegistry.Register<{baseTypeName}>(new CommandRunnerRegistration((services, options) =>");
            source.AppendLine("            {");

            var missing = commandInfos.Where(info => info.SyncRunner is null && info.AsyncRunner is null).ToList();
            if (missing.Count > 0)
            {
                var missingList = string.Join(", ", missing.Select(info => $"\"{info.CommandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}\""));
                source.AppendLine("                if (options.MissingBehavior == CommandRunnerMissingBehavior.Throw)");
                source.AppendLine("                {");
                source.AppendLine($"                    var missing = new[] {{ {missingList} }};");
                source.AppendLine("                    throw new InvalidOperationException(\"Missing command runners for: \" + string.Join(\", \", missing));");
                source.AppendLine("                }");
                source.AppendLine();
            }

            var runnerTypes = new List<INamedTypeSymbol>();
            var seenRunnerTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var runnerType in commandInfos.SelectMany(info => new[] { info.SyncRunner, info.AsyncRunner }))
            {
                if (runnerType is not null && seenRunnerTypes.Add(runnerType))
                {
                    runnerTypes.Add(runnerType);
                }
            }

            source.AppendLine("                if (options.AutoRegisterRunners)");
            source.AppendLine("                {");
            foreach (var runnerType in runnerTypes)
            {
                var runnerTypeName = runnerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                source.AppendLine($"                    services.Add(new ServiceDescriptor(typeof({runnerTypeName}), typeof({runnerTypeName}), options.Lifetime));");
            }
            source.AppendLine("                }");

            var providerInterface = hasAsync
                ? $"IAsyncCommandRunnerProvider<{baseTypeName}>"
                : $"ICommandRunnerProvider<{baseTypeName}>";
            var providerType = hasAsync
                ? $"AsyncCommandRunnerProvider_{providerId}"
                : $"CommandRunnerProvider_{providerId}";

            source.AppendLine();
            source.AppendLine($"                services.Add(new ServiceDescriptor(typeof({providerInterface}), sp => new {providerType}(sp, options.MissingBehavior), options.Lifetime));");
            source.AppendLine("            }));");
        }

        source.AppendLine("        }");
        source.AppendLine("    }");

        foreach (var baseType in uniqueBaseTypes)
        {
            var commandInfos = GetCommandInfos(baseType, allTypes, syncRunnerMap, asyncRunnerMap);
            var providerId = ToSafeIdentifier(baseType);
            var baseTypeName = baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var hasAsync = commandInfos.Any(info => info.AsyncRunner is not null);

            if (hasAsync)
            {
                source.AppendLine();
                source.AppendLine($"    internal sealed class AsyncCommandRunnerProvider_{providerId} : IAsyncCommandRunnerProvider<{baseTypeName}>");
                source.AppendLine("    {");
                source.AppendLine("        private readonly IServiceProvider _serviceProvider;");
                source.AppendLine("        private readonly CommandRunnerMissingBehavior _missingBehavior;");
                source.AppendLine();
                source.AppendLine($"        public AsyncCommandRunnerProvider_{providerId}(IServiceProvider serviceProvider, CommandRunnerMissingBehavior missingBehavior)");
                source.AppendLine("        {");
                source.AppendLine("            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));");
                source.AppendLine("            _missingBehavior = missingBehavior;");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        public Task RunAsync({baseTypeName} command)");
                source.AppendLine("        {");
                source.AppendLine("            if (command is null)");
                source.AppendLine("            {");
                source.AppendLine("                throw new ArgumentNullException(nameof(command));");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            switch (command)");
                source.AppendLine("            {");

                foreach (var info in commandInfos)
                {
                    var commandTypeName = info.CommandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (info.AsyncRunner is not null)
                    {
                        var runnerTypeName = info.AsyncRunner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        source.AppendLine($"                case {commandTypeName} typed:");
                        source.AppendLine($"                    return _serviceProvider.GetRequiredService<{runnerTypeName}>().RunAsync(typed);");
                    }
                    else if (info.SyncRunner is not null)
                    {
                        var runnerTypeName = info.SyncRunner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        source.AppendLine($"                case {commandTypeName} typed:");
                        source.AppendLine($"                    _serviceProvider.GetRequiredService<{runnerTypeName}>().Run(typed);");
                        source.AppendLine("                    return Task.CompletedTask;");
                    }
                }

                source.AppendLine("                default:");
                source.AppendLine("                    return HandleMissing(command);");
                source.AppendLine("            }");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        public async Task RunAsync(IEnumerable<{baseTypeName}> commands)");
                source.AppendLine("        {");
                source.AppendLine("            if (commands is null)");
                source.AppendLine("            {");
                source.AppendLine("                throw new ArgumentNullException(nameof(commands));");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            foreach (var command in commands)");
                source.AppendLine("            {");
                source.AppendLine("                await RunAsync(command).ConfigureAwait(false);");
                source.AppendLine("            }");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        private Task HandleMissing({baseTypeName} command)");
                source.AppendLine("        {");
                source.AppendLine("            if (_missingBehavior == CommandRunnerMissingBehavior.NoOp)");
                source.AppendLine("            {");
                source.AppendLine("                return Task.CompletedTask;");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            throw new InvalidOperationException($\"No command runner registered for {command.GetType().FullName}.\");");
                source.AppendLine("        }");
                source.AppendLine("    }");
            }
            else
            {
                source.AppendLine();
                source.AppendLine($"    internal sealed class CommandRunnerProvider_{providerId} : ICommandRunnerProvider<{baseTypeName}>");
                source.AppendLine("    {");
                source.AppendLine("        private readonly IServiceProvider _serviceProvider;");
                source.AppendLine("        private readonly CommandRunnerMissingBehavior _missingBehavior;");
                source.AppendLine();
                source.AppendLine($"        public CommandRunnerProvider_{providerId}(IServiceProvider serviceProvider, CommandRunnerMissingBehavior missingBehavior)");
                source.AppendLine("        {");
                source.AppendLine("            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));");
                source.AppendLine("            _missingBehavior = missingBehavior;");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        public void Run({baseTypeName} command)");
                source.AppendLine("        {");
                source.AppendLine("            if (command is null)");
                source.AppendLine("            {");
                source.AppendLine("                throw new ArgumentNullException(nameof(command));");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            switch (command)");
                source.AppendLine("            {");

                foreach (var info in commandInfos)
                {
                    if (info.SyncRunner is null)
                    {
                        continue;
                    }

                    var commandTypeName = info.CommandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var runnerTypeName = info.SyncRunner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    source.AppendLine($"                case {commandTypeName} typed:");
                    source.AppendLine($"                    _serviceProvider.GetRequiredService<{runnerTypeName}>().Run(typed);");
                    source.AppendLine("                    return;");
                }

                source.AppendLine("                default:");
                source.AppendLine("                    HandleMissing(command);");
                source.AppendLine("                    return;");
                source.AppendLine("            }");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        public void Run(IEnumerable<{baseTypeName}> commands)");
                source.AppendLine("        {");
                source.AppendLine("            if (commands is null)");
                source.AppendLine("            {");
                source.AppendLine("                throw new ArgumentNullException(nameof(commands));");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            foreach (var command in commands)");
                source.AppendLine("            {");
                source.AppendLine("                Run(command);");
                source.AppendLine("            }");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        private void HandleMissing({baseTypeName} command)");
                source.AppendLine("        {");
                source.AppendLine("            if (_missingBehavior == CommandRunnerMissingBehavior.NoOp)");
                source.AppendLine("            {");
                source.AppendLine("                return;");
                source.AppendLine("            }");
                source.AppendLine();
                source.AppendLine("            throw new InvalidOperationException($\"No command runner registered for {command.GetType().FullName}.\");");
                source.AppendLine("        }");
                source.AppendLine("    }");
            }
        }

        source.AppendLine("}");
        context.AddSource("CommandRunnerRegistry.g.cs", source.ToString());
    }

    private static List<CommandInfo> GetCommandInfos(
        INamedTypeSymbol baseType,
        List<INamedTypeSymbol> allTypes,
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> syncRunnerMap,
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> asyncRunnerMap)
    {
        var results = new List<CommandInfo>();
        foreach (var type in allTypes)
        {
            if (type.IsAbstract || type.TypeParameters.Length > 0)
            {
                continue;
            }

            if (!IsAssignableTo(type, baseType))
            {
                continue;
            }

            syncRunnerMap.TryGetValue(type, out var syncRunner);
            asyncRunnerMap.TryGetValue(type, out var asyncRunner);

            results.Add(new CommandInfo(type, syncRunner, asyncRunner));
        }

        return results
            .OrderBy(info => info.CommandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsAssignableTo(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        if (SymbolEqualityComparer.Default.Equals(type, baseType))
        {
            return true;
        }

        if (baseType.TypeKind == TypeKind.Interface)
        {
            return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, baseType));
        }

        var current = type.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static void CollectTypes(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> types)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            types.Add(type);
            CollectNestedTypes(type, types);
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            CollectTypes(nestedNamespace, types);
        }
    }

    private static void CollectNestedTypes(INamedTypeSymbol type, List<INamedTypeSymbol> types)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            types.Add(nested);
            CollectNestedTypes(nested, types);
        }
    }

    private static string ToSafeIdentifier(INamedTypeSymbol symbol)
    {
        var name = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var builder = new StringBuilder(name.Length + 4);
        foreach (var ch in name)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        if (builder.Length == 0 || char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }

    private sealed class CommandInfo
    {
        public CommandInfo(INamedTypeSymbol commandType, INamedTypeSymbol? syncRunner, INamedTypeSymbol? asyncRunner)
        {
            CommandType = commandType;
            SyncRunner = syncRunner;
            AsyncRunner = asyncRunner;
        }

        public INamedTypeSymbol CommandType { get; }
        public INamedTypeSymbol? SyncRunner { get; }
        public INamedTypeSymbol? AsyncRunner { get; }
    }
}
