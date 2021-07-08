
using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using VRC.Core;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace VRCPlusPet
{
    class Patches
    {
        static HarmonyLib.Harmony modHarmonyInstance = new HarmonyLib.Harmony(BuildInfo.Name);
        static MethodInfo currVRCPPatch;
        static int lastVRCPMethodNum = 0;

        static int
            patchNum = 0,
            patchesCount = 4;

        static Transform petTransformCache;

        static Image
            bubbleImageComponentCache,
            petImageComponentCache;

        static void Patch(MethodBase TargetMethod, HarmonyMethod Prefix, HarmonyMethod Postfix) {
            try
            {
                MelonLogger.Msg($"Patching method [{++patchNum}/{patchesCount}] - Success");
                modHarmonyInstance.Patch(TargetMethod, Prefix, Postfix);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Patching method [{++patchNum}/{patchesCount}] - Error: {e.Message}");
            }
        }

        static HarmonyMethod GetLocalPatchMethod(string name) => new HarmonyMethod(typeof(Patches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));

        static void PatchVRCP()
        {
            if (lastVRCPMethodNum > 8)
            {
                MelonLogger.Error("Patching method [4] - Error: Proper FVRCP method not found, unpatching all methods...");
                modHarmonyInstance.UnpatchAll();
                lastVRCPMethodNum++;
                return;
            }

            //Rebuild warning
            MethodInfo methodInfo = typeof(ObjectPublicBoDaBoStApBoStSiDaAcUnique).GetMethods().Where(method => method.Name.Length == 30 && method.Name == $"Method_Public_Static_Boolean_{lastVRCPMethodNum}").First();
            lastVRCPMethodNum++;
            currVRCPPatch = methodInfo;
            Patch(methodInfo, GetLocalPatchMethod(nameof(FakeVRCPlusPatch)), null);
        }

        public static void CheckVRCPPatch()
        {
            GameObject shortcutMenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");
            GameObject vrcPlusBanner = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/HeaderContainer/VRCPlusBanner");
            GameObject vrcPlusMiniBanner = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/VRCPlusMiniBanner");

            shortcutMenu.SetActive(true);

            while ((vrcPlusBanner.activeSelf || vrcPlusMiniBanner.activeSelf) && lastVRCPMethodNum < 9)
            {
                shortcutMenu.SetActive(true);
                shortcutMenu.SetActive(false);
                shortcutMenu.SetActive(true);

                if (vrcPlusBanner.activeSelf || vrcPlusMiniBanner.activeSelf)
                {
                    MelonLogger.Warning($"Patching method [4] - Current FVRCP patch is not working, trying to patch another method... [{lastVRCPMethodNum}/8]");
                    modHarmonyInstance.Unpatch(currVRCPPatch, HarmonyPatchType.All);
                    patchNum--;
                    PatchVRCP();
                }
                else
                {
                    MelonLogger.Warning($"Patching method [4] - Found proper method! FVRCP successfuly patched.");
                    shortcutMenu.SetActive(false);
                    currVRCPPatch = null;
                    break;
                }
            }
        }

        public static void DoPatches()
        {
            Utils.LogAsHeader("Patching methods...");

            Patch(typeof(VRCPlusThankYou).GetMethod("OnEnable"), GetLocalPatchMethod(nameof(OnEnablePetPatch)), null);
            Patch(typeof(APIUser).GetMethod("get_isSupporter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));
            Patch(typeof(APIUser).GetMethod("get_isEarlyAdopter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));

            bool errorFound = false;

            try
            {
                PatchVRCP();
                MelonLogger.Msg($"Patching method [4] - Patch will be checked after UI init...");

                if (patchNum != patchesCount)
                {
                    MelonLogger.Error("Patching method [4] - Error: Unknown, unpatching all methods...");
                    modHarmonyInstance.UnpatchAll();
                    errorFound = true;
                }
            }
            catch
            {
                MelonLogger.Error("Patching method [4] - Error: Class was renamed");
                errorFound = true;
            }

            Utils.LogAsHeader(errorFound ? "Patching Failed!" : "Patching complete!");
        }

        #region Patches
        static void FakeVRCPlusSocialPatch(APIUser __instance, ref bool __result)
        {
            if (__instance.IsSelf && Utils.GetPref(VRCPlusPet.mlCfgNameFakeVRCP))
                __result = true;
        }

        static bool FakeVRCPlusPatch(ref bool __result)
        {
            if (Utils.GetPref(VRCPlusPet.mlCfgNameFakeVRCP))
            {
                __result = true;
                return false;
            }

            return true;
        }

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
