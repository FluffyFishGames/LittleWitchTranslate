using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SunHavenTranslate
{
    class Program
    {
        protected static List<string> ValidGameNames = new List<string>() { "LWIW", "Little Witch in the Woods" };
        protected static string GameName = "LWIW";
        static void Main(string[] args)
        {
            var directory = Environment.CurrentDirectory;
            directory = @"G:\Games\Little Witch in the Woods";
            //args = new string[] { "extract" };
            if (args.Length > 1 && (args[0] == "extract" || args[0] == "merge"))
                directory = args[1];

            while (true)
            {
                GameName = Verify(directory);
                if (GameName != null)
                    break;
                if (directory != "") System.Console.WriteLine(IsGerman() ? "Spiel nicht gefunden unter: " + directory : "Game was not found at path " + directory);
                System.Console.WriteLine(IsGerman() ? "Bitte geben Sie den Pfad zu Ihrem Spiel an: " : "Please enter the game path: ");
                directory = System.Console.ReadLine();
            }

            System.Console.WriteLine(IsGerman() ? "Spiel gefunden. Wende Änderungen an..." : "Game found. Applying patch...");
            Apply(directory);
        }

        static string Verify(string directory)
        {
            for (var i = 0; i < ValidGameNames.Count; i++)
            {
                if (File.Exists(Path.Combine(directory, ValidGameNames[i] + "_Data", "Managed", "Assembly-CSharp.dll")) &&
                    File.Exists(Path.Combine(directory, ValidGameNames[i] + "_Data", "Managed", "Unity.TextMeshPro.dll")) &&
                    File.Exists(Path.Combine(directory, ValidGameNames[i] + "_Data", "Managed", "DialogueSystem.dll")) &&
                    File.Exists(Path.Combine(directory, ValidGameNames[i] + "_Data", "Managed", "UnityEngine.CoreModule.dll")) &&
                    File.Exists(Path.Combine(directory, ValidGameNames[i] + ".exe")))
                    return ValidGameNames[i];
            }
            return null;
        }

        static bool IsGerman()
        {
            return CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.ToLowerInvariant() == "de";
        }

        static void Apply(string directory)
        {
            var translatorBackupPath = Path.Combine(directory, "TranslatorBackup");
            var versionFile = Path.Combine(translatorBackupPath, "version");
            var gameWasUpdated = false;
            var currentDirectory = Environment.CurrentDirectory;
            var checkFiles = new string[] { "TranslatorPlugin.dll", "table.orig", "table.trans" };
            foreach (var checkFile in checkFiles)
            {
                if (!File.Exists(Path.Combine(currentDirectory, checkFile)))
                {
                    System.Console.WriteLine(IsGerman() ? checkFile + " konnte nicht gefunden werden. Fortsetzung nicht möglich :(" : checkFile + " is missing! Can't proceed :(");
                    System.Console.Read();
                    return;
                }
            }

            System.Console.WriteLine(IsGerman() ? "Prüfung abgeschlossen. Glückwunsch!" : "Sanity checks completed! Congratulations!");
            System.Console.WriteLine("");
            System.Console.WriteLine(IsGerman() ? "Willkommen bei meinem Little Witch in the Wood Übersetzer Mod" : "Welcome to my Little Witch in the Woods translator mod.");
            System.Console.WriteLine("");
            System.Console.WriteLine(IsGerman() ? "Diese Software ist kostenlos. Das heißt, wenn Sie hierfür bezahlt haben, wurden Sie betrogen." : "This software is freeware. This means if somebody made you pay for it you have been ripped off.");
            System.Console.WriteLine(IsGerman() ? "Die offizielle Downloadseite für diese Mod ist potatoepet.de" : "The official download site for this tool is potatoepet.de");
            System.Console.WriteLine(IsGerman() ? "Sie können den Quellcode dieser quelloffenen Mod auf github.com/FluffyFishGames finden" : "You can also find this software as open source on github.com/FluffyFishGames");
            System.Console.WriteLine("");
            System.Console.WriteLine(IsGerman() ? "Diese Mod wird Ihre Spieldateien verändern. Deswegen müssen Sie einige Dinge wissen." : "This tool will modify your game files. Therefore you need to know a couple of things.");
            System.Console.WriteLine(IsGerman() ? "Nicht jeder Fehler, den Sie im Spiel finden, tritt unbedingt in einem unmodifizierten Spiel auf." : "Not every bug you might encounter is necessarily in the unmodded game.");
            System.Console.WriteLine(IsGerman() ? "Bevor Sie also einen Fehler an SUNNY SIDE UP melden, stellen Sie sicher, dass der Fehler auch im unmodifizierten Spiel auftritt." : "Before submitting bug reports to SUNNY SIDE UP please ensure your error also occurs in an unmodded game first.");
            System.Console.WriteLine(IsGerman() ? "Sollte es ein Spielupdate geben, wird der Mod kaputt gehen. Versuchen Sie nicht den Mod einfach erneut auszuführen, da dies zu kaputten Spieldateien führen kann." : "In case of an update this mod WILL break. DO NOT just start it again to translate as it will corrupt your files.");
            System.Console.WriteLine(IsGerman() ? "Der beste Weg mit einem Update umzugehen, ist zu warten, bis der Mod aktualisiert wird." : "Best way to handle updates is to wait for this tool to get updated.");
            System.Console.WriteLine("");
            /*System.Console.WriteLine(IsGerman() ? "Zusätzliche Mods" : "Additional mods");
            System.Console.WriteLine(IsGerman() ? "Es gibt ein paar kleine zusätzliche Mods, die aktiviert werden können. Geben Sie dafür den Buchstaben \"c\" ein und wählen dann die entsprechenden Mods aus" : "There are a few small additional mods that can be activated. For this, enter the letter \"c\" and then select the appropriate mods");
            System.Console.WriteLine("");*/
            List<int> options = new List<int>();
            while (true)
            {
                System.Console.WriteLine(IsGerman() ? "Bitte bestätigen Sie, dass Sie diesen Text verstanden haben, indem Sie \"y\" eingeben und Ihre Eingabe mit der Enter-Taste bestätigen. "/*Alternativ drücken Sie die Taste \"c\" um Anpassungen vorzunehmen."*/ : "Please confirm that you have understood this small disclaimer by typing \"y\" and confirm your input by hitting return. " /*Alternatively, press the \"c\" key to make adjustments."*/);
                var input = System.Console.ReadLine().ToLowerInvariant().Trim();
                if (input == "y")
                {
                    System.Console.WriteLine(IsGerman() ? "Los geht's!" : "Here we go!");
                    ApplyPatches(directory, options);
                    break;
                }
                /*else if (input == "c")
                {
                    System.Console.WriteLine("");
                    System.Console.WriteLine(IsGerman() ? "Anpassungen" : "Adjustements");
                    System.Console.WriteLine("");
                    System.Console.WriteLine(IsGerman() ? "Folgende Anpassungen stehen zur Verfügung. Geben Sie die Zahlen der Einträge, getrennt durch ein Leerzeichen, ein, um diese zu aktivieren." : "The following adjustments are available. Enter the numbers of the entries, separated by a space, to activate them.");
                    System.Console.WriteLine("");
                    System.Console.WriteLine(IsGerman() ? "1 - Jederzeit schlafen können" : "1 - Go to sleep anytime");
                    System.Console.WriteLine(IsGerman() ? "2 - Immer neue Dialoge" : "2 - Always new dialogue");
                    var input2 = System.Console.ReadLine().ToLowerInvariant().Trim();
                    var entries = input2.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    options.Clear();
                    for (var j = 0; j < entries.Length; j++)
                    {
                        if (entries[j] == "1")
                            options.Add(1);
                        if (entries[j] == "2")
                            options.Add(2);
                    }
                }*/
                else
                {
                }
            }
        }

        static bool CheckIfModded(string directory)
        {
            var managedPath = Path.Combine(directory, GameName + "_Data", "Managed");
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
        static void ApplyPatches(string directory, List<int> options)
        {
            System.Console.WriteLine("Copying files if needed...");

            var managedPath = Path.Combine(directory, GameName + "_Data", "Managed");

            bool isModded = CheckIfModded(directory);
            CopyFiles(directory, isModded);
            var translatorBackupPath = Path.Combine(directory, "TranslatorBackup");


            System.Console.WriteLine("Reading assemblies and fetching types and enums...");

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.Combine(managedPath));

            System.Console.WriteLine("Fetching methods from TranslatorPlugin.dll");
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

            System.Console.WriteLine("Fetching methods from UnityEngine.CoreModule.dll");

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
                System.Console.WriteLine("Couldn't find necessary methods. Can't proceed :(");
                System.Console.Read();
                return;
            }

            System.Console.WriteLine("Patching Unity.TextMeshPro.dll");
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
                    System.Console.WriteLine("Patching TMPro.TMP_Text.text setter...");
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
                System.Console.WriteLine("Adding start method to TMPro.TextMeshProUGUI...");
                startMethod = new MethodDefinition("Start", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, textMeshProModule.TypeSystem.Void);
                textMeshProGUI.Methods.Add(startMethod);
            }

            if (startMethod != null)
            {
                System.Console.WriteLine("Filling method body of TMPro.TextMeshProUGUI.Start...");
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

            System.Console.WriteLine("Saving Unity.TextMeshPro.dll");
            textMeshProAssembly.Write(System.IO.Path.Combine(managedPath, "Unity.TextMeshPro.dll"));

            System.Console.WriteLine("Assembly saved successfully!");

            System.Console.WriteLine("Patching DialogueSystem.dll");
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
            System.Console.WriteLine("Saving DialogueSystem.dll");

            dialogueSystemAssembly.Write(System.IO.Path.Combine(managedPath, "DialogueSystem.dll"));

            System.Console.WriteLine("Assembly saved successfully!");

            var assemblyCSharp = System.IO.Path.Combine(translatorBackupPath, "Assembly-CSharp.dll");
            var assemblyCSharpAssembly = AssemblyDefinition.ReadAssembly(assemblyCSharp, new ReaderParameters { AssemblyResolver = resolver });
            var assemblyCSharpModule = assemblyCSharpAssembly.MainModule;
            assemblyCSharpModule.Resources.Add(new EmbeddedResource("Modded", ManifestResourceAttributes.Public, new byte[] { }));
            
            System.Console.WriteLine("Saving Assembly-CSharp.dll");
            assemblyCSharpAssembly.Write(System.IO.Path.Combine(managedPath, "Assembly-CSharp.dll"));

            System.Console.WriteLine("Patching complete! :)");
            System.Console.ReadLine();
        }

        static void CopyFiles(string directory, bool isModded)
        {
            var translatorBackupPath = Path.Combine(directory, "TranslatorBackup");
            if (!Directory.Exists(translatorBackupPath))
            {
                System.Console.WriteLine("Translator Backup directory doesn't exist yet. Creating now.");
                Directory.CreateDirectory(translatorBackupPath);
            }

            var currentDirectory = Environment.CurrentDirectory;
            if (Path.GetFullPath(currentDirectory) != Path.GetFullPath(directory))
            {
                var copyFiles = new string[] { "table.orig", "table.trans" };
                foreach (var copyFile in copyFiles)
                {
                    System.Console.WriteLine("Copying " + copyFile);
                    File.Copy(Path.Combine(currentDirectory, copyFile), Path.Combine(directory, copyFile), true);
                }
            }

            var managedPath = Path.Combine(directory, GameName + "_Data", "Managed");

            System.Console.WriteLine("Copying TranslatorPlugin.dll to Managed directory...");
            File.Copy(Path.Combine(currentDirectory, "TranslatorPlugin.dll"), Path.Combine(managedPath, "TranslatorPlugin.dll"), true);

            var checkFiles = new string[] { "Assembly-CSharp.dll", "Unity.TextMeshPro.dll", "DialogueSystem.dll", "UnityEngine.CoreModule.dll" };
            foreach (var checkFile in checkFiles)
            {
                if (!isModded || !System.IO.File.Exists(Path.Combine(translatorBackupPath, checkFile)))
                {
                    System.Console.WriteLine(checkFile + " need to be copied. Copying now.");
                    File.Copy(Path.Combine(managedPath, checkFile), Path.Combine(translatorBackupPath, checkFile), true);
                }
            }
        }
    }
}
