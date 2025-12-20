# SurvivorEnemyAndSkillCustomizer

New setting scale, is comma deliniated triple of floats.

New setting "InterruptPriority", can be Any, Skill, PrioritySkill, Pain, Frozen, Vehicle, Death

I highly recommend using [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager), since it makes you not have to wait 5 years for launching the game.

Feel free to complain to me about my bad code or report bugs at GiGaGon#1754

Made for Contra.

I decided to try and fix AsterAether's mod Character Customizer since it was broken. Unfortunatly, I'm too small brain to understand their code so I just re-wrote it from scratch, and here is the product. 

Mod completly overhauled. Should be a lot more self-explanitory. Also ~~stole~~ borrowed more code from Aster to make my mod work during runs.

You can find AsterAether's [GitHub here](https://github.com/AsterAether) and [Thunderstore here](https://thunderstore.io/package/AsterAether/).

## Changelog:

V 1.3.1

 - Fixed not applying changes on load.

 - Finally replaced the dependancy on R2API with one on just Networking.

V 1.3.0

 - Fixed some null strings causing issues.

 - Added a scale config.

V 1.2.1

 - Fixed some illegal name characters not properly getting removed.

V 1.2.0

- Fixed mod not reloading configs properly.

- Added lunar skills.

- Fixed some skills breaking due to invalid chaaracters in their names.

- Networking compatibility settings implemented.

- Don't ask where 1.1.0 went.

V 1.0.0

- Uploaded.