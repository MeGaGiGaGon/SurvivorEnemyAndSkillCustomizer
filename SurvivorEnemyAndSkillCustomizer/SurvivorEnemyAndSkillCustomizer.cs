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
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class SurvivorEnemyAndSkillCustomizer : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "GiGaGon";
        public const string PluginName = "SurvivorEnemyAndSkillCustomizer";
        public const string PluginVersion = "1.2.0";

        internal class ModConfig
        {
            public static ConfigEntry<KeyboardShortcut> reloadKeyBind;
            public static ConfigEntry<bool> generateConfigs;
            public static ConfigEntry<bool> midRunChanges;

            public static void InitConfig(ConfigFile config)
            {
                reloadKeyBind = config.Bind("_General", "Reload Keybind", new KeyboardShortcut(KeyCode.F8), "Keybind to press to reload the mod's configs.");
                generateConfigs = config.Bind("_General", "Generate Configs", true, "If disabled, new configs will not be generated. Existing configs will still function normally. Can be used to speed up load times durring testing/playing.");
                midRunChanges = config.Bind("_General", "Mid Run Changes", true, "If enabled, the mod will attempt to make the changes mid run.");
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
            Config.Reload();
            if (ModConfig.generateConfigs.Value)
            {
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Generating Configs");
                GenerateConfigs();
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Configs Generated");
            }
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Making Changes");
            ChangeValues(BodyCatalog.allBodyPrefabBodyBodyComponents);
            Debug.Log("SurvivorEnemyAndSkillCustomizer - Changes Made");
            if (Run.instance && ModConfig.midRunChanges.Value)
            {
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Making Mid-Run Changes");
                ChangeValues(CharacterMaster.instancesList.Select(x => x.GetBody()));
                Debug.Log("SurvivorEnemyAndSkillCustomizer - Mid-Run Changes Made");
            }
            Config.Reload();
        }

        public void GenerateConfigs()
        {
            foreach (CharacterBody character in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                string name = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(character.baseNameToken));
                if (name == "") continue;

                ConfigEntry<bool> isEnabled = Config.Bind(name, $"{name} Enable", false, $"If true, {name}'s configs will be generated/values will be changed.");
                if (isEnabled.Value)
                {
                    foreach (var val in SurvivorModifyableValues)
                        Config.Bind(name, $"{name}_{val.Item1}", "Unchanged", $"Type: {val.Item2}, Default: {val.Item3(character)}");
                    foreach (SkillDef skill in character.GetComponents<GenericSkill>().SelectMany(x => x.skillFamily.variants).Select(x => x.skillDef))
                    {
                        string skName = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(skill.skillNameToken));
                        if (skName == "") continue;

                        ConfigEntry<bool> skEnabled = Config.Bind($"{name}_{skName}", $"{name}_{skName} Enable", false, $"If true, {name}'s skill {skName}'s configs will be generated/values will be changed.");
                        if (skEnabled.Value)
                        {
                            foreach (var val in SkillModifyableValues)
                                Config.Bind($"{name}_{skName}", $"{name}_{skName}_{val.Item1}", "Unchanged", $"Type: {val.Item2}, Default: {val.Item3(skill)}, Description: {val.Item5}");
                        }
                    }
                }
            }

            string lunarName = "Lunar Skills";
            ConfigEntry<bool> lunarEnabled = Config.Bind(lunarName, $"{lunarName} Enable", false, $"If true, the lunar skill replacement's configs will be generated/changed.");
            if (lunarEnabled.Value) {
                foreach (SkillDef skill in LunarSkills.Select(x => GetSkillFromToken(x)))
                {
                    string skName = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(skill.skillNameToken));
                    if (skName == "") continue;

                    ConfigEntry<bool> skEnabled = Config.Bind($"{lunarName}_{skName}", $"{lunarName}_{skName} Enable", false, $"If true, {skName}'s configs will be generated/values will be changed.");
                    if (skEnabled.Value)
                    {
                        foreach (var val in SkillModifyableValues)
                            Config.Bind($"{lunarName}_{skName}", $"{lunarName}_{skName}_{val.Item1}", "Unchanged", $"Type: {val.Item2}, Default: {val.Item3(skill)}, Description: {val.Item5}");
                    }
                }
            }
        }

        public void ChangeValues(IEnumerable<CharacterBody> characterBodies)
        {
            foreach (CharacterBody character in characterBodies)
            {
                string name = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(character.baseNameToken));
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
                        string skName = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(skill.skillNameToken));
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

            string lunarName = "Lunar Skills";
            if (Config.TryGetEntry(lunarName, $"{lunarName} Enable", out ConfigEntry<bool> lunarEnable) && lunarEnable.Value)
            {
                foreach (SkillDef skill in LunarSkills.Select(GetSkillFromToken))
                {
                    string skName = RemoveIllegalChars(Language.english.GetLocalizedStringByToken(skill.skillNameToken));
                    if (skName == "") continue;

                    if (Config.TryGetEntry($"{lunarName}_{skName}", $"{lunarName}_{skName} Enable", out ConfigEntry<bool> skEnable) && skEnable.Value)
                    {
                        foreach (var val in SkillModifyableValues)
                        {
                            if (Config.TryGetEntry($"{lunarName}_{skName}", $"{lunarName}_{skName}_{val.Item1}", out ConfigEntry<string> entry) && entry.Value != "Unchanged")
                            {
                                Debug.Log($"SurvivorEnemyAndSkillCustomizer - Changing {lunarName}_{skName}_{val.Item1} to {entry.Value}");
                                val.Item4(skill, entry.Value);
                            }
                        }
                    }
                }
            }
        }

        public string RemoveIllegalChars(string input)
        {
            return input.Replace("\n", "").Replace("\t", "").Replace("\\", "").Replace("\"", "").Replace("\'", "").Replace("[", "").Replace("]", "");
        }

        public SkillDef GetSkillFromToken(string input)
        {
            return SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName(input));
        }
    }
}
