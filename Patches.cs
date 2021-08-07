
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
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
        static int
            lastVRCPMethodNum = 0,
            maxVRCPPatchNum = 0;

        static PropertyInfo[] vrcpProperties;
        static Dictionary<PropertyInfo, bool> oldPropertyValues = new Dictionary<PropertyInfo, bool>();

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

        public static void CheckVRCPPatch()
        {
            //Rebuild warning
            vrcpProperties = typeof(ObjectPublicBoDaBoStApBoStSiDaAcUnique).GetProperties().Where(property => property.Name.Contains("field_Private_Static_Boolean_")).ToArray();
            maxVRCPPatchNum = vrcpProperties.Count();

            GameObject shortcutMenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");
            GameObject vrcPlusBanner = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/HeaderContainer/VRCPlusBanner");
            GameObject vrcPlusMiniBanner = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/VRCPlusMiniBanner");

            shortcutMenu.SetActive(true);

            while ((vrcPlusBanner.activeSelf || vrcPlusMiniBanner.activeSelf) && lastVRCPMethodNum < (maxVRCPPatchNum + 1))
            {
                shortcutMenu.SetActive(true);
                shortcutMenu.SetActive(false);
                shortcutMenu.SetActive(true);

                if (vrcPlusBanner.activeSelf || vrcPlusMiniBanner.activeSelf)
                {
                    if (lastVRCPMethodNum >= maxVRCPPatchNum)
                    {
                        MelonLogger.Error("Patching method [4] - Error: Proper VRCP method not found, unpatching all methods...");

                        foreach(KeyValuePair<PropertyInfo, bool> keyValuePair in oldPropertyValues)
                            keyValuePair.Key.SetValue(null, keyValuePair.Value);

                        break;
                    }

                    PropertyInfo propertyInfo = vrcpProperties[lastVRCPMethodNum++];
                    oldPropertyValues.Add(propertyInfo, (bool)propertyInfo.GetValue(null));
                    propertyInfo.SetValue(null, true);
                }
                else
                {
                    MelonLogger.Warning($"Patching method [4] - Found proper method! VRCP successfuly patched.");
                    break;
                }
            }

            vrcpProperties = null;
            oldPropertyValues = null;
            shortcutMenu.SetActive(false);
        }

        public static void DoPatches()
        {
            Utils.LogAsHeader("Patching methods...");

            Patch(typeof(VRCPlusThankYou).GetMethod("OnEnable"), GetLocalPatchMethod(nameof(OnEnablePetPatch)), null);
            Patch(typeof(APIUser).GetMethod("get_isSupporter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));
            Patch(typeof(APIUser).GetMethod("get_isEarlyAdopter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));

            MelonLogger.Msg($"Patching method [4/4] - will be patched after UI init.");

            if (patchNum != (patchesCount - 1))
            {
                Utils.LogAsHeader("Patching Failed! Unpatching all methods...");
                modHarmonyInstance.UnpatchAll();
            }
            else
                Utils.LogAsHeader("Patching complete!");
        }

        #region Patches
        static void FakeVRCPlusSocialPatch(APIUser __instance, ref bool __result)
        {
            if (__instance.IsSelf && Utils.GetPref(VRCPlusPet.mlCfgNameFakeVRCP))
                __result = true;
        }

        static bool FakeVRCPlusPatch(ref bool __result)
        {
            if (vrcpProperties != null || Utils.GetPref(VRCPlusPet.mlCfgNameFakeVRCP))
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
