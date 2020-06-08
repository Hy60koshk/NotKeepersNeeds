using System;
using Harmony;

namespace NotKeepersNeeds {
	[HarmonyPatch(typeof(WorldGameObject))]
	[HarmonyPatch("energy", PropertyMethod.Setter)]
	internal class WorldGameObject_EnergySetter_Patcher {

		[HarmonyPrefix]
		public static bool Prefix(WorldGameObject __instance, ref float value) {
			if (!__instance.is_player) {
				return false;
			}
			float oldVal = __instance.energy;
			float delta = oldVal - value;
			if (delta < 0) {
				Config.Options opts = Config.GetOptions();
				delta *= delta > 0 ? opts.EnergyDrainMult : opts.EnergyReplenMult;
				value = oldVal - delta;
			}
			return true;
		}
	}
}