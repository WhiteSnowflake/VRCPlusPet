
using System.IO;
using System.Linq;
using System.Collections;
using MelonLoader;
using UnityEngine;
using VRC.Core;

namespace VRCPlusPet
{
    public static class BuildInfo
    {
        public const string Name = "VRCPlusPet";
        public const string Description = "Hides VRC+ advertising, can replace default pet, his phrases, poke sounds and chat bubble.";
        public const string Author = "WhiteSnowflake";
        public const string Company = null;
        public const string Version = "1.1.8";
        public const string DownloadLink = "https://github.com/WhiteSnowflake/VRCPlusPet/tree/local-vrcp-version";
    }

    public class VRCPlusPet : MelonMod
    {
        static string
            mlCfgNameReplacePet = "Replace Pet (After Restart)",
            mlCfgNameReplaceBubble = "Replace Bubble (After Restart)",
            mlCfgNameReplacePhrases = "Replace Phrases (After Restart)",
            mlCfgNameReplaceSounds = "Replace Sounds (After Restart)";

        public static string
            mlCfgNameFakeVRCP = "Fake VRCP",
            mlCfgNameHideGalleryTab = "Hide Gallery menu tab (VRC+ Only)",
            mlCfgNameHideVRCPTab = "Hide VRC+ menu tab",
            mlCfgNameHideSocialSupporterButton = "Hide Social Supporter button",
            mlCfgNameHideGalleryButton = "Hide Gallery button";

        public static bool
            cachedCfgFakeVRCP,
            cachedCfgHideGalleryTab,
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

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameFakeVRCP, false);
            cachedCfgFakeVRCP = Utils.GetPref(mlCfgNameFakeVRCP);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideGalleryTab, false);
            cachedCfgHideGalleryTab = Utils.GetPref(mlCfgNameHideGalleryTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideVRCPTab, false);
            cachedCfgHideVRCPTab = Utils.GetPref(mlCfgNameHideVRCPTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideGalleryButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideSocialSupporterButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePet, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceBubble, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePhrases, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceSounds, false);

            if (!MelonHandler.Mods.Any(mod => mod.Info.Name == "UI Expansion Kit"))
                MelonLogger.Warning("UIExpansionKit not found, visual preferences cannot be accessed");

            if (Utils.GetPref(mlCfgNameFakeVRCP))
                MelonLogger.Msg($"Option \"{mlCfgNameFakeVRCP}\" | VRC+ will be cracked locally");

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

            if (cachedCfgHideGalleryTab)
                MelonLogger.Msg($"Option \"{mlCfgNameHideGalleryTab}\" | 'Gallery' menu tab will be hided");

            if (Utils.GetPref(mlCfgNameHideGalleryButton))
                MelonLogger.Msg($"Option \"{mlCfgNameHideGalleryButton}\" | 'Gallery' button will be hided");

            Patches.DoPatches();
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
        }
    }
}
