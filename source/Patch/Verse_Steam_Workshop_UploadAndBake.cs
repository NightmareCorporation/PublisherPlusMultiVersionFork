using HarmonyLib;
using System.Collections.Generic;
using Verse.Steam;

namespace PublisherPlus.Patch
{
#if !v1_3 && !v1_4 && !v1_5 && !v1_6
    internal class Verse_Steam_Workshop_UploadAndBake
    {
        /// <summary>
        /// Removes all calls to Workshop.Upload. This allows the baking method to be called in isolation before P+ grabs the files to mirror to its temp folder. Then the upload gets called by P+
        /// </summary>
        public static IEnumerable<CodeInstruction> RemoveUploadCallsFromBaking(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.SearchForward(ci => ci.Calls(AccessTools.Method(typeof(Workshop), "Upload")));
            while(matcher.IsValid)
            {
                //Log.Message($"Removing upload call at pos {matcher.Pos}");
                matcher.Advance(-2)
                    .RemoveInstructions(3)
                    .SearchForward(ci => ci.Calls(AccessTools.Method(typeof(Workshop), "Upload")));

            }
            return matcher.Instructions();
        }
    }
}
#endif