using System.IO;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace NotKeepersNeeds {
	public class MainPatcher {
		public static void Patch() {
			HarmonyInstance harmony = HarmonyInstance.Create("com.koschk.notkeepersneeds.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			HarmonyInstance.DEBUG = true;
		}
	}

	public class Config {
		private static Options options_ = null;

		public class Options {
			public float DmgMult = 1;
			public float GlobalDmgMult = 1;
			public float RegenMult = 1;
			public float SprintSpeed = 2;
			public float DefaultSpeed = 1;
			public float EnergyDrainMult = 1;
			public float EnergyReplenMult = 1;
			public float EnergyForSprint = 0;
			public float CraftingSpeed = 1;
			public float InteractionSpeed = 1;
			public float TimeMult = 1;
			public float SleepTimeMult = 1;
			public float OrbsMult = 1;
			public bool OrbsHasConst = false;
			public bool OrbsConstAddIfZero = false;
			public bool SprintToggle = false;
			public bool RoundDown = false;
			public int[] OrbsConstant = new int[] { 0, 0, 0 };
			public KeyCode SprintKey = KeyCode.LeftShift;
			//public KeyCode ConfigReloadKey = KeyCode.Semicolon;

			public float SavedHP = 1;
			public float SavedEnergy = 1;
			public bool SprintToggleOn = false;
			public bool _SprintStillPressed = false;

			public int GetOrbCount(int orig, int idx) {
				if (OrbsHasConst && (OrbsConstAddIfZero || orig > 0)) {
					orig += OrbsConstant[idx];
				}
				if (OrbsMult == 1) {
					return orig > 0 ? orig : 0;
				}
				if (orig == 0) {
					return 0;
				}
				float tmp = orig * OrbsMult;
				if (tmp < 0) {
					return 0;
				}
				if (RoundDown) {
					return (int)(tmp - (tmp % 1));
				}
				else {
					return (int)(tmp + (1 - tmp % 1));
				}
			}
		}

		public static Options GetOptions() {
			return GetOptions(false);
		}
		public static Options GetOptions(bool forceReload) {
			if (options_ != null && !forceReload) {
				return options_;
			}
			options_ = new Options();
			try {
				//options_.SavedEnergy = MainGame.me.player.energy;
				//options_.SavedHP = MainGame.me.player.hp;

				string cfgPath = @"./QMods/NotKeepersNeeds/config.txt";
				if (File.Exists(cfgPath)) {
					string[] lines = File.ReadAllLines(cfgPath);
					foreach (string line in lines) {
						if (line.Length < 3 || line[0] == '#') {
							continue;
						}
						string[] pair = line.Split('=');
						if (pair.Length > 1) {
							string key = pair[0];
							if (key == "SprintKey") {
								try {
									KeyCode code = Enum<KeyCode>.Parse(pair[1]);
									options_.SprintKey = code;
								}
								catch { }
							}
							else if (key == "OrbsConstant") {
								string[] ocValues = pair[1].Split(':');
								options_.OrbsHasConst = true;
								int ocVal = 0;
								for (int i = 0; (i < ocValues.Length) && (i < options_.OrbsConstant.Length); i++) {
									if (int.TryParse(ocValues[i], out ocVal)) {
										options_.OrbsConstant[i] = ocVal;
									}
								}
							}
							else if (key == "SprintToggle") {
								if (pair[1] == "1" || pair[1].ToLower() == "true") {
									options_.SprintToggle = true;
								}
							}
							else if (key == "RoundDown") {
								if (pair[1] == "1" || pair[1].ToLower() == "true") {
									options_.RoundDown = true;
								}
							}
							else if (key == "OrbsConstAddIfZero") {
								if (pair[1] == "1" || pair[1].ToLower() == "true") {
									options_.OrbsConstAddIfZero = true;
								}
							}
							else {
								float value = 0;
								if (float.TryParse(pair[1], out value)) {
									switch (key) {
										case "DmgMult":
											if (value < 0)
												value = 0;
											options_.DmgMult = value;
											break;
										case "GlobalDmgMult":
											if (value < 0)
												value = 0;
											options_.GlobalDmgMult = value;
											break;
										case "RegenMult":
											if (value > 0) {
												options_.RegenMult = value;
											}
											break;
										case "SprintSpeed":
											if (value > 0) {
												options_.SprintSpeed = value;
											}
											break;
										case "DefaultSpeed":
											if (value > 0) {
												options_.DefaultSpeed = value;
											}
											break;
										case "TimeMult":
											if (value > 0.0009) {
												options_.TimeMult = value;
											}
											break;
										case "SleepTimeMult":
											if (value > 0.09) {
												options_.SleepTimeMult = value;
											}
											break;
										case "EnergyDrainMult":
											if (value < 0)
												value = 0;
											options_.EnergyDrainMult = value;
											break;
										case "EnergyReplenMult":
											if (value > 0) {
												options_.EnergyReplenMult = value;
											}
											break;
										case "EnergyForSprint":
											options_.EnergyForSprint = value;
											break;
										case "CraftingSpeed":
											if (value > 0) {
												options_.CraftingSpeed = value;
											}
											break;
										case "InteractionSpeed":
											if (value > 0) {
												options_.InteractionSpeed = value;
											}
											break;
										case "OrbsMult":
											if (value < 0)
												value = 0;
											options_.OrbsMult = value;
											break;
									}
								}
							}
						}
					}
				}
			}
			catch (System.Exception ex) {
				File.AppendAllText(@"./QMods/NotKeepersNeeds/log.txt", ex.Message + "\r\n");
			}
			return options_;
		}
	}
}