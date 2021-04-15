using DCPM.Common;
using DCPM.PluginBase;
using UnityEngine;

public class SmoothCamera : DeadCorePlugin
{
    public override string Name => "Smooth Camera";
	public override string Author => "Standalone, Francessco121";
	public override string Version => "2.0";
    public override string Desc => "Smooths camera movement to the game's render framerate to fix the camera being locked to the game's 50 FPS physics framerate.";

    bool smooth = true;
	GameObject smoothMovementGO;
	Vector3 velocity = Vector3.zero;

	void EnableSmoothing()
	{
		smoothMovementGO = new GameObject();

		PluginConsole.WriteLine("Smooth Camera Enabled", this);
		PluginSettings.Instance.SetSetting("SmoothCameraToggle", smooth);
	}

	void DisableSmoothing()
	{
        Destroy(smoothMovementGO);

		PluginConsole.WriteLine("Smooth Camera Disabled", this);
		PluginSettings.Instance.SetSetting("SmoothCameraToggle", smooth);
	}

	void Cmd_Toggle(string[] input)
	{
		smooth = !smooth;

		if (smooth)
		{
			EnableSmoothing();
		}
		else
		{
			DisableSmoothing();
		}
	}

	void Awake()
	{
		PluginConsole.RegisterConsoleCommand("sc_toggle", new PluginConsole.ConsoleCommandCallback(Cmd_Toggle), "Toggles smooth camera movement", this);

		if (bool.TryParse(PluginSettings.Instance.GetSetting("SmoothCameraToggle", smooth), out smooth))
		{
			PluginConsole.WriteLine("Smooth toggle setting loaded from file, smoothing enabled = " + smooth.ToString(), this);
		}
		else
		{
			PluginConsole.WriteLine("Could not load smooth toggle setting from file, using default: true", null);
		}
	}

	void OnLevelWasLoaded(int _level)
	{
		var level = (DeadCoreLevels.Levels)_level;

		if (smooth && 
			level != DeadCoreLevels.Levels.Main_Menu && 
			level != DeadCoreLevels.Levels.Loading_Screen)
		{
			EnableSmoothing();
		}
	}

	void Update()
	{
		if (smooth && GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
		{
			smoothMovementGO.transform.rotation = Camera.main.transform.parent.rotation;
		}
	}

	void LateUpdate()
	{
		if (smooth && GameManager.Instance.CurrentGameState == GameManager.GameState.InGame)
		{
			Vector3 position = Camera.main.transform.parent.position;

			// Interpolate smooth move GO to the current real camera position over fixed frame time
			//
			// This will result in the camera's position being behind 1 frame at the start of a fixed
			// update, and be up-to-date right as the next fixed update begins. Time.deltaTime is used
			// to smooth between these two positions, which will let this look correct regardless of framerate.
			smoothMovementGO.transform.position = Vector3.SmoothDamp(
				current: smoothMovementGO.transform.position, 
				target: position, 
				currentVelocity: ref velocity, 
				smoothTime: Time.fixedDeltaTime);

			// If the camera's real position ends up much further than it would have ended up due to player
			// velocity (e.g. due to the player returning to a checkpoint), just teleport the smooth move GO
			// instead of interpolating.
			var body = Camera.main.GetComponentInParent<Rigidbody>();
			if ((smoothMovementGO.transform.position - position).magnitude > body.velocity.magnitude * 2)
				smoothMovementGO.transform.position = Camera.main.transform.position;

			// Force the camera's position to the current smooth move GO position
			Camera.main.transform.position = smoothMovementGO.transform.position;
		}
	}
}
