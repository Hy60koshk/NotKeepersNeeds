using System;
using Harmony;
using UnityEngine;

namespace NotKeepersNeeds {
	[HarmonyPatch(typeof(MovementComponent))]
	[HarmonyPatch("UpdateMovement")]
	internal class MovementComponent_UpdateMovement_Patch {
		[HarmonyPrefix]
		public static bool Prefix(MovementComponent __instance, Vector2 dir, ref float delta_time) {
			if (!__instance.wgo.is_player || __instance.player_controlled_by_script || __instance.wgo.is_dead) {
				return false;
			}
			Config.Options opts = Config.GetOptions();
			bool isSprintPressed = Input.GetKey(opts.SprintKey);
			if (opts.SprintToggle) {
				if (isSprintPressed && !opts._SprintStillPressed) {
					isSprintPressed = opts.SprintToggleOn = !opts.SprintToggleOn;
					opts._SprintStillPressed = true;
				}
				else {
					if (!isSprintPressed) {
						opts._SprintStillPressed = false;
					}
					isSprintPressed = opts.SprintToggleOn;
				}
			}
			if (dir.normalized.magnitude.EqualsTo(0.0f, 1E-05f) && __instance.delta_vec.magnitude.EqualsTo(0.0f, 1E-05f)) {
				return false;
			}

			float speed = __instance.wgo.data.GetParam("speed", 0.0f);
			if (speed > 0) {
				speed = 3.3f + __instance.wgo.data.GetParam("speed_buff", 0.0f);
				float energydt = delta_time * opts.EnergyForSprint;
				if (isSprintPressed && (MainGame.me.player.energy >= energydt)) {
					__instance.SetSpeed(speed * opts.SprintSpeed);
					if (energydt > 0) {
						MainGame.me.player.energy -= energydt;
					}
				}
				else if (opts.DefaultSpeed != 1) {
					__instance.SetSpeed(speed * opts.DefaultSpeed);
				}
			}
			return true;
		}
		/*[HarmonyPostfix]
		public static void Postfix(MovementComponent __instance) {
			if (__instance.wgo.is_player) {
				Config.Options opts = Config.GetOptions();
				bool isSprintPressed = Input.GetKey(opts.SprintKey);
				if (opts.SprintToggle) {
					if (isSprintPressed && !opts._SprintStillPressed) {
						isSprintPressed = opts.SprintToggleOn = !opts.SprintToggleOn;
						opts._SprintStillPressed = true;
					}
					else {
						if (!isSprintPressed) {
							opts._SprintStillPressed = false;
						}
						isSprintPressed = opts.SprintToggleOn;
					}
				}

				float energydt = Time.deltaTime * opts.EnergyForSprint;
				if (isSprintPressed && (MainGame.me.player.energy >= energydt)) {
					__instance.SetSpeed(opts.SprintSpeed);
					if (opts.EnergyForSprint > 0) {
						MainGame.me.player.energy -= energydt;
					}
				}
				else {
					__instance.SetSpeed(opts.DefaultSpeed);
				}
			}
		}*/
	}
}