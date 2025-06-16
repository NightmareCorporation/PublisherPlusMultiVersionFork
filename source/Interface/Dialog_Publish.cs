using PublisherPlus.Data;
using RimWorld;
using System.Collections.Generic;
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
		private const float ButtonHeight = 50f;

		private readonly WorkshopPackage package;
		Vector2 _initialSize;
		private readonly List<Page> pages;
		private Page currentPage;

		public Dialog_Publish(WorkshopItemHook hook)
		{
			const int minSize = 600;
			float width = Mathf.Max(Screen.width * 0.5f, minSize);
			float height = Mathf.Max(Screen.height * 0.75f, minSize);

			_initialSize = new Vector2(width, height);

			package = new WorkshopPackage(hook);
			pages = new List<Page>()
			{
				new Page_Details(package),
				new Page_Content(package),
				new Page_Finalize(package),
			};
			currentPage = pages[0];

			doCloseButton = false;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = false;
			draggable = true;
			resizeable = true;
		}

		public override Vector2 InitialSize => _initialSize;
		public override void OnCancelKeyPressed() => PreviousPage();
		private int CurrentIndex => pages.IndexOf(currentPage);

		private void NextPage()
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();

			int index = CurrentIndex;
			if(index == pages.Count - 1)
			{
				Upload();
			}
			else
			{
				currentPage = pages[index - 1];
			}
		}

		private void PreviousPage()
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();

			int index = CurrentIndex;
			if(index == 0)
			{
				Close();
			}
			else
			{
				currentPage = pages[index - 1];
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			GameFont previousFont = Text.Font;
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight);
			Widgets.Label(titleRect, currentPage.Title);
			Text.Font = previousFont;
			Widgets.DrawLineHorizontal(titleRect.x, titleRect.yMax + (Padding / 2f), titleRect.width);

			Rect contentRect = new Rect(inRect.x, titleRect.yMax + Padding, inRect.width, inRect.height - (titleRect.height + (Padding * 2f) + ButtonHeight));

			currentPage.DoWindowContents(contentRect);

			Rect buttonRect = new Rect(inRect.x, contentRect.yMax + Padding, inRect.width, ButtonHeight);
			GridLayout grid = new GridLayout(buttonRect, 6);

			string previousText = CurrentIndex == 0 ? Lang.Get("Button.Close") : Lang.Get("Button.Back");
			if(WidgetsPlus.ButtonText(grid.GetCellRect(0, 0, 2), previousText))
			{
				PreviousPage();
			}
			if(WidgetsPlus.ButtonText(grid.GetCellRect(2, 0), Lang.Get("Button.Default")))
			{
				package.ResetConfig();
			}
			if(WidgetsPlus.ButtonText(grid.GetCellRect(3, 0), Lang.Get("Button.Save")))
			{
				package.SaveConfig();
			}
			string nextText = CurrentIndex == 2 ? Lang.Get("Button.Publish") : Lang.Get("Button.Next");
			if(WidgetsPlus.ButtonText(grid.GetCellRect(4, 0, 2), nextText, package.HasContent()))
			{
				NextPage();
			}
		}

		private void Upload()
		{
			package.Upload();
			Close();
		}
	}
}
