using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace ObjectMapper.GenerateImplementationAnalyzer.Test
{
    [TestClass]
    public class AttributeUsageUnitTests : DiagnosticVerifier
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

        #region TestValidUse
        /// <summary>
        /// Tests the valid use.
        /// </summary>
        [TestMethod]
        public void TestValidUse()
        {
            var test = @"
using ObjectMapper.Framework;

namespace TestClassLibrary
{
    public class ClassA
    {
    }

    public class ClassB
    {
    }

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public void MapObject(ClassA source, ClassB target)
        {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }
        #endregion

        #region TestUseWithInvalidReturnType
        /// <summary>
        /// Tests use with invalid return type.
        /// </summary>
        [TestMethod]
        public void TestUseWithInvalidReturnType()
        {
            var test = @"
using ObjectMapper.Framework;

namespace TestClassLibrary
{
    public class ClassA
    {
    }

    public class ClassB
    {
    }

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public ClassA MapObject(ClassA source, ClassB target)
        {
            return source;
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "OMAU01",
                Message = "Attribute cannot be applied to this method.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region TestUseWithTooLittleArguments
        /// <summary>
        /// Tests the use with too little arguments.
        /// </summary>
        [TestMethod]
        public void TestUseWithTooLittleArguments()
        {
            var test = @"
using ObjectMapper.Framework;

namespace TestClassLibrary
{
    public class ClassA
    {
    }

    public class ClassB
    {
    }

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public void MapObject(ClassA source)
        {
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "OMAU01",
                Message = "Attribute cannot be applied to this method.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region TestUseWithTooManyArguments
        /// <summary>
        /// Tests the use with too many arguments.
        /// </summary>
        [TestMethod]
        public void TestUseWithTooManyArguments()
        {
            var test = @"
using ObjectMapper.Framework;

namespace TestClassLibrary
{
    public class ClassA
    {
    }

    public class ClassB
    {
    }

    public static class MappingClass
    {
        [ObjectMapperMethod]
        public void MapObject(ClassA source, ClassB target, ClassB tooMany)
        {
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "OMAU01",
                Message = "Attribute cannot be applied to this method.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        #endregion

        #region Setup
        /// <summary>
        /// Get the CSharp analyzer being tested - to be implemented in non-abstract class
        /// </summary>
        /// <returns></returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AttributeUsageCodeAnalyzer();
        }
        #endregion
    }
}
