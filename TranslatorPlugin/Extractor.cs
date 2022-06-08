using Language.Lua;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.QuestMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TranslatorPlugin
{
    public class Extractor : MonoBehaviour
    {
        void Start()
        {
        }

        bool Extracted = false;
        HashSet<string> Found = new HashSet<string>();

        void ExtractFields(List<PixelCrushers.DialogueSystem.Field> fields)
        {
            if (fields == null)
                return;
            foreach (var f in fields)
            {
                if (f.title.EndsWith("en") && f.value.Trim() != "" && !Found.Contains(f.value.Trim()))
                {
                    Found.Add(f.value.Trim());
                    System.IO.File.AppendAllText("text.txt", f.value + "\r\n");
                }
            }
        }

        void ExtractTable(LuaTable table, HashSet<LuaTable> walked = null)
        {
            if (table == null)
                return;
            if (walked == null)
                walked = new HashSet<LuaTable>();
            if (walked.Contains(table))
                return;
            walked.Add(table);
            ExtractTable(table.MetaTable);
            foreach (var kv in table.Dict)
            {
                if (kv.Value is LuaTable table2)
                    ExtractTable(table2, walked);
                if (kv.Value is LuaString str)
                {
                    if (!Found.Contains(str.Text.Trim()))
                    {
                        System.IO.File.AppendAllText("text.txt", str.Text + "\r\n");
                        Found.Add(str.Text.Trim());
                    }
                }
            }
            foreach (var v in table.List)
            {
                if (v is LuaTable table2)
                    ExtractTable(table2, walked);
                if (v is LuaString str)
                {
                    if (!Found.Contains(str.Text.Trim()))
                    {
                        System.IO.File.AppendAllText("text.txt", str.Text + "\r\n");
                        Found.Add(str.Text.Trim());
                    }
                }
            }
        }

        void Extract(string txt)
        {
            if (txt == null)
                return;
            if (!Found.Contains(txt.Trim()))
            {
                System.IO.File.AppendAllText("text.txt", txt + "\r\n");
                Found.Add(txt.Trim());
            }
        }

        void Extract(StringField field)
        {
            if (field == null)
                return;
            if (field.text != null)
                Extract(field.text);
            if (field.value != null)
                Extract(field.value);
        }
        void ExtractQuest(Quest quest, HashSet<Quest> walked = null)
        {
            if (quest == null)
                return;
            if (walked == null)
                walked = new HashSet<Quest>();
            if (walked.Contains(quest))
                return;
            walked.Add(quest);

            for (var i = 0; i < quest.counterList.Count; i++)
            {
                var counter = quest.counterList[i];
                //Extract(counter.name);
                foreach (var message in counter.messageEventList)
                {
                    Extract(message.message);
                    Extract(message.parameter);
                }
            }

            if (quest.currentSpeaker != null)
                Extract(quest.currentSpeaker.displayName);
            Extract(quest.greeter);
            Extract(quest.group);
            if (quest.labels != null)
                foreach (var label in quest.labels)
                    Extract(label);
            //Extract(quest.name);
            Extract(quest.nodeList);
            Extract(quest.title);
            if (quest.speakers != null)
                foreach (var speaker in quest.speakers)
                    Extract(speaker);

            if (quest.offerConditionsUnmetContentList != null)
                foreach (var content in quest.offerConditionsUnmetContentList)
                    Extract(content);
            if (quest.offerContentList != null)
                foreach (var content in quest.offerContentList)
                    Extract(content);

            Extract(quest.startNode);

            if (quest.tagDictionary != null && quest.tagDictionary.dict != null)
            {
                foreach (var kv in quest.tagDictionary.dict)
                    Extract(kv.Value);
            }
            if (quest.stateInfoList != null)
            {
                foreach (var state in quest.stateInfoList)
                {
                    if (state.categorizedContentList != null)
                    {
                        foreach (var action in state.categorizedContentList)
                        {
                            if (action.contentList != null)
                            {
                                foreach (var content in action.contentList)
                                {
                                    Extract(content);
                                }
                            }
                        }
                    }
                }
            }
        }

        void Extract(List<QuestNode> nodes)
        {
            if (nodes == null)
                return;
            foreach (var node in nodes)
            {
                Extract(node);
            }
        }
        void Extract(QuestNode node)
        {
            Extract(node.childList);
            if (node.tagDictionary != null && node.tagDictionary.dict != null)
            {
                foreach (var kv in node.tagDictionary.dict)
                    Extract(kv.Value);
            }
            Extract(node.speaker);
            if (node.stateInfoList != null)
            {
                foreach (var state in node.stateInfoList)
                {
                    if (state.categorizedContentList != null)
                    {
                        foreach (var action in state.categorizedContentList)
                        {
                            if (action.contentList != null)
                            {
                                foreach (var content in action.contentList)
                                {
                                    Extract(content);
                                }
                            }
                        }
                    }
                }
            }
        }

        void Extract(QuestContent content)
        {
            if (content == null)
                return;
            Extract(content.originalText);
            Extract(content.runtimeText);
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.T))
            {
                if (!Extracted)
                {
                    Extracted = true;
                    var tables = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetAllTables().WaitForCompletion();
                    foreach (var table in tables)
                    {
                        if (table.LocaleIdentifier.Code.StartsWith("en"))
                        {
                            foreach (var val in table)
                            //for (var i = 0; i < table.Count; i++)
                            {
                                if (!Found.Contains(val.Value.Value.Trim()))
                                {
                                    
                                    System.IO.File.AppendAllText("text.txt", val.Value.Value + "\r\n");
                                    Found.Add(val.Value.Value.Trim());
                                }
                            }
                        }
                    }
                    System.IO.File.AppendAllText("log.txt", "Extracting...\r\n");
                    if (DialogueManager.MasterDatabase != null)
                    {
                        if (DialogueManager.MasterDatabase.conversations == null)
                        {
                            System.IO.File.AppendAllText("log.txt", "Conversations is null!\r\n");
                            return;
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.conversations)
                        {
                            ExtractFields(kv.fields);
                            if (kv.dialogueEntries != null)
                                foreach (var d in kv.dialogueEntries)
                                    ExtractFields(d.fields);
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.keywords)
                        {
                            ExtractFields(kv.fields);
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.items)
                        {
                            ExtractFields(kv.fields);
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.locations)
                        {
                            ExtractFields(kv.fields);
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.actors)
                        {
                            ExtractFields(kv.fields);
                        }
                        foreach (var kv in DialogueManager.MasterDatabase.variables)
                        {
                            ExtractFields(kv.fields);
                        }
                    }
                    else
                    {
                        System.IO.File.AppendAllText("log.txt", "DialogueManager.MasterDatabase is null\r\n");
                    }
                //ExtractTable(Lua.Environment);
                    var properties = typeof(QuestMachine).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var prop in properties)
                    {
                        if (prop.Name == "questAssets")
                        {
                            var vals = (Dictionary<string, Quest>)prop.GetValue(null);
                            if (vals == null)
                                System.IO.File.AppendAllText("log.txt", "Quests is null :(\r\n");
                            else
                            {
                                System.IO.File.AppendAllText("log.txt", "Quests: " + vals.Count + "\r\n");
                                foreach (var quest in vals)
                                {
                                    ExtractQuest(quest.Value);
                                }
                            }
                        }
                    }
                    /*
                    var vals = (Dictionary<string, Quest>) questAssets.GetValue(null);
                    System.IO.File.AppendAllText("log.txt", "Quests: " + vals.Count + "\r\n");*/
                    /*if (Lua.Environment.GetValue("Conversation") is LuaTable conversationTable)
                    {
                        ExtractTable(conversationTable);
                    }
                    if (Lua.Environment.GetValue("Actor") is LuaTable actorTable)
                    {
                        ExtractTable(actorTable);
                    }
                    if (Lua.Environment.GetValue("Item") is LuaTable itemTable)
                    {
                        ExtractTable(itemTable);
                    }
                    if (Lua.Environment.GetValue("Location") is LuaTable locationTable)
                    {
                        ExtractTable(locationTable);
                    }
                    if (Lua.Environment.GetValue("Keyword") is LuaTable keywordTable)
                    {
                        ExtractTable(keywordTable);
                    }*/
                }
            }
            else Extracted = false;
        }
    }
}
