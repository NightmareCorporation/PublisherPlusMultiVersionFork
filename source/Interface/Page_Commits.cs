using PublisherPlus.Data;
using PublisherPlus.Data.CommitList;
using PublisherPlus.Settings;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    [StaticConstructorOnStartup]
    public class Page_Commits : Page
    {
        bool isGitRepositoryPresent = false;
        float scrollHeight = 9999;
        Vector2 scrollPos;
        CommitCollection commitCollection;
        string startCommit;
        string changeLogText;

        IEnumerable<CommitEntry> includedCommits => commitCollection
            .Where(entry => entry.IsIncludedInChangeLog);

        public Page_Commits(ManagedWorkshopPackage package) : base(package)
        {
            startCommit = package.SerializedData.lastPublishedCommit;
            startCommitText = startCommit ?? "";
            BuildCommitList();
        }

        public override string Title => Language.Get("Title.Commits");

        public override void DoWindowContents(Rect inRect)
        {
            RectDivider divider = new RectDivider(inRect, this.GetType().GetHashCode());
            const float previewHeightRatio = 0.3f;
            Rect previewRect = divider.NewRow(inRect.height * previewHeightRatio, VerticalJustification.Bottom).Rect;
            Rect scrollRect = divider.Rect;

            Utility.MakeAndBeginScrollView(scrollRect, scrollHeight, ref scrollPos, out Listing_Standard list);
            if(list.ButtonText(Language.Get("Commits.Refresh")))
            {
                BuildCommitList();
            }
            if(isGitRepositoryPresent)
            {
                DoCommits(list);
            }
            else
            {
                list.Label(Language.Get("Commits.NoGitRepository"));
            }

            Utility.EndScrollView(list, out scrollHeight);

            DrawPreview(previewRect);
        }

        private void DoCommits(Listing_Standard list)
        {
            DoStartCommit(list);

            list.Label(Language.Get("Commits.NewCommits").Bold());
            if(commitCollection.EnumerableNullOrEmpty())
            {
                list.Label(Language.Get("Commits.NoCommits"));
            }
            else
            {
                bool insertSeparator = false;
                foreach(CommitEntry entry in commitCollection)
                {
                    if(insertSeparator)
                    {
                        list.GapLine();
                    }
                    entry.Draw(list);

                    insertSeparator = true;
                }
            }
        }

        string startCommitText;
        private static readonly Texture2D warningIcon = Resources.Load<Texture2D>("Textures/UI/Widgets/YellowWarning");
        private void DoStartCommit(Listing_Standard list)
        {
            const string fakeHashForLength = "______";
            Rect rect = list.GetRect(Text.LineHeight);
            TooltipHandler.TipRegion(rect, Language.Get("Commits.LastCommit.Hint"));
            WidgetRow row = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, rect.width);

            string label = Language.Get("Commits.LastCommit");
            row.Label(label);

            Rect hashTextRect = row.ButtonRect(fakeHashForLength);
            startCommitText = Widgets.TextArea(hashTextRect, startCommitText);
            if(startCommitText.Length >= CommitEntry.HashLength)
            {
                startCommit = startCommitText.Substring(0, CommitEntry.HashLength);
            }

            if(!commitCollection.Any(commit => commit.ShortHash == startCommitText))
            {
                row.Icon(warningIcon, Language.Get("Commits.CommitNotInList"));
            }
            if(startCommitText.Length == 6)
            {
                if(row.ButtonText(Language.Get("Commits.OverwriteStartCommit"), Language.Get("Commits.OverwriteStartCommit.Tip")))
                {
                    commitCollection.ExcludeAllCommitsBefore(startCommit);
                }
            }
        }

        private void BuildCommitList()
        {
            if(!TryFetchRepositoryPath(out FileSystemInfo gitPath))
            {
                Log.Message($"could not find git repository");
                return;
            }
            Log.Message($"found repository at {gitPath}");
            string projectPath = (gitPath as DirectoryInfo).Parent.FullName;
            string command = PublisherPlusSettings.GitLogCommand;
            string logResult = Utility.RunGitCommand(command, workingDirectory: projectPath);
            Log.Message($"git log command\n{command}\nat dir\n{projectPath}\nproduces result:\n{logResult}");
            commitCollection = new CommitCollection(logResult);
            package.CurrentCommitHash = commitCollection.FirstOrDefault()?.ShortHash;
            startCommit = package.SerializedData.lastPublishedCommit;
            if(startCommit != null)
            {
                commitCollection.ExcludeAllCommitsBefore(startCommit);
            }
        }

        Texture2D lineTexture = SolidColorMaterials.NewSolidColorTexture(Color.gray);
        private void DrawPreview(Rect previewRect)
        {
            previewRect = previewRect.ContractedBy(2);
            Widgets.DrawBox(previewRect, 2, lineTexture);
            previewRect = previewRect.ContractedBy(2);
            if(!isGitRepositoryPresent)
            {
                return;
            }
            TooltipHandler.TipRegion(previewRect, Language.Get("Commits.Preview"));
            if(!includedCommits.EnumerableNullOrEmpty())
            {
                changeLogText = string.Join("\n", includedCommits.Select(c => c.Content));
                package.ChangeLog = changeLogText;
            }
            else
            {
                changeLogText = null;
            }
            Widgets.TextArea(previewRect, changeLogText ?? Language.Get("Commits.NoCommitsSelectedPreview"), readOnly: true);
        }

        private bool TryFetchRepositoryPath(out FileSystemInfo repositoryPath)
        {
            const string searchPattern = ".git";
            repositoryPath = package.ModRootDirectory.GetFileSystemInfos(searchPattern).FirstOrDefault() as DirectoryInfo;
            isGitRepositoryPresent = repositoryPath != null;
            return isGitRepositoryPresent;
        }

    }
}
