using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bloc.SourceGenerators;

[Generator]
internal class RecordGenerator : ISourceGenerator
{
    private INamedTypeSymbol? listSymbol;
    private INamedTypeSymbol? dictionarySymbol;
    private INamedTypeSymbol? collectionSymbol;

    private INamedTypeSymbol? recordAttributeSymbol;
    private INamedTypeSymbol? compareUsingAttributeSymbol;
    private INamedTypeSymbol? doNotCompareAttributeSymbol;

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        InitializeRequiredSymbols(context);

        var classSymbols = context.Compilation.SyntaxTrees
            .SelectMany(x => x.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .Select(x => context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Where(x => x.ContainingType is null)
            .Where(x => HasAttribute(x, recordAttributeSymbol));

        foreach (var classSymbol in classSymbols)
        {
            string name = $"{classSymbol.Name}.g.cs";

            string source =
                $"""
                using System.Linq;
                using Bloc.Utils.Extensions;

                namespace {classSymbol.ContainingNamespace.ToDisplayString()};
                            
                [System.CodeDom.Compiler.GeneratedCode("Record.SourceGenerator", "1.0.0")]
                {RenderClass(classSymbol)}
                """;

            context.AddSource(name, source);
        }
    }

    private void InitializeRequiredSymbols(GeneratorExecutionContext context)
    {
        listSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.IList");
        dictionarySymbol = context.Compilation.GetTypeByMetadataName("System.Collections.IDictionary");
        collectionSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.ICollection");

        recordAttributeSymbol = context.Compilation.GetTypeByMetadataName("Bloc.Utils.Attributes.RecordAttribute");
        compareUsingAttributeSymbol = context.Compilation.GetTypeByMetadataName("Bloc.Utils.Attributes.CompareUsingAttribute`1");
        doNotCompareAttributeSymbol = context.Compilation.GetTypeByMetadataName("Bloc.Utils.Attributes.DoNotCompareAttribute");
    }

    private string RenderClass(INamedTypeSymbol classSymbol)
    {
        string methods = RenderMethods(classSymbol);

        var nestedClasses = classSymbol.GetMembers()
            .OfType<INamedTypeSymbol>()
            .Where(x => HasAttribute(x, recordAttributeSymbol))
            .Select(RenderClass)
            .Select(Indent);

        return
            $$"""
            partial class {{classSymbol.Name}}
            {
            {{string.Join("\r\n\r\n", new[] { methods }.Concat(nestedClasses))}}
            }
            """;
    }

    private string RenderMethods(INamedTypeSymbol classSymbol)
    {
        const string HASH_SEPARATOR = ", ";
        const string EQUALS_SEPARATOR = " &&\r\n            ";

        var members = GetAllMembers(classSymbol).ToList();
        var hashExpressions = members.Select(GetHashExpression);
        var equalsExpressions = members.Select(GetEqualsExpression);

        return
            $$"""
                public override int GetHashCode()
                {
                    return System.HashCode.Combine({{(hashExpressions.Any() ? string.Join(HASH_SEPARATOR, hashExpressions) : "0")}});
                }

                public override bool Equals(object obj)
                {
                    return obj is {{classSymbol.Name}}{{(equalsExpressions.Any() ? string.Join(EQUALS_SEPARATOR, new[] { " other" }.Concat(equalsExpressions)) : "")}};
                }
            """;
    }

    private IEnumerable<ISymbol> GetAllMembers(INamedTypeSymbol classSymbol)
    {
        var inheritedMembers = classSymbol.BaseType is not null
            ? GetAllMembers(classSymbol.BaseType)
            : Enumerable.Empty<ISymbol>();

        var currentMembers = classSymbol.GetMembers()
            .Where(x => x is IFieldSymbol or IPropertySymbol)
            .Where(x => !x.IsStatic && !x.IsImplicitlyDeclared && x.CanBeReferencedByName)
            .Where(x => !HasAttribute(x, doNotCompareAttributeSymbol));

        return inheritedMembers.Concat(currentMembers);
    }

    private string GetHashExpression(ISymbol symbol)
    {
        var type = GetSymbolType(symbol);

        return Implements(type, collectionSymbol)
            ? $"this.{symbol.Name}.Count"
            : $"this.{symbol.Name}";
    }

    private string GetEqualsExpression(ISymbol symbol)
    {
        var type = GetSymbolType(symbol);

        if (type.IsValueType)
            return $"this.{symbol.Name} == other.{symbol.Name}";

        if (Implements(type, listSymbol))
            return $"this.{symbol.Name}.SequenceEqual(other.{symbol.Name}{GetEqualityComparer(symbol)})";

        if (Implements(type, dictionarySymbol))
            return $"this.{symbol.Name}.KeyValueEqual(other.{symbol.Name}{GetEqualityComparer(symbol)})";

        return $"Equals(this.{symbol.Name}, other.{symbol.Name})";
    }

    private string GetEqualityComparer(ISymbol symbol)
    {
        var attributeData = symbol
            .GetAttributes()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass?.OriginalDefinition, compareUsingAttributeSymbol));

        return attributeData?.AttributeClass is not null
            ? $", new {attributeData.AttributeClass.TypeArguments[0].ToDisplayString()}()"
            : "";
    }

    private static bool Implements(ITypeSymbol type, ITypeSymbol? @interface)
    {
        return type.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, @interface));
    }

    private static bool HasAttribute(ISymbol symbol, ITypeSymbol? attribute)
    {
        return symbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass?.OriginalDefinition, attribute));
    }

    private static string Indent(string input)
    {
        string[] lines = input.Split('\n');

        return "    " + string.Join("\n    ", lines);
    }

    private static ITypeSymbol GetSymbolType(ISymbol symbol)
    {
        return symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => throw new Exception()
        };
    }
}