using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ChaosConductor
{
    static class ModAssets
    {
        static bool areAssetsLoaded;
        static AssetBundle loadedAssets;

        public static IEnumerator InitAssets()
        {
            // Load up assetbundle into memory
            string modAssetsLocation = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Constants.MODASSETS_FILENAME);
            ModLogger.Log($"InitAssets() retrieving assets: {modAssetsLocation}");

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(modAssetsLocation);
            yield return request;
            loadedAssets = request.assetBundle;
            areAssetsLoaded = true;

            ModLogger.Log($"InitAssets() completed! {loadedAssets.GetAllAssetNames().Length} asset(s) loaded.");

            // Debugging
            //foreach (string name in loadedAssets.GetAllAssetNames())
            //{
            //    ModLogger.Log(name);
            //}
        }

        public static T GetAssetFromBundle<T>(string name) where T : UnityEngine.Object
        {
            if (areAssetsLoaded)
            {
                return loadedAssets.LoadAsset<T>(name);
            }
            else
            {
                return null;
            }
        }
    }
}
