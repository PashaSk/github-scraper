using ClassScraper.DomainObjects.Github;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassScraper.AspGithubClassScraper.Utils
{
    //grab files and analyze them 
    public class AspTermsExtractor : IAspTermsExtractor
    {
        ClassSyntaxWalker _walker;
        public AspTermsExtractor()
        {
            _walker = new ClassSyntaxWalker();
        }

        public IEnumerable<TermEntity> ExtractFileTerms(string fileString)
        {
            _walker.TermList.Clear();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(fileString);
            if (tree.HasCompilationUnitRoot == false)
            {
                return null;
            }
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            _walker.Visit(root);

            return _walker.TermList;
        }

    }

    public class ClassSyntaxWalker : CSharpSyntaxWalker
    {
        public ICollection<TermEntity> TermList { get; } = new List<TermEntity>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            TermList.Add(new TermEntity()
            {
                ID = Guid.NewGuid(),
                Name = node.Identifier.ValueText,
                TermType = TermType.Class
            });

            base.VisitClassDeclaration(node);
        }
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            TermList.Add(new TermEntity()
            {
                ID = Guid.NewGuid(),
                Name = node.Identifier.ValueText,
                TermType = TermType.Interface
            });

            base.VisitInterfaceDeclaration(node);
        }
        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            TermList.Add(new TermEntity()
            {
                ID = Guid.NewGuid(),
                Name = node.Identifier.ValueText,
                TermType = TermType.Enum
            });

            base.VisitEnumDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach(var variable in node.Declaration.Variables)
            {
                TermList.Add(new TermEntity()
                {
                    ID = Guid.NewGuid(),
                    Name = variable.Identifier.ValueText,
                    TermType = TermType.Field
                });
            }

            base.VisitFieldDeclaration(node);
        }
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            TermList.Add(new TermEntity()
            {
                ID = Guid.NewGuid(),
                Name = node.Identifier.ValueText,
                TermType = TermType.Property
            });

            base.VisitPropertyDeclaration(node);
        }
    }

    public interface IAspTermsExtractor
    {
        public IEnumerable<TermEntity> ExtractFileTerms(string fileString);
    }
}
