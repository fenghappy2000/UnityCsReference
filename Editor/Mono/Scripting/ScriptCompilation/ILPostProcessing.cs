// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditorInternal;

namespace UnityEditor.Scripting.ScriptCompilation
{
    internal interface IILPostProcessing
    {
        bool HasPostProcessors { get; }
        string[] PostProcessorAssemblyPaths { get; }
        string[] AssemblySearchPaths { get; }
    }

    internal static class ILPostProcessingSearchPaths
    {
        class SearchPathComparer : IComparer<string>
        {
            readonly string _editorAssemblyPath;
            readonly string _editorAssemblyPathPrefix;

            public SearchPathComparer(string editorContentPath)
            {
                _editorAssemblyPath = $"{editorContentPath}/Managed";
                _editorAssemblyPathPrefix = $"{editorContentPath}/Managed/";
            }

            int SearchPathPriority(string path)
            {
                if (path == _editorAssemblyPath || path.StartsWith(_editorAssemblyPathPrefix))
                {
                    return 0;
                }
                return 1;
            }
            public int Compare(string leftPath, string rightPath)
            {
                var leftPrio = SearchPathPriority(leftPath);
                var rightPrio = SearchPathPriority(rightPath);
                if (leftPrio == rightPrio)
                {
                    return Comparer<string>.Default.Compare(leftPath, rightPath);
                }
                return Comparer<int>.Default.Compare(leftPrio, rightPrio);
            }
        }
        public static IEnumerable<string> SortSearchPathWithEditorAssembliesFirst(this IEnumerable<string> searchPaths, string editorContentPath)
        {
            return searchPaths.OrderBy(path => path, new SearchPathComparer(editorContentPath));
        }
    }

    internal class ILPostProcessing : IILPostProcessing
    {
        EditorCompilation editorCompilation;

        TargetAssembly[] codeGenAssemblies;
        string[] postProcessorAssemblyPaths;
        string[] assemblySearchPaths;

        public ILPostProcessing(EditorCompilation editorCompilation)
        {
            this.editorCompilation = editorCompilation;
        }

        TargetAssembly[] CodeGenAssemblies
        {
            get
            {
                if (codeGenAssemblies == null)
                {
                    codeGenAssemblies = editorCompilation.CustomTargetAssemblies.Where(e => UnityCodeGenHelpers.IsCodeGen(e.Value.Filename)).Select(e => e.Value).ToArray();
                }

                return codeGenAssemblies;
            }
        }


        public bool HasPostProcessors
        {
            get
            {
                return CodeGenAssemblies != null && CodeGenAssemblies.Length > 0;
            }
        }

        public string[] PostProcessorAssemblyPaths
        {
            get
            {
                if (postProcessorAssemblyPaths == null)
                {
                    postProcessorAssemblyPaths = CodeGenAssemblies.Select(a => AssetPath.GetFullPath(a.FullPath(editorCompilation.GetEditorAssembliesOutputDirectory()))).ToArray();
                }

                return postProcessorAssemblyPaths;
            }
        }

        public string[] AssemblySearchPaths
        {
            get
            {
                if (assemblySearchPaths == null)
                {
                    var assemblyReferences = CodeGenAssemblies.SelectMany(a => a.References);
                    var precompiledReferences = CodeGenAssemblies.SelectMany(a => a.ExplicitPrecompiledReferences);

                    var assemblyOutputFullPath = AssetPath.GetFullPath(editorCompilation.GetEditorAssembliesOutputDirectory());
                    var assemblyReferencesPaths = assemblyReferences.Select(a => AssetPath.GetFullPath(a.FullPath(assemblyOutputFullPath)));

                    var precompiledAssembliesDictionary = editorCompilation.PrecompiledAssemblyProvider.GetPrecompiledAssembliesDictionary(EditorScriptCompilationOptions.BuildingForEditor | EditorScriptCompilationOptions.BuildingWithAsserts, BuildTargetGroup.Unknown, BuildTarget.Android);

                    var precompiledReferencesPaths = precompiledReferences
                        .Where(x => precompiledAssembliesDictionary.ContainsKey(x))
                        .Select(x => precompiledAssembliesDictionary[x].Path);

                    List<string> paths = new List<string>();

                    paths.Add(InternalEditorUtility.GetEngineCoreModuleAssemblyPath());
                    paths.Add(InternalEditorUtility.GetEditorAssemblyPath());
                    paths.Add(assemblyOutputFullPath);

                    var allPaths = assemblyReferencesPaths.Concat(precompiledReferencesPaths).Concat(paths);
                    assemblySearchPaths = allPaths.Select(AssetPath.GetDirectoryName).Distinct().SortSearchPathWithEditorAssembliesFirst(EditorApplication.applicationContentsPath).ToArray();
                }

                return assemblySearchPaths;
            }
        }
    }
}
