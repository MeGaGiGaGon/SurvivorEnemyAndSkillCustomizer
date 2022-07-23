using System;
using System.Collections.Generic;
using EntityStates;
using BepInEx.Configuration;
using RoR2;
using R2API.Utils;
using RoR2.Skills;
using UnityEngine;
using System.Linq;

namespace SurvivorEnemyAndSkillCustomizer
{
    public class ROStuff
    {
        public static readonly IReadOnlyDictionary<string, InterruptPriority> SToInPr = new Dictionary<string, InterruptPriority>
        {
            { "Any",           InterruptPriority.Any           },
            { "Skill",         InterruptPriority.Skill         },
            { "PrioritySkill", InterruptPriority.PrioritySkill },
            { "Pain",          InterruptPriority.Pain          },
            { "Frozen",        InterruptPriority.Frozen        },
            { "Vehicle",       InterruptPriority.Vehicle       },
            { "Death",         InterruptPriority.Death         }
        };

        public static readonly List<Tuple<string, string, Func<CharacterBody, string>, Action<CharacterBody, string>>> SurvivorModifyableValues = new()
            {
                new( "baseAcceleration",         "float", (character) => character.baseAcceleration.ToString(),         (character, value) => character.baseAcceleration = float.Parse(value)                       ),
                new( "baseArmor",                "float", (character) => character.baseArmor.ToString(),                (character, value) => character.baseArmor = float.Parse(value)                              ),
                new( "levelArmor",               "float", (character) => character.levelArmor.ToString(),               (character, value) => character.levelArmor = float.Parse(value)                             ),
                new( "baseAttackSpeed",          "float", (character) => character.baseAttackSpeed.ToString(),          (character, value) => character.baseAttackSpeed = float.Parse(value)                        ),
                new( "levelAttackSpeed",         "float", (character) => character.levelAttackSpeed.ToString(),         (character, value) => character.levelAttackSpeed = float.Parse(value)                       ),
                new( "baseCrit",                 "float", (character) => character.baseCrit.ToString(),                 (character, value) => character.baseCrit = float.Parse(value)                               ),
                new( "levelCrit",                "float", (character) => character.levelCrit.ToString(),                (character, value) => character.levelCrit = float.Parse(value)                              ),
                new( "baseDamage",               "float", (character) => character.baseDamage.ToString(),               (character, value) => character.baseDamage = float.Parse(value)                             ),
                new( "levelDamage",              "float", (character) => character.levelDamage.ToString(),              (character, value) => character.levelDamage = float.Parse(value)                            ),
                new( "baseJumpCount",              "int", (character) => character.baseJumpCount.ToString(),            (character, value) => character.baseJumpCount = int.Parse(value)                            ),
                new( "baseJumpPower",            "float", (character) => character.baseJumpPower.ToString(),            (character, value) => character.baseJumpPower = float.Parse(value)                          ),
                new( "levelJumpPower",           "float", (character) => character.levelJumpPower.ToString(),           (character, value) => character.levelJumpPower = float.Parse(value)                         ),
                new( "baseMaxHealth",            "float", (character) => character.baseMaxHealth.ToString(),            (character, value) => character.baseMaxHealth = float.Parse(value)                          ),
                new( "levelMaxHealth",           "float", (character) => character.levelMaxHealth.ToString(),           (character, value) => character.levelMaxHealth = float.Parse(value)                         ),
                new( "baseMaxShield",            "float", (character) => character.baseMaxShield.ToString(),            (character, value) => character.baseMaxShield = float.Parse(value)                          ),
                new( "levelMaxShield",           "float", (character) => character.levelMaxShield.ToString(),           (character, value) => character.levelMaxShield = float.Parse(value)                         ),
                new( "baseMoveSpeed",            "float", (character) => character.baseMoveSpeed.ToString(),            (character, value) => character.baseMoveSpeed = float.Parse(value)                          ),
                new( "levelMoveSpeed",           "float", (character) => character.levelMoveSpeed.ToString(),           (character, value) => character.levelMoveSpeed = float.Parse(value)                         ),
                new( "baseRegen",                "float", (character) => character.baseRegen.ToString(),                (character, value) => character.baseRegen = float.Parse(value)                              ),
                new( "levelRegen",               "float", (character) => character.levelRegen.ToString(),               (character, value) => character.levelRegen = float.Parse(value)                             ),
                new( "sprintingSpeedMultiplier", "float", (character) => character.sprintingSpeedMultiplier.ToString(), (character, value) => character.sprintingSpeedMultiplier = float.Parse(value)               ),
                new( "scale",                    "float", (character) => {var a = character.GetComponent<ModelLocator>().modelTransform.localScale.ToString(); return a.Substring(1, a.Length - 2); },     (character, value) => 
                    {
                        var a = value.Split(',').Select(x => float.Parse(x)).ToArray();
                        character.GetComponent<ModelLocator>().modelTransform.localScale = new Vector3(a[0], a[1], a[2]);
                    })
        };

