using PublisherPlus.Data;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;
using GridLayout = Verse.GridLayout;

namespace PublisherPlus.Interface
{
	/// <summary>
	/// Mostly code to handle rendering and interacting with the upload dialouges
	/// </summary>
	internal class Dialog_Publish : Window
	{
		private const float Padding = 12f;
		private const float ScrollBarWidth = 20f;
		private const float ButtonHeight = 50f;

		private readonly WorkshopPackage _pack;
		private Vector2 _scroll;
		public override Vector2 InitialSize => new Vector2(600f, 600f);

		private int _page;

		public Dialog_Publish(WorkshopItemHook hook)
		{
			_pack = new WorkshopPackage(hook);
			doCloseButton = false;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = false;
		}

		private string GetTitle()
		{
			if(_page == 0)
			{
				return Lang.Get("Title.Details");
			}
			if(_page == 1)
			{
				return Lang.Get("Title.Contents");
			}
			if(_page == 2)
			{
				return Lang.Get("Title.Finalize");
			}

			throw new ArgumentOutOfRangeException();
		}

		public override void OnCancelKeyPressed() => PreviousPage();

		private void NextPage()
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();

			if(_page == 2)
			{
				Package();
				return;
			}
			_page++;
		}

		private void PreviousPage()
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();

			if(_page == 0)
			{
				Close();
				return;
			}

			_page--;
		}

		public override void DoWindowContents(Rect inRect)
		{
			GameFont previousFont = Text.Font;
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight);
			Widgets.Label(titleRect, GetTitle());
			Text.Font = previousFont;
			Widgets.DrawLineHorizontal(titleRect.x, titleRect.yMax + (Padding / 2f), titleRect.width);

			Rect contentRect = new Rect(inRect.x, titleRect.yMax + Padding, inRect.width, inRect.height - (titleRect.height + (Padding * 2f) + ButtonHeight));

			if(_page == 0)
			{
				DoDetails(contentRect);
			}
			else if(_page == 1)
			{
				DoContents(contentRect);
			}
			else if(_page == 2)
			{
				DoFinalize(contentRect);
			}

			Rect buttonRect = new Rect(inRect.x, contentRect.yMax + Padding, inRect.width, ButtonHeight);
			GridLayout grid = new GridLayout(buttonRect, 6);

			if(WidgetsPlus.ButtonText(grid.GetCellRect(0, 0, 2), _page == 0 ? Lang.Get("Button.Close") : Lang.Get("Button.Back")))
			{
				PreviousPage();
			}
			if(WidgetsPlus.ButtonText(grid.GetCellRect(2, 0), Lang.Get("Button.Default")))
			{
				ResetConfig();
			}
			if(WidgetsPlus.ButtonText(grid.GetCellRect(3, 0), Lang.Get("Button.Save")))
			{
				SaveConfig();
			}
			if(WidgetsPlus.ButtonText(grid.GetCellRect(4, 0, 2), _page == 2 ? Lang.Get("Button.Publish") : Lang.Get("Button.Next"), _pack.HasContent()))
			{
				NextPage();
			}
		}

		private void DoDetails(Rect rect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(rect);
			list.Gap();

			list.Label(Lang.Get("FileId").Bold());
			list.Label(_pack.Id.Italic());
			list.GapLine();

			list.Label(Lang.Get("Title").Bold());
			_pack.Title = list.TextEntry(_pack.Title);
			const string experimentalMode = "*#exp#"; // Experimental Mode: Can load tags in xml
			if(_pack.Title.EndsWith(experimentalMode))
			{
				_pack.Title = _pack.Title.Substring(0, _pack.Title.Length - experimentalMode.Length);
				Startup.ExperimentalMode = true;
				Startup.Warning("Experimental Mode activated");
			}
			list.GapLine();

			list.Label(Lang.Get("Description").Bold() + (_pack.IsNewCreation ? null : Lang.Get("DescriptionLocked")));
			string description = list.TextEntry(_pack.Description, 6);
			if(_pack.IsNewCreation)
			{
				_pack.Description = description;
			}
			list.GapLine();

			list.Label(Lang.Get("Tags").Bold());
			list.Label(_pack.Tags.ToCommaList().Italic());
			list.GapLine();

			list.Label(Lang.Get("PreviewFile").Bold() + (_pack.PreviewExists ? null : Lang.Get("PreviewNotFound").Italic()));
			_pack.Preview = list.TextEntry(_pack.Preview);

			list.End();
		}

		private void DoContents(Rect rect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(rect);
			list.Gap();
			list.Label(Lang.Get("ContentDirectory").Bold());
			list.Label(_pack.SourceDirectory.FullName.Italic());
			list.GapLine();
			list.End();

			Listing_Standard filterList = new Listing_Standard();
			Rect filterRect = new Rect(rect.x, rect.y + list.CurHeight, rect.width, rect.height - list.CurHeight);

			float listingSize = Text.LineHeight + filterList.verticalSpacing;
			int listingCount = _pack.AllContent.Count();
			Rect filterViewRect = new Rect(0f, 0f, rect.width - ScrollBarWidth, listingCount * listingSize);

			Widgets.BeginScrollView(filterRect, ref _scroll, filterViewRect);
			filterList.Begin(new Rect(0, _scroll.y, filterViewRect.width, filterRect.height));

			int startIndex = (int)(_scroll.y / listingSize);
			int indexRange = Math.Min((int)(filterRect.height / listingSize) + 1, listingCount);
			int endIndex = startIndex + indexRange;

			if(startIndex >= 0 && endIndex <= listingCount)
			{
				for(int i = startIndex; i < endIndex; i++)
				{
					System.IO.FileSystemInfo item = _pack.AllContent.ElementAt(i);
					string path = _pack.GetRelativePath(item);

					bool isIncluded = _pack.IsIncluded(item);
					bool include = isIncluded;
					filterList.CheckboxLabeled(item.IsDirectory() ? path.Bold() : path, ref include, item.FullName, isIncluded ? (Color?)null : Color.red);

					if(include != isIncluded)
					{
						_pack.SetIncluded(item, include);
					}
				}
			}

			filterList.End();
			Widgets.EndScrollView();
		}

		private void DoFinalize(Rect rect)
		{
			Verse.Listing_Standard list = new Verse.Listing_Standard();
			list.Begin(rect);
			list.Gap();
			list.Label(Lang.Get("FinalInformation", _pack.Title.Bold(), _pack.Id.Bold()));
			list.End();
		}

		private void SaveConfig() => _pack.SaveConfig();
		private void ResetConfig() => _pack.ResetConfig();

		private void Package()
		{
			_pack.Upload();
			Close();
		}
	}
}
