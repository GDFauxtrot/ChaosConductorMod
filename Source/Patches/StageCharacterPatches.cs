using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.IO;
using ChaosConductor.Shared;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace ChaosConductor
{
    [HarmonyPatch(typeof(StageCharacter), nameof(StageCharacter.SetExpression))]
    class Patch_StageCharacter_SetExpression
    {

        static AccessTools.FieldRef<StageCharacter, string> characterNameRef = AccessTools.FieldRefAccess<StageCharacter, string>("characterName");
        static AccessTools.FieldRef<StageCharacter, string> currentExpressionRef = AccessTools.FieldRefAccess<StageCharacter, string>("currentExpression");
        static AccessTools.FieldRef<StageCharacter, Image> characterImageRef = AccessTools.FieldRefAccess<StageCharacter, Image>("characterImage");

        static bool Prefix(StageCharacter __instance, string expressionIn)
        {
            // CUSTOM POSES SUPPORT - take the name of the character in question and search for a custom pose in the mod location.
            // This also overrides base game characters! You can extend a character's amount of expressions or simply replace them!
            bool executeOriginal = true;

            // Only run this logic if the ModManager loaded and we've loaded up a custom!
            if (ModManager.IsLoaded && !string.IsNullOrEmpty(ModManager.Instance.currentlyLoadedCustomPath))
            {
                ModManager.Instance.characterCustomExpression[characterNameRef(__instance)] = false;

                string charExpressionPath = Utils.GetCustomExpressionPath(ModManager.Instance.currentlyLoadedCustomPath, characterNameRef(__instance), expressionIn);

                if (File.Exists(charExpressionPath))
                {
                    // Image file found! Set up the expression!
                    byte[] pngBytes = File.ReadAllBytes(charExpressionPath);
                    Texture2D tex2D = new Texture2D(2, 2); // Empty for now

                    if (tex2D.LoadImage(pngBytes))
                    {
                        executeOriginal = false;
                        characterImageRef(__instance).sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero, 100f);
                        currentExpressionRef(__instance) = expressionIn;
                        ModManager.Instance.characterCustomExpression[characterNameRef(__instance)] = true;
                    }
                }
            }
            
            return executeOriginal;
        }
    }

    [HarmonyPatch(typeof(StageCharacter), "SpriteAtlasLoaded")]
    class Patch_StageCharacter_SpriteAtlasLoaded
    {
        static AccessTools.FieldRef<StageCharacter, string> characterNameRef = AccessTools.FieldRefAccess<StageCharacter, string>("characterName");
        static AccessTools.FieldRef<StageCharacter, SpriteAtlas> characterExpressionsRef = AccessTools.FieldRefAccess<StageCharacter, SpriteAtlas>("characterExpressions");

        static bool Prefix(StageCharacter __instance, AsyncOperationHandle<SpriteAtlas> obj)
        {
            bool executeOriginal = true;

            if (ModManager.IsLoaded)
            {
                if (ModManager.Instance.characterCustomExpression.ContainsKey(characterNameRef(__instance)) && ModManager.Instance.characterCustomExpression[characterNameRef(__instance)])
                {
                    executeOriginal = false;
                    // Assign SpriteAtlas without doing the rest of the code which modifies our expression
                    switch (obj.Status)
                    {
                        case AsyncOperationStatus.Succeeded:
                            characterExpressionsRef(__instance) = obj.Result;
                            break;
                        default: // Suppress original Debug.LogError
                            break;
                    }
                }
            }

            return executeOriginal;
        }
    }

    [HarmonyPatch(typeof(StageCharacter), nameof(StageCharacter.Initialize), new System.Type[] { typeof(string), typeof(string) })]
    class Patch_StageCharacter_Initialize
    {
        static AccessTools.FieldRef<StageCharacter, string> characterNameRef = AccessTools.FieldRefAccess<StageCharacter, string>("characterName");
        static AccessTools.FieldRef<StageCharacter, string> assetKeyRef = AccessTools.FieldRefAccess<StageCharacter, string>("assetKey");
        static AccessTools.FieldRef<StageCharacter, string> currentExpressionRef = AccessTools.FieldRefAccess<StageCharacter, string>("currentExpression");
        static AccessTools.FieldRef<StageCharacter, RectTransform> characterTransformRef = AccessTools.FieldRefAccess<StageCharacter, RectTransform>("characterTransform");
        static AccessTools.FieldRef<StageCharacter, Vector2> targetAnchoredPositionRef = AccessTools.FieldRefAccess<StageCharacter, Vector2>("targetAnchoredPosition");
        static AccessTools.FieldRef<StageCharacter, Image> characterImageRef = AccessTools.FieldRefAccess<StageCharacter, Image>("characterImage");

        static bool Prefix(StageCharacter __instance, string characterName, string initialExpression)
        {
            bool executeOriginal = true;

            // Create func in shared to check name. If it has an Addressable or is in the OG func's list, load original. Otherwise do our own shit
            if (!SharedUtils.IsBaseGameName(characterName))
            {
                executeOriginal = false;

                // Set up StageCharacter manually
                characterNameRef(__instance) = characterName;
                assetKeyRef(__instance) = "portraits/" + characterName;
                currentExpressionRef(__instance) = initialExpression;
                targetAnchoredPositionRef(__instance) = characterTransformRef(__instance).anchoredPosition;

                // Since we are loading a new character, we need to load up an expression asset of whatever the initial expression is
                string charExpressionPath = Utils.GetCustomExpressionPath(ModManager.Instance.currentlyLoadedCustomPath, characterNameRef(__instance), initialExpression);

                if (File.Exists(charExpressionPath))
                {
                    // Image file found! Set up the expression!
                    byte[] pngBytes = File.ReadAllBytes(charExpressionPath);
                    Texture2D tex2D = new Texture2D(2, 2); // Empty for now

                    if (tex2D.LoadImage(pngBytes))
                    {
                        characterImageRef(__instance).sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero, 100f);
                        ModManager.Instance.characterCustomExpression[characterNameRef(__instance)] = true;
                    }
                }
            }

            return executeOriginal;
        }

        [HarmonyPatch(typeof(StageCharacter), "Update")]
        class Patch_StageCharacter_Update
        {
            static AccessTools.FieldRef<StageCharacter, Image> characterImageRef = AccessTools.FieldRefAccess<StageCharacter, Image>("characterImage");
            static AccessTools.FieldRef<StageCharacter, float> fadeSpeedRef = AccessTools.FieldRefAccess<StageCharacter, float>("fadeSpeed");
            static AccessTools.FieldRef<StageCharacter, AsyncOperationHandle<SpriteAtlas>> asynchOperationHandleRef = AccessTools.FieldRefAccess<StageCharacter, AsyncOperationHandle<SpriteAtlas>>("asynchOperationHandle");
            
            static void Postfix(StageCharacter __instance)
            {
                // Clever bastards put a check on the Addressable AsyncLoadOperartion to see if sprites have loaded yet...
                // custom chars would never load this way so this check keeps their alpha at 0
                // Recreate the last if-statement here but with the asyncOperation check removed
                if (!__instance.isExiting && characterImageRef(__instance).color.a < 1f && !asynchOperationHandleRef(__instance).IsValid())
                {
                    Color color = characterImageRef(__instance).color;
                    color.a = Mathf.Lerp(color.a, 1f, 12f * Time.deltaTime);
                    characterImageRef(__instance).color = color;
                }
            }
        }
    }
}
