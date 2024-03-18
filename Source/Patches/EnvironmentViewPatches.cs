using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Ink.Runtime;

namespace ChaosConductor
{
	[HarmonyPatch(typeof(EnvironmentView), "OnMoveToEnvironment")]
	class Patch_EnvironmentView_OnMoveToEnvironment
	{
		static AccessTools.FieldRef<EnvironmentView, string> environmentKeyRef = AccessTools.FieldRefAccess<EnvironmentView, string>("environmentKey");

		// Override the original OnMoveToEnvironment function. In order to allow for modded content to load environments from the original game for their
		// own custom usage, there must be a distinction made between the scene name and the environment name - a scene can specify what environment it
		// takes place in! This is done with an Ink tag "inherit_environment". We will look for this tag when a knot is loaded and, if it exists, make
		// the swap occur here for asset loading purposes.
		static bool Prefix(EnvironmentView __instance, Dictionary<string, object> message)
		{
			bool executeOriginal = true;

			string envName = (string)message["DestinationKey"];
			List<string> tagsFound = StoryManager.Instance.story.TagsForContentAtPath(envName);

			if (tagsFound != null)
            {
				// If we found tags, start looking through them!
				foreach (string tag in tagsFound)
				{
					string[] tagSplit = tag.Split(':');
					string tagKey = tagSplit[0].Trim();
					string tagValue = tagSplit[1].Trim();

					switch (tagKey)
                    {
						// Force the environment for the scene to the specified one from the base game. This allows custom stories
						// to use in-game environments with all of the objects already in place
						case "inherit_environment":
							if (ModManager.IsLoaded)
							{
								ModManager.Instance.customKnot = envName;
								message["DestinationKey"] = tagValue;
							}
							break;
						// The assigned custom environment for this knot. This environment would be defined and loaded from the
						// custom story's folder 
						case "environment":
							//executeOriginal = false;
							if (ModManager.IsLoaded)
							{
								// Set up Ink knot to point to
								ModManager.Instance.customKnot = envName;
								// Point to environment name for custom logic to load from
								ModManager.Instance.currentCustomEnvironment = tagValue;
								// Name_Entry is an empty scene we can use!
								message["DestinationKey"] = "Name_Entry";
							}
							break;
                    }
				}
            }

			return executeOriginal;
		}
	}

	[HarmonyPatch(typeof(EnvironmentView), "SwitchEnvironmentsCoroutine")]
	class Patch_EnvironmentView_SwitchEnvironmentsCoroutine
	{
		// Enumerators get roughly compiled down into:
		// IEnumerator<T> Enumerator() return new Enumerator_Impl();
		// class Enumerator_Impl : IEnumerator<T> { ... }

		// So if we want to hijack the coroutine, we can simply initialize a new Impl and ignore the old one
		static bool Prefix(EnvironmentView __instance, object[] __args, ref IEnumerator __result)
		{
			if (!ModManager.IsLoaded || string.IsNullOrEmpty(ModManager.Instance.currentlyLoadedCustomPath))
			{
				return true;
			}

			__result = new SwitchEnvironmentsCoroutineEnumerator(0)
			{
				_this = __instance,
				handleToCheck = (AsyncOperationHandle<GameObject>)__args[0],
				assetKey = (string)__args[1],
				goToTopKnot = (bool)__args[2],
				inConversation = (bool)__args[3]
			};
			return false; // Skip original method
		}
	}

	class SwitchEnvironmentsCoroutineEnumerator : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int state;
		private object current;
		public EnvironmentView _this;
		public AsyncOperationHandle<GameObject> handleToCheck;
		public string assetKey;
		public bool goToTopKnot;
		public bool inConversation;

		object IEnumerator<object>.Current { get { return current; } }
		object IEnumerator.Current { get { return current; } }

		// Harmony field access
		static AccessTools.FieldRef<EnvironmentView, string> environmentKeyRef = AccessTools.FieldRefAccess<EnvironmentView, string>("environmentKey");
		static AccessTools.FieldRef<EnvironmentView, CanvasGroup> charactersRef = AccessTools.FieldRefAccess<EnvironmentView, CanvasGroup>("characters");
		static AccessTools.FieldRef<EnvironmentView, RectTransform> fullEnvironmentRef = AccessTools.FieldRefAccess<EnvironmentView, RectTransform>("fullEnvironment");

