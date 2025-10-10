using PublisherPlus.Data;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    public class Page_Details : Page
    {
        public Page_Details(ManagedWorkshopPackage package) : base(package) { }

        public override string Title => Language.Get("Title.Details");

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            FillWithDetails(list, package);
            list.End();
        }

        public static void FillWithDetails(Listing_Standard list, ManagedWorkshopPackage package)
        {
            list.Label(Language.Get("FileId").Bold());
            list.Label(package.HumanReadablePackageId.Italic());
            list.GapLine();

            list.Label(Language.Get("Title").Bold());
            list.Label(package.metaData.Name);
            list.GapLine();

            const int lineCountForTextEntries = 6;
            list.Label(Language.Get("Description").Bold());
            list.TextEntry(package.metaData.Description, lineCountForTextEntries);
            list.GapLine();

            if(!package.ChangeLog.NullOrEmpty())
            {
                list.Label(Language.Get("ChangeLog").Bold());
                list.TextEntry(package.ChangeLog, lineCountForTextEntries);
                list.GapLine();
            }

            list.Label(Language.Get("Tags").Bold());
            list.Label(package.metaData.GetWorkshopTags().ToCommaList().Italic());
        }
    }
}
