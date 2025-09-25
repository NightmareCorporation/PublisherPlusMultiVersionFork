using Verse;

namespace PublisherPlus
{
    internal static class Language
    {
        public static string Get(string key, params NamedArgument[] args)
        {
            return (Startup.ModId + "." + key).Translate().Formatted(args);
        }
    }
}
