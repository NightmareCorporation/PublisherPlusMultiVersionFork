using PublisherPlus.Data;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

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
			if(list.ButtonText("Refetch"))
			{
				package.SetAllFiles();
			}
			list.Gap();
			list.Label(Language.Get("ContentDirectory").Bold());
			list.Label(package.ModRootDirectory.FullName.Italic());
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
					SoundDefOf.Click.PlayOneShotOnCamera();
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
			if(previousValue == false && package.SerializedData.GitIgnore.UseGitIgnore)
			{
				package.gitIgnoreFilter.ParseGitIgnore();
			}
		}

		private void DoFileList(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();

			float entryHeight = Text.LineHeight + list.verticalSpacing;
			int listingCount = 9999;
			const float sliderWidth = 20f;
			Rect scrollRect = new Rect(0f, 0f, inRect.width - sliderWidth, listingCount * entryHeight);

			Widgets.BeginScrollView(inRect, ref scrollPos, scrollRect);
			list.Begin(scrollRect);

			package.fileTreeFilter.root.TryDraw(list);

			list.End();
			Widgets.EndScrollView();
		}
	}
}
