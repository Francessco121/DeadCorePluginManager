using System;
using ManagedSteam;
using UnityEngine;

namespace DCPM.Common
{
	public static class GlobalVars
	{
        public static string RootFolder => Environment.CurrentDirectory + "\\DeadCore_Data\\Managed";
        public static Font UnityArialFont => unityArialFont;
        public static string SteamName => Steam.Instance.Friends.GetPersonaName();

        readonly static Font unityArialFont = Resources.FindObjectsOfTypeAll<Font>()[0];
	}
}
