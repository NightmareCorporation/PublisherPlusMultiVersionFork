using PublisherPlus.Data;
using UnityEngine;

namespace PublisherPlus.Interface
{
    public class Page_Finalize : Page
    {
        public Page_Finalize(ManagedWorkshopPackage package) : base(package) { }

        public override string Title => Language.Get("Title.Finalize");

        public override void DoWindowContents(Rect inRect)
        {
            Verse.Listing_Standard list = new Verse.Listing_Standard();
            list.Begin(inRect);
            list.Gap();
            list.Label(Language.Get("FinalInformation", package.UploadablePackage.Title.Bold(), package.SerializedData.UploadablePackage.ReadablePublishedFileId.Bold()));
            list.End();
        }
    }
}
