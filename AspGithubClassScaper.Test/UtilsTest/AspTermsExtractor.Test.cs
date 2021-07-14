using ClassScraper.AspGithubClassScraper.Utils;
using System.Linq;
using Xunit;

namespace AspGithubClassScaper.Test.UtilsTest
{
    public class AspTermsExtractorTest
    {
        AspTermsExtractor _extractor;
        public AspTermsExtractorTest()
        {
            _extractor = new AspTermsExtractor();
        }

        [Fact]
        public void CheckExist()
        {
            Assert.NotNull(_extractor);
        }

        [Fact]
        public void CheckMultipleClasses()
        {
            var terms = _extractor.ExtractFileTerms(s_multipleClasses);
            Assert.Equal(3, terms.Count());
            var classList = terms.Where(t => t.TermType == ClassScraper.DomainObjects.Github.TermType.Class);
            var enumItem = terms.FirstOrDefault(t => t.TermType == ClassScraper.DomainObjects.Github.TermType.Enum);
            Assert.Equal("NoopAccount", classList.First().Name);
            Assert.Equal("NoopAccount2", classList.Last().Name);
            Assert.Equal("AccountState", enumItem.Name);
        }
        [Fact]
        public void CheckNestedClass()
        {
            var terms = _extractor.ExtractFileTerms(s_withNestedClass);
            Assert.Equal(3, terms.Count());
            Assert.Equal("NestedParentClass", terms.First().Name);
            Assert.Equal("NestedClass", terms.ElementAt(1).Name);
            Assert.Equal("AbstractSiblingClass", terms.Last().Name);
        }
        [Fact]
        public void CheckBrokenClass()
        {
            var terms = _extractor.ExtractFileTerms(s_brokenClass);
            Assert.Empty(terms);
        }
        [Fact]
        public void CheckClassWithInterface()
        {
            var terms = _extractor.ExtractFileTerms(s_classWithInterface);
            Assert.Equal(3, terms.Count());
            Assert.Equal("NestedTestClass", terms.First().Name);
            Assert.Equal("AbstractSiblingClass", terms.ElementAt(1).Name);
            Assert.Equal("TestInterface", terms.Last().Name);
            Assert.Equal(ClassScraper.DomainObjects.Github.TermType.Interface, terms.Last().TermType);
        }

        [Fact]
        public void CheckClassFields()
        {
            var terms = _extractor.ExtractFileTerms(s_classWithFields);
            var fields = terms.Where(f => f.TermType == ClassScraper.DomainObjects.Github.TermType.Field);
            
            Assert.Equal("field1", fields.First().Name);
            Assert.Equal("field2", fields.ElementAt(1).Name);
            Assert.Equal("arrayField", fields.ElementAt(5).Name);
            Assert.Equal(6, fields.Count());
        }
        [Fact]
        public void CheckClassProperties()
        {
            var terms = _extractor.ExtractFileTerms(s_classWithFields);
            var properties = terms.Where(f => f.TermType == ClassScraper.DomainObjects.Github.TermType.Property);

            Assert.Equal("publicProperty", properties.First().Name);
            Assert.Equal("privateProperty", properties.ElementAt(1).Name);            
            Assert.Equal(2, properties.Count());
        }
        #region TestData        
        private static string s_multipleClasses = @"using System;
            using System.Collections.Generic;
            using System.Net;
            using System.Xml;
            using Infratel.RecurlyLibrary;
            using Infratel.Utils.HttpRetry;

            namespace NoopData
            {    
                public class NoopAccount : NoopEntity
                {
      
                    [Flags]
                    public enum AccountState : short
                    {
                        Closed = 1,
                        Active = 2,
                        PastDue = 4
                    }

        
                }

                public sealed class NoopAccount2
                {        
                }
            }
        ";
        private static string s_withNestedClass = @"using System;
                namespace TestNested
                {    
                    public class NestedParentClass : NoopEntity
                    {                             
                        private class NestedClass {
                        }
        
                    }

                    public abstract class AbstractSiblingClass
                    {        
                    }
                }
                ";

        private static string s_brokenClass = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit";
        private static string s_classWithInterface = @"using System;
                namespace TestNested
                {    
                    public class NestedTestClass : NoopEntity
                    {                                     
                    }

                    public abstract class AbstractSiblingClass
                    {        
                    }
                    
                    public interface TestInterface {
                    }
                }";
        private static string s_classWithFields = @"namespace CustomNamespace
                {
                    internal class InternalNestedClassFieldTest
                    {
                        class Styles
                        {
                            public readonly string field1;
                            public readonly int field2;
                            public readonly Object field3;
                            private readonly GUIContent field4;
                            protected readonly string field5;          
                            public readonly int[] arrayField = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };


			                public string publicProperty { get; set; }
			                private string privateProperty { get; set; }
		                }
        
                    }
                }";
        #endregion
    }
}
