using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DCPM.Common;
using DCPM.PluginBase;

namespace DCPM
{
	internal class AssemblyManager : IPluginInfo
	{
        public string Name => "Assembly Manager";
        public string Author => "Standalone";
        public string Version => "1.0";
        public string Desc => "Simple assembly loader, not actually a plugin";

        public static AssemblyManager Instance
		{
			get
			{
				if (instance == null)
					instance = new AssemblyManager();

				return instance;
			}
		}

		static AssemblyManager instance;

		private AssemblyManager() { }

		public IEnumerable<Type> GetTypesFromAssemblies<T>(IEnumerable<Assembly> assemblies)
		{
			List<Type> list = new List<Type>();
			foreach (Assembly assembly in assemblies)
			{
				IEnumerable<Type> types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsAssignableFrom(typeof(T)) || type.IsSubclassOf(typeof(T)))
					{
						PluginConsole.WriteLine(type.Name + " is a valid type of " + typeof(T).Name + ", adding to 'returnVal'", this);
						list.Add(type);
					}
				}
			}

			return list;
		}

		public IEnumerable<Assembly> LoadAssemblies(string path)
		{
			AssemblyName assemblyName = null;
			List<Assembly> list = new List<Assembly>();

			foreach (string assemblyFile in Directory.GetFiles(path, "*.dll"))
			{
				try
				{
					assemblyName = AssemblyName.GetAssemblyName(assemblyFile);

					Assembly assembly = Assembly.Load(assemblyName);
					
					PluginConsole.WriteLine("Assembly loaded: " + assembly.ToString(), this);
					list.Add(assembly);
				}
				catch (Exception ex)
				{
					PluginConsole.WriteLine("Error loading " + assemblyName.FullName, this);
					PluginConsole.WriteLine(ex.ToString(), this);
				}
			}

			return list;
		}
	}
}
