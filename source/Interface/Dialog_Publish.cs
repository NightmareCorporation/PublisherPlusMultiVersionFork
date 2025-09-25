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

        private readonly ManagedWorkshopPackage package;
        Vector2 _initialSize;
        private readonly List<Page> pages;
        private Page currentPage;

        public Dialog_Publish(ModMetaData metaData)
        {
            float width = Mathf.Max(Screen.width * 0.5f, MinimumSize.x);
            float height = Mathf.Max(Screen.height * 0.75f, MinimumSize.y);

            _initialSize = new Vector2(width, height);

            package = new ManagedWorkshopPackage(metaData);
            pages = new List<Page>()
            {
                new Page_Details(package),
                new Page_Contents(package),
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

        private Vector2 MinimumSize = new Vector2(600, 600);
        public override Vector2 InitialSize => _initialSize;
        public override void OnCancelKeyPressed() => PreviousPage();
        private int CurrentIndex => pages.IndexOf(currentPage);
        private bool IsFirstPage => CurrentIndex == 0;
        private bool IsLastPage => CurrentIndex == pages.Count - 1;

        private void NextPage()
        {
            if(IsLastPage)
            {
                Upload();
            }
            else
            {
                currentPage = pages[CurrentIndex + 1];
            }
        }

        private void PreviousPage()
        {
            if(IsFirstPage)
            {
                Close();
            }
            else
            {
                currentPage = pages[CurrentIndex - 1];
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            EnforceMinimumSize();

            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            GameFont previousFont = Text.Font;
            Text.Font = GameFont.Medium;
            list.Label(currentPage.Title);
            Text.Font = previousFont;
            list.GapLine();

            Rect contentRect = list.GetRect(inRect.height - list.CurHeight - ButtonHeight);
            currentPage.DoWindowContents(contentRect);

            Rect buttonRect = list.GetRect(ButtonHeight);
            DoButtonRow(buttonRect);

            list.End();
        }

        private void EnforceMinimumSize()
        {
            windowRect.width = Mathf.Max(windowRect.width, MinimumSize.x);
            windowRect.height = Mathf.Max(windowRect.height, MinimumSize.y);
        }

        private void DoButtonRow(Rect inRect)
        {
            GridLayout grid = new GridLayout(inRect, 6);

            string previousText = IsFirstPage ? Language.Get("Button.Close") : Language.Get("Button.Back");
            if(Widgets.ButtonText(grid.GetCellRect(0, 0, 2), previousText))
            {
                PreviousPage();
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            if(Widgets.ButtonText(grid.GetCellRect(2, 0), Language.Get("Button.Default")))
            {
                package.ResetConfig();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            if(Widgets.ButtonText(grid.GetCellRect(3, 0), Language.Get("Button.Save")))
            {
                package.SaveToConfigFile();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            string nextText = IsLastPage ? Language.Get("Button.Publish") : Language.Get("Button.Next");
            if(Widgets.ButtonText(grid.GetCellRect(4, 0, 2), nextText))
            {
                NextPage();
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        }

        private void Upload()
        {
            package.UploadToWorkshop();
            Close();
        }
    }
}
