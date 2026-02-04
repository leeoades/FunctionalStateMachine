using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FunctionalStateMachine.Diagrams
{
    [Generator]
    public sealed class StateMachineDiagramGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "FunctionalStateMachine.Diagrams.StateMachineDiagramAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource(
                    "StateMachineDiagramAttribute.g.cs",
                    """
                    using System;

                    namespace FunctionalStateMachine.Diagrams
                    {
                        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                    internal sealed class StateMachineDiagramAttribute : Attribute
                    {
                        public StateMachineDiagramAttribute(string outputPath)
                        {
                            OutputPath = outputPath;
                        }

                        public string OutputPath { get; }
                    }
                }
                """);
        });

            var diagrams = context.SyntaxProvider.ForAttributeWithMetadataName(
                    AttributeName,
                    static (node, _) => node is MethodDeclarationSyntax,
                    static (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode)
                .Combine(context.CompilationProvider)
                .Combine(context.AnalyzerConfigOptionsProvider);

            context.RegisterSourceOutput(
                diagrams,
                static (ctx, data) =>
                {
                    var ((methodSyntax, compilation), options) = data;
                    if (!options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDir))
                    {
                        return;
                    }

                    var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
                    if (model.GetDeclaredSymbol(methodSyntax, ctx.CancellationToken) is not IMethodSymbol methodSymbol)
                    {
                        return;
                    }

                    var attribute = methodSymbol.GetAttributes()
                        .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == AttributeName);
                    if (attribute is null)
                    {
                        return;
                    }

            var outputPathValue = attribute.ConstructorArguments.Length == 1
                ? attribute.ConstructorArguments[0].Value?.ToString()
                : null;
            if (string.IsNullOrWhiteSpace(outputPathValue))
            {
                outputPathValue = $"{methodSymbol.Name}.md";
            }

            var diagramName = Path.GetFileNameWithoutExtension(outputPathValue);
            if (string.IsNullOrWhiteSpace(diagramName))
            {
                diagramName = methodSymbol.Name;
            }

                    var chains = DiagramBuilder.GetInvocationChains(methodSyntax);
                    var diagram = DiagramBuilder.BuildDiagram(diagramName!, chains);
                    if (diagram is null)
                    {
                        return;
                    }

            var outputPath = Path.IsPathRooted(outputPathValue)
                ? outputPathValue
                : Path.Combine(projectDir, outputPathValue);
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

                    if (File.Exists(outputPath))
                    {
                        var existing = File.ReadAllText(outputPath);
                        if (string.Equals(existing, diagram, StringComparison.Ordinal))
                        {
                            return;
                        }
                    }

                    File.WriteAllText(outputPath, diagram);
                });
        }
    }
}

internal static class DiagramBuilder
{
    public static List<List<InvocationInfo>> GetInvocationChains(MethodDeclarationSyntax method)
    {
        var chains = new List<List<InvocationInfo>>();
        var invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Parent is not InvocationExpressionSyntax)
            .ToList();

        foreach (var invocation in invocations)
        {
            var chain = UnwindChain(invocation);
            if (chain.Count > 0)
            {
                chains.Add(chain);
            }
        }

