using HarmonyLib;
using PublisherPlus.Data;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.Steam;

namespace PublisherPlus.Patch
{
    [HarmonyPatch(typeof(Workshop))]
    public static class Workshop_Upload
    {
        /// Before:
        /// Call_t steamAPICall_t = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, "[Auto-generated text]: Update on " + DateTime.Now.ToString() + ".");
        /// After: 
        /// Call_t steamAPICall_t = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, HELPER_ChangeLogForMetaData());
        [HarmonyPatch("Upload")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectChangeLogForUpdates(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceStringInputWithChangeLog(instructions, false);
        }
        /// Before:
        /// SteamAPICall_t steamAPICall_t = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, "[Auto-generated text]: Initial upload.");
        /// After: 
        /// SteamAPICall_t steamAPICall_t = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, HELPER_ChangeLogForMetaData());
        [HarmonyPatch("OnItemCreated")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectChangeLogForNewUploads(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceStringInputWithChangeLog(instructions, true);
        }

        private static IEnumerable<CodeInstruction> ReplaceStringInputWithChangeLog(IEnumerable<CodeInstruction> instructions, bool isInitialUpload)
        {
            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.Calls(AccessTools.Method(typeof(SteamUGC), nameof(SteamUGC.SubmitItemUpdate))))
                .ThrowIfInvalid("Could not find patching anchor")
                .Advance(-1)
                .RemoveSearchBackward(ci => ci.LoadsField(AccessTools.Field(typeof(Workshop), "curUpdateHandle")))
                .Advance(1)
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(isInitialUpload ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0),
                    CodeInstruction.Call(typeof(Workshop_Upload), nameof(HELPER_ChangeLogForMetaData))
                })
                .Instructions();
        }

        private static string HELPER_ChangeLogForMetaData(bool isInitialUpload)
        {
            ManagedWorkshopPackage package = ManagedWorkshopPackage.Current;
            if(!package.ChangeLog.NullOrEmpty())
            {
                return package.ChangeLog;
            }
            if(isInitialUpload)
            {
                return "[Auto-generated text]: Initial upload.";
            }
            return $"[Auto-generated text]: Update on {DateTime.Now}.";
        }
    }
}
