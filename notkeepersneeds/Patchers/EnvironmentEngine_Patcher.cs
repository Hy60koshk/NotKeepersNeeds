using System;
using System.IO;
using Harmony;
using UnityEngine;

namespace NotKeepersNeeds {
	[HarmonyPatch(typeof(EnvironmentEngine))]
	[HarmonyPatch("Update")]
	class EnvironmentEngine_Update_Patch {
		[HarmonyPrefix]
		static bool Prefix(EnvironmentEngine __instance) {
			float mult = Config.GetOptions().TimeMult;
			if (!(mult == 1 || MainGame.game_starting || MainGame.paused || !MainGame.game_started || __instance.IsTimeStopped())) {
				float accountableDelta = Time.deltaTime / 225f; // since this._cur_time += deltaTime / 225f

				if (Time.timeScale == 10f) {
					mult *= Config.GetOptions().SleepTimeMult;
				}
				float adjDelta = accountableDelta * mult - accountableDelta;
				EnvironmentEngine.SetTime(__instance.time_of_day.time_of_day + adjDelta);
			}
			return true;
		}
	}
}