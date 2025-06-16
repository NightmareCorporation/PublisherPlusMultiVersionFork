using PublisherPlus.Data;
using UnityEngine;

namespace PublisherPlus.Interface
{
	public class Page_Finalize : Page
	{
		public Page_Finalize(WorkshopPackage package) : base(package) { }

		public override string Title => throw new System.NotImplementedException();

		public override void DoWindowContents(Rect inRect)
		{
			Verse.Listing_Standard list = new Verse.Listing_Standard();
			list.Begin(inRect);
			list.Gap();
			list.Label(Lang.Get("FinalInformation", package.Title.Bold(), package.Id.Bold()));
			list.End();
		}
	}
}
