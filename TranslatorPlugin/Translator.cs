using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;

namespace TranslatorPlugin
{
    public class Translator
    {
        private static HashSet<string> AllTranslations = new HashSet<string>();
        private static HashSet<string> MissingTranslations = new HashSet<string>();
        private static Dictionary<string, string> Translations = new Dictionary<string, string>();
        private static bool Initialized = false;
        private static bool ComponentsAdded = false;
        public static void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                //Ignore.Initialize();
                var lines1 = System.IO.File.ReadAllLines("table.orig");
                var lines2 = System.IO.File.ReadAllLines("table.trans");
                if (lines2.Length >= lines1.Length)
                {
                    for (var i = 0; i < lines1.Length; i++)
                    {
                        var l = lines1[i];
                        var ind = l.IndexOf("||");
                        if (ind > 0)
                            l = l.Substring(0, ind).Trim() + "||" + l.Substring(ind + 2).Trim();
                        else l = l.Trim();
                        //System.IO.File.AppendAllText("log.txt", l + "\r\n");
                        if (!Translations.ContainsKey(l))
                        {
                            Translations.Add(l, lines2[i]);
                            AllTranslations.Add(l.Trim());
                            AllTranslations.Add(lines2[i].Trim());
                        }
                    }
                }
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            }
        }

        public static void CheckDatabase(DialogueDatabase database)
        {
            return;
            try
            {
                if (database != null)
                {
                    if (database.conversations != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.conversations)
                        {
                            TranslateFields(kv.fields);
                            if (kv.dialogueEntries != null)
                                foreach (var d in kv.dialogueEntries)
                                    TranslateFields(d.fields);
                        }
                    }

                    if (database.keywords != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.keywords)
                        {
                            TranslateFields(kv.fields);
                        }
                    }

                    if (database.items != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.items)
                        {
                            TranslateFields(kv.fields);
                        }
                    }

                    if (database.locations != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.locations)
                        {
                            TranslateFields(kv.fields);
                        }
                    }

                    if (database.actors != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.actors)
                        {
                            TranslateFields(kv.fields);
                        }
                    }

                    if (database.variables != null)
                    {
                        foreach (var kv in DialogueManager.MasterDatabase.variables)
                        {
                            TranslateFields(kv.fields);
                        }
                    }
                }
            }
            catch (Exception e) { }
        }
        static void TranslateFields(List<Field> fields)
        {
            if (fields == null)
                return;
            foreach (var f in fields)
            {
                if (f.title.EndsWith("en") && f.value.Trim() != "")
                {
                    f.value = Translator.TranslateString(f.value, "");
                }
            }
        }
        private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (!ComponentsAdded)
            {
                var go = new UnityEngine.GameObject();
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<Extractor>();
                go.AddComponent<TranslatorComponent>();
                ComponentsAdded = true; 
            }
        }

        public static bool IsUnknown(string line)
        {
            return !AllTranslations.Contains(line.Trim());
        }

        public static void CheckForUnknown(string text)
        {
            if (!text.Contains("<alpha=#00 id = \"a\">"))
            {
                var lines = text.Split(new char[] { '\r', '\n' });
                foreach (var line in lines)
                {
                    if (!AllTranslations.Contains(line.Trim()) && !MissingTranslations.Contains(line.Trim()))
                    {
                        MissingTranslations.Add(line.Trim());
                        //System.IO.File.AppendAllText("missing.txt", line + "\r\n");
                    }
                }
            }
        }

        private static HashSet<string> DontTouchText = new HashSet<string>() {
        };

        private static HashSet<string> DontTouchTextParent = new HashSet<string>() {
        };

        private static HashSet<string> TextFields = new HashSet<string>();
        private static TMP_FontAsset FontAsset;

        public static void ChangeTextMesh(TMPro.TextMeshProUGUI text)
        {
            /*
            if (Font != "default")
            {
                if (FontAsset == null)
                {
                    MaterialReferenceManager.TryGetFontAsset(Font.GetHashCode(), out FontAsset);

                    if (FontAsset == null)
                    {
                        FontAsset = UnityEngine.Resources.Load<TMP_FontAsset>(TMP_Settings.defaultFontAssetPath + Font);
                        if (FontAsset == null)
                        {
                            try
                            {
                                var path = System.IO.Path.Combine(Environment.CurrentDirectory, Font.ToLowerInvariant());
                                //System.IO.File.AppendAllText("log.txt", "Trying to load font " + Font + " from " + path + "!\r\n");
                                if (System.IO.File.Exists(path))
                                {
                                    var bytes = System.IO.File.ReadAllBytes(path);
                                    //System.IO.File.AppendAllText("log.txt", "Loaded " + bytes.Length + " bytes!\r\n");
                                    var bundle = UnityEngine.AssetBundle.LoadFromMemory(bytes);
                                    //System.IO.File.AppendAllText("log.txt", "Asset bundle: " + bundle + "!\r\n");
                                    FontAsset = bundle.LoadAsset<TMP_FontAsset>("Assets/" + Font + ".asset");
                                    //System.IO.File.AppendAllText("log.txt", "Font " + Font + " found in assetbundle!\r\n");
                                }

                            }
                            catch (Exception e)
                            {
                            }
                        }
                        if (FontAsset != null)
                            MaterialReferenceManager.AddFontAsset(FontAsset);
                    }
                }
                if (FontAsset != null)
                    text.font = FontAsset;
            }
            if (TextScale != 100)
            {
                text.fontSize = text.fontSize * (TextScale / 100f);
                text.fontSizeMin = text.fontSizeMin * (TextScale / 100f);
                text.fontSizeMax = text.fontSizeMax * (TextScale / 100f);
            }
            if (!text.autoSizeTextContainer)
            {
                if (!DontTouchText.Contains(text.transform.name) && (text.transform.parent == null || !DontTouchTextParent.Contains(text.transform.parent.name)))
                {
                    text.fontSizeMax = text.fontSize;
                    text.fontSizeMin = text.fontSize / 3f;
                    text.enableAutoSizing = true;
                }
            }*/
        }

        public static string TrimAndGetWhitespaces(string str, out string before, out string after)
        {
            var emptySpacesBefore = 0;
            var emptySpacesAfter = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ')
                    break;
                emptySpacesBefore++;
            }
            for (var i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] != ' ')
                    break;
                emptySpacesAfter++;
            }
            before = new string(' ', emptySpacesBefore);
            after = new string(' ', emptySpacesAfter);
            return str.Trim();
        }

        private static string TranslateLine(string st, string context)
        {
            var ret = "";
            var trimmed = TrimAndGetWhitespaces(st, out var trimBefore, out var trimAfter);
            if (trimmed != "")
            {
                if (context != null && Translations.ContainsKey(context + "||" + trimmed))
                    return trimBefore + Translations[context + "||" + trimmed] + trimAfter;
                else if (Translations.ContainsKey(trimmed))
                    return trimBefore + Translations[trimmed] + trimAfter;
                else
                {
                    //Extractor.AddUnknown(st);
                    return st;
                }
            }
            else
                return st;
        }
        public static string TranslateString(string st, string context)
        {
            Initialize();
            if (st == null) return null;
            context = context.Trim();
            var currentStr = "";
            var ret = "";
            for (var i = 0; i < st.Length; i++)
            {
                if (st[i] == '\r' || st[i] == '\n')
                {
                    if (currentStr != "")
                    {
                        ret += TranslateLine(currentStr, context);
                    }
                    ret += st[i];
                    currentStr = "";
                }
                else currentStr += st[i];
            }

            ret += TranslateLine(currentStr, context);
            return ret;
        }
    }
}
