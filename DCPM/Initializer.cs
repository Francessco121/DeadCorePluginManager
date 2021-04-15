namespace DCPM
{
    public static class Initializer
	{
		/// <summary>
		/// Initializes the DeadCore Plugin Manager.
		/// <para/>
		/// Must be called from DeadCore's Assembly-CSharp DLL (usually from GameManager.Awake).
		/// </summary>
		public static void Initialize()
		{
			PluginManager.Instance.Initialize();
		}
	}
}
