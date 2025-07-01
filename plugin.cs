using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CustomKill.Commands;
using CustomKill.Config;
using CustomKill.Database;
using CustomKill.Services;
using HarmonyLib;
using System.Reflection;
using VampireCommandFramework;

namespace CustomKill
{
    [BepInPlugin("com.beardmagics.customkill", "CustomKill", "1.1.51")]
    [BepInDependency("gg.deca.VampireCommandFramework", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        private Harmony _harmony;
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public override void Load()
        {
            Instance = this;
            Logger = Log;
            //Add clan user tracking days for .top to recognize and tally clan kills / damage -> output to config file    

            // Register VCF commands
            CommandRegistry.RegisterAll(Assembly.GetExecutingAssembly());

            // Init config systems
            ColorSettings.Init(BepInEx.Paths.ConfigPath);

            PvPStatsService.Init();
            KillfeedSettings.Init(BepInEx.Paths.ConfigPath);

            Logger.LogInfo("CustomKill config initialized.");
            Logger.LogInfo("CustomKill v1.1.51 is loading...");

            _harmony = new Harmony("com.beardmagics.customkill");
            _harmony.PatchAll();

            Logger.LogInfo("CustomKill fully loaded.");
        }
    }
}
