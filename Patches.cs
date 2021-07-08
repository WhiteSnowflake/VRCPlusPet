
using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace VRCPlusPet
{
    class Patches
    {
        static HarmonyLib.Harmony modHarmonyInstance = new HarmonyLib.Harmony(BuildInfo.Name);

        static int
            patchNum = 0,
            patchesCount = 1;

        static Transform petTransformCache;

        static Image
            bubbleImageComponentCache,
            petImageComponentCache;



        static void Patch(MethodBase TargetMethod, HarmonyMethod Prefix, HarmonyMethod Postfix)
        {
            try
            {
                modHarmonyInstance.Patch(TargetMethod, Prefix, Postfix);
                MelonLogger.Msg($"Patching method [{++patchNum}/{patchesCount}] - Success");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Patching method [{++patchNum}/{patchesCount}] - Error: {e.Message}");
            }
        }

        static HarmonyMethod GetLocalPatchMethod(string name) => new HarmonyMethod(typeof(Patches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));

        public static void DoPatches()
        {
            Utils.LogAsHeader("Patching methods...");

            Patch(typeof(VRCPlusThankYou).GetMethod("OnEnable"), GetLocalPatchMethod(nameof(OnEnablePetPatch)), null);

            Utils.LogAsHeader("Patching complete!");
        }

        #region Patches
        static void OnEnablePetPatch(VRCPlusThankYou __instance)
        {
            __instance.field_Public_Boolean_0 = false; //oncePerWorld

            if (VRCPlusPet.petNormalPhrases.Count > 0)
                __instance.field_Public_List_1_String_0 = VRCPlusPet.petNormalPhrases; //normalPhrases

            if (VRCPlusPet.petPokePhrases.Count > 0)
                __instance.field_Public_List_1_String_1 = VRCPlusPet.petPokePhrases; //pokePhrases

            if (Utils.audioClips.Count > 0)
                __instance.field_Public_List_1_AudioClip_0 = Utils.audioClips; //sounds

            if (petTransformCache == null)
                petTransformCache = __instance.transform;

            if (VRCPlusPet.bubbleSprite != null)
            {
                if (bubbleImageComponentCache == null)
                    bubbleImageComponentCache = petTransformCache.Find("Dialog Bubble").Find("Bubble").GetComponent<Image>();

                bubbleImageComponentCache.sprite = VRCPlusPet.bubbleSprite;
            }

            if (VRCPlusPet.petSprite != null)
            {
                if (petImageComponentCache == null)
                    petImageComponentCache = petTransformCache.FindChild("Character").GetComponent<Image>();

                petImageComponentCache.sprite = VRCPlusPet.petSprite;
            }
        }
        #endregion
    }
}
