// https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html
// https://docs.unity3d.com/ScriptReference/AssemblyReloadEvents.html

using System.IO;
using Mono.Cecil;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;

namespace Mirror.Weaver
{
    // InitializeOnLoad is needed for Unity to call the static constructor on load
    [InitializeOnLoad]
    public class CompilationFinishedHook
    {
        static CompilationFinishedHook()
        {
            //UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEvents_BeforeAssemblyReload;


            // assemblyPath: Library/ScriptAssemblies/Assembly-CSharp.dll/
            // assemblyPath: Library/ScriptAssemblies/Assembly-CSharp-Editor.dll

            CompilationPipeline.assemblyCompilationFinished += (assemblyPath, messages) =>
            {
                EditorApplication.LockReloadAssemblies();
                                 
                // UnityEngineCoreModule.DLL path:
                string unityEngineCoreModuleDLL = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
                //Debug.Log("unityEngineCoreModuleDLL=" + unityEngineCoreModuleDLL);

                // outputDirectory is the directory of assemblyPath
                string outputDirectory = Path.GetDirectoryName(assemblyPath);
                //Debug.Log("outputDirectory=" + outputDirectory);

                // unity calls it for Library/ScriptAssemblies/Assembly-CSharp-Editor.dll too, but we don't want to (and can't) weave this one
                bool buildingForEditor = assemblyPath.EndsWith("Editor.dll");
                string unetDLL = "Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll";

                if (assemblyPath == unetDLL)
                {
                    Debug.Log("Cannot weave mirror runtime");
                    return;
                }

                if (!buildingForEditor)
                {
                    // assemblyResolver: unity uses this by default:
                    //   ICompilationExtension compilationExtension = GetCompilationExtension();
                    //   IAssemblyResolver assemblyResolver = compilationExtension.GetAssemblyResolver(editor, file, null);
                    // but Weaver creates it's own if null, which is this one:
                    IAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
                    if (Program.Process(unityEngineCoreModuleDLL, unetDLL, outputDirectory, new string[1] { assemblyPath }, new string[0], assemblyResolver, Debug.LogWarning, Debug.LogError))
                    {
                        UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
                        Debug.Log("Weaving succeeded for: " + assemblyPath);
                    }
                    else
                        Debug.LogError("Weaving failed for: " + assemblyPath);
                }

                EditorApplication.UnlockReloadAssemblies();

            };

        }
        /*
        static private bool loading = false;

        static void AssemblyReloadEvents_BeforeAssemblyReload()
        {
            if (loading)
            {
                Debug.LogError("Already loading");
                return;
            }
            loading = true;
            // UnityEngineCoreModule.DLL path:
            string unityEngineCoreModuleDLL = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
            //Debug.Log("unityEngineCoreModuleDLL=" + unityEngineCoreModuleDLL);
            string assemblyPath = "Library/ScriptAssemblies/Assembly-CSharp.dll";

            string unetDLL = "Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll";

            // outputDirectory is the directory of assemblyPath
            string outputDirectory = Path.GetDirectoryName(assemblyPath);
            //Debug.Log("outputDirectory=" + outputDirectory);

            // unity calls it for Library/ScriptAssemblies/Assembly-CSharp-Editor.dll too, but we don't want to (and can't) weave this one
            bool buildingForEditor = assemblyPath.EndsWith("Editor.dll");

            if (!buildingForEditor)
            {
                // assemblyResolver: unity uses this by default:
                //   ICompilationExtension compilationExtension = GetCompilationExtension();
                //   IAssemblyResolver assemblyResolver = compilationExtension.GetAssemblyResolver(editor, file, null);
                // but Weaver creates it's own if null, which is this one:
                IAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
                if (Program.Process(unityEngineCoreModuleDLL, unetDLL, outputDirectory, new string[1] { assemblyPath }, new string[0], assemblyResolver, Debug.LogWarning, Debug.LogError))
                {
                    Debug.Log("Weaving succeeded for: " + assemblyPath);
                }
                else
                    Debug.LogError("Weaving failed for: " + assemblyPath);
            }
            loading = false;
        }
        */
    }
}