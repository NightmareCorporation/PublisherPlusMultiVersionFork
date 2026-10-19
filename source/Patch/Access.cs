using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.Steam;

namespace PublisherPlus.Patch
{
    internal static class Access
    {
#if v1_7
        private static readonly MethodInfo Method_Verse_Steam_Workshop_BakeAndUpload = AccessTools.Method(typeof(Workshop), "BakeAndUpload", new[] { typeof(ModMetaData) });
        public static void Method_Verse_Steam_Workshop_BakeAndUpload_Call(ModMetaData item) => Method_Verse_Steam_Workshop_BakeAndUpload.Invoke(null, new object[] { item });
#endif
        private static readonly MethodInfo Method_Verse_Steam_Workshop_Upload = AccessTools.Method(typeof(Workshop), "Upload", new[] { typeof(WorkshopUploadable) });
        public static void Method_Verse_Steam_Workshop_Upload_Call(WorkshopUploadable item) => Method_Verse_Steam_Workshop_Upload.Invoke(null, new object[] { item });
    }
}
