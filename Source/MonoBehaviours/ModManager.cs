using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using ChaosConductor.Shared;

namespace ChaosConductor
{
    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance;
        public static bool IsLoaded { get { return Instance != null; } }

        public StoryStructure currentlyLoadedCustom;
        public string currentlyLoadedCustomPath;
        public string currentCustomEnvironment;
        public string customKnot = "";

        public Dictionary<string, bool> characterCustomExpression;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            characterCustomExpression = new Dictionary<string, bool>();
        }

        void Start()
        {
            Instantiate(ModAssets.GetAssetFromBundle<GameObject>("CustomsMenu"), GameObject.Find("ScreenRootLayer").transform);

            EventManager.StartListening("StartCustomStory", StartCustomStory);
        }

        private void StartCustomStory(Dictionary<string, object> message)
        {
            StoryStructure custom = (StoryStructure) message["value"];
            currentlyLoadedCustom = Utils.CombineBaseStoryWithMod(Utils.GetBaseStory(), custom);
            currentlyLoadedCustomPath = (string) message["path"];

            StoryManager.Instance.StartStory();
            CustomsMenuManager.Instance.HideCustomsMenu();

            // Find knot that is the custom's entry point. If it doesn't exist, show a popup explaining so
            // TODO do the popup thing
            string entryKnot = SharedUtils.TryGetTagInfo(custom.tagInfo, "modstart", "Name_Entry");
            // Assign custom name if it has been forced by the custom's creator
            string customName = SharedUtils.TryGetTagInfo(custom.tagInfo, "playername", "Barry");
            StoryManager.Instance.SetVariable("PLAYER_NAME", customName.Trim());

            EventManager.TriggerEvent("ToggleMap", new Dictionary<string, object> { { "value", false } });
            StageManager.Instance.FadeToBlack(1f, delegate
            {
                EventManager.TriggerEvent("MoveToEnvironment", new Dictionary<string, object>
                {
                    {
                        "DestinationKey",
                        entryKnot
                    },
                    { "GoToTopKnot", true }
                });
            });
        }
    }
}