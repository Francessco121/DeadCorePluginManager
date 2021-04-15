using System;
using System.Collections.Generic;
using System.IO;
using DCPM.PluginBase;
using UnityEngine;

namespace DCPM.Common
{
	public class PluginConsole : DeadCorePlugin
	{
		public delegate void ConsoleCommandCallback(string[] input);

		class ConsoleCommand
		{
			public IPluginInfo Owner;
			public ConsoleCommandCallback Callback;
			public string Description;

			public ConsoleCommand(ConsoleCommandCallback callback, string description, IPluginInfo owner)
			{
				Callback = callback;
				Description = description;
				Owner = owner;
			}
		}

		public override string Name => "Plugin Console";
        public override string Author => "Standalone";
        public override string Version => "1.2";
        public override string Desc => "Provides access to a simple in game console (as well as logging to file) and allows plugins to register console commands";

        public static PluginConsole Instance => instance;

		static PluginConsole instance;
		static string logsLocation;
		static bool logToFile = true;
		static Dictionary<string, ConsoleCommand> registeredConsoleCommands;

		readonly char[] trimParams = new char[] { ' ' };

		static Queue<string> textQueue;

		string consoleInput;

		Vector2 scrollPos;
		Rect consoleRect;
		Rect scrollableRect;
		Rect textInputRect;
		Rect submitButtonRect;
		KeyCode consoleKeyBind;
		static bool drawConsole;
		GUIStyle textLineStyle;

		void Cmd_Clear(string[] input)
		{
			textQueue.Clear();
		}

		void Cmd_List(string[] input)
		{
			foreach (KeyValuePair<string, ConsoleCommand> keyValuePair in registeredConsoleCommands)
			{
				WriteLine(string.Concat(new string[]
				{
					"[",
					keyValuePair.Value.Owner.Name,
					"] '",
					keyValuePair.Key,
					"': ",
					keyValuePair.Value.Description
				}), this);
			}
		}

		void Awake()
		{
			if (instance != null)
			{
				Destroy(this);
			}
			else
			{
				instance = this;

				registeredConsoleCommands = new Dictionary<string, ConsoleCommand>();
				logsLocation = GlobalVars.RootFolder + "\\dcpm-logs\\";
				textQueue = new Queue<string>();
				consoleInput = "";
				drawConsole = false;
				consoleKeyBind = PluginSettings.Instance.GetKeyCode("TogglePluginConsole", KeyCode.BackQuote);
				
				consoleRect = new Rect(10f, 10f, 1000f, 600f);
				scrollableRect = new Rect(consoleRect.left + 5f, consoleRect.top, consoleRect.width - 5f, consoleRect.height - 30f);
				textInputRect = new Rect(consoleRect.left + 5f, consoleRect.top + consoleRect.height - 25f, consoleRect.width - 100f, 20f);
				submitButtonRect = new Rect(consoleRect.left + consoleRect.width - 95f, consoleRect.top + consoleRect.height - 25f, 85f, 20f);
				
				RegisterConsoleCommand("clear", Cmd_Clear, "Clear the console history", this);
				RegisterConsoleCommand("listcommands", Cmd_List, "Display a list of all registered console commands", this);
				
				WriteLine("Version " + Version + " Initialized", this);
			}
		}

		void Update()
		{
			if (Input.GetKeyDown(consoleKeyBind))
			{
				if (drawConsole)
				{
					drawConsole = false;
					DisableCursor();
				}
				else
				{
					drawConsole = true;
					EnableCursor();
				}
			}
		}

		void OnGUI()
		{
			if (drawConsole)
			{
				if (textLineStyle == null)
				{
					textLineStyle = new GUIStyle(GUI.skin.label);
					textLineStyle.margin = new RectOffset(0, 0, 0, 0);
					textLineStyle.padding = new RectOffset(0, 0, 0, 0);
				}

				GUI.Box(consoleRect, "");

				GUILayout.BeginArea(scrollableRect);

				scrollPos = GUILayout.BeginScrollView(scrollPos, new GUILayoutOption[]
				{
					GUILayout.Width(scrollableRect.width),
					GUILayout.Height(scrollableRect.height)
				});

				foreach (string text in textQueue)
				{
					GUILayout.Label(text, textLineStyle);
				}

				GUILayout.EndScrollView();
				GUILayout.EndArea();

				consoleInput = GUI.TextField(textInputRect, consoleInput);

				if ((Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) ||
					GUI.Button(submitButtonRect, "Submit"))
				{
					Submit(consoleInput);
					consoleInput = "";
				}
			}
		}

		void Submit(string input)
		{
			if (input.TrimStart(trimParams).Length != 0)
			{
				WriteLine(input, null);

				foreach (KeyValuePair<string, ConsoleCommand> keyValuePair in registeredConsoleCommands)
				{
					if (input.StartsWith(keyValuePair.Key))
					{
						try
						{
							string[] array = input.Replace(keyValuePair.Key, "").Trim(trimParams).Split(' ');

							if (array[0] == "")
							{
								array = new string[0];
							}

							keyValuePair.Value.Callback(array);
						}
						catch (Exception ex)
						{
							WriteLine(
								"Error: Console command '" + 
								keyValuePair.Key + 
								"' caused an exception and has been removed from the registered console commands to prevent further errors", 
								this);

							WriteLine(ex.ToString(), this);

							registeredConsoleCommands.Remove(keyValuePair.Key);
						}
					}
				}
			}
		}

		void DisableCursor()
		{
			if (GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
			{
				Screen.showCursor = false;
				Screen.lockCursor = true;

				MouseLook[] array = FindObjectsOfType(typeof(MouseLook)) as MouseLook[];

				foreach (MouseLook mouseLook in array)
				{
					mouseLook.enabled = true;
				}
			}
		}

		void EnableCursor()
		{
			if (GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
			{
				Screen.showCursor = true;
				Screen.lockCursor = false;

				MouseLook[] array = FindObjectsOfType(typeof(MouseLook)) as MouseLook[];

				foreach (MouseLook mouseLook in array)
				{
					mouseLook.enabled = false;
				}
			}
		}

		public static void RegisterConsoleCommand(string command, ConsoleCommandCallback callback, string description, IPluginInfo owner)
		{
			registeredConsoleCommands.Add(command, new ConsoleCommand(callback, description, owner));
		}

		public static void HideConsole()
		{
			drawConsole = false;
		}

		public static void WriteLine(string message, IPluginInfo plugin = null, string logFile = "default.log", params object[] args)
		{
			if (!Directory.Exists(logsLocation))
				Directory.CreateDirectory(logsLocation);

			message = string.Format(message, args);

			if (plugin != null)
			{
				message = string.Format("[{0}] {1}", plugin.Name, message);
			}

			textQueue.Enqueue(message);

			if (logToFile)
			{
				message = string.Format("[{0}] {1}", DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss"), message);
				
				using (StreamWriter streamWriter = new StreamWriter(new FileStream(logsLocation + logFile, FileMode.Append, FileAccess.Write)))
				{
					streamWriter.WriteLine(message, args);
				}
			}
		}
	}
}
