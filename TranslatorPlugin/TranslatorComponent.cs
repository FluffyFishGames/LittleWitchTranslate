using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TranslatorPlugin
{
    public class TranslatorComponent : MonoBehaviour
    {
        void Start()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "Storyline")
            {
                ReplaceTextes();
            }
            //System.IO.File.AppendAllText("scene.txt", arg0.name + " _ " + arg1 + "\r\n");
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
        public static void ReplaceTextes()
        {
            var tables = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetAllTables().WaitForCompletion();
            foreach (var table in tables)
            {
                if (table.LocaleIdentifier.Code.StartsWith("en"))
                {
                    foreach (var val in table)
                    {
                        val.Value.Value = Translator.TranslateString(val.Value.Value, "");
                    }
                }
            }
            if (DialogueManager.MasterDatabase != null)
            {
                if (DialogueManager.MasterDatabase.conversations != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.conversations)
                    {
                        TranslateFields(kv.fields);
                        if (kv.dialogueEntries != null)
                            foreach (var d in kv.dialogueEntries)
                                TranslateFields(d.fields);
                    }
                }
                if (DialogueManager.MasterDatabase.keywords != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.keywords)
                    {
                        TranslateFields(kv.fields);
                    }
                }
                if (DialogueManager.MasterDatabase.items != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.items)
                    {
                        TranslateFields(kv.fields);
                    }
                }
                if (DialogueManager.MasterDatabase.locations != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.locations)
                    {
                        TranslateFields(kv.fields);
                    }
                }
                if (DialogueManager.MasterDatabase.actors != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.actors)
                    {
                        TranslateFields(kv.fields);
                    }
                }
                if (DialogueManager.MasterDatabase.variables != null)
                {
                    foreach (var kv in DialogueManager.MasterDatabase.variables)
                    {
                        TranslateFields(kv.fields);
                    }
                }
            }
        }
    }
}