        return chains;
    }

    public static string? BuildDiagram(string name, List<List<InvocationInfo>> chains)
    {
        var states = new HashSet<string>();
        var transitions = new HashSet<Transition>();
        var startState = (string?)null;
        var childToParent = new Dictionary<string, string>(StringComparer.Ordinal);
        var initialSubStates = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var chain in chains)
        {
            string? currentState = null;
            string? currentTrigger = null;
            bool pendingTrigger = false;
            bool hasTransition = false;
            var guardLabels = new List<string>();

            foreach (var step in chain)
            {
                switch (step.Name)
                {
                    case "StartWith":
                        startState = GetFirstArg(step);
                        if (startState != null)
                        {
                            states.Add(startState);
                        }

                        break;
                    case "For":
                        if (pendingTrigger && !hasTransition && currentState != null && currentTrigger != null)
                        {
                            transitions.Add(new Transition(currentState, currentState,
                                ApplyGuardLabel(currentTrigger, guardLabels)));
                        }

                        currentState = GetFirstArg(step);
                        if (currentState != null)
                        {
                            states.Add(currentState);
                        }

                        currentTrigger = null;
                        pendingTrigger = false;
                        hasTransition = false;
                        guardLabels.Clear();
                        break;
                    case "SubStateOf":
                        var parent = GetFirstArg(step);
                        if (currentState != null && parent != null)
                        {
                            childToParent[currentState] = parent;
                            states.Add(parent);
                        }

                        break;
                    case "StartsWith":
                        var initialSubState = GetFirstArg(step);
                        if (currentState != null && initialSubState != null)
                        {
                            initialSubStates[currentState] = initialSubState;
                            states.Add(initialSubState);
                        }

                        break;
                    case "On":
                        if (pendingTrigger && !hasTransition && currentState != null && currentTrigger != null)
                        {
                            transitions.Add(new Transition(currentState, currentState,
                                ApplyGuardLabel(currentTrigger, guardLabels)));
                        }

                        currentTrigger = GetTriggerLabel(step);
                        pendingTrigger = currentTrigger != null;
                        hasTransition = false;
                        guardLabels.Clear();
                        break;
                    case "Guard":
                        if (TryGetGuardLabel(step, out var guardLabel))
                        {
                            guardLabels.Add(guardLabel);
                        }

                        break;
                    case "Immediately":
                        if (pendingTrigger && !hasTransition && currentState != null && currentTrigger != null)
                        {
                            transitions.Add(new Transition(currentState, currentState,
                                ApplyGuardLabel(currentTrigger, guardLabels)));
                        }

                        currentTrigger = "immediate";
                        pendingTrigger = true;
                        hasTransition = false;
                        guardLabels.Clear();
                        break;
                    case "TransitionTo":
                        var target = GetFirstArg(step);
                        if (currentState != null && target != null)
                        {
                            var label = ApplyGuardLabel(currentTrigger ?? "internal", guardLabels);
                            transitions.Add(new Transition(currentState, target, label));
                            states.Add(target);
                            hasTransition = true;
                            pendingTrigger = false;
                            guardLabels.Clear();
                        }

                        break;
                    case "Build":
                        if (pendingTrigger && !hasTransition && currentState != null && currentTrigger != null)
                        {
                            transitions.Add(new Transition(currentState, currentState,
                                ApplyGuardLabel(currentTrigger, guardLabels)));
                            pendingTrigger = false;
                            guardLabels.Clear();
                        }

                        break;
                }
            }
        }

        if (startState != null)
        {
            startState = ResolveInitialSubState(startState, initialSubStates);
        }

        if (states.Count == 0 && transitions.Count == 0)
        {
            return null;
        }

        var parentToChildren = BuildParentMap(childToParent);
        var ids = states.ToDictionary(
            state => state,
            state => $"S_{Sanitize(state)}");
        var superStates = new HashSet<string>(parentToChildren.Keys, StringComparer.Ordinal);
        var transitionsList = transitions
            .Select(transition => transition with { To = ResolveInitialSubState(transition.To, initialSubStates) })
            .OrderBy(t => t.From, StringComparer.Ordinal)
            .ThenBy(t => t.To, StringComparer.Ordinal)
            .ThenBy(t => t.Label, StringComparer.Ordinal)
            .ToList();
        var transitionStates = new HashSet<string>(
            transitionsList.SelectMany(t => new[] { t.From, t.To }),
            StringComparer.Ordinal);
        var portStates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var state in superStates)
        {
            if (transitionStates.Contains(state)
                || string.Equals(startState, state, StringComparison.Ordinal))
            {
                portStates.Add(state);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("# " + name);
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart LR");
        if (startState != null)
        {
            var startId = portStates.Contains(startState)
                ? $"P_{Sanitize(startState)}"
                : ids[startState];
            sb.AppendLine($"  START((start)) --> {startId}");
        }

        var rendered = new HashSet<string>(StringComparer.Ordinal);
        var renderedPorts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var root in states
                     .Where(state => !childToParent.ContainsKey(state))
                     .OrderBy(state => state, StringComparer.Ordinal))
        {
            RenderState(
                root,
                sb,
                ids,
                parentToChildren,
                transitionStates,
                startState,
                portStates,
                rendered,
                renderedPorts,
                1);
        }

        foreach (var state in states.Where(s => !superStates.Contains(s))
                                    .OrderBy(s => s, StringComparer.Ordinal))
        {
            if (rendered.Add(state))
            {
                sb.AppendLine($"  {ids[state]}[{state}]");
            }
        }

        if (renderedPorts.Count > 0)
        {
            sb.AppendLine("  classDef superstatePort fill:transparent,stroke:transparent;");
            foreach (var port in renderedPorts.OrderBy(value => value, StringComparer.Ordinal))
            {
                sb.AppendLine($"  class P_{Sanitize(port)} superstatePort;");
            }
        }

        foreach (var transition in transitionsList)
        {
            if (!ids.TryGetValue(transition.From, out var fromId)
                || !ids.TryGetValue(transition.To, out var toId))
            {
                continue;
            }

            if (portStates.Contains(transition.From))
            {
                fromId = $"P_{Sanitize(transition.From)}";
            }

            if (portStates.Contains(transition.To))
            {
                toId = $"P_{Sanitize(transition.To)}";
            }

            sb.AppendLine($"  {fromId} -->|{transition.Label}| {toId}");
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    public static string? GenerateDiagram(string source, string methodName, string diagramName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.ValueText, methodName, StringComparison.Ordinal));
        if (method is null)
        {
            return null;
        }

        var chains = GetInvocationChains(method);
        return BuildDiagram(diagramName, chains);
    }

    private static List<InvocationInfo> UnwindChain(InvocationExpressionSyntax invocation)
    {
        var chain = new List<InvocationInfo>();
        ExpressionSyntax? current = invocation;
        while (current is InvocationExpressionSyntax currentInvocation)
        {
            if (currentInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var name = memberAccess.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.ValueText,
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                    _ => memberAccess.Name.ToString()
                };

                var typeArgs = memberAccess.Name is GenericNameSyntax generic
                    ? generic.TypeArgumentList.Arguments.Select(arg => arg.ToString()).ToList()
                    : [];

                var args = currentInvocation.ArgumentList.Arguments.Select(arg => arg.ToString()).ToList();
                chain.Add(new InvocationInfo(name, args, typeArgs));
                current = memberAccess.Expression;
                continue;
            }

            break;
        }

        chain.Reverse();
        return chain;
    }

    private static string? GetFirstArg(InvocationInfo step)
    {
        return step.Arguments.Count > 0 ? step.Arguments[0] : null;
    }

    private static string? GetTriggerLabel(InvocationInfo step)
    {
        if (step.TypeArguments.Count > 0)
        {
            return step.TypeArguments[0];
        }

        return GetFirstArg(step);
    }

    private static string ResolveInitialSubState(
        string state,
        IReadOnlyDictionary<string, string> initialSubStates)
    {
        var current = state;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (initialSubStates.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
            {
                return current;
            }

            current = next;
        }

        return current;
    }

    private static string ApplyGuardLabel(string label, List<string> guardLabels)
    {
        if (guardLabels.Count == 0)
        {
            return label;
        }

        var guardText = string.Join(" && ", guardLabels);
        return $"{label} [{guardText}]";
    }

    private static bool TryGetGuardLabel(InvocationInfo step, out string label)
    {
        label = string.Empty;
        if (step.Arguments.Count < 2)
        {
            return false;
        }

        label = UnwrapStringLiteral(step.Arguments[0]);
        return !string.IsNullOrWhiteSpace(label);
    }

    private static string UnwrapStringLiteral(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length >= 2
            && trimmed.StartsWith("\"", StringComparison.Ordinal)
            && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            trimmed = trimmed.Substring(1, trimmed.Length - 2);
        }

        return trimmed;
    }

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        return builder.ToString();
    }

    private static Dictionary<string, List<string>> BuildParentMap(Dictionary<string, string> childToParent)
    {
        var parentToChildren = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var entry in childToParent)
        {
            var child = entry.Key;
            var parent = entry.Value;
            if (!parentToChildren.TryGetValue(parent, out var children))
            {
                children = [];
                parentToChildren[parent] = children;
            }

            if (!children.Contains(child))
            {
                children.Add(child);
            }
        }

        foreach (var entry in parentToChildren.Values)
        {
            entry.Sort(StringComparer.Ordinal);
        }

        return parentToChildren;
    }

    private static void RenderState(
        string state,
        StringBuilder sb,
        IReadOnlyDictionary<string, string> ids,
        IReadOnlyDictionary<string, List<string>> parentToChildren,
        ISet<string> transitionStates,
        string? startState,
        ISet<string> portStates,
        ISet<string> rendered,
        ISet<string> renderedPorts,
        int depth)
    {
        if (!parentToChildren.TryGetValue(state, out var children))
        {
            if (rendered.Add(state))
            {
                sb.AppendLine($"{Indent(depth)}{ids[state]}[{state}]");
            }

            return;
        }

        sb.AppendLine($"{Indent(depth)}subgraph SG_{Sanitize(state)}[{state}]");
        if (portStates.Contains(state) && renderedPorts.Add(state))
        {
            sb.AppendLine($"{Indent(depth + 1)}P_{Sanitize(state)}(( ))");
        }

        foreach (var child in children)
        {
            RenderState(
                child,
                sb,
                ids,
                parentToChildren,
                transitionStates,
                startState,
                portStates,
                rendered,
                renderedPorts,
                depth + 1);
        }

        sb.AppendLine($"{Indent(depth)}end");
    }

    private static string Indent(int depth) => new(' ', depth * 2);

    internal sealed record InvocationInfo(string Name, List<string> Arguments, List<string> TypeArguments);

    internal sealed record Transition(string From, string To, string Label);
}
