using HarmonyLib;
using UnityEngine;
using Ink.Runtime;
using Newtonsoft.Json;
using ChaosConductor.Shared;

namespace ChaosConductor
{
	[HarmonyPatch(typeof(StoryManager), nameof(StoryManager.StartStory))]
	class Patch_StoryManager_StartStory
	{
		static AccessTools.FieldRef<StoryManager, Story> _storyRef = AccessTools.FieldRefAccess<StoryManager, Story>("_story");

		// Override the original StartStory function, copying the original behavior and replacing it with a Story load from custom JSON.
		static bool Prefix(StoryManager __instance)
		{
			System.Action<Story> OnCreateStory = AccessTools.StaticFieldRefAccess<StoryManager, System.Action<Story>>("OnCreateStory");

			// Retrieve the base game's story file. If there is a custom story selected, load and combine it with the base game's. Then, play!
			string storyJson = Utils.ConvertStoryStructureToInkJSON(Utils.GetBaseStory());

			if (ModManager.IsLoaded && ModManager.Instance.currentlyLoadedCustom != null)
            {
				// currentlyLoadedCustom has already combined with the base story in ModManager
				storyJson = Utils.ConvertStoryStructureToInkJSON(ModManager.Instance.currentlyLoadedCustom);
			}

			// Original function (with the change being what JSON story is loaded here!)
			_storyRef(__instance) = new Story(storyJson);
			if (OnCreateStory != null)
			{
				OnCreateStory(_storyRef(__instance));
			}
			EventManager.TriggerEvent("StoryInitialized", null);
			__instance.Continue();

			return false; // Don't run original function (we recreated it already)
		}
	}
}
