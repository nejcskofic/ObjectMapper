using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapper.GenerateImplementationAnalyzer.Utilities
{
    internal static class FrameworkHelpers
    {
        private static readonly ImmutableArray<byte> _publicKeyToken = ImmutableArray.Create<byte>(38, 23, 68, 249, 236, 31, 118, 87);
        public static bool IsObjectMapperFrameworkAssembly(IAssemblySymbol assemblySymbol)
        {
            if (assemblySymbol.Name != "ObjectMapper.Framework")
            {
                return false;
            }
            if (!assemblySymbol.Identity.IsStrongName || !_publicKeyToken.SequenceEqual(assemblySymbol.Identity.PublicKeyToken))
            {
                return false;
            }
            return true;
        }
    }
}
