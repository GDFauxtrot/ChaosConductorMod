using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using System.IO;

namespace ChaosConductor.Shared
{
    public class SharedUtils
    {
        /// <summary>
        /// Converts Ink JSON to a StoryStructure object. The StoryStructure defines lists and dictionaries within the Ink JSON structure.
        /// </summary>
        public static StoryStructure ConvertJSONToStoryStructure(string jsonStory)
        {
            if (string.IsNullOrEmpty(jsonStory))
            {
                return null;
            }

            StoryStructureDeserialized inkJsonDeserialized = null;
            StoryStructure storyStructure = null;

            // Wrap deserialization and object conversions in exception catchers (loading custom content is vewy scawy)
            try
            {
                inkJsonDeserialized = JsonConvert.DeserializeObject<StoryStructureDeserialized>(jsonStory);

                // Create StoryStructure from the StoryStructureDeserialized object. The Deserialized version is the closest to the JSON,
                // and the StoryStructure is a bit more extended and better-defined.
                storyStructure = new StoryStructure()
                {
                    inkVersion = inkJsonDeserialized.inkVersion,
                    root = new StoryRoot()
                    {
                        topmostInfo = (JArray)inkJsonDeserialized.root[0],
                        done = (string)inkJsonDeserialized.root[1],
                        knots = (JObject)inkJsonDeserialized.root[2]
                    },
                    listDefs = new Dictionary<string, Dictionary<string, int>>(),
                    tagInfo = new Dictionary<string, string>()
                };
            }
            catch
            {
                Debug.LogError($"ERROR! Unable to parse JSON file from mod folder! Is this a valid mod JSON file?");
                return null;
            }

            // Popuplate listDefs from the Deserialized version. The conversion takes a bit more manual work.
            foreach (var listDefs in inkJsonDeserialized.listDefs)
            {
                storyStructure.listDefs.Add(listDefs.Key, new Dictionary<string, int>());

                foreach (var listDefItem in (JObject)listDefs.Value)
                {
                    storyStructure.listDefs[listDefs.Key].Add(listDefItem.Key, (int)listDefItem.Value);
                }
            }
            // Also populate tagInfo from the Deserialized version
            foreach (JToken topmostInfoItem in storyStructure.root.topmostInfo)
            {
                // TIL: "as" returns null if not impossible. Can skip a try-catch!
                JObject topmostObj = topmostInfoItem as JObject;
                if (topmostObj != null)
                {
                    if (topmostObj.ContainsKey("#"))
                    {
                        string[] tagSplit = topmostObj["#"].ToString().Split(':');
                        string tagKey = tagSplit[0].Trim();
                        string tagValue = tagSplit[1].Trim();

                        if (storyStructure.tagInfo.ContainsKey(tagKey))
                        {
                            Debug.LogWarning($"Duplicate Ink tag key detected during parsing! \"{tagKey}\"");
                            storyStructure.tagInfo[tagKey] = tagValue;
                        }
                        else
                        {
                            storyStructure.tagInfo.Add(tagKey, tagValue);
                        }
                    }
                }
            }

            return storyStructure;
        }

        /// <summary>
        /// Returns a StoryStructure object for a given user-made Ink story file name, or null if invalid/missing/etc.
        /// </summary>
        public static StoryStructure GetCustomStory(string assetName)
        {
            if (!File.Exists(assetName))
            {
                return null;
            }

            Debug.Log($"Parsing mod file '{assetName}'...");

            string jsonText = "";

            try
            {
                jsonText = File.ReadAllText(assetName);
            }
            catch
            {
                Debug.LogError($"ERROR! Unable to read JSON file '{Path.GetFileName(assetName)}' from mod folder! What happened here?");
            }

            return ConvertJSONToStoryStructure(jsonText);
        }

        public static string TryGetTagInfo(Dictionary<string, string> tagInfo, string key, string defaultValue)
        {
            string result = defaultValue;

            if (tagInfo.ContainsKey(key))
            {
                result = tagInfo[key];
            }

            return result;
        }

        public static bool IsBaseGameName(string name)
        {
            List<string> baseGameNames = new List<string>() {
                // In-game portraits
                "amy", "arm", "blaze", "conductor", "eggman",
                "espio", "knuckles", "namelessmc", "rouge",
                "shadow", "sonic", "tails", "train", "vector", "barry",
                // Names not loaded via Addressable but is still a base game name
                "flicky", "conductor's wife", "everyone" };

            return baseGameNames.Contains(name);
        }

        public static bool IsBaseGameEnvironment(string env)
        {
            List<string> baseGameEnvironments = new List<string>()
            {
                "Casino", "Conductor_Car", "DiningCloset", "Library", "LockdownDiningCar",
                "Lounge", "MessyDiningCar", "Prologue", "SafeRoom", "Saloon"
            };

            return baseGameEnvironments.Contains(env);
        }
    }
}
