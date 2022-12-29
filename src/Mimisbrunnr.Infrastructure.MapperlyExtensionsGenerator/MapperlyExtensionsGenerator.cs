using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mimisbrunnr.Infrastructure.MapperlyExtensionsGenerator;

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
        if (classesWithAttribute == null || !classesWithAttribute.Any())
            return;
        foreach (var declaration in classesWithAttribute)
        {
            context.AddSource($"{declaration.Identifier}Extensions.g.cs",
                SourceText.From(GenerateClass(context, declaration), Encoding.UTF8));
            Debug.WriteLine($"{declaration.Identifier}Extensions.g.cs");
        }
    }

    private string GenerateClass(GeneratorExecutionContext context,
        ClassDeclarationSyntax declaration)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated code");
        
        var methodDeclarations = declaration.Members
            .Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>().ToArray();
        var usingList = new List<string>();
        foreach (var methodDeclaration in methodDeclarations)
            usingList.AddRange(GenerateUsings(methodDeclaration, context));

        foreach (var classUsing in usingList.Distinct())
            sb.AppendLine($"using {classUsing};");

        var namespaceName = GetNamespace(declaration);
        namespaceName = string.IsNullOrEmpty(namespaceName) ? "MapperlyExtensions" : namespaceName;
        sb.Append($@"
namespace {namespaceName}
{{
    public static class {declaration.Identifier}Extensions 
    {{
        private static readonly {declaration.Identifier} _mapper = new {declaration.Identifier}();
");
        foreach (MethodDeclarationSyntax classMethod in methodDeclarations)
            sb.AppendLine(GenerateMethod(classMethod));

        sb.Append(@"
    }
}
");
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
        foreach (var parameter in parameters)
            usingList.Add(GetTypeNamespace(model, parameter.Type));
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