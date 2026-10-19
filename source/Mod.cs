using HarmonyLib;
using PublisherPlus.Compatibility;
using PublisherPlus.Patch;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.Steam;

namespace PublisherPlus
{
    [StaticConstructorOnStartup]
    internal static class Mod
    {
        public const string Id = "PublisherPlus";
        public const string Name = Id;
        public const string Version = "1.9.0";

        public static bool ExperimentalMode { get; set; }

        static Mod()
        {
            Harmony harmony = new Harmony(Id);
            harmony.PatchAll();
            RunManualPatches(harmony);

            FluffyModManager.AddCompatibility(harmony);
        }

        private static void RunManualPatches(Harmony harmony)
        {
#if v1_7
            // looks a bit odd, but the upload call is hidden in a nested class as well
            IEnumerable<MethodInfo> bakeMethods = AccessTools.InnerTypes(typeof(Workshop))
                .Concat(typeof(Workshop))
                .SelectMany(type => type.GetMethods(AccessTools.allDeclared))
                .Where((MethodInfo mi) => mi.Name.Contains("BakeAndUpload"));
            foreach(MethodInfo method in bakeMethods)
            {
                harmony.Patch(method, transpiler: new HarmonyMethod(Verse_Steam_Workshop_UploadAndBake.RemoveUploadCallsFromBaking));
            }
#endif
        }

        public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));
        public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));
        public static void Error(string message) => Verse.Log.Error(PrefixMessage(message));

        private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";

        public class Exception : System.Exception
        {
            public Exception(string message) : base(PrefixMessage(message))
            { }
        }
    }
}
