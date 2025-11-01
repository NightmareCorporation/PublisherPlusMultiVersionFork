using System;
using System.IO;
using UnityEngine;
using Verse;

namespace PublisherPlus.Settings
{
    public class PublisherPlusMod : Mod
    {
        public PublisherPlusMod(ModContentPack content) : base(content)
        {
            GetSettings<PublisherPlusSettings>();
        }

        public override string SettingsCategory()
        {
            return Language.Get("Settings.Title");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.CheckboxLabeled(Language.Get("Settings.UseRelativePathToParent"), ref PublisherPlusSettings.UseRelativePathToParentForFileTree, Language.Get("Settings.UseRelativePathToParent.Tip"));
            list.CheckboxLabeled(Language.Get("Settings.ShowFileSize"), ref PublisherPlusSettings.ShowFileSizeInFileTree);
            list.CheckboxLabeled(Language.Get("Settings.IndentButtonsWithTree"), ref PublisherPlusSettings.IndentButtonsWithTree, Language.Get("Settings.IndentButtonsWithTree.Tip"));
            list.CheckboxLabeled(Language.Get("Settings.EmulateGitNotInstalled"), ref PublisherPlusSettings.EmulateGitNotInstalled, Language.Get("Settings.EmulateGitNotInstalled.Tip"));

            DoTempFolder(list);
            DoGitLogCommand(list);

            list.End();
        }

        static string tempFolderText = PublisherPlusSettings.TempFolderPath;
        private void DoTempFolder(Listing_Standard list)
        {
            list.Label(Language.Get("Settings.CurrentTempPath", tempFolderText.Italic()));

            Action saveAction = TrySetTempFolder;
            Action resetAction = () =>
            {
                tempFolderText = GenFilePaths.TempFolderPath;
                TrySetTempFolder();
            };
            list.TextAreaWithSaveResetButtons(ref tempFolderText, saveAction, resetAction);
        }

        static string gitCommandText = PublisherPlusSettings.GitLogCommand;
        private void DoGitLogCommand(Listing_Standard list)
        {
            list.Label(Language.Get("Settings.GitLogCommand"));
            Action saveAction = () => PublisherPlusSettings.GitLogCommand = gitCommandText;
            Action resetAction = PublisherPlusSettings.ResetGitLogCommand;
            list.TextAreaWithSaveResetButtons(ref gitCommandText, saveAction, resetAction);
        }

        private void TrySetTempFolder()
        {
            if(!Directory.Exists(tempFolderText))
            {
                Dialog_MessageBox messageBox = new Dialog_MessageBox(text: Language.Get("Settings.FolderDoesNotExist"),
                    Language.Get("Settings.FolderDoesNotExist.CreateFolder"), () =>
                    {
                        Directory.CreateDirectory(tempFolderText);
                        TrySetTempFolder();
                    },
                    Language.Get("Settings.Cancel"), () => { });
                Find.WindowStack.Add(messageBox);
                return;
            }
            PublisherPlusSettings.TempFolderPath = tempFolderText;
        }
    }

    public class PublisherPlusSettings : ModSettings
    {
        static bool defaultUseRelativePathToParentForFileTree = true;
        public static bool UseRelativePathToParentForFileTree = defaultUseRelativePathToParentForFileTree;
        static bool defaultShowFileSizeInFileTree = true;
        public static bool ShowFileSizeInFileTree = defaultShowFileSizeInFileTree;
        static bool defaultIndentButtonsWithTree = true;
        public static bool IndentButtonsWithTree = defaultIndentButtonsWithTree;
        static bool defaultEmulateGitNotInstalled = false;
        public static bool EmulateGitNotInstalled = defaultEmulateGitNotInstalled;
        static string defaultTempFolderPath = GenFilePaths.TempFolderPath;
        public static string TempFolderPath = defaultTempFolderPath;
        const string defaultGitLogCommand = @"log --max-count 20 --pretty=format:%H%n%cI%n%s%n%b---";
        public static string GitLogCommand = defaultGitLogCommand;

        public static void ResetGitLogCommand()
        {
            GitLogCommand = defaultGitLogCommand;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UseRelativePathToParentForFileTree, nameof(UseRelativePathToParentForFileTree), defaultUseRelativePathToParentForFileTree);
            Scribe_Values.Look(ref ShowFileSizeInFileTree, nameof(ShowFileSizeInFileTree), defaultShowFileSizeInFileTree);
            Scribe_Values.Look(ref IndentButtonsWithTree, nameof(IndentButtonsWithTree), defaultIndentButtonsWithTree);
            Scribe_Values.Look(ref EmulateGitNotInstalled, nameof(EmulateGitNotInstalled), defaultEmulateGitNotInstalled);
            Scribe_Values.Look(ref TempFolderPath, nameof(TempFolderPath), defaultTempFolderPath);
            Scribe_Values.Look(ref GitLogCommand, nameof(GitLogCommand), defaultGitLogCommand);
        }
    }
}
