using PublisherPlus.Data;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
	public class Page_Contents : Page
	{
		private Vector2 scrollPos;

		public Page_Contents(WorkshopPackage package) : base(package) { }

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
			if(package.useGitIgnore)
			{
				Rect rowRect = list.GetRect(Text.LineHeight);
				string parseLabel = Language.Get("Settings.ParseGitIgnore");
				float buttonWidth = Text.CalcSize(parseLabel + "    ").x;
				Rect buttonRect = rowRect.RightPartPixels(buttonWidth);
				checkboxRect = rowRect.LeftPartPixels(rowRect.width - buttonRect.width);
				if(Widgets.ButtonText(buttonRect, parseLabel))
				{
					package.ParseGitIgnore();
				}
			}
			else
			{
				checkboxRect = list.GetRect(Text.LineHeight);
			}
			bool previousValue = package.useGitIgnore;
			Widgets.CheckboxLabeled(checkboxRect, Language.Get("Settings.GitIgnore"), ref package.useGitIgnore);
			if(previousValue == false && package.useGitIgnore)
			{
				package.ParseGitIgnore();
			}
		}

		private void DoFileList(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();

			float entryHeight = Text.LineHeight + list.verticalSpacing;
			int listingCount = package.AllContent.Count();
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
				for(int i = startIndex; i < endIndex; i++)
				{
					FileSystemInfo item = package.AllContent.ElementAt(i);
					DoFileEntry(list, item);
				}
			}

			list.End();
			Widgets.EndScrollView();
		}

		private void DoFileEntry(Listing_Standard list, FileSystemInfo file)
		{
			string path = package.GetRelativePath(file);
			path = file.IsDirectory() ? path.Bold() : path;

			bool isIncluded = package.IsIncluded(file);
			bool include = isIncluded;
			Color? color = isIncluded ? (Color?)null : Color.red;

			string fileLabel = package.GetRelativePath(file);
			for(int i = 0; i < fileLabel.Count(c => c == Path.DirectorySeparatorChar); i++)
			{
				fileLabel = $"  {fileLabel}";
			}
			list.CheckboxLabeled(fileLabel, ref include, file.FullName, color);

			if(include != isIncluded)
			{
				package.SetIncluded(file, include);
			}
		}
	}
}
