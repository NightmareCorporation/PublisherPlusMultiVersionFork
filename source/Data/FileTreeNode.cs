using PublisherPlus.Settings;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PublisherPlus.Data
{
    public class FileTreeNode
    {
        /// <summary>
        /// <see cref="null"/> if root node
        /// </summary>
        public FileTreeNode parent;
        public readonly FileSystemInfo entryInfo;
        readonly ManagedWorkshopPackage package;

        string label;
        public List<FileTreeNode> children = new List<FileTreeNode>();
        HashSet<FileInfo> filesInThisNode = new HashSet<FileInfo>();
        public HashSet<FileInfo> FilesInThisNode => filesInThisNode;

        int depth = 0;
        bool isExpanded = true;

        string readableByteSize;

        public FileTreeNode(FileSystemInfo entry, FileTreeNode parent, ManagedWorkshopPackage package)
        {
            this.entryInfo = entry;
            this.package = package;
            BuildTreeFromNode(parent, package);
            SetLabels(package);
            package.fileTree.Notify_NodeAdded(entry, this);
        }

        public bool IsRoot => parent == null;
        public bool HasChildren => !children.Any();

        private void TrackFile(FileInfo file)
        {
            if(!filesInThisNode.Contains(file))
            {
                filesInThisNode.Add(file);
            }
            if(!IsRoot)
            {
                parent.TrackFile(file);
            }
        }

        private void SetLabels(ManagedWorkshopPackage package)
        {
            if(IsRoot)
            {
                label = Path.DirectorySeparatorChar.ToString();
            }
            else
            {
                if(PublisherPlusSettings.UseRelativePathToParentForFileTree)
                {
                    label = entryInfo.GetRelativePathTo(parent.entryInfo) ?? "";
                }
                else
                {
                    label = entryInfo.GetRelativePathTo(package.ModRootDirectory);
                }
            }
            readableByteSize = filesInThisNode.Sum(f => f.Length).HumanReadableBytes();
        }

        private void BuildTreeFromNode(FileTreeNode parent, ManagedWorkshopPackage package)
        {
            this.parent = parent;
            if(!IsRoot)
            {
                depth = parent.depth + 1;
            }

            if(entryInfo is FileInfo file)
            {
                TrackFile(file);
            }

            if(entryInfo is DirectoryInfo directory)
            {
                foreach(FileSystemInfo item in directory.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly))
                {
                    children.Add(new FileTreeNode(item, this, package));
                }
            }
        }

        const float indentSizePerDepth = 8f;
        public void TryDraw(Listing_Standard list)
        {
            RectDivider divider;
            Rect rowRect = list.GetRect(Text.LineHeight);
            divider = new RectDivider(rowRect, typeof(FileFilter_FileTree).GetHashCode());

            if(PublisherPlusSettings.IndentButtonsWithTree)
            {
                IndentForDepth();
            }
            DoExpandToggle();
            DoIncludedToggle();
            if(PublisherPlusSettings.ShowFileSizeInFileTree)
            {
                DoFileInfo();
            }
            DoFileLabel();

            if(isExpanded)
            {
                foreach(FileTreeNode item in children)
                {
                    item.TryDraw(list);
                }
            }

            void DoFileLabel()
            {
                if(!PublisherPlusSettings.IndentButtonsWithTree)
                {
                    IndentForDepth();
                }
                Rect labelRect = divider.Rect;
                Color previousColor = GUI.color;
                bool isIncluded = package.AllowsPublishing(entryInfo, out string reason);
                Color color = isIncluded ? GUI.color : Color.red;
                GUI.color = color;
                Widgets.Label(labelRect, label);
                if(reason != null)
                {
                    TooltipHandler.TipRegion(labelRect, Language.Get("FileNotAllowedToPublishReason", reason));
                }
                GUI.color = previousColor;
            }

            void IndentForDepth()
            {
                divider.NewCol(depth * indentSizePerDepth, HorizontalJustification.Left);
            }

            void DoExpandToggle()
            {
                Rect rect = divider.NewCol(Text.LineHeight, HorizontalJustification.Left);
                if(!(entryInfo is DirectoryInfo))
                {
                    // consume the visual space, but don't draw anything in it
                    return;
                }
                string text = isExpanded ? Language.Get("Collapse") : Language.Get("Expand");
                Texture2D texture = isExpanded ? TexButton.Minus : TexButton.Plus;
                if(Widgets.ButtonImage(rect, texture))
                {
                    isExpanded = !isExpanded;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
            }

            void DoIncludedToggle()
            {
                Rect rect = divider.NewCol(Text.LineHeight, HorizontalJustification.Left);
                bool isIncludedByTreeFilter = package.fileTreeFilter.AllowsPublishing(this);
                string text = null;
                Texture2D texture = null;
                if(package.fileTreeFilter.IsPartial(this))
                {
                    text = Language.Get("Partial");
                    texture = Widgets.CheckboxPartialTex;
                }
                else if(isIncludedByTreeFilter)
                {
                    text = Language.Get("Included");
                    texture = Widgets.CheckboxOnTex;
                }
                else
                {
                    text = Language.Get("Excluded");
                    texture = Widgets.CheckboxOffTex;
                }
                if(Widgets.ButtonImage(rect, texture, tooltip: text))
                {
                    package.fileTreeFilter.ToggleState(this);
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
            }

            void DoFileInfo()
            {
                float width = Text.CalcSize(readableByteSize).x;
                Rect rect = divider.NewCol(width, HorizontalJustification.Right);
                Widgets.Label(rect, readableByteSize);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().ToString()}(entry={entryInfo?.FullName ?? "NULL"})";
        }
    }
}
