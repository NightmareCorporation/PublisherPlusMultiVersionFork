using PublisherPlus.Data;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    public class Page_Contents : Page
    {
        private Vector2 scrollPos;
        private readonly ControlRibbon controlRibbon;

        public Page_Contents(ManagedWorkshopPackage package) : base(package)
        {
            controlRibbon = new ControlRibbon(package);
        }

        public override string Title => Language.Get("Title.Contents");

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            controlRibbon.Draw(list.GetRect(Text.LineHeight));

            //if(list.ButtonText("Refetch files"))
            //{
            //    SoundDefOf.Click.PlayOneShotOnCamera();
            //    package.RefetchFiles();
            //}
            list.Gap();
            list.Label(Language.Get("ContentDirectory").Bold());
            list.Label(package.ModRootDirectory.FullName.Italic());
            list.GapLine();

            Rect fileListRect = list.GetRect(inRect.height - list.CurHeight);
            DoFileList(fileListRect);

            list.End();
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

            package.fileTree.Root.TryDraw(list);

            list.End();
            Widgets.EndScrollView();
        }
    }
}
