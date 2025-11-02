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

        public static IntRange drawEntriesRange = new IntRange(0, 9999);
        public static int currentEntryID = 0;
        public static int maxEntries = 9999;
        private void DoFileList(Rect inRect)
        {
            currentEntryID = 0;
            Listing_Standard list = new Listing_Standard();

            float entryHeight = Text.LineHeight;
            const float sliderWidth = 20f;
            Rect scrollRect = new Rect(0f, 0f, inRect.width - sliderWidth, maxEntries * entryHeight);

            Widgets.BeginScrollView(inRect, ref scrollPos, scrollRect);
            list.Begin(scrollRect);

            drawEntriesRange.min = Mathf.FloorToInt(scrollPos.y / entryHeight);
            drawEntriesRange.max = Mathf.CeilToInt(drawEntriesRange.min + inRect.height / entryHeight);
            string log = $"range: {drawEntriesRange}, max: {maxEntries}";
            Log.ErrorOnce(log, log.GetHashCode());
            package.fileTree.Root.TryDraw(list);

            maxEntries = currentEntryID;
            list.End();
            Widgets.EndScrollView();
        }
    }
}
