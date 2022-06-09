using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;

namespace LittleWitchTranslate
{
    public class Mod : ModManagerGUI.IMod
    {
        public override bool Verify(string directory)
        {
            if (directory == null)
                return false;
            for (var i = 0; i < ModManagerGUI.ModManager.Configuration.FileNames.Length; i++)
            {
                var name = ModManagerGUI.ModManager.Configuration.FileNames[i];
                if (File.Exists(Path.Combine(directory, name + "_Data", "Managed", "Assembly-CSharp.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "Unity.TextMeshPro.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "DialogueSystem.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "UnityEngine.CoreModule.dll")) &&
                    File.Exists(Path.Combine(directory, name + ".exe")))
                    return true;
            }
            return false;
        }

        public string GetGameName(string directory)
        {
            if (directory == null)
                return null;
            for (var i = 0; i < ModManagerGUI.ModManager.Configuration.FileNames.Length; i++)
            {
                var name = ModManagerGUI.ModManager.Configuration.FileNames[i];
                if (File.Exists(Path.Combine(directory, name + "_Data", "Managed", "Assembly-CSharp.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "Unity.TextMeshPro.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "DialogueSystem.dll")) &&
                    File.Exists(Path.Combine(directory, name + "_Data", "Managed", "UnityEngine.CoreModule.dll")) &&
                    File.Exists(Path.Combine(directory, name + ".exe")))
                    return name;
            }
            return null;
        }

        private void WriteLog(string log)
        {
            if (OnLog != null)
                OnLog(log);
        }
        public override void Apply(string gameDirectory, HashSet<int> options)
        {
            var gameName = GetGameName(gameDirectory);
            WriteLog("Game name is: " + gameName);
            WriteLog("Copying files if needed...");

            var managedPath = Path.Combine(gameDirectory, gameName + "_Data", "Managed");

            bool isModded = CheckIfModded(gameDirectory, gameName);
            CopyFiles(gameDirectory, isModded, gameName);
            var translatorBackupPath = Path.Combine(gameDirectory, "TranslatorBackup");

            WriteLog("Reading assemblies and fetching types and enums...");

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.Combine(managedPath));

            WriteLog("Fetching methods from TranslatorPlugin.dll");
            MethodDefinition translateStringMethod = null;
            MethodDefinition changeTextMeshMethod = null;
            MethodDefinition getTextMethod = null;
            MethodDefinition textAssetConstructor = null;
            MethodDefinition initializeMethod = null;
            MethodDefinition checkDatabaseMethod = null;

            var translatorAssembly = AssemblyDefinition.ReadAssembly(System.IO.Path.Combine(managedPath, "TranslatorPlugin.dll"));
            var translatorType = translatorAssembly.MainModule.GetType("TranslatorPlugin.Translator");

            foreach (var method in translatorType.Methods)
            {
                if (method.Name == "TranslateString")
                    translateStringMethod = method;
                if (method.Name == "ChangeTextMesh")
                    changeTextMeshMethod = method;
                if (method.Name == "Initialize")
                    initializeMethod = method;
                if (method.Name == "CheckDatabase")
                    checkDatabaseMethod = method;
            }

            WriteLog("Fetching methods from UnityEngine.CoreModule.dll");

            var unityEngineCoreModule = System.IO.Path.Combine(translatorBackupPath, "UnityEngine.CoreModule.dll");
            var coreModuleAssembly = AssemblyDefinition.ReadAssembly(unityEngineCoreModule, new ReaderParameters() { AssemblyResolver = resolver });
            MethodDefinition objectSetName = null;
            MethodDefinition objectGetName = null;
            var objectClass = coreModuleAssembly.MainModule.GetType("UnityEngine.Object");
            foreach (var p in objectClass.Properties)
            {
                if (p.Name == "name")
                {
                    objectSetName = p.SetMethod;
                    objectGetName = p.GetMethod;
                }
            }
            var textAssetClass = coreModuleAssembly.MainModule.GetType("UnityEngine.TextAsset");

            foreach (var p in textAssetClass.Properties)
            {
                if (p.Name == "text")
                    getTextMethod = p.GetMethod;
            }
            foreach (var m in textAssetClass.Methods)
            {
                if (m.Name == ".ctor" && m.Parameters.Count == 1)
                    textAssetConstructor = m;
            }

            if (translateStringMethod == null || changeTextMeshMethod == null || getTextMethod == null || textAssetConstructor == null)
            {
                WriteLog("Couldn't find necessary methods. Can't proceed :(");
                return;
            }

            WriteLog("Patching Unity.TextMeshPro.dll");
            var textmeshPro = System.IO.Path.Combine(translatorBackupPath, "Unity.TextMeshPro.dll");
            var textMeshProAssembly = AssemblyDefinition.ReadAssembly(textmeshPro, new ReaderParameters() { AssemblyResolver = resolver });
            var textMeshProModule = textMeshProAssembly.MainModule;
            var tmpText = textMeshProModule.GetType("TMPro.TMP_Text");
            PropertyDefinition textProperty = null;
            foreach (var property in tmpText.Properties)
            {
                if (property.Name == "text")
                    textProperty = property;
            }
            var getStringRef = textMeshProModule.ImportReference(translateStringMethod);
            var changeTextRef = textMeshProModule.ImportReference(changeTextMeshMethod);

            if (textProperty != null && translateStringMethod != null)
            {
                var mRef = textMeshProModule.ImportReference(translateStringMethod);
                var setMethod = textProperty.SetMethod;
                var body = setMethod.Body;
                var processor = body.GetILProcessor();
                var firstInstruction = body.Instructions[0];
                if (firstInstruction.OpCode.Code == Code.Ldarg_0)
                {
                    WriteLog("Patching TMPro.TMP_Text.text setter...");
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_1));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldstr, setMethod.FullName));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, mRef));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Starg_S, setMethod.Parameters[0]));
                }
            }

            var textMeshProGUI = textMeshProModule.GetType("TMPro.TextMeshProUGUI");
            MethodDefinition startMethod = null;
            foreach (var method in textMeshProGUI.Methods)
            {
                if (method.Name == "Start")
                    startMethod = method;
            }

            if (startMethod == null)
            {
                WriteLog("Adding start method to TMPro.TextMeshProUGUI...");
                startMethod = new MethodDefinition("Start", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, textMeshProModule.TypeSystem.Void);
                textMeshProGUI.Methods.Add(startMethod);
            }

            if (startMethod != null)
            {
                WriteLog("Filling method body of TMPro.TextMeshProUGUI.Start...");
                var setMethod = textProperty.SetMethod;
                var getMethod = textProperty.GetMethod;
                var body = startMethod.Body;
                body.Instructions.Clear();
                var processor = body.GetILProcessor();

                processor.Emit(OpCodes.Nop);
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Call, textMeshProModule.ImportReference(getMethod));
                processor.Emit(OpCodes.Ldstr, startMethod.FullName);
                processor.Emit(OpCodes.Call, getStringRef);
                processor.Emit(OpCodes.Call, textMeshProModule.ImportReference(setMethod));
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Call, changeTextRef);
                processor.Emit(OpCodes.Nop);
                processor.Emit(OpCodes.Ret);
            }

            WriteLog("Saving Unity.TextMeshPro.dll");
            textMeshProAssembly.Write(System.IO.Path.Combine(managedPath, "Unity.TextMeshPro.dll"));

            WriteLog("Assembly saved successfully!");

            WriteLog("Patching DialogueSystem.dll");
            var dialogueSystem = System.IO.Path.Combine(translatorBackupPath, "DialogueSystem.dll");
            var dialogueSystemAssembly = AssemblyDefinition.ReadAssembly(dialogueSystem, new ReaderParameters() { AssemblyResolver = resolver });
            var dialogueSystemModule = dialogueSystemAssembly.MainModule;
            var dialogueDatabase = dialogueSystemModule.GetType("PixelCrushers.DialogueSystem.DialogueDatabase");

            foreach (var method in dialogueDatabase.Methods)
            {
                if (method.Name == "Add")
                {
                    var body = method.Body;
                    body.SimplifyMacros();
                    var proc = body.GetILProcessor();
                    var first = body.Instructions[0];
                    proc.InsertBefore(first, proc.Create(OpCodes.Ldarg_1));
                    proc.InsertBefore(first, proc.Create(OpCodes.Call, method.Module.ImportReference(checkDatabaseMethod)));
                    body.Optimize();
                }
            }
            WriteLog("Saving DialogueSystem.dll");

            dialogueSystemAssembly.Write(System.IO.Path.Combine(managedPath, "DialogueSystem.dll"));

            WriteLog("Assembly saved successfully!");

            var assemblyCSharp = System.IO.Path.Combine(translatorBackupPath, "Assembly-CSharp.dll");
            var assemblyCSharpAssembly = AssemblyDefinition.ReadAssembly(assemblyCSharp, new ReaderParameters { AssemblyResolver = resolver });
            var assemblyCSharpModule = assemblyCSharpAssembly.MainModule;
            assemblyCSharpModule.Resources.Add(new EmbeddedResource("Modded", ManifestResourceAttributes.Public, new byte[] { }));

            WriteLog("Saving Assembly-CSharp.dll");
            assemblyCSharpAssembly.Write(System.IO.Path.Combine(managedPath, "Assembly-CSharp.dll"));

            WriteLog("Patching complete! :)");
        }

        void CopyFiles(string gameDirectory, bool isModded, string gameName)
        {
            var translatorBackupPath = Path.Combine(gameDirectory, "TranslatorBackup");
            if (!Directory.Exists(translatorBackupPath))
            {
                WriteLog("Translator Backup directory doesn't exist yet. Creating now.");
                Directory.CreateDirectory(translatorBackupPath);
            }

            var currentDirectory = Environment.CurrentDirectory;
            if (Path.GetFullPath(currentDirectory) != Path.GetFullPath(gameDirectory))
            {
                var copyFiles = new string[] { "table.orig", "table.trans" };
                foreach (var copyFile in copyFiles)
                {
                    WriteLog("Copying " + copyFile);
                    File.Copy(Path.Combine(currentDirectory, copyFile), Path.Combine(gameDirectory, copyFile), true);
                }
            }

            var managedPath = Path.Combine(gameDirectory, gameName + "_Data", "Managed");

            WriteLog("Copying TranslatorPlugin.dll to Managed directory...");
            File.Copy(Path.Combine(currentDirectory, "TranslatorPlugin.dll"), Path.Combine(managedPath, "TranslatorPlugin.dll"), true);

            var checkFiles = new string[] { "Assembly-CSharp.dll", "Unity.TextMeshPro.dll", "DialogueSystem.dll", "UnityEngine.CoreModule.dll" };
            foreach (var checkFile in checkFiles)
            {
                if (!isModded || !System.IO.File.Exists(Path.Combine(translatorBackupPath, checkFile)))
                {
                    WriteLog(checkFile + " need to be copied. Copying now.");
                    File.Copy(Path.Combine(managedPath, checkFile), Path.Combine(translatorBackupPath, checkFile), true);
                }
            }
        }

        bool CheckIfModded(string gameDirectory, string gameName)
        {
            var managedPath = Path.Combine(gameDirectory, gameName + "_Data", "Managed");
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.Combine(managedPath));
            var assemblyCSharp = System.IO.Path.Combine(managedPath, "Assembly-CSharp.dll");
            var assemblyCSharpAssembly = AssemblyDefinition.ReadAssembly(assemblyCSharp, new ReaderParameters { AssemblyResolver = resolver });
            var assemblyCSharpModule = assemblyCSharpAssembly.MainModule;
            try
            {
                foreach (var resource in assemblyCSharpModule.Resources)
                {
                    if (resource.Name == "Modded")
                        return true;
                }
                return false;
            }
            finally
            {
                assemblyCSharpModule.Dispose();
                assemblyCSharpAssembly.Dispose();
            }

        }


    }
}