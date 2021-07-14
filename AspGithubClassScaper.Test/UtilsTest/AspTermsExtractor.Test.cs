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
            var terms = _extractor.ExtractFileTerms(MiltipleClasses);
            Assert.Equal(2, terms.Count());
            var classList = terms.Where(t => t.TermType == ClassScraper.DomainObjects.Github.TermType.Class);
            Assert.Equal("NoopAccount", classList.First().Name);
            Assert.Equal("NoopAccount2", classList.Last().Name);
        }
        [Fact]
        public void CheckNestedClass()
        {
            var terms = _extractor.ExtractFileTerms(WithNestedClass);
            Assert.Equal(3, terms.Count());
            Assert.Equal("NestedParentClass", terms.First().Name);
            Assert.Equal("NestedClass", terms.ElementAt(1).Name);
            Assert.Equal("AbstractSiblingClass", terms.Last().Name);
        }
        [Fact]
        public void CheckBrokenClass()
        {
            var terms = _extractor.ExtractFileTerms(BrokenClass);
            Assert.Empty(terms);
        }
        [Fact]
        public void CheckClassWithInterface()
        {
            var terms = _extractor.ExtractFileTerms(ClassWithInterface);
            Assert.Equal(3, terms.Count());
            Assert.Equal("NestedTestClass", terms.First().Name);
            Assert.Equal("AbstractSiblingClass", terms.ElementAt(1).Name);
            Assert.Equal("TestInterface", terms.Last().Name);
            Assert.Equal(ClassScraper.DomainObjects.Github.TermType.Interface, terms.Last().TermType);
        }
        #region TestData        
        public static string MiltipleClasses = @"using System;
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
        public static string WithNestedClass = @"using System;
                namespace TestNested
                {    
                    public class NestedParentClass : NoopEntity
                    {
      
                        [Flags]
                        public enum AccountState : short
                        {
                            Closed = 1,
                            Active = 2,
                            PastDue = 4
                        }

                        private class NestedClass {
                        }
        
                    }

                    public abstract class AbstractSiblingClass
                    {        
                    }
                }
                ";

        public static string BrokenClass = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit";
        public static string ClassWithInterface = @"using System;
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
        #endregion
    }
}
