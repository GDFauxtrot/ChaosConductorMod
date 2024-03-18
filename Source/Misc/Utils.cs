using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChaosConductor.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using System.IO;

namespace ChaosConductor
{
    class Utils
    {
        /// <summary>
        /// Returns a StoryStructure object representing the original game's Ink story structure.
        /// </summary>
        public static StoryStructure GetBaseStory()
        {
            AccessTools.FieldRef<StoryManager, TextAsset> inkJSONAssetRef = AccessTools.FieldRefAccess<StoryManager, TextAsset>("inkJSONAsset");
            TextAsset fileAsset = inkJSONAssetRef(StoryManager.Instance);

            return ConvertJSONToStoryStructure(fileAsset.text);
        }

        /// <summary>
        /// Converts Ink JSON to a StoryStructure object. The StoryStructure defines lists and dictionaries within the Ink JSON structure.
        /// </summary>
        public static StoryStructure ConvertJSONToStoryStructure(string jsonStory)
        {
            StoryStructureDeserialized inkJsonDeserialized = JsonConvert.DeserializeObject<StoryStructureDeserialized>(jsonStory);

            // Create StoryStructure from the StoryStructureDeserialized object. The Deserialized version is the closest to the JSON,
            // and the StoryStructure is a bit more extended and better-defined.
            StoryStructure storyStructure = new StoryStructure()
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

            // Popuplate listDefs from the Deserialized version. The conversion takes a bit more manual work.
            foreach (var listDefs in inkJsonDeserialized.listDefs)
            {
                storyStructure.listDefs.Add(listDefs.Key, new Dictionary<string, int>());

                foreach (var listDefItem in (JObject)listDefs.Value)
                {
                    storyStructure.listDefs[listDefs.Key].Add(listDefItem.Key, (int)listDefItem.Value);
                }
            }

            return storyStructure;
        }

        /// <summary>
        /// Combines the loaded StoryStructure for the main game's story file and a mod's story file.
        /// </summary>
        public static StoryStructure CombineBaseStoryWithMod(StoryStructure baseStory, StoryStructure modStory)
        {
            // New combined StoryStructure uses the base game story as its basis.
            StoryStructure combinedStory = baseStory.DeepCopy();

            // Combine listDefs (excluding any named Inventory, don't want to risk someone modifying it and breaking the game)
            foreach (var newDef in modStory.listDefs)
            {
                if (newDef.Key != "Inventory")
                {
                    combinedStory.listDefs.Add(newDef.Key, newDef.Value);
                }
            }

            // Combine some root values
            foreach (var newKnot in modStory.root.knots)
            {
                // Special case: combine global vars at the end of the base
                if (newKnot.Key == "global decl")
                {
                    // TODO
                }
                // Add knot if it isn't already in the base story (AKA add all unique knots & functions)
                else if (!baseStory.root.knots.ContainsKey(newKnot.Key))
                {
                    combinedStory.root.knots.Add(newKnot.Key, newKnot.Value);
                }
            }

            // Look for any global tags from the mod and add them to the StoryStructure
            foreach (var tagKV in modStory.tagInfo)
            {
                // I don't think the base and mod StoryStructure will ever contain duplicate keys?? IDK just to be safe man this shit's scary
                if (combinedStory.tagInfo.ContainsKey(tagKV.Key))
                {
                    combinedStory.tagInfo[tagKV.Key] = tagKV.Value;
                }
                else
                {
                    combinedStory.tagInfo.Add(tagKV.Key, tagKV.Value);
                }
                
            }

            return combinedStory;
        }

        public static string ConvertStoryStructureToInkJSON(StoryStructure story)
        {
            // Convert back to StoryStructureDeserialized
            StoryStructureDeserialized storyReDeserialized = new StoryStructureDeserialized()
            {
                inkVersion = story.inkVersion,
                root = new List<object>(new object[] { story.root.topmostInfo, story.root.done, story.root.knots }),
                listDefs = JObject.FromObject(story.listDefs)
            };

            return JsonConvert.SerializeObject(storyReDeserialized);
        }

        public static string GetCustomExpressionPath(string currentCustomPath, string characterName, string expression)
        {
            string characterPath = Path.Combine(Path.Combine(currentCustomPath, "Characters"), characterName);
            string expressionPath = Path.Combine(characterPath, $"{characterName}_{expression}.png");

            return expressionPath;
        }

        public static string GetEnvironmentPath(string currentCustomPath, string environmentName)
        {
            return Path.Combine(Path.Combine(currentCustomPath, "Environments"), environmentName);
        }

        public static void SetupEnvironment(RectTransform parentRect, Image imgComponent, Sprite envImage)
        {
            // Setup Image - it's very simple, BG is H/W stretch and it's the parent rect that is adjusted
            imgComponent.sprite = envImage;
            imgComponent.color = Color.white;

            // Onto the parent RectTransform - center environment to image
            PositionEnvironment(parentRect, envImage.rect.width, envImage.rect.height);
        }

        public static void PositionEnvironment(RectTransform rectTransform, float environmentWidth, float environmentHeight, float centerX = 0.5f, float centerY = 0.5f)
        {
            // anchorMin and anchorMax is left-center (0, 0.5)
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(0f, 0.5f);
            // sizeDelta is the image width
            rectTransform.sizeDelta = new Vector2(environmentWidth, environmentHeight);

            // Adjust centering based on centerX/Y values -- 0.0 is left/top-justified, 1.0 is right/bottom-justified, 0.5 is centered
            // Canvas is always 1920x1080!
            float xDiff = environmentWidth - Constants.CANVAS_X;
            float yDiff = environmentHeight - Constants.CANVAS_Y;

            if (xDiff >= 0f)
            {
                // Image is wider than canvas - this is the main way the image will be aligned
                rectTransform.anchoredPosition = new Vector2((-xDiff * centerX), (-yDiff / 2f) + (yDiff * centerY));
            }
            else
            {
                // Image is narrower than canvas - this is not desireable, but just in case it happens, here's some positioning
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-environmentWidth / 2f, (-yDiff / 2f) + (yDiff * centerY));
            }
        }
    }
}
