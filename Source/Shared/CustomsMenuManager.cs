using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChaosConductor.Shared
{
    public class CustomsMenuManager : MonoBehaviour
    {
        public static CustomsMenuManager Instance;

        public static string CUSTOMS_PATH { get; private set; }
        public static readonly string CUSTOMS_DEFAULT_TITLE = "Custom Story";
        public static readonly string CUSTOMS_DEFAULT_AUTHOR = "Anonymous";
        public static readonly string CUSTOMS_DEFAULT_VERSION = "v1.0";
        public static readonly string CUSTOMS_DEFAULT_DESCRIPTION = "I don't currently have a description!";

        public static readonly string CUSTOMS_ROOT_NAME = "customs";

        public GameObject customsFileParent;
        public GameObject customsFileTemplate;

        public TextMeshProUGUI customsTitle;
        public TextMeshProUGUI customsAuthor;
        public TextMeshProUGUI customsVersion;
        public TextMeshProUGUI customsDescription;

        public Button playButton;
        public Image playButtonImg;

        private List<StoryStructure> currentlyFoundCustoms;
        private List<string> currentlyFoundCustomsPaths;
        private int currentlySelectedLoadedCustoms = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CUSTOMS_PATH = $"{Application.persistentDataPath}/{CUSTOMS_ROOT_NAME}";

            currentlyFoundCustoms = new List<StoryStructure>();
            currentlyFoundCustomsPaths = new List<string>();
            customsFileTemplate.SetActive(false);
        }

        private void Start()
        {
            EventManager.StartListening("OpenCustomsMenu", OnOpenCustomsMenu);
            EventManager.StartListening("CloseCustomsMenu", OnCloseCustomsMenu);
            HideCustomsMenu();

            playButton.onClick.AddListener(StartLoadedCustomStory);
        }

        private void OnOpenCustomsMenu(Dictionary<string, object> message)
        {
            ShowCustomsMenu();
            RefreshCustoms();
            UpdateSelectedCustom(currentlySelectedLoadedCustoms);
        }

        private void OnCloseCustomsMenu(Dictionary<string, object> message)
        {
            HideCustomsMenu();
        }

        public void ShowCustomsMenu()
        {
            gameObject.SetActive(true);
        }

        public void HideCustomsMenu()
        {
            gameObject.SetActive(false);
        }

        public void RefreshCustoms()
        {
            ReadCustomsFromFolder(CUSTOMS_PATH);
            currentlySelectedLoadedCustoms = currentlyFoundCustoms.Count > 0 ? 0 : -1;

            Debug.Log($"Found {currentlyFoundCustoms.Count} custom stories!");

            // Remove and replace all customs in the list
            foreach (Transform child in customsFileParent.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
            int i = 0;
            foreach (StoryStructure custom in currentlyFoundCustoms)
            {
                GameObject newStoryObject = Instantiate(customsFileTemplate, customsFileParent.transform);
                newStoryObject.SetActive(true);
                CustomsFileView storyView = newStoryObject.GetComponent<CustomsFileView>();

                storyView.InitCustomsFileView(this, i++,
                    SharedUtils.TryGetTagInfo(custom.tagInfo, "title", CUSTOMS_DEFAULT_TITLE),
                    SharedUtils.TryGetTagInfo(custom.tagInfo, "author", CUSTOMS_DEFAULT_AUTHOR),
                    SharedUtils.TryGetTagInfo(custom.tagInfo, "version", CUSTOMS_DEFAULT_VERSION));
            }
        }

        public void ReadCustomsFromFolder(string folder)
        {
            currentlyFoundCustoms.Clear();
            currentlyFoundCustomsPaths.Clear();

            try
            {
                // Get ALL json files within the specified path. Then we can parse each and determine if they're valid Ink stories
                foreach (string file in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
                {
                    StoryStructure customStory = SharedUtils.GetCustomStory(file);

                    if (customStory == null)
                    {
                        continue;
                    }

                    currentlyFoundCustoms.Add(customStory);
                    currentlyFoundCustomsPaths.Add(Path.GetDirectoryName(file));
                    Debug.Log(Path.GetDirectoryName(file));
                }
            }
            catch
            {
                Debug.LogError($"Customs folder not found in the user's directory! Make sure the folder '{folder}' exists!");
            }
        }

        public void UpdateSelectedCustom(int selectedCustomIndex)
        {
            currentlySelectedLoadedCustoms = selectedCustomIndex;

            if (currentlySelectedLoadedCustoms < 0 || currentlySelectedLoadedCustoms >= currentlyFoundCustoms.Count)
            {
                customsTitle.text = "";
                customsAuthor.text = "";
                customsVersion.text = "";
                customsDescription.text = "";

                playButton.interactable = false;
                playButtonImg.enabled = false;
            }
            else
            {
                StoryStructure chosenCustom = currentlyFoundCustoms[currentlySelectedLoadedCustoms];

                // Assign defaults for fields not filled in by the customs file
                customsTitle.text = SharedUtils.TryGetTagInfo(chosenCustom.tagInfo, "title", CUSTOMS_DEFAULT_TITLE);
                customsAuthor.text = "By: " + SharedUtils.TryGetTagInfo(chosenCustom.tagInfo, "author", CUSTOMS_DEFAULT_AUTHOR);
                customsVersion.text = SharedUtils.TryGetTagInfo(chosenCustom.tagInfo, "version", CUSTOMS_DEFAULT_VERSION);
                customsDescription.text = SharedUtils.TryGetTagInfo(chosenCustom.tagInfo, "description", CUSTOMS_DEFAULT_DESCRIPTION);

                playButton.interactable = true;
                playButtonImg.enabled = true;
            }
        }

        public void StartLoadedCustomStory()
        {
            if (currentlySelectedLoadedCustoms < 0 || currentlySelectedLoadedCustoms >= currentlyFoundCustoms.Count)
            {
                Debug.LogError("Trying to load custom story but index is invalid!!! What the fuck happened??? wtf");
                return;
            }

            // Load the custom story by name. The chosen StoryStructure should be loaded into the StoryManager first before
            // executing the custom's initial knot!
            EventManager.TriggerEvent("StartCustomStory", new Dictionary<string, object> { { "value", currentlyFoundCustoms[currentlySelectedLoadedCustoms] }, { "path", currentlyFoundCustomsPaths[currentlySelectedLoadedCustoms] } });
        }
    }
}
