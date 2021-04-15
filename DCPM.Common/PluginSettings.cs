using System;
using System.Collections.Generic;
using System.IO;
using DCPM.PluginBase;
using UnityEngine;

namespace DCPM.Common
{
	public class PluginSettings : IPluginInfo
	{
        public string Name => "Plugin Settings";
        public string Author => "Standalone";
        public string Version => "1.1";
        public string Desc => "Simple settings class, not actually a plugin";

        public static PluginSettings Instance
		{
			get
			{
				if (instance == null)
					instance = new PluginSettings();

				return instance;
			}
		}

		static PluginSettings instance;

		readonly Dictionary<string, string> settingsDictionary;
		readonly string settingsLocation;
		readonly string settingsFile;

		private PluginSettings()
		{
			settingsDictionary = new Dictionary<string, string>();
			settingsLocation = GlobalVars.RootFolder + "\\dcpm-settings\\";
			settingsFile = "settings.txt";

			LoadSettings();

			PluginConsole.WriteLine("Version " + Version + " Initialized", this);
		}

		public bool SetKeyCode(string settingName, KeyCode keyCode)
		{
			bool result = true;
			if (settingsDictionary.ContainsValue(keyCode.ToString()))
			{
				result = false;
				PluginConsole.WriteLine("KeyCode '" + keyCode.ToString() + "' is already bound", this);
			}
			else
			{
				if (settingsDictionary.ContainsKey(settingName))
				{
					settingsDictionary[settingName] = keyCode.ToString();
				}
				else
				{
					settingsDictionary.Add(settingName, keyCode.ToString());
				}

				SaveSettings();
			}

			return result;
		}

		public KeyCode GetKeyCode(string settingName, KeyCode defaultKeyCode)
		{
			KeyCode result = defaultKeyCode;
			if (settingsDictionary.ContainsKey(settingName))
			{
				try
				{
					result = (KeyCode)Enum.Parse(typeof(KeyCode), settingsDictionary[settingName]);
				}
				catch
				{
					PluginConsole.WriteLine(string.Concat(new string[]
					{
						"Error converting '",
						settingName,
						"' = '",
						settingsDictionary[settingName],
						"' to a UnityEngine.KeyCode"
					}), this);
				}
			}
			else
			{
				SetKeyCode(settingName, defaultKeyCode);
			}

			return result;
		}

		public void SetSetting(string settingName, object settingValue)
		{
			if (settingsDictionary.ContainsKey(settingName))
			{
				settingsDictionary[settingName] = settingValue.ToString();
			}
			else
			{
				settingsDictionary.Add(settingName, settingValue.ToString());
			}

			SaveSettings();
		}

		public string GetSetting(string settingName, object defaultValue)
		{
			string result = defaultValue.ToString();
			if (settingsDictionary.ContainsKey(settingName))
			{
				result = settingsDictionary[settingName];
			}
			else
			{
				if (defaultValue != null)
				{
					SetSetting(settingName, defaultValue);
				}
			}

			return result;
		}

		void SaveSettings()
		{
			if (!Directory.Exists(settingsLocation))
				Directory.CreateDirectory(settingsLocation);

			try
			{
				using (StreamWriter streamWriter = new StreamWriter(new FileStream(settingsLocation + settingsFile, FileMode.Create, FileAccess.Write, FileShare.None)))
				{
					foreach (KeyValuePair<string, string> keyValuePair in settingsDictionary)
					{
						streamWriter.WriteLine("{0} = {1}", keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
			catch (Exception ex)
			{
				PluginConsole.WriteLine(ex.ToString(), this);
			}

			PluginConsole.WriteLine("Settings saved to file", this);
		}

		void LoadSettings()
		{
			if (!Directory.Exists(settingsLocation))
				Directory.CreateDirectory(settingsLocation);

			if (File.Exists(settingsLocation + settingsFile))
			{
				PluginConsole.WriteLine("Loading Dictionary from file", this);

				using (StreamReader streamReader = new StreamReader(new FileStream(settingsLocation + settingsFile, FileMode.OpenOrCreate, FileAccess.Read)))
				{
					while (streamReader.Peek() >= 0)
					{
						string text = streamReader.ReadLine();
						string[] array = text.Split(new string[] { " = " }, StringSplitOptions.None);

						if (array.Length == 2)
						{
							settingsDictionary.Add(array[0], array[1]);

							PluginConsole.WriteLine(string.Concat(new string[]
							{
								"Added setting '",
								array[0],
								"' = '",
								array[1],
								"' to the settings dictionary"
							}), this);
						}
					}
				}
				PluginConsole.WriteLine("Settings Dictionary loaded from file", this);
			}
			else
			{
				PluginConsole.WriteLine("No settings file found, creating an empty one", this);
			}
		}
	}
}
