using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using HarmonyLib;
using HarmonyLib.Tools;

/**
 * CHAOS CONDUCTOR
 * A mod for The Murder of Sonic The Hedgehog
 * Written by GDFauxtrot
 * 
 * This mod aims to establish a robust set of tooling for users to create their own custom stories
 * within TMoSTH without any elaborate coding required!
 * 
 * Creators are able to add their own stories with branching narratives with the existing cast of
 * characters -- or, add their own characters, environments and SFX in order to create a fully
 * custom story using TMoSTH's coding framework!
 */

namespace ChaosConductor
{
    [BepInPlugin(Constants.PLUGIN_GUID, Constants.PLUGIN_NAME, Constants.PLUGIN_VERSION)]
    [BepInProcess("The Murder of Sonic The Hedgehog.exe")]
    public class Main : BaseUnityPlugin
    {
        public static BaseUnityPlugin Instance;

        internal static ManualLogSource Log;

        // List of additional libraries used (loaded in order)
        internal static string[] additionalDlls = { "ChaosConductor.Shared" };

        void Awake()
        {
            Instance = this;

            // In order for ModLogger to work properly
            Log = Logger;

            // Debugging Harmony
            HarmonyFileLog.Enabled = true;

            // Apply Harmony patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.PLUGIN_GUID);

            if (Harmony.HasAnyPatches(Constants.PLUGIN_GUID))
            {
                ModLogger.Log("Harmony patches successfully applied!");
            }

            // Load additional assemblies (if there are any)
            LoadAdditionalAssemblies();

            // Load custom assetbundle content
            StartCoroutine(ModAssets.InitAssets());

            // Success!
            ModLogger.Log($"Plugin {Constants.PLUGIN_GUID} is loaded!");

            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        void OnSceneChanged(Scene current, Scene next)
        {
            ModLogger.Log($"Scene changed: {(current.name.IsNullOrWhiteSpace() ? "NULL" : current.name)}->{next.name}");

            if (next.name == "Main")
            {
                // Instantiate ModManager singleton - the mod is now fully active!
                if (ModManager.Instance == null)
                {
                    GameObject modManagerObj = new GameObject("ModManager");
                    modManagerObj.AddComponent<ModManager>();
                }

                // Create new "CUSTOMS" main menu button to access custom content
                GameObject loadGameGO = GameObject.Find("LoadGame");
                GameObject customsMenuBtn = Instantiate(loadGameGO, loadGameGO.transform.parent);
                customsMenuBtn.name = "Customs";
                customsMenuBtn.GetComponentInChildren<TextMeshProUGUI>().text = "CUSTOMS";
                // Setup Customs button to fire an event through the in-game EventManager to open the Customs menu
                AccessTools.Field(typeof(TriggerEmptyEvent), "eventName").SetValue(customsMenuBtn.GetComponent<TriggerEmptyEvent>(), "OpenCustomsMenu");
            }
        }

        /// <summary>
        /// Load additional assemblies in the order defined
        /// </summary>
        private void LoadAdditionalAssemblies()
        {
            ModLogger.Log("Loading additional assemblies...");
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (string dllName in additionalDlls)
            {
                ModLogger.Log($"  {dllName}");
                Assembly.LoadFile(Path.Combine(currentDir, $"{dllName}.dll"));
            }
            ModLogger.Log("Assemblies loaded!");
        }
    }
}
