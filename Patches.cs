
using System;
using System.Linq;
using System.Reflection;
using Harmony;
using VRC.Core;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnhollowerRuntimeLib.XrefScans;

namespace VRCPlusPet
{
    class Patches
    {
        static HarmonyInstance modHarmonyInstance = HarmonyInstance.Create(BuildInfo.Name);

        static int patchNum = 0;

        static Transform petTransformCache;

        static Image
            bubbleImageComponentCache,
            petImageComponentCache;



        static void Patch(MethodBase TargetMethod, HarmonyMethod Prefix, HarmonyMethod Postfix) {
            try
            {
                modHarmonyInstance.Patch(TargetMethod, Prefix, Postfix);
                MelonLogger.Msg($"Patching method [{++patchNum}] - Success");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Patching method [{++patchNum}] - Error: {e.Message}");
            }
        }

        static HarmonyMethod GetLocalPatchMethod(string name) => new HarmonyMethod(typeof(Patches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));

        public static void DoPatches()
        {
            Utils.LogAsHeader("Patching methods...");

            Patch(typeof(VRCPlusThankYou).GetMethod("OnEnable"), GetLocalPatchMethod(nameof(OnEnablePetPatch)), null);
            Patch(typeof(APIUser).GetMethod("get_isSupporter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));
            Patch(typeof(APIUser).GetMethod("get_isEarlyAdopter"), null, GetLocalPatchMethod(nameof(FakeVRCPlusSocialPatch)));

            //Rebuild warning
            foreach (MethodInfo methodInfo in typeof(ObjectPublicBoDaBoStApBoStSiDaAcUnique).GetMethods().Where(method => method.Name.StartsWith("Method_Public_Static_Boolean_")))
            {
                int count = 0;
                var instances = XrefScanner.UsedBy(methodInfo);

                foreach (XrefInstance instance in instances)
                {
                    MethodBase calledMethod = instance.TryResolve();

                    if (calledMethod != null && calledMethod.Name == ".ctor" || calledMethod.Name == "LateUpdate" || calledMethod.Name == "CancelProcessing")
                    {
                        count++;

                        if (count == 9)
                        {
                            Patch(methodInfo, GetLocalPatchMethod(nameof(FakeVRCPlusPatch)), null);
                            break;
                        }
                    }
                }
            }

            Utils.LogAsHeader("Patching complete!");
        }

        static void SMElementActiveSetter(GameObject go)
        {
            if (go.name == "UserIconCameraButton")
                go.SetActive(!MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideIconCameraButton));
            else if (go.name == "UserIconButton")
                go.SetActive(!MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideUserIconsButton));
        }

        #region Patches
        static void FakeVRCPlusSocialPatch(APIUser __instance, ref bool __result)
        {
            if (__instance.IsSelf && MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameFakeVRCP))
                __result = true;
        }

        static bool FakeVRCPlusPatch(ref bool __result)
        {
            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameFakeVRCP))
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
