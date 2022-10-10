using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(FP2Dualsense.MichaelGallinago.BuildInfo.Description)]
[assembly: AssemblyDescription(FP2Dualsense.MichaelGallinago.BuildInfo.Description)]
[assembly: AssemblyCompany(FP2Dualsense.MichaelGallinago.BuildInfo.Company)]
[assembly: AssemblyProduct(FP2Dualsense.MichaelGallinago.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + FP2Dualsense.MichaelGallinago.BuildInfo.Author)]
[assembly: AssemblyTrademark(FP2Dualsense.MichaelGallinago.BuildInfo.Company)]
[assembly: AssemblyVersion(FP2Dualsense.MichaelGallinago.BuildInfo.Version)]
[assembly: AssemblyFileVersion(FP2Dualsense.MichaelGallinago.BuildInfo.Version)]
[assembly: MelonInfo(typeof(FP2Dualsense.MichaelGallinago.FP2Dualsense), FP2Dualsense.MichaelGallinago.BuildInfo.Name, FP2Dualsense.MichaelGallinago.BuildInfo.Version, FP2Dualsense.MichaelGallinago.BuildInfo.Author, FP2Dualsense.MichaelGallinago.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]