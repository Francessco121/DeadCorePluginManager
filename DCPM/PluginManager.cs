using System;
using System.Collections.Generic;
using System.IO;
using DCPM.Common;
using DCPM.PluginBase;
using UnityEngine;

namespace DCPM
{
	internal class PluginManager : IPluginInfo
	{
        public string Name => "Plugin Manager";
        public string Author => "Standalone";
        public string Version => "1.1";
        public string Desc => "Provides a simple platform for creating and deploying plugins for DeadCore";

        public static PluginManager Instance
		{
			get
			{
				if (instance == null)
					instance = new PluginManager();

				return instance;
			}
		}

		static PluginManager instance;

		string pluginsLocation;
		List<DeadCorePlugin> loadedPlugins;
		GameObject pluginContainerObject;

		void Cmd_ListPlugins(string[] input)
		{
			int num = 0;
			foreach (IPluginInfo pluginInfo in loadedPlugins)
			{
				object[] array = new object[7];

				int num2 = 0;
				int num3 = num;

				num = num3 + 1;

				array[num2] = num3;

				array[1] = ": ";
				array[2] = pluginInfo.Name;
				array[3] = " version ";
				array[4] = pluginInfo.Version;
				array[5] = " by ";
				array[6] = pluginInfo.Author;

				PluginConsole.WriteLine(string.Concat(array), this);
			}
		}

		void Cmd_PluginInfo(string[] input)
		{
			int num;
			if (input.Length >= 1 && int.TryParse(input[0], out num) && num >= 0 && loadedPlugins.Count > num)
			{
				PluginConsole.WriteLine(string.Concat(new string[]
				{
					loadedPlugins[num].Name,
					" version ",
					loadedPlugins[num].Version,
					" by ",
					loadedPlugins[num].Author
				}), this);

				PluginConsole.WriteLine(loadedPlugins[num].Name + " Description: " + loadedPlugins[num].Desc, this);
			}
			else
			{
				PluginConsole.WriteLine("Invalid argument", this);
			}
		}

		public void Initialize()
		{
			pluginContainerObject = new GameObject();
			UnityEngine.Object.DontDestroyOnLoad(pluginContainerObject);

			loadedPlugins = new List<DeadCorePlugin>();
			loadedPlugins.Add(this.pluginContainerObject.AddComponent<PluginConsole>());
			
			pluginsLocation = GlobalVars.RootFolder + "\\dcpm-plugins\\";

			if (!Directory.Exists(pluginsLocation))
				Directory.CreateDirectory(pluginsLocation);

			PluginConsole.RegisterConsoleCommand("plugins_list", Cmd_ListPlugins, "Lists all automatically loaded plugins", this);
			PluginConsole.RegisterConsoleCommand("plugins_info", Cmd_PluginInfo, "Display more detailed information about a loaded plugin, usage: 'plugins info <plugin id>' get the plugin id by using 'plugins list'", this);

			PluginConsole.WriteLine("Variables Initialized", this);

			LoadAllPlugins();
		}

		public void LoadAllPlugins()
		{
			PluginConsole.WriteLine("Loading Plugins", this);
			
			IEnumerable<Type> typesFromAssemblies = AssemblyManager.Instance.GetTypesFromAssemblies<DeadCorePlugin>(AssemblyManager.Instance.LoadAssemblies(pluginsLocation));
			PluginConsole.WriteLine("Plugin Types loaded from Assemblies", this);
			
			foreach (Type type in typesFromAssemblies)
			{
				try
				{
					PluginConsole.WriteLine("Attempting to attach type " + type.Name, this);
					
					DeadCorePlugin deadCorePlugin = pluginContainerObject.AddComponent(type) as DeadCorePlugin;
					loadedPlugins.Add(deadCorePlugin);

					PluginConsole.WriteLine(deadCorePlugin.Name + " version " + deadCorePlugin.Version + " has been attached to the container", this);
				}
				catch (Exception ex)
				{
					PluginConsole.WriteLine("Error attaching " + type.Name + " to the plugin GameObject container", this);
					PluginConsole.WriteLine(ex.ToString(), this);
				}
			}
			
			PluginConsole.WriteLine("Plugins Loaded", this);
			PluginConsole.WriteLine("Version " + this.Version + " Initialized", this);
			PluginConsole.WriteLine("Use 'listcommands' to list available console commands", this);
		}
	}
}
