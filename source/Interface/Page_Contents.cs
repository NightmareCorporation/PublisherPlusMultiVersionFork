using PublisherPlus.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
	public class Page_Contents : Page
	{
		private Vector2 scrollPos;

		public Page_Contents(ManagedWorkshopPackage package) : base(package) { }

		public override string Title => Language.Get("Title.Contents");

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(inRect);

			DoGitIgnoreControl(list);
			list.Gap();
			list.Label(Language.Get("ContentDirectory").Bold());
			list.Label(package.SourceDirectory.FullName.Italic());
			list.GapLine();

			Rect fileListRect = list.GetRect(inRect.height - list.CurHeight);
			DoFileList(fileListRect);

			list.End();
		}

		private void DoGitIgnoreControl(Listing_Standard list)
		{
			Rect checkboxRect;
			bool useGitIgnore = package.SerializedData.GitIgnore.UseGitIgnore;
			if(useGitIgnore)
			{
				Rect rowRect = list.GetRect(Text.LineHeight);

				string parseLabel = Language.Get("GitIgnore.ParseGitIgnore");
				float buttonWidth = Text.CalcSize(parseLabel + "    ").x;
				Rect buttonRect = rowRect.RightPartPixels(buttonWidth);
				rowRect.xMax -= buttonRect.width;
				if(Widgets.ButtonText(buttonRect, parseLabel))
				{
					package.gitIgnoreFilter.ParseGitIgnore();
				}

				Rect infoIconRect = rowRect.RightPartPixels(Text.LineHeight);
				rowRect.xMax -= infoIconRect.width;
				Widgets.DrawTextureFitted(infoIconRect, TexButton.Info, 1);
				string gitIgnoreInfoText = package.gitIgnoreFilter.GitIgnoreInfoText;
				TooltipHandler.TipRegion(infoIconRect, gitIgnoreInfoText);

				checkboxRect = rowRect;
			}
			else
			{
				checkboxRect = list.GetRect(Text.LineHeight);
			}
			bool previousValue = useGitIgnore;
			Widgets.CheckboxLabeled(checkboxRect, Language.Get("GitIgnore.UseGitIgnore"), ref package.SerializedData.GitIgnore.UseGitIgnore);
			if(previousValue == false && useGitIgnore)
			{
				package.gitIgnoreFilter.ParseGitIgnore();
			}
		}

		private void DoFileList(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();

			float entryHeight = Text.LineHeight + list.verticalSpacing;
			int listingCount = package.AllFiles.Count();
			const float sliderWidth = 20f;
			Rect scrollRect = new Rect(0f, 0f, inRect.width - sliderWidth, listingCount * entryHeight);

			Widgets.BeginScrollView(inRect, ref scrollPos, scrollRect);
			list.Begin(new Rect(0, scrollPos.y, scrollRect.width, inRect.height));

			// only drawing the range of entries that are currently "visible" prevents UI lag from massive file lists
			int startIndex = (int)(scrollPos.y / entryHeight);
			int indexRange = Math.Min((int)(inRect.height / entryHeight) + 1, listingCount);
			int endIndex = startIndex + indexRange;


			if(startIndex >= 0 && endIndex <= listingCount)
			{
				IReadOnlyCollection<FileSystemInfo> files = package.AllFiles
					.OrderByDescending(f => package.AllowsPublishing(f, out _))
					.Skip(startIndex)
					.Take(indexRange)
					.ToList();

				foreach(FileSystemInfo item in files)
				{
					DoFileEntry(list, item);
				}
			}

			list.End();
			Widgets.EndScrollView();
		}

		private void DoFileEntry(Listing_Standard list, FileSystemInfo file)
		{
			string fileLabel = package.GetRelativePath(file);
			fileLabel = file.IsDirectory() ? fileLabel.Bold() : fileLabel;
			int indentCount = fileLabel.Count(c => c == Path.DirectorySeparatorChar);
			fileLabel = fileLabel.Indent(indentCount);

			bool canPublishFile = package.AllowsPublishing(file, out string reason);
			bool isIncludedByTreeFilter = package.fileTreeExclusionFilter.AllowsPublishing(file);
			Color? color = canPublishFile ? (Color?)null : Color.red;

			string tooltip = file.FullName;
			if(reason != null)
			{
				tooltip += $"\n\n{Language.Get("FileNotAllowedToPublishReason", reason)}";
			}

			bool previousIncludedByTreeFilter = isIncludedByTreeFilter;
			list.CheckboxLabeled(fileLabel, ref isIncludedByTreeFilter, tooltip, color);
			if(isIncludedByTreeFilter != previousIncludedByTreeFilter)
			{
				package.fileTreeExclusionFilter.SetExcluded(file, !isIncludedByTreeFilter);
			}
		}
	}
}
