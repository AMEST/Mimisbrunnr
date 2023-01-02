using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Mimisbrunnr.Infrastructure.MapperlyExtensionsGenerator
{

[Generator]
public class MapperlyExtensionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var classesWithAttribute = context.Compilation.SyntaxTrees
            .SelectMany(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(r => r.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.GetText().ToString() == "Mapper")));
        foreach (var declaration in classesWithAttribute)
        {
            var methodDeclarations = declaration.Members
                .Where(m => m.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>().ToArray();
            var usings = new List<string>();
            foreach (var methodDeclaration in methodDeclarations)
                usings.AddRange(GenerateUsings(methodDeclaration, context));
            
            context.AddSource($"{declaration.Identifier}Extensions.g.cs",
                SourceText.From(GenerateClass(declaration, methodDeclarations, usings), Encoding.UTF8));
        }
    }

    private string GenerateClass(ClassDeclarationSyntax declaration, MethodDeclarationSyntax[] methodDeclarations,
        IList<string> usings)
    {
        var namespaceName = GetNamespace(declaration);
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        foreach (var classUsing in usings.Distinct())
            sb.AppendLine($"using {classUsing};");
        sb.Append($@"
namespace {(string.IsNullOrEmpty(namespaceName) ? "MapperlyExtensions" : namespaceName)}
{{
    public static class {declaration.Identifier}Extensions 
    {{
        private static readonly {declaration.Identifier} _mapper = new {declaration.Identifier}();
");
        
        foreach (MethodDeclarationSyntax classMethod in methodDeclarations)
            sb.AppendLine(GenerateMethod(classMethod));
        sb.Append(@"
    }
}");
        return sb.ToString();
    }

    private string GenerateMethod(MethodDeclarationSyntax classMethod)
    {
        var parameters = classMethod.ParameterList.Parameters;
        var newMethodDeclaration =
            $"        public static {classMethod.ReturnType} {classMethod.Identifier}";
        var methodParams =
            $"(this {string.Join(", ", parameters.Select(p => $"{p.Type} {p.Identifier}"))})";
        var methodBody = $"_mapper.{classMethod.Identifier}({string.Join(", ", parameters.Select(p => p.Identifier))})";
        return $"{newMethodDeclaration} {methodParams} => {methodBody};";
    }

    private IList<string> GenerateUsings(MethodDeclarationSyntax classMethod,
        GeneratorExecutionContext context)
    {
        var model = GetSemanticModel(classMethod, context);
        var usingList = new List<string>();
        usingList.Add(GetNamespace(classMethod));
        usingList.Add(GetTypeNamespace(model, classMethod.ReturnType));
        var parameters = classMethod.ParameterList.Parameters;
        usingList.AddRange(parameters.Select(parameter => GetTypeNamespace(model, parameter.Type)));
        return usingList;
    }
    
    private SemanticModel GetSemanticModel(SyntaxNode node, GeneratorExecutionContext context)
    {
        var compilationUnitSyntax = node.FirstAncestorOrSelf<CompilationUnitSyntax>();
        return context.Compilation.GetSemanticModel(compilationUnitSyntax.SyntaxTree);
    }

    private string GetTypeNamespace(SemanticModel model, TypeSyntax type)
    {
        return model.GetTypeInfo(type).Type?.ContainingNamespace.ToDisplayString();
    }

    private static string GetNamespace(SyntaxNode syntaxNode)
    {
        return string.Join(".", syntaxNode
            .Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Reverse()
            .Select(_ => _.Name)
        );
    }
}
} 
