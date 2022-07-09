using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurvivorEnemyAndSkillCustomizer.ROStuff;
using System.Linq;
using R2API.Utils;

namespace SurvivorEnemyAndSkillCustomizer
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SurvivorEnemyAndSkillCustomizer : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "GiGaGon";
        public const string PluginName = "SurvivorEnemyAndSkillCustomizer";
        public const string PluginVersion = "1.0.0";

        internal class ModConfig
        {
            public static ConfigEntry<KeyboardShortcut> reloadKeyBind;
            public static ConfigEntry<bool> generateConfigs;

            public static void InitConfig(ConfigFile config)
            {
                reloadKeyBind = config.Bind("_General", "Reload Keybind", new KeyboardShortcut(KeyCode.F8), "Keybind to press to reload the mod's configs.");
                generateConfigs = config.Bind("_General", "Generate Configs", true, "If disabled, new configs will not be generated. Existing configs will still function normally. Can be used to speed up load times durring testing/playing.");
            }
        }
        private void Awake()
        {
            ModConfig.InitConfig(Config);
            On.RoR2.RoR2Application.OnLoad += AfterLoad;
        }
        private void Update()
        {
            if (ModConfig.reloadKeyBind.Value.IsDown())
            {
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Reloading Config");
                ModConfig.InitConfig(Config);
                MakeChanges();
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Reloading Finished");
            }
        }
        private IEnumerator AfterLoad(On.RoR2.RoR2Application.orig_OnLoad orig, RoR2Application self)
        {
            yield return orig(self);
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Inital Load");
            MakeChanges();
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Loading Finished");
        }


        private void MakeChanges()
        {
            if (ModConfig.generateConfigs.Value)
            {
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Generating Configs");
                GenerateConfigs();
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Configs Generated");
            }
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Making Changes");
            ChangeValues(BodyCatalog.allBodyPrefabBodyBodyComponents);
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Changes Made");
            if (Run.instance)
            {
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Making Mid-Run Changes");
                ChangeValues(CharacterMaster.instancesList.Select(x => x.GetBody()));
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Mid-Run Changes Made");
            }
        }

        public void GenerateConfigs()
        {
            foreach (CharacterBody character in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                string name = Language.english.GetLocalizedStringByToken(character.baseNameToken);

                ConfigEntry<bool> isEnabled = Config.Bind(name, $"{name} Enable", false, $"If true, {name}'s configs will be generated/values will be changed.");
                if (isEnabled.Value)
                {
                    foreach (var val in SurvivorModifyableValues)
                        Config.Bind(name, $"{name}_{val.Item1}", "Unchanged", $"Type: {val.Item2}, Default: {val.Item3(character)}");
                    foreach (SkillDef skill in character.GetComponents<GenericSkill>().SelectMany(x => x.skillFamily.variants).Select(x => x.skillDef))
                    {
                        string skName = Language.english.GetLocalizedStringByToken(skill.skillNameToken);

                        ConfigEntry<bool> skEnabled = Config.Bind($"{name}_{skName}", $"{name}_{skName} Enable", false, $"If true, {name}'s skill {skName}'s configs will be generated/values will be changed.");
                        if (skEnabled.Value)
                        {
                            foreach (var val in SkillModifyableValues)
                                Config.Bind($"{name}_{skName}", $"{name}_{skName}_{val.Item1}", "Unchanged", $"Type: {val.Item2}, Default: {val.Item3(skill)}, Description: {val.Item5}");
                        }
                    }
                }
            }
        }

        public void ChangeValues(IEnumerable<CharacterBody> characterBodies)
        {
            foreach (CharacterBody character in characterBodies)
            {
                string name = Language.english.GetLocalizedStringByToken(character.baseNameToken);
                if (Config.TryGetEntry(name, $"{name} Enable", out ConfigEntry<bool> enable) && enable.Value)
                {
                    foreach (var val in SurvivorModifyableValues)
                        if (Config.TryGetEntry(name, $"{name}_{val.Item1}", out ConfigEntry<string> entry) && entry.Value != "Unchanged")
                        {
                            Debug.Log($"SurvivorEnemyAndSkillCustomizer - Changing {name}_{val.Item1} to {entry.Value}");
                            val.Item4(character, entry.Value);
                        }

                    foreach (SkillDef skill in character.GetComponents<GenericSkill>().SelectMany(x => x.skillFamily.variants).Select(x => x.skillDef))
                    {
                        string skName = Language.english.GetLocalizedStringByToken(skill.skillNameToken);
                        if (Config.TryGetEntry($"{name}_{skName}", $"{name}_{skName} Enable", out ConfigEntry<bool> skEnable) && skEnable.Value)
                        {
                            foreach (var val in SkillModifyableValues)
                            {
                                if (Config.TryGetEntry($"{name}_{skName}", $"{name}_{skName}_{val.Item1}", out ConfigEntry<string> entry) && entry.Value != "Unchanged")
                                {
                                    Debug.Log($"SurvivorEnemyAndSkillCustomizer - Changing {name}_{skName}_{val.Item1} to {entry.Value}");
                                    val.Item4(skill, entry.Value);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
