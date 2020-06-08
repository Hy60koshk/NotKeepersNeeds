using System;
using Harmony;

namespace NotKeepersNeeds {
	/*[HarmonyPatch(typeof(PlayerComponent))]
	[HarmonyPatch("Update")]
	internal class PlayerComponent_Update_Patcher {

		[HarmonyPrefix]
		public static bool Prefix(PlayerComponent __instance) {
			Config.Options opts = Config.GetOptions();
			WorldGameObject player = MainGame.me.player;

			if (player.hp != opts.SavedHP) {
				float mult = player.hp > opts.SavedHP ? opts.RegenMult : opts.DmgMult;
				float newhp = opts.SavedHP - (opts.SavedHP - player.hp) * mult;
				player.hp = newhp;
				opts.SavedHP = newhp;
			}
			/*if (player.energy != opts.SavedEnergy) {
				float mult = player.energy > opts.SavedEnergy ? opts.EnergyReplenMult : opts.EnergyDrainMult;
				float newenergy = opts.SavedEnergy - (opts.SavedEnergy - player.energy) * mult;
				player.energy = newenergy;
				opts.SavedEnergy = newenergy;
			}
			return true;
		}
	}*/
	[HarmonyPatch(typeof(PlayerComponent))]
	[HarmonyPatch("CheckEnergy")]
	internal class PlayerComponent_CheckEnergy_Patcher {

		[HarmonyPrefix]
		public static bool Prefix(PlayerComponent __instance, ref float need_energy) {
			if (need_energy == 0) {
				return false;
			}
			Config.Options opts = Config.GetOptions();
			float mult = need_energy > 0 ? opts.EnergyDrainMult : opts.EnergyReplenMult;
			need_energy *= mult;
			return true;
		}
	}
	[HarmonyPatch(typeof(PlayerComponent))]
	[HarmonyPatch("TrySpendEnergy")]
	internal class PlayerComponent_TrySpendEnergy_Patcher {

		[HarmonyPrefix]
		public static bool Prefix(PlayerComponent __instance, ref float need_energy) {
			if (need_energy == 0) {
				return false;
			}
			Config.Options opts = Config.GetOptions();
			float mult = need_energy > 0 ? opts.EnergyDrainMult : opts.EnergyReplenMult;
			need_energy *= mult;
			return true;
		}
	}
}