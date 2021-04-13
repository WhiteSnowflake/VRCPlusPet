
using System;
using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(VRCPlusPet.BuildInfo.Description)]
[assembly: AssemblyDescription(VRCPlusPet.BuildInfo.Description)]
[assembly: AssemblyCompany(VRCPlusPet.BuildInfo.Company)]
[assembly: AssemblyProduct(VRCPlusPet.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + VRCPlusPet.BuildInfo.Author)]
[assembly: AssemblyTrademark(VRCPlusPet.BuildInfo.Company)]
[assembly: AssemblyVersion(VRCPlusPet.BuildInfo.Version)]
[assembly: AssemblyFileVersion(VRCPlusPet.BuildInfo.Version)]
[assembly: MelonInfo(typeof(VRCPlusPet.VRCPlusPet), VRCPlusPet.BuildInfo.Name, VRCPlusPet.BuildInfo.Version, VRCPlusPet.BuildInfo.Author, VRCPlusPet.BuildInfo.DownloadLink)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkCyan)]