		public SwitchEnvironmentsCoroutineEnumerator(int state)
		{
			this.state = state;
		}

		void System.IDisposable.Dispose() { }

		private bool MoveNext()
		{
			int num = this.state;
			EnvironmentView environmentView = this._this;
			if (num != 0)
			{
				return false;
			}
			this.state = -1;
			StageManager.Instance.ToggleStage(true, 0f);
			GameManager.Instance.UnloadActiveScreen();
			environmentView.UnloadCurrentEnvironment();
			GameObject gameObject = GameObject.Instantiate<GameObject>(this.handleToCheck.Result, environmentView.transform);
			gameObject.transform.SetAsFirstSibling();

			// MOD
			if (ModManager.IsLoaded)
            {
				// currentCustomEnvironment - Start pulling down custom environment from customs folder!
				if (!string.IsNullOrEmpty(ModManager.Instance.currentCustomEnvironment))
				{
					string environmentPath = Utils.GetEnvironmentPath(ModManager.Instance.currentlyLoadedCustomPath, ModManager.Instance.currentCustomEnvironment);
					string bgPath = System.IO.Path.Combine(environmentPath, "BG.png");
					if (File.Exists(bgPath))
					{
						// Image file found! Set up the environment!
						byte[] pngBytes = File.ReadAllBytes(bgPath);
						Texture2D tex2D = new Texture2D(2, 2); // Empty for now

						if (tex2D.LoadImage(pngBytes))
						{
							Sprite bgSprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero, 100f);
							GameObject backgroundGO = gameObject.transform.Find("Background").gameObject;

							Utils.SetupEnvironment(gameObject.GetComponent<RectTransform>(), backgroundGO.GetComponent<Image>(), bgSprite);
							//backgroundGO.GetComponent<Image>().sprite = bgSprite;
							//backgroundGO.GetComponent<Image>().color = Color.white;
						}
					}
				}
			}
			// MOD_END

			// environmentView.environmentKey = this.assetKey;
			// environmentView.characters = gameObject.transform.Find("Characters").GetComponent<CanvasGroup>();
			// environmentView.fullEnvironment = (gameObject.transform as RectTransform);
			// environmentView.UpdatePanningBounds();
			environmentKeyRef(environmentView) = this.assetKey;
			charactersRef(environmentView) = gameObject.transform.Find("Characters").GetComponent<CanvasGroup>();
			fullEnvironmentRef(environmentView) = gameObject.GetComponent<RectTransform>();
			AccessTools.Method(typeof(EnvironmentView), "UpdatePanningBounds").Invoke(_this, new object[] { });

			EventManager.TriggerEvent("HideLoadingScreen", null);
			if (this.goToTopKnot)
			{
				//StoryManager.Instance.GoToKnot(environmentView.environmentKey.Replace("Environments/", ""));

				// MOD
				if (ModManager.IsLoaded)
				{
					// customKnot - Load the custom story knot with an original base game environment
					if (!string.IsNullOrEmpty(ModManager.Instance.customKnot))
                    {
						StoryManager.Instance.GoToKnot(ModManager.Instance.customKnot);
						ModManager.Instance.customKnot = "";
					}
				}
				else
                {
					StoryManager.Instance.GoToKnot(environmentKeyRef(environmentView).Replace("Environments/", ""));
                }
				// MOD_END

				environmentView.ToggleCharacters(false, 0f);
			}
			if (this.inConversation)
			{
				environmentView.ToggleCharacters(false, 0f);
				DialogView.Instance.ToggleClickInput(true);
			}
			else
			{
				StageManager.Instance.ToggleStage(false, 0f);
				environmentView.ToggleCharacters(true, 0f);
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			return MoveNext();
		}

		public void Reset()
		{
			throw new System.NotSupportedException();
		}
	}
}