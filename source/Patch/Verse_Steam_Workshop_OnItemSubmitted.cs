using HarmonyLib;
using PublisherPlus.Data;
using Steamworks;
using Verse.Steam;

namespace PublisherPlus.Patch
{
    [HarmonyPatch(typeof(Workshop), "OnItemSubmitted")]
    internal static class Verse_Steam_Workshop_OnItemSubmitted
    {
        [HarmonyPostfix]
        public static void NotifyManagedPackageOfPublish(SubmitItemUpdateResult_t result)
        {
            string fileId = result.m_nPublishedFileId.ToString();
            ManagedWorkshopPackage.OnUploaded(fileId);
        }
    }
}
