using HarmonyLib;
using PublisherPlus.Compatibility;
using Verse;

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
      var harmony = new Harmony(Id);
      harmony.PatchAll();

      FluffyModManager.AddCompatibility(harmony);
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
