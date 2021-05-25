
using System;
using System.IO;
using System.Linq;
using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VRC.Core;

namespace VRCPlusPet
{
    public static class BuildInfo
    {
        public const string Name = "VRCPlusPet";
        public const string Description = "Hides VRC+ advertising, can replace default pet, his phrases, poke sounds and chat bubble. Safe version.";
        public const string Author = "WhiteSnowflake";
        public const string Company = null;
        public const string Version = "2.0.0";
        public const string DownloadLink = "https://github.com/WhiteSnowflake/VRCPlusPet";
    }

    public class VRCPlusPet : MelonMod
    {
        static string
            mlCfgNameReplacePet = "Replace Pet",
            mlCfgNameReplaceBubble = "Replace Bubble",
            mlCfgNameReplacePhrases = "Replace Phrases",
            mlCfgNameReplaceSounds = "Replace Sounds";

        public static string
            mlCfgNameHideUserIconTab = "Hide User Icons menu tab (VRC+ Only)",
            mlCfgNameHideVRCPTab = "Hide VRC+ menu tab",
            mlCfgNameHideSocialSupporterButton = "Hide Social Supporter button",
            mlCfgNameHideUserIconsButton = "Hide User Icons button",
            mlCfgNameHideIconCameraButton = "Hide Icon Camera button";

        public static bool
            cachedCfgHideUserIconTab,
            cachedCfgHideVRCPTab;

        public static Sprite
            petSprite,
            bubbleSprite;

        public static Il2CppSystem.Collections.Generic.List<string>
            petNormalPhrases = new Il2CppSystem.Collections.Generic.List<string>(),
            petPokePhrases = new Il2CppSystem.Collections.Generic.List<string>(),
            emptyList = null;

        public override void OnPreferencesSaved() => Utils.InitUI();

        public override void OnApplicationStart()
        {
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BadGoDisabler>();

            Utils.LogAsHeader("Initializing, preparing for patching...");

            MelonPreferences.CreateCategory(BuildInfo.Name, BuildInfo.Name);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideUserIconTab, false);
            cachedCfgHideUserIconTab = Utils.GetPref(mlCfgNameHideUserIconTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideVRCPTab, false);
            cachedCfgHideVRCPTab = Utils.GetPref(mlCfgNameHideVRCPTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideIconCameraButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideUserIconsButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideSocialSupporterButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePet, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceBubble, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePhrases, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceSounds, false);

            if (!MelonHandler.Mods.Any(mod => mod.Info.Name == "UI Expansion Kit"))
                MelonLogger.Warning("UIExpansionKit not found, visual preferences cannot be accessed");

            if (Utils.GetPref(mlCfgNameReplacePet))
            {
                MelonLogger.Msg($"Option \"{mlCfgNameReplacePet}\" | Pet image will be replaced");
                Utils.SetupSprite("pet.png", mlCfgNameReplacePet, ref petSprite);
            }

            if (Utils.GetPref(mlCfgNameReplaceBubble))
            {
                MelonLogger.Msg($"Option \"{mlCfgNameReplaceBubble}\" | Bubble image will be replaced");
                Utils.SetupSprite("bubble.png", mlCfgNameReplaceBubble, ref bubbleSprite, true);
            }

            if (Utils.GetPref(mlCfgNameReplacePhrases))
            {
                MelonLogger.Msg($"Option \"{mlCfgNameReplacePhrases}\" | Pet phrases will be replaced");
                Utils.SetupConfigFile("normalPhrases.txt", ref petNormalPhrases);
                Utils.SetupConfigFile("pokePhrases.txt", ref petPokePhrases);
            }

            if (Utils.GetPref(mlCfgNameReplaceSounds))
            {
                MelonLogger.Msg($"Option \"{mlCfgNameReplaceSounds}\" | Pet sounds will be replaced");

                foreach (string fileName in Directory.GetFiles(Utils.SetupConfigFile("audio", ref emptyList, true), "*.*", SearchOption.TopDirectoryOnly))
                    if (fileName.Contains(".ogg") || fileName.Contains(".wav"))
                        MelonCoroutines.Start(Utils.SetupAudioFile(Path.Combine("file://", fileName)));
                    else
                        MelonLogger.Warning("Option \"aud\" | File has wrong audio format (Only .ogg/.wav are supported), will be ignored");
            }

            if (cachedCfgHideVRCPTab)
                MelonLogger.Msg($"Option \"{mlCfgNameHideVRCPTab}\" | Menu 'VRC+' tab will be hided");

            if (cachedCfgHideUserIconTab)
                MelonLogger.Msg($"Option \"{mlCfgNameHideUserIconTab}\" | Menu 'User Icons' tab will be hided");

            if (Utils.GetPref(mlCfgNameHideIconCameraButton))
                MelonLogger.Msg($"Option \"{mlCfgNameHideIconCameraButton}\" | 'Icon Camera' button will be hided");

            if (Utils.GetPref(mlCfgNameHideUserIconsButton))
                MelonLogger.Msg($"Option \"{mlCfgNameHideUserIconsButton}\" | 'User Icons' button will be hided");

            Patches.DoPatches();
        }

        static void CheckAndRemoveAds(GameObject go, UnityEvent unityEvent)
        {
            if (go.name == "VRCPlusMiniBanner" || go.name == "VRCPlusBanner")
            {
                MelonLogger.Msg($"Disabling: [{go.name}] | Reason: [GameObject Name]");
                Utils.SetBadGoDisabler(go, false);
            }
            else if (go.name != "SupporterButton")
            {
                for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
                {
                    string methodName = unityEvent.GetPersistentMethodName(i);

                    if (methodName == "OpenSubscribeToVRCPlusPage" || methodName == "ShowVRChatUpgradePage")
                    {
                        MelonLogger.Msg($"Disabling: [{go.name}] | Reason: [Method - {methodName}]");
                        Utils.SetBadGoDisabler(go, false);
                    }
                }
            }
        }

        static IEnumerator WaitForAPIUserAndInitUI()
        {
            while (APIUser.CurrentUser == null)
                yield return null;

            Utils.InitUI(true);
        }
    
        public override void VRChat_OnUiManagerInit()
        {
            MelonCoroutines.Start(WaitForAPIUserAndInitUI());

            Utils.LogAsHeader("UI Initialized, disabling adverts...");

            bool error = false;

            try
            {
                Resources.FindObjectsOfTypeAll<Button>().ToList().ForEach(button => CheckAndRemoveAds(button.gameObject, button.onClick));
                Resources.FindObjectsOfTypeAll<TweenButton>().ToList().ForEach(button => CheckAndRemoveAds(button.gameObject, button.field_Public_UnityEvent_0));
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Adverts disabling: [{e.Message}]");
                error = true;
            }

            Utils.LogAsHeader(error ? "Adverts disabling failed!" : "Adverts disabled!");
        }
    }
}
