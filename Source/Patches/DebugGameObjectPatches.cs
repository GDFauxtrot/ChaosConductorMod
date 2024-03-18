using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ChaosConductor
{
    [HarmonyPatch(typeof(DebugGameObject), "Awake")]
    class Patch_DebugGameObject_Awake
    {
        static void Postfix(DebugGameObject __instance)
        {
            //__instance.gameObject.SetActive(true);
        }
    }
}