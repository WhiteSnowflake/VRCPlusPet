
using System.IO;
using System.Linq;
using MelonLoader;
using UnityEngine;

namespace VRCPlusPet
{
    public static class BuildInfo
    {
        public const string Name = "VRCPlusPet";
        public const string Description = "Hides VRC+ advertising, can replace default pet, his phrases, poke sounds and chat bubble.";
        public const string Author = "WhiteSnowflake";
        public const string Company = null;
        public const string Version = "1.1.4";
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
            mlCfgNameFakeVRCP = "Fake VRCP",
            mlCfgNameHideUserIconTab = "Hide User Icons menu tab",
            mlCfgNameHideVRCPTab = "Hide VRC+ menu tab",
            mlCfgNameHideSocialSupporterButton = "Hide Social Supporter button",
            mlCfgNameHideUserIconsButton = "Hide User Icons button",
            mlCfgNameHideIconCameraButton = "Hide Icon Camera button";

        public static bool
            cachedCfgFakeVRCP,
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

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameFakeVRCP, false);
            cachedCfgFakeVRCP = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameFakeVRCP);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideUserIconTab, false);
            cachedCfgHideUserIconTab = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameHideUserIconTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideVRCPTab, false);
            cachedCfgHideVRCPTab = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameHideVRCPTab);

            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideIconCameraButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideUserIconsButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameHideSocialSupporterButton, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePet, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceBubble, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplacePhrases, false);
            MelonPreferences.CreateEntry(BuildInfo.Name, mlCfgNameReplaceSounds, false);

            if (!MelonHandler.Mods.Any(mod => mod.Info.Name == "UI Expansion Kit"))
                MelonLogger.Warning("UIExpansionKit not found, visual preferences cannot be accessed");

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameFakeVRCP))
                MelonLogger.Msg(string.Format("Option \"{0}\" | VRC+ will be cracked locally", mlCfgNameFakeVRCP));

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameReplacePet))
            {
                MelonLogger.Msg(string.Format("Option \"{0}\" | Pet image will be replaced", mlCfgNameReplacePet));
                Utils.SetupSprite("pet.png", mlCfgNameReplacePet, ref petSprite);
            }

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameReplaceBubble))
            {
                MelonLogger.Msg(string.Format("Option \"{0}\" | Bubble image will be replaced", mlCfgNameReplaceBubble));
                Utils.SetupSprite("bubble.png", mlCfgNameReplaceBubble, ref bubbleSprite, true);
            }

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameReplacePhrases))
            {
                MelonLogger.Msg(string.Format("Option \"{0}\" | Pet phrases will be replaced", mlCfgNameReplacePhrases));
                Utils.SetupConfigFile("normalPhrases.txt", ref petNormalPhrases);
                Utils.SetupConfigFile("pokePhrases.txt", ref petPokePhrases);
            }

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameReplaceSounds))
            {
                MelonLogger.Msg(string.Format("Option \"{0}\" | Pet sounds will be replaced", mlCfgNameReplaceSounds));

                foreach (string fileName in Directory.GetFiles(Utils.SetupConfigFile("audio", ref emptyList, true), "*.*", SearchOption.TopDirectoryOnly))
                    if (fileName.Contains(".ogg") || fileName.Contains(".wav"))
                        MelonCoroutines.Start(Utils.SetupAudioFile(Path.Combine("file://", fileName)));
                    else
                        MelonLogger.Warning("Option \"aud\" | File has wrong audio format (Only .ogg/.wav are supported), will be ignored");
            }

            if (cachedCfgHideVRCPTab)
                MelonLogger.Msg(string.Format("Option \"{0}\" | Menu 'VRC+' tab will be hided", mlCfgNameHideVRCPTab));

            if (cachedCfgHideUserIconTab)
                MelonLogger.Msg(string.Format("Option \"{0}\" | Menu 'User Icons' tab will be hided", mlCfgNameHideUserIconTab));

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameHideIconCameraButton))
                MelonLogger.Msg(string.Format("Option \"{0}\" | 'Icon Camera' button will be hided", mlCfgNameHideIconCameraButton));

            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameHideUserIconsButton))
                MelonLogger.Msg(string.Format("Option \"{0}\" | 'User Icons' button will be hided", mlCfgNameHideUserIconsButton));

            Patches.DoPatches();
        }

        public override void VRChat_OnUiManagerInit()
        {
            if (MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, mlCfgNameFakeVRCP))
                Utils.InitUI(true);
        }
    }
}