        public static readonly List<Tuple<string, string, Func<SkillDef, string>, Action<SkillDef, string>, string>> SkillModifyableValues = new()
        {
            new( "InterruptPriority",            "interruptPriority", (skill) => skill.interruptPriority.ToString(),            (skill, value) => skill.interruptPriority = SToInPr[value],               "Priority of the skill. Explained on webpage."                                                     ),
            new( "baseRechargeInterval",         "float",             (skill) => skill.baseRechargeInterval.ToString(),         (skill, value) => skill.baseRechargeInterval = float.Parse(value),        "How long it takes for this skill to recharge after being used."                                   ),
            new( "baseMaxStock",                 "int",               (skill) => skill.baseMaxStock.ToString(),                 (skill, value) => skill.baseMaxStock = int.Parse(value),                  "Maximum number of charges this skill can carry."                                                  ),
            new( "rechargeStock",                "int",               (skill) => skill.rechargeStock.ToString(),                (skill, value) => skill.rechargeStock = int.Parse(value),                 "How much stock to restore on a recharge."                                                         ),
            new( "requiredStock",                "int",               (skill) => skill.requiredStock.ToString(),                (skill, value) => skill.requiredStock = int.Parse(value),                 "How much stock is required to activate this skill."                                               ),
            new( "stockToConsume",               "int",               (skill) => skill.stockToConsume.ToString(),               (skill, value) => skill.stockToConsume = int.Parse(value),                "How much stock to deduct when the skill is activated."                                            ),
            new( "resetCooldownTimerOnUse",      "bool",              (skill) => skill.resetCooldownTimerOnUse.ToString(),      (skill, value) => skill.resetCooldownTimerOnUse = bool.Parse(value),      "Whether or not it resets any progress on cooldowns."                                              ),
            new( "fullRestockOnAssign",          "bool",              (skill) => skill.fullRestockOnAssign.ToString(),          (skill, value) => skill.fullRestockOnAssign = bool.Parse(value),          "Whether or not to fully restock this skill when it's assigned."                                   ),
            new( "dontAllowPastMaxStocks",       "bool",              (skill) => skill.dontAllowPastMaxStocks.ToString(),       (skill, value) => skill.dontAllowPastMaxStocks = bool.Parse(value),       "Whether or not this skill can hold past it's maximum stock."                                      ),
            new( "beginSkillCooldownOnSkillEnd", "bool",              (skill) => skill.beginSkillCooldownOnSkillEnd.ToString(), (skill, value) => skill.beginSkillCooldownOnSkillEnd = bool.Parse(value), "Whether or not the cooldown waits until it leaves the set state"                                  ),
            new( "cancelSprintingOnActivation",  "bool",              (skill) => skill.cancelSprintingOnActivation.ToString(),  (skill, value) => skill.cancelSprintingOnActivation = bool.Parse(value),  "Whether or not activating the skill forces off sprinting."                                        ),
            new( "forceSprintDuringState",       "bool",              (skill) => skill.forceSprintDuringState.ToString(),       (skill, value) => skill.forceSprintDuringState = bool.Parse(value),       "Whether or not this skill is considered 'mobility'. Currently just forces sprint."                ),
            new( "canceledFromSprinting",        "bool",              (skill) => skill.canceledFromSprinting.ToString(),        (skill, value) => skill.canceledFromSprinting = bool.Parse(value),        "Whether or not sprinting sets the skill's state to be reset."                                     ),
            new( "isCombatSkill",                "bool",              (skill) => skill.isCombatSkill.ToString(),                (skill, value) => skill.isCombatSkill = bool.Parse(value),                "Whether or not this is considered a combat skill. If true, will stop items like Red Whip on use." ),
            new( "mustKeyPress",                 "bool",              (skill) => skill.mustKeyPress.ToString(),                 (skill, value) => skill.mustKeyPress = bool.Parse(value),                 "The skill can't be activated if the key is held."                                                 )
        };     
        
        public static readonly List<string> LunarSkills = new()
        {
            "LunarPrimaryReplacement",
            "LunarSecondaryReplacement",
            "LunarUtilityReplacement",
            "LunarSpecialReplacement"
        };
    }
}
