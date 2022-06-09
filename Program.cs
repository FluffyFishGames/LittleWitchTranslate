using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LittleWitchTranslate
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ModManagerGUI.ModManager.Start(new Mod(), new ModManagerGUI.Configuration() {
                ApplicationName = "Little Witch Translator",
                GameName = "Little Witch in the Woods",
                FileNames = new string[] { "LWIW", "Little Witch in the Woods" },
                DeveloperName = "SUNNY SIDE UP",
                SteamAppID = "1594940",
                AdditionalMods = new string[][] { }
            });
        }
    }
}
