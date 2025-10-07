using PublisherPlus.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    public class Page_Commits : Page
    {
        bool isGitRepositoryPresent = false;
        float scrollHeight = 9999;
        Vector2 scrollPos;

        public Page_Commits(ManagedWorkshopPackage package) : base(package) { }

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
                DrawCommits(list);
            }
            else
            {
                list.Label(Language.Get("Commits.NoGitRepository"));
            }

            Utility.EndScrollView(list, out scrollHeight);

            DrawPreview(previewRect);
        }

        private void DrawCommits(Listing_Standard list)
        {
            Rect lastCommitRect = list.GetRect(Text.LineHeight);
            Utility.Label(lastCommitRect, Language.Get("Commits.LastCommit"), TextAnchor.UpperLeft);
            Utility.Label(lastCommitRect, "0000", TextAnchor.UpperRight);
            TooltipHandler.TipRegion(lastCommitRect, Language.Get("Commits.LastCommit.Hint"));

            list.Label(Language.Get("Commits.NewCommits").Bold());
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
            string command = @"log --max-count 20 --pretty=format:%H%n%s%n%b\r\n";
            string logResult = Utility.RunGitCommand(command, workingDirectory: projectPath);
            Log.Message(logResult);
        }

        private void DrawPreview(Rect previewRect)
        {
            if(!isGitRepositoryPresent)
            {
                return;
            }
            TooltipHandler.TipRegion(previewRect, Language.Get("Commits.Preview"));
            Widgets.TextArea(previewRect, "---", readOnly: true);
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
