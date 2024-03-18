using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace ChaosConductor
{
    /*
     * 
     * 
     */
    
    [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.PlayAddressableMusic))]
    class Patch_AudioManager_PlayAddressableMusic
    {
        static bool Prefix(AudioManager __instance, string addressableKey, float fadeInDuration = 0f)
        {
            bool executeOriginal = true;

            if (ModManager.IsLoaded && ModManager.Instance.currentlyLoadedCustom != null)
            {
                string audioPath = Path.Combine(Path.Combine(ModManager.Instance.currentlyLoadedCustomPath, "Audio"), addressableKey);

                ModLogger.Log($"Looking for custom music: {audioPath}");
                string[] matchingFiles = Directory.GetFiles(Path.GetDirectoryName(audioPath), $"{Path.GetFileNameWithoutExtension(audioPath)}.*", SearchOption.TopDirectoryOnly);
                string[] acceptableExtensions = new[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif" };
                foreach (string file in matchingFiles)
                {
                    foreach (string extension in acceptableExtensions)
                    {
                        if (Path.GetExtension(file) == extension)
                        {
                            ModLogger.Log("Music found!");
                            // File found! Load the file into an AudioClip and pass it along to PlayMusic!
                            __instance.StartCoroutine(PatchUtils_AudioManager.SetAudioCoroutine(__instance, file, fadeInDuration, true));
                            executeOriginal = false;
                            break;
                        }
                    }
                }
            }

            return executeOriginal;
        }

        
    }

    [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.PlayAddressableSFX))]
    class Patch_AudioManager_PlayAddressableSFX
    {
        static bool Prefix(AudioManager __instance, string addressableKey, bool useMusicVolume = false)
        {
            bool executeOriginal = true;

            if (ModManager.IsLoaded && ModManager.Instance.currentlyLoadedCustom != null)
            {
                string audioPath = Path.Combine(Path.Combine(ModManager.Instance.currentlyLoadedCustomPath, "Audio"), addressableKey);

                ModLogger.Log($"Looking for custom SFX: {audioPath}");
                string[] matchingFiles = Directory.GetFiles(Path.GetDirectoryName(audioPath), $"{Path.GetFileNameWithoutExtension(audioPath)}.*", SearchOption.TopDirectoryOnly);
                string[] acceptableExtensions = new[] { "wav", "mp3", "ogg", "aiff", "aif" };
                foreach (string file in matchingFiles)
                {
                    foreach (string extension in acceptableExtensions)
                    {
                        if (Path.GetExtension(file) == extension)
                        {
                            ModLogger.Log("SFX found!");
                            // File found! Load the file into an AudioClip and pass it along to PlaySFX/PlayStorySFX!
                            __instance.StartCoroutine(PatchUtils_AudioManager.SetAudioCoroutine(__instance, file, 0f, false, useMusicVolume));
                            executeOriginal = false;
                        }
                    }
                }
            }

            return executeOriginal;
        }


    }

    public static class PatchUtils_AudioManager
    {
        public static IEnumerator SetAudioCoroutine(AudioManager instance, string file, float fadeInDuration, bool loadAsMusic, bool useMusicVolume = false)
        {
            AudioType incomingType = AudioType.UNKNOWN;
            switch (Path.GetExtension(file))
            {
                case ".wav":
                    incomingType = AudioType.WAV; break;
                case ".mp3":
                    incomingType = AudioType.MPEG; break;
                case ".ogg":
                    incomingType = AudioType.OGGVORBIS; break;
                case ".aiff":
                case ".aif":
                    incomingType = AudioType.AIFF; break;
            }

            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + file, incomingType);
            yield return req.SendWebRequest();
            AudioClip audio = DownloadHandlerAudioClip.GetContent(req);

            if (loadAsMusic)
            {
                // Play audio as music
                instance.PlayMusic(audio, fadeInDuration);
            }
            else
            {
                if (useMusicVolume)
                {
                    // Play audio as "story SFX" (SFX but affected by the Music audio channel)
                    instance.PlayStorySFX(audio);
                }
                else
                {
                    // Play audio as regular SFX (affected by the SFX audio channel)
                    instance.PlaySFX(audio);
                }
            }
        }
    }
}