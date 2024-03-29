﻿using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Multiplayer.Tools.NetStats.CodeGen
{
    internal interface IAssemblyProcessor
    {
        bool ImportReferences(ModuleDefinition moduleDefinition,
            IAssemblyProcessingLogger logger);

        bool ProcessAssembly(
            ICompiledAssembly assembly,
            AssemblyDefinition assemblyDefinition,
            ModuleDefinition mainModule,
            IAssemblyProcessingLogger logger);
    }
}
