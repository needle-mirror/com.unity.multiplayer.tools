using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine;
using ILPPInterface = Unity.CompilationPipeline.Common.ILPostProcessing.ILPostProcessor;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Unity.Multiplayer.Tools.NetStats.CodeGen
{
    sealed class EventPayloadRegistrationILPP : ILPPInterface
    {
        public override ILPPInterface GetInstance() => this;

        static readonly string[] k_AllowListedAssemblyNames =
        {
            "Unity.Multiplayer.Tools.NetStats.Tests.Editor",
            "Unity.Multiplayer.Tools.NetworkProfiler.Tests.Editor"
        };

        public override bool WillProcess(ICompiledAssembly compiledAssembly) =>
            compiledAssembly.Defines.Contains("MULTIPLAYER_TOOLS")
            || k_AllowListedAssemblyNames.Contains(compiledAssembly.Name);

        readonly List<DiagnosticMessage> m_Diagnostics = new List<DiagnosticMessage>();

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
            {
                return null;
            }
            
            m_Diagnostics.Clear();
            
            // read
            var assemblyDefinition = CodeGenHelpers.AssemblyDefinitionFor(compiledAssembly, out var resolver);
            if (assemblyDefinition == null)
            {
                m_Diagnostics.AddError($"Cannot read assembly definition: {compiledAssembly.Name}");
                return null;
            }

            // process
            var mainModule = assemblyDefinition.MainModule;
            if (mainModule != null)
            {
                if (ImportReferences(mainModule))
                {
                    var fieldTypes = mainModule.GetTypes()
                        .SelectMany(t => t.Fields)
                        .Where(f => IsEventType(f.FieldType))
                        .Select(f => ((GenericInstanceType)f.FieldType).GenericArguments[0].Resolve());

                    var variableTypes = mainModule.GetTypes()
                        .SelectMany(t => t.Methods)
                        .Where(m => m.HasBody)
                        .SelectMany(m => m.Body.Variables)
                        .Where(v => IsEventType(v.VariableType))
                        .Select(v => ((GenericInstanceType)v.VariableType).GenericArguments[0].Resolve());

                    var metricTypes = new List<TypeDefinition>();
                    metricTypes.AddRange(fieldTypes);
                    metricTypes.AddRange(variableTypes);

                    metricTypes = metricTypes.Distinct().Where(n => n != null).ToList();

                    if (metricTypes.Count == 0)
                    {
                        return null;
                    }

                    try
                    {
                        CreateModuleInitializer(mainModule, assemblyDefinition, metricTypes);
                    }
                    catch (Exception e)
                    {
                        m_Diagnostics.AddError((e.ToString() + e.StackTrace.ToString()).Replace("\n", "|").Replace("\r", "|"));
                    }
                }
                else
                {
                    m_Diagnostics.AddError($"Cannot import references into main module: {mainModule.Name}");
                }
            }
            else
            {
                m_Diagnostics.AddError($"Cannot get main module from assembly definition: {compiledAssembly.Name}");
            }

            // write
            var pe = new MemoryStream();
            var pdb = new MemoryStream();

            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            assemblyDefinition.Write(pe, writerParameters);

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), m_Diagnostics);
        }

        TypeReference m_EventType_TypeRef;
        TypeReference m_EventMetricFactory_TypeRef;
        MethodReference m_EventMetricFactory_Register_TypeRef;

        bool ImportReferences(ModuleDefinition moduleDefinition)
        {
            m_EventType_TypeRef = moduleDefinition.ImportReference(typeof(EventMetric<>));
            m_EventMetricFactory_TypeRef = moduleDefinition.ImportReference(typeof(EventMetricFactory));
            m_EventMetricFactory_Register_TypeRef =
                moduleDefinition.ImportReference(
                    typeof(EventMetricFactory).GetMethod(
                        nameof(EventMetricFactory.RegisterType),
                        BindingFlags.Static | BindingFlags.NonPublic));
            
            return true;
        }

        bool IsEventType(TypeReference type)
        {
            return type.IsGenericInstance && type.Resolve() == m_EventType_TypeRef.Resolve();
        }
        
        MethodDefinition GetOrCreateStaticConstructor(TypeDefinition typeDefinition)
        {
            var staticCtorMethodDef = typeDefinition.GetStaticConstructor();
            if (staticCtorMethodDef == null)
            {
                staticCtorMethodDef = new MethodDefinition(
                    ".cctor", // Static Constructor (constant-constructor)
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName |
                    MethodAttributes.Static,
                    typeDefinition.Module.TypeSystem.Void);
                staticCtorMethodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                typeDefinition.Methods.Add(staticCtorMethodDef);
            }

            return staticCtorMethodDef;
        }

        // Creates a static module constructor (which is executed when the module is loaded) that registers all the
        // metric types types in the assembly with the EventMetricFactory
        void CreateModuleInitializer(ModuleDefinition module, AssemblyDefinition assembly, List<TypeDefinition> metricTypes)
        {
            foreach (var typeDefinition in assembly.MainModule.Types)
            {
                if (typeDefinition.FullName == "<Module>")
                {
                    var staticCtorMethodDef = GetOrCreateStaticConstructor(typeDefinition);

                    var processor = staticCtorMethodDef.Body.GetILProcessor();

                    var instructions = new List<Instruction>();

                    foreach (var type in metricTypes)
                    {
                        var importedType = module.ImportReference(type);
                        
                        var method = new MethodReference(m_EventMetricFactory_Register_TypeRef.Name, module.TypeSystem.Void, m_EventMetricFactory_TypeRef);
                        var genericParameter = new GenericParameter(method);
                        method.GenericParameters.Add(genericParameter);
                        method.HasThis = false;
                        var genericInstanceMethod = new GenericInstanceMethod(method);
                        genericInstanceMethod.GenericArguments.Add(importedType);

                        instructions.Add(processor.Create(OpCodes.Call, genericInstanceMethod));
                    }

                    instructions.ForEach(instruction => processor.Body.Instructions.Insert(processor.Body.Instructions.Count - 1, instruction));
                    break;
                }
            }
        }

    }
}
