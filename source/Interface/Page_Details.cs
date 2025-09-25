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
            list.Gap();

            list.Label(Language.Get("FileId").Bold());
            list.Label(package.HumanReadablePackageId.Italic());
            list.GapLine();

            list.Label(Language.Get("Title").Bold());
            list.Label(package.metaData.Name);
            list.GapLine();

            list.Label(Language.Get("Description").Bold());
            list.Label(package.metaData.Description, Text.LineHeight * 6);
            list.GapLine();

            list.Label(Language.Get("Tags").Bold());
            list.Label(package.metaData.GetWorkshopTags().ToCommaList().Italic());

            list.End();
        }
    }
}
