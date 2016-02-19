using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace ObjectMapper.GenerateImplementationAnalyzer.Test
{
    [TestClass]
    public class GenerateImplementationUnitTest : CodeFixVerifier
    {
        #region TestEmptyCompilationUnit
        /// <summary>
        /// Tests the empty compilation unit.
        /// </summary>
        [TestMethod]
        public void TestEmptyCompilationUnit()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }
        #endregion

        #region TestIObjectMapperInterfaceWithoutDefinedMethods
        /// <summary>
        /// Tests the IObjectMapper interface without interface methods defined.
        /// </summary>
        [TestMethod]
        public void TestIObjectMapperInterfaceWithoutDefinedMethods()
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
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 27) }
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
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TestIObjectMapperInterfaceWithDefinedMethods
        /// <summary>
        /// Tests the IObjectMapper interface with interface methods defined.
        /// </summary>
        [TestMethod]
        public void TestIObjectMapperInterfaceWithDefinedMethods()
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

        public void MapObject(ClassB target)
        {
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
            var expectedOnInterface = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 27) }
            };

            var expectedOnMethod = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 21) }
            };

            VerifyCSharpDiagnostic(test, expectedOnInterface, expectedOnMethod);

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
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TestIObjectMapperAdapterInterfaceWithoutDefinedMethods
        /// <summary>
        /// Tests the IObjectMapperAdapter interface without interface methods defined.
        /// </summary>
        [TestMethod]
        public void TestIObjectMapperAdapterInterfaceWithoutDefinedMethods()
        {
            var test = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public class MapperAdapter : IObjectMapperAdapter<ClassA, ClassB>
    {

    }
}";
            var expected = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 40, 34) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public class MapperAdapter : IObjectMapperAdapter<ClassA, ClassB>
    {
        public void MapObject(ClassA source, ClassB target)
        {
            target.Prop1 = source.Prop1 ?? default(int);
            target.Prop2 = source.Prop2;
            target.Prop3 = source.Prop3;
            target.Prop4.CopyFrom(source.Prop4);
            target.Prop5.CopyFrom(source.Prop5);
            target.Prop6.CopyFrom(source.Prop6);
        }

        public void MapObject(ClassB source, ClassA target)
        {
            target.Prop1 = source.Prop1;
            target.Prop2 = source.Prop2;
            target.Prop3 = source.Prop3;
            target.Prop4.CopyFrom(source.Prop4);
            target.Prop5.CopyFrom(source.Prop5);
            target.Prop6.CopyFrom(source.Prop6);
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TestIObjectMapperAdapterInterfaceWithDefinedMethods
        /// <summary>
        /// Tests the IObjectMapperAdapter interface with interface methods defined.
        /// </summary>
        [TestMethod]
        public void TestIObjectMapperAdapterInterfaceWithDefinedMethods()
        {
            var test = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public class MapperAdapter : IObjectMapperAdapter<ClassA, ClassB>
    {
        public void MapObject(ClassA source, ClassB target)
        {
        }

        public void MapObject(ClassB source, ClassA target)
        {
        }
    }
}";
            var expectedOnInterface = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 40, 34) }
            };

            var expectedOnFirstMethod = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 42, 21) }
            };

            var expectedOnSecondMethod = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 46, 21) }
            };

            VerifyCSharpDiagnostic(test, expectedOnInterface, expectedOnFirstMethod, expectedOnSecondMethod);

            var fixtest = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public class MapperAdapter : IObjectMapperAdapter<ClassA, ClassB>
    {
        public void MapObject(ClassA source, ClassB target)
        {
            target.Prop1 = source.Prop1 ?? default(int);
            target.Prop2 = source.Prop2;
            target.Prop3 = source.Prop3;
            target.Prop4.CopyFrom(source.Prop4);
            target.Prop5.CopyFrom(source.Prop5);
            target.Prop6.CopyFrom(source.Prop6);
        }

        public void MapObject(ClassB source, ClassA target)
        {
            target.Prop1 = source.Prop1;
            target.Prop2 = source.Prop2;
            target.Prop3 = source.Prop3;
            target.Prop4.CopyFrom(source.Prop4);
            target.Prop5.CopyFrom(source.Prop5);
            target.Prop6.CopyFrom(source.Prop6);
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region TestObjectMapperMethodAttribute
        /// <summary>
        /// Tests ObjectMapperMethodAttribute.
        /// </summary>
        [TestMethod]
        public void TestObjectMapperMethodAttribute()
        {
            var test = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public void MapObject(ClassA source, ClassB target)
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "OMCG01",
                Message = "Implementation of mapping method(s) can be generated.",
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 43, 21) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using ObjectMapper.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestClassLibrary
{
    public class ClassA
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

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public void MapObject(ClassA source, ClassB target)
        {
            target.Prop1 = source.Prop1 ?? default(int);
            target.Prop2 = source.Prop2;
            target.Prop3 = source.Prop3;
            target.Prop4.CopyFrom(source.Prop4);
            target.Prop5.CopyFrom(source.Prop5);
            target.Prop6.CopyFrom(source.Prop6);
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }
        #endregion

        #region Setup
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new GenerateImplementationCodeGenerator();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new GenerateImplementationCodeAnalyzer();
        }
        #endregion
    }
}