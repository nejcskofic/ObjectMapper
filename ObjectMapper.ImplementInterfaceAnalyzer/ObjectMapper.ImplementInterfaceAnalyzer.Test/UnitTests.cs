using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using ObjectMapper.ImplementInterfaceAnalyzer;

namespace ObjectMapper.ImplementInterfaceAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA : IObjectMapper<ClassB>
    {
        public int? Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }

        private LinkedList<int> _prop5;
        public LinkedList<int> Prop5 { get { if (_prop5 == null) _prop5 = new LinkedList<int>(); return _prop5; } }

        private Collection<int> _prop6;
        public Collection<int> Prop6 { get { if (_prop6 == null) _prop6 = new Collection<int>(); return _prop6; } }
    }

    public class ClassB
    {
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }

        private LinkedList<int> _prop5;
        public LinkedList<int> Prop5 { get { if (_prop4 == null) _prop5 = new LinkedList<int>(); return _prop5; } }

        private Collection<int> _prop6;
        public Collection<int> Prop6 { get { if (_prop6 == null) _prop6 = new Collection<int>(); return _prop6; } }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ObjectMapperCG001",
                Message = String.Format("Implementation of interface '{0}' can be generated.", "ObjectMapper.Framework.IObjectMapper<TestClassLibrary.ClassB>"),
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 27)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA : IObjectMapper<ClassB>
    {
        public int? Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }

        private LinkedList<int> _prop5;
        public LinkedList<int> Prop5 { get { if (_prop5 == null) _prop5 = new LinkedList<int>(); return _prop5; } }

        private Collection<int> _prop6;
        public Collection<int> Prop6 { get { if (_prop6 == null) _prop6 = new Collection<int>(); return _prop6; } }

        public void MapObject(ClassB target)
        {
            target.Prop1 = this.Prop1 ?? default(int);
            target.Prop2 = this.Prop2;
            target.Prop3 = this.Prop3;
            target.Prop4.CopyFrom(this.Prop4);
            target.Prop5.CopyFrom(this.Prop5);
            target.Prop6.CopyFrom(this.Prop6);
        }
    }

    public class ClassB
    {
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }

        private LinkedList<int> _prop5;
        public LinkedList<int> Prop5 { get { if (_prop4 == null) _prop5 = new LinkedList<int>(); return _prop5; } }

        private Collection<int> _prop6;
        public Collection<int> Prop6 { get { if (_prop6 == null) _prop6 = new Collection<int>(); return _prop6; } }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ObjectMapperImplementInterfaceAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ObjectMapperImplementInterfaceAnalyzerAnalyzer();
        }
    }
}