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
			Listing_Standard mainList = new Listing_Standard();
			mainList.Begin(inRect);

			mainList.CheckboxLabeled(Language.Get("Settings.GitIgnore"), ref package.useGitIgnore);
			mainList.Gap();
			mainList.Label(Language.Get("ContentDirectory").Bold());
			mainList.Label(package.SourceDirectory.FullName.Italic());
			mainList.GapLine();

			Rect fileListRect = mainList.GetRect(inRect.height - mainList.CurHeight);
			DoFileList(fileListRect);

			mainList.End();
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

			list.CheckboxLabeled(path, ref include, file.FullName, color);

			if(include != isIncluded)
			{
				package.SetIncluded(file, include);
			}
		}
	}
}
