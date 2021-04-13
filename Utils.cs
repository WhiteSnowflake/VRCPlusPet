
using Harmony;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace VRCPlusPet
{
    class Utils
    {
        static string
            configPath = "VRCPlusPet_Config",
            fullconfigPath = Path.Combine(MelonUtils.UserDataDirectory, configPath),
            spacesForHeader = new string('-', 45);

        static Dictionary<string, float> originalSizes = new Dictionary<string, float>();

        public static Il2CppSystem.Collections.Generic.List<AudioClip> audioClips = new Il2CppSystem.Collections.Generic.List<AudioClip>();



        public static IEnumerator SetupAudioFile(string filePath)
        {
            WWW www = new WWW(filePath, null, new Il2CppSystem.Collections.Generic.Dictionary<string, string>());
            yield return www;

            AudioClip audioClip = www.GetAudioClip();
            audioClip.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            audioClips.Add(audioClip);
        }

        public static string SetupConfigFile(string fileName, ref Il2CppSystem.Collections.Generic.List<string> phrasesArray, bool isDirectory = false)
        {
            if (!Directory.Exists(fullconfigPath))
                Directory.CreateDirectory(fullconfigPath);

            string filePath = Path.Combine(fullconfigPath, fileName);

            if (isDirectory)
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                return filePath;
            }

            if (File.Exists(filePath))
            {
                if (phrasesArray != null)
                    foreach (string line in File.ReadAllLines(filePath))
                        if (!string.IsNullOrEmpty(line))
                            phrasesArray.Add(line);

                return filePath;
            }
            else
            {
                if (phrasesArray != null)
                    File.Create(filePath);

                return null;
            }
        }

        public static void SetupSprite(string fileName, string configName, ref Sprite sprite, bool specialBorder = false)
        {
            string texturePath = SetupConfigFile(fileName, ref VRCPlusPet.emptyList);

            if (texturePath != null)
            {
                Texture2D newTexture = new Texture2D(2, 2);
                byte[] imageByteArray = File.ReadAllBytes(texturePath);

                //poka-yoke
                if (imageByteArray.Length < 67 || !ImageConversion.LoadImage(newTexture, imageByteArray))
                    MelonLogger.Error(string.Format("Option \"{0}\" | Image loading error", configName));
                else
                {
                    sprite = Sprite.CreateSprite(newTexture, new Rect(.0f, .0f, newTexture.width, newTexture.height), new Vector2(.5f, .5f), 100f, 0, 0, specialBorder ? new Vector4(35f, 55f, 62f, 41f) : new Vector4(), false);
                    sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
            }
            else
                MelonLogger.Warning(string.Format("Option \"{0}\" | Image not found (UserData/{1}/{2})", configName, configPath, fileName));
        }

        public static void InitUI(bool firstInit = false)
        {
            SetBadGoDisabler(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/UserIconCameraButton"), !MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideIconCameraButton));
            GameObject.Find("UserInterface/QuickMenu/ShortcutMenu/UserIconButton")?.SetActive(!MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideUserIconsButton));
            GameObject.Find("UserInterface/MenuContent/Screens/UserInfo/User Panel/Supporter")?.SetActiveRecursively(!MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideSocialSupporterButton));

            bool fakeVRCP = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameFakeVRCP);
            bool hideUserIconTab = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideUserIconTab);
            bool hideVRCPTab = MelonPreferences.GetEntryValue<bool>(BuildInfo.Name, VRCPlusPet.mlCfgNameHideVRCPTab);

            if (firstInit || VRCPlusPet.cachedCfgFakeVRCP != fakeVRCP || VRCPlusPet.cachedCfgHideUserIconTab != hideUserIconTab || VRCPlusPet.cachedCfgHideVRCPTab != hideVRCPTab)
            {
                VRCPlusPet.cachedCfgFakeVRCP = fakeVRCP;
                VRCPlusPet.cachedCfgHideUserIconTab = hideUserIconTab;
                VRCPlusPet.cachedCfgHideVRCPTab = hideVRCPTab;

                Transform tabsTransform = GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content").transform;

                for (int i = 0; i < tabsTransform.childCount; i++)
                {
                    Transform childTransform = tabsTransform.GetChild(i);
                    string childName = childTransform.name;

                    if (childName != "Search")
                    {
                        if (childName == "VRC+PageTab")
                            childTransform.gameObject.SetActive(!hideVRCPTab);
                        else
                        {
                            LayoutElement childLayoutElement = childTransform.GetComponent<LayoutElement>();

                            if (childName == "UserIconTab")
                                SetBadGoDisabler(childLayoutElement.gameObject, !hideUserIconTab);
                            else
                            {
                                if (hideUserIconTab)
                                {
                                    if (!originalSizes.ContainsKey(childName))
                                        originalSizes.Add(childName, childLayoutElement.preferredWidth);

                                    childLayoutElement.preferredWidth = 250f;
                                }
                                else
                                    childLayoutElement.preferredWidth = originalSizes.GetValueSafe(childName);
                            }
                        }
                    }
                }
            }
        }

        public static void LogAsHeader(string title)
        {
            MelonLogger.Msg(spacesForHeader);
            MelonLogger.Msg(title);
            MelonLogger.Msg(spacesForHeader);
        }

        static void SetBadGoDisabler(GameObject go, bool isActive)
        {
            if (go == null)
                return;

            BadGoDisabler badGoDisabler = go.GetComponent<BadGoDisabler>();

            if (isActive && badGoDisabler != null)
                GameObject.Destroy(badGoDisabler);
            else if (badGoDisabler == null)
                go.AddComponent<BadGoDisabler>();
        }
    }
}
