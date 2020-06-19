using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NKN_Configurator {
	public partial class Form1 : Form {
		private enum Lang {
			EN, RU
		}

		private static Form1 theForm_ = null;
		private static Options options_ = null;
		private static Dictionary<string, CfgComponent> components_ = new Dictionary<string, CfgComponent>();
		private static Dictionary<string, string> selectedLocale_;
		private static string cfgPath_ = "config.txt";

		private static Dictionary<string, string[]> optsOnPage_ = new Dictionary<string, string[]>() {
			{ "System", new string[] { "TimeMult", "SleepTimeMult", "InteractionSpeed", "CraftingSpeed",
				"InflationAmount" , "DullInventoryMusic"} },
			{ "Sprint", new string[] { "EnergyDrainMult" , "EnergyReplenMult", "DefaultSpeed", "SprintSpeed",
				"EnergyForSprint", "SprintKey", "SprintToggle"} },
			{ "HP", new string[] { "RegenMult", "DmgMult", "GlobalDmgMult"
				, "HealthRegen", "HealIfTired", "HealthRegenPerSecond"} },
			{ "Orbs", new string[] { "OrbsMult", "RoundDown", "OrbsConstAddIfZero", "OrbsConstant"} }
		};
		private static short panelIdx_ = 0;
		private static List<CfgPanel> cfgPanels_ = new List<CfgPanel>(3);

		public Form1() {
			InitializeComponent();
			Size = new Size(620, 420);
			theForm_ = this;
			saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
			try {
				if (File.Exists(cfgPath_)) {
					string[] lines = File.ReadAllLines(cfgPath_);
					if (lines.Length > 0) {
						Dictionary<string, string> rawValues = new Dictionary<string, string>();
						foreach (string line in lines) {
							if (line.Length < 3 || line[0] == '#') {
								continue;
							}
							string[] pair = line.Split('=');
							if (pair.Length > 1) {
								rawValues.Add(pair[0], pair[1]);
							}
						}
						if (rawValues.Count > 0) {
							options_ = new Options(rawValues);
						}
					}
				}
			}
			catch {
				MessageBox.Show("An error occured while reading the config file. Options will be set to default.");
			}
			if (options_ == null) {
				options_ = new Options();
			}
		}

		private class CfgPanel : Panel {
			private List<CfgComponent> cfgComponents_ = new List<CfgComponent>(8);
			public List<CfgComponent> CfgComponents => cfgComponents_;
			private Button btnNext_ = null;

			public void Show() {
				BringToFront();
				Visible = true;
			}

			public void Hide() {
				Visible = false;
				theForm_.Controls.Remove(this);
			}

			public CfgPanel(string name, bool isLast) : this(name) {
				btnNext_.Text = selectedLocale_["Finish"];
			}
			public CfgPanel(string name) : base() {
				SuspendLayout();
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
				Location = new Point(12, 12);
				Size = new Size(580, 357);
				Visible = false;
				
				string[] opts = optsOnPage_[name];
				foreach (string opt in opts) {
					if (options_.FloatParams.ContainsKey(opt)) {
						Add(new CfgFloatComponent(opt, options_.FloatParams[opt]));
					}
					else if (options_.BoolParams.ContainsKey(opt)) {
						Add(new CfgBoolComponent(opt, options_.BoolParams[opt]));
					}
					else if (options_.ArrayParams.ContainsKey(opt)) {
						Add(new CfgIntArrayComponent(opt, options_.AdditionalLabelKeysForParams[opt], options_.ArrayParams[opt]));
					}
					else if (options_.OptionsParams.ContainsKey(opt)) {
						Add(new CfgComboboxComponent(opt, false, options_.AvailableOptionsForParams[opt], options_.OptionsParams[opt]));
					}
				}
				Font btnFont = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
				Size btnSize = new Size(128, 42);

				Button btnBack = new Button();
				btnBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
				btnBack.Font = btnFont;
				btnBack.Location = new Point(0, 313);
				btnBack.Name = "btnBack" + name;
				btnBack.Size = btnSize;
				btnBack.Text = selectedLocale_["Back"];
				btnBack.UseVisualStyleBackColor = true;
				btnBack.Click += new EventHandler(btnBack_Click);

				btnNext_ = new Button();
				btnNext_.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
				btnNext_.Font = btnFont;
				btnNext_.Location = new Point(452, 313);
				btnNext_.Name = "btnNext" + name;
				btnNext_.Size = btnSize;
				btnNext_.Text = selectedLocale_["Proceed"];
				btnNext_.UseVisualStyleBackColor = true;
				btnNext_.Click += new EventHandler(btnNext_Click);

				Controls.Add(btnBack);
				Controls.Add(btnNext_);
				theForm_.Controls.Add(this);
				ResumeLayout(false);
				PerformLayout();
			}

			public void Add(CfgComponent value) {
				Controls.Add(value.Label);
				Controls.Add(value.Help);

				int offset = 30 * cfgComponents_.Count;
				value.Label.Location = new Point(6, 11 + offset);
				value.Help.Location = new Point(426, 10 + offset);

				Control input = value.GetInput();
				if (input == null) {
					value.PlaceInput(this);
				}
				else {
					Controls.Add(value.GetInput());
					value.GetInput().Location = new Point(332, 10 + offset);
				}
				cfgComponents_.Add(value);
			}
		}

		private abstract class CfgComponent {
			protected Label label_;
			protected Button help_;
			protected string name_ = "";

			public Label Label => label_;
			public Button Help => help_;

			public abstract string GetValue();
			public abstract Control GetInput();
			public abstract void PlaceInput(CfgPanel panel);

			public string GetCfgString() {
				string value = GetValue();
				return value == null ? "" : name_ + "=" + value + "\r\n";
			}

			public CfgComponent(string name) {
				name_ = name;
				label_ = new Label();
				label_.Anchor = AnchorStyles.None;
				label_.Font = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
				label_.Location = new Point(6, 11);
				label_.Name = "lb" + name;
				label_.Size = new Size(320, 20);
				label_.TextAlign = ContentAlignment.MiddleRight;
				label_.Text = selectedLocale_[name];

				help_ = new Button();
				help_.Anchor = AnchorStyles.None;
				help_.BackgroundImage = Properties.Resources.help;
				help_.BackgroundImageLayout = ImageLayout.Center;
				help_.Cursor = Cursors.Hand;
				help_.FlatAppearance.BorderSize = 0;
				help_.FlatStyle = FlatStyle.Flat;
				help_.Name = "help" + name;
				help_.Size = new Size(36, 24);
				help_.UseVisualStyleBackColor = true;

				help_.Click += new EventHandler(btnHelp_Click);
			}
		}

		private class CfgFloatComponent : CfgComponent {
			protected TextBox textBox_;
			public override Control GetInput() {
				return textBox_;
			}
			public override void PlaceInput(CfgPanel panel) { }

			public CfgFloatComponent(string name) : base(name) {
				textBox_ = new TextBox();
				textBox_.Anchor = AnchorStyles.None;
				textBox_.Font = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
				textBox_.Name = "tb" + name;
				textBox_.Size = new Size(84, 24);
			}

			public CfgFloatComponent(string name, double value) : this(name) {
				textBox_.Text = value.ToString().Replace(',', '.');
			}

			public override string GetValue() {
				string strValue = textBox_.Text;
				if (strValue.Length == 0) {
					return null;
				}
				try {
					double val = Convert.ToDouble(strValue.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					return val.ToString().Replace(',', '.');
				} catch {
					return null;
				}
			}
		}

		private class CfgIntArrayComponent : CfgComponent {
			private NumericUpDown[] numericInputs_;
			private string[] labelKeys_;

			public override Control GetInput() {
				return null;
			}

			public override void PlaceInput(CfgPanel panel) {
				int offset = 30 * panel.CfgComponents.Count;
				for (int i = 0; i < numericInputs_.Length; i++) {
					NumericUpDown numericInput = numericInputs_[i];
					numericInput.Location = new Point(332, 10 + offset);
					panel.Controls.Add(numericInput);
					if (i > 0) {
						string key = labelKeys_[i - 1];
						Label addLabel = new Label();
						addLabel.Anchor = AnchorStyles.None;
						addLabel.Font = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
						addLabel.Location = new Point(6, 11);
						addLabel.Name = "lb" + name_ + key;
						addLabel.Size = new Size(320, 20);
						addLabel.TextAlign = ContentAlignment.MiddleRight;
						addLabel.Text = selectedLocale_[key];
						addLabel.Location = new Point(6, 11 + offset);
						panel.Controls.Add(addLabel);
					}
					offset += 30;
				}
			}

			public CfgIntArrayComponent(string name, string[] additionalLabelKeys, int[] values) : base(name) {
				numericInputs_ = new NumericUpDown[values.Length];
				labelKeys_ = additionalLabelKeys;
				for (int i = 0; i < values.Length; i++) {
					int val = values[i];
					numericInputs_[i] = new NumericUpDown();
					NumericUpDown numericInput = numericInputs_[i];
					numericInput.Anchor = AnchorStyles.None;
					numericInput.Font = new Font("Verdana", 10F);
					numericInput.Location = new Point(332, 158);
					numericInput.Maximum = new decimal(Math.Max(100000, val));
					numericInput.Minimum = new decimal(Math.Min(-100000, val));
					numericInput.Name = "nude" + name + i;
					numericInput.Size = new Size(84, 24);
					numericInput.Value = values[i];
				}
			}

			public override string GetValue() {
				string strval = "";
				foreach (NumericUpDown nude in numericInputs_) {
					int value = Convert.ToInt32(nude.Value);
					strval += value + ":";
				}
				return strval;
			}
		}

		private class CfgBoolComponent : CfgComponent {
			protected ComboBox comboBox_;
			public override Control GetInput() {
				return comboBox_;
			}
			public override void PlaceInput(CfgPanel panel) {}

			public CfgBoolComponent(string name) : base(name) {
				comboBox_ = new ComboBox();
				comboBox_.Font = new Font("Verdana", 10F);
				comboBox_.FormattingEnabled = true;
				comboBox_.Anchor = AnchorStyles.None;
				comboBox_.Name = "cb" + name;
				comboBox_.Size = new Size(84, 24);
				comboBox_.DropDownStyle = ComboBoxStyle.DropDownList;
			}

			public CfgBoolComponent(string name, bool value) : this(name) {
				comboBox_.Items.Clear();
				comboBox_.Items.Add(selectedLocale_["boolDisabled"]);
				comboBox_.Items.Add(selectedLocale_["boolEnabled"]);
				comboBox_.SelectedIndex = value ? 1 : 0;
			}

			public override string GetValue() {
				return comboBox_.SelectedIndex == 1 ? "true" : "false";
			}
		}

		private class CfgComboboxComponent : CfgBoolComponent {
			string[] options_ = null;

			public CfgComboboxComponent(string name, bool isLoc, string[] options, string value) : base(name) {
				comboBox_.Items.Clear();
				options_ = options;
				int idx = 0;
				for (int i = 0; i < options.Length; i++) {
					string opt = options[i];
					if (opt == value) {
						idx = i;
					}
					comboBox_.Items.Add(isLoc ? selectedLocale_[options[i]] : options[i]);
				}
				comboBox_.SelectedIndex = idx;
			}

			public override string GetValue() {
				return options_[comboBox_.SelectedIndex];
			}
		}

		private class Options {

			private Dictionary<string, double> floatParams_ = new Dictionary<string, double>() {
				{ "DmgMult", 1 }
				, { "GlobalDmgMult", 1 }
				, { "RegenMult", 1 }
				, { "HealthRegenPerSecond", 0.5 }
				, { "DefaultSpeed", 1 }
				, { "SprintSpeed", 2 }
				, { "EnergyDrainMult", 1 }
				, { "EnergyReplenMult", 1 }
				, { "EnergyForSprint", 0.01 }
				, { "CraftingSpeed", 1 }
				, { "InteractionSpeed", 1 }
				, { "TimeMult", 1 }
				, { "SleepTimeMult", 1 }
				, { "OrbsMult", 1 }
				, { "InflationAmount", 1 }
			};
			private Dictionary<string, bool> boolParams_ = new Dictionary<string, bool>() {
				{ "OrbsConstAddIfZero", false }
				, { "SprintToggle", false }
				, { "RoundDown", false }
				, { "DullInventoryMusic", false }
				, { "HealthRegen", false }
				, { "HealIfTired", false }
			};
			private Dictionary<string, int[]> arrayParams_ = new Dictionary<string, int[]>() {
				{ "OrbsConstant", new int[]{ 0, 0, 0 } }
			};
			private Dictionary<string, string[]> additionalLabelKeysForParams_ = new Dictionary<string, string[]>() {
				{ "OrbsConstant", new string[] { "OCGreen", "OCBlue" } }
			};
			private Dictionary<string, string> optionsParams_ = new Dictionary<string, string>() {
				{ "SprintKey", "LeftShift" }
			};
			private Dictionary<string, string[]> availableOptionsForParams_ = new Dictionary<string, string[]>() {
				{ "SprintKey", new string[] { "LeftShift", "LeftControl", "LeftAlt", "Space"
					, "RightShift", "RightControl", "RightAlt", "Z", "X", "CapsLock", "Backspace", "ScrollLock", "Pause", "PageUp" } }
			};
			public Dictionary<string, double> FloatParams => floatParams_;
			public Dictionary<string, bool> BoolParams => boolParams_;
			public Dictionary<string, int[]> ArrayParams => arrayParams_;
			public Dictionary<string, string> OptionsParams => optionsParams_;
			public Dictionary<string, string[]> AdditionalLabelKeysForParams => additionalLabelKeysForParams_;
			public Dictionary<string, string[]> AvailableOptionsForParams => availableOptionsForParams_;

			public Options() { }

			public Options(Dictionary<string, string> rawValues) {
				double fvalue = 0;
				List<string> keyslist = floatParams_.Keys.ToList();
				foreach (string key in keyslist) {
					if (rawValues.ContainsKey(key)) {
						if (double.TryParse(rawValues[key], out fvalue)) {
							floatParams_[key] = fvalue;
						}
					}
				}
				keyslist = boolParams_.Keys.ToList();
				foreach (string key in keyslist) {
					if (rawValues.ContainsKey(key)) {
						string bvalue = rawValues[key];
						if (bvalue == "1" || bvalue.ToLower() == "true") {
							boolParams_[key] = true;
						}
					}
				}
				if (rawValues.ContainsKey("SprintKey")) {
					optionsParams_["SprintKey"] = rawValues["SprintKey"];
				}
				if (rawValues.ContainsKey("OrbsConstant")) {
					string[] ocValues = rawValues["OrbsConstant"].Split(':');
					int[] orbsConst = arrayParams_["OrbsConstant"];
					int ocVal = 0;
					for (int i = 0; (i < ocValues.Length) && (i < 3); i++) {
						if (int.TryParse(ocValues[i], out ocVal)) {
							orbsConst[i] = ocVal;
						}
					}
				}
			}
		}

		private static void btnHelp_Click(object sender, EventArgs e) {
			MessageBox.Show(selectedLocale_[((Control)sender).Name]);
		}
		private static void btnNext_Click(object sender, EventArgs e) {
			if (panelIdx_ == cfgPanels_.Count - 1) {
				theForm_.saveFileDialog1.ShowDialog();
			}
			else {
				panelIdx_++;
				cfgPanels_[panelIdx_].Show();
			}
		}
		private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			try {
				StringBuilder result = new StringBuilder();
				foreach (CfgPanel panel in cfgPanels_) {
					foreach (CfgComponent cfgComponent in panel.CfgComponents) {
						result.Append(cfgComponent.GetCfgString());
					}
				}
				File.WriteAllText(theForm_.saveFileDialog1.FileName, result.ToString());
				MessageBox.Show(selectedLocale_["savedSuccesfully"]);
			}
			catch (Exception ex) {
				MessageBox.Show("Could not save the configuration file:\r\n\r\n" + ex.Message);
			}
		}
		private static void btnBack_Click(object sender, EventArgs e) {
			if (panelIdx_ == 0) {
				foreach (CfgPanel panel in cfgPanels_) {
					panel.Hide();
					panel.Dispose();
				}
				cfgPanels_.Clear();
			}
			else {
				cfgPanels_[panelIdx_].Visible = false;
				panelIdx_--;
			}
		}

		private void btnLangRu_Click(object sender, EventArgs e) {
			StartConf(Lang.RU);
		}

		private void btnLangEn_Click(object sender, EventArgs e) {
			StartConf(Lang.EN);
		}

		private void StartConf(Lang lang) {
			selectedLocale_ = locales_[lang];
			cfgPanels_.Add(new CfgPanel("System"));
			cfgPanels_.Add(new CfgPanel("Sprint"));
			cfgPanels_.Add(new CfgPanel("HP"));
			cfgPanels_.Add(new CfgPanel("Orbs", true));
			cfgPanels_[0].Show();
		}
	}
}
