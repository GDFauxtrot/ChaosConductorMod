using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosConductor
{
	/// <summary>
	/// Harmony Patch -- Add Ink functions here!! These are evaluated AFTER the base games' functions.
	/// 
	/// TODO find a way to squash the original function's Debug.LogError when it doesn't find a function name
	/// </summary>
    [HarmonyPatch(typeof(InkParser), nameof(InkParser.ParseAsFunction))]
    class Patch_InkParser_ParseAsFunction
    {
		static AccessTools.FieldRef<EnvironmentView, RectTransform> fullEnvironmentRef = AccessTools.FieldRefAccess<EnvironmentView, RectTransform>("fullEnvironment");

		static void Postfix(string text, ref bool __result)
		{
			// Set canContinue to false if Ink should pause while something is running
			bool canContinue = __result;

			// Similar behavior to base impl - find function name and arguments and separate them out. array[0].Trim() is the function name,
			// and args is the arguments passed in from Ink still intact as a string (no splitting or anything done to it)
			string[] array = text.Split('(');
			if (array.Length != 0)
			{
				string args = "";
				if (array.Length > 1)
				{
					array[1] = array[1].Replace(")", "");
					args = array[1].Trim();
				}
				// Cache environment object for use in Ink function code
				GameObject currentEnvironment = null;
				if (GameManager.Instance.currentEnvironment != null && fullEnvironmentRef(GameManager.Instance.currentEnvironment) != null)
				{
					currentEnvironment = fullEnvironmentRef(GameManager.Instance.currentEnvironment).gameObject;
				}

				switch (array[0].Trim())
				{
					case "Custom_LoadEnvironmentImage":
						Custom_LoadEnvironmentImage(args, currentEnvironment);
						
						break;
					case "Custom_SetEnvironmentPos":
						if (currentEnvironment == null)
                        {
							break;
                        }

						// Adjust the background (and by extension the world) to a percentage [0-1] from the left and top of the image.
						// Examples: (0.5, 0.5) = center | (0, 0) = top-left | (1, 1) = bottom-right
						
						// Parse args
						string[] setEnvArgs = args.Split(',');
						float setEnvArgX, setEnvArgY;
						setEnvArgX = setEnvArgY = 0.5f;
						float.TryParse(setEnvArgs[0].Trim(), out setEnvArgX);
						float.TryParse(setEnvArgs[1].Trim(), out setEnvArgY);

						// Get current environment's RectTransform and set it up!
						Sprite setEnvSpr = currentEnvironment.transform.Find("Background").GetComponent<Image>().sprite;
						Utils.PositionEnvironment(currentEnvironment.GetComponent<RectTransform>(), setEnvSpr.rect.width, setEnvSpr.rect.height, setEnvArgX, setEnvArgY);
						break;
				}
			}

			__result = canContinue;
		}

		/// <summary>
		/// Loads an environment image from the currently loaded custom's Environments folder. May also be used to load from
		/// another Environment subfolder path!
		/// </summary>
		static void Custom_LoadEnvironmentImage(string args, GameObject currentEnvironment)
        {
			if (currentEnvironment == null)
			{
				return;
			}

			// Start pulling down custom environment from customs folder
			if (!string.IsNullOrEmpty(ModManager.Instance.currentCustomEnvironment))
			{
				// Try to find the image by simply looking inside the currently loaded custom environment
				string environmentPath = Utils.GetEnvironmentPath(ModManager.Instance.currentlyLoadedCustomPath, ModManager.Instance.currentCustomEnvironment);
				string bgPath = Path.Combine(environmentPath, $"{args}.png");
				if (!File.Exists(bgPath))
				{
					// Alternative - if the user specified a BG from a different environment, look there
					bgPath = Path.Combine(Path.Combine(ModManager.Instance.currentlyLoadedCustomPath, "Environments"), $"{args}.png");
					// If it wasn't found, player is SOL I guess
				}
				if (File.Exists(bgPath))
				{
					// Image file found! Set up the environment!
					byte[] pngBytes = File.ReadAllBytes(bgPath);
					Texture2D tex2D = new Texture2D(2, 2); // Empty for now

					if (tex2D.LoadImage(pngBytes))
					{
						Sprite bgSprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero, 100f);

						// Get current environment's RectTransform and Image components and set them up!
						GameObject backgroundGO = currentEnvironment.transform.Find("Background").gameObject;

						Utils.SetupEnvironment(currentEnvironment.GetComponent<RectTransform>(), backgroundGO.GetComponent<Image>(), bgSprite);
					}
				}
				else
				{
					ModLogger.Log($"Custom_LoadEnvironmentImage FAILED! Background image '{args}' cannot be found!");
				}
			}
		}
    }
}