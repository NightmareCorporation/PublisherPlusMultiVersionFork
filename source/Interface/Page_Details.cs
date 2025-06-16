using PublisherPlus.Data;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
	public class Page_Details : Page
	{
		public Page_Details(WorkshopPackage package) : base(package) { }

		public override string Title => throw new System.NotImplementedException();

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(inRect);
			list.Gap();

			list.Label(Lang.Get("FileId").Bold());
			list.Label(package.Id.Italic());
			list.GapLine();

			list.Label(Lang.Get("Title").Bold());
			package.Title = list.TextEntry(package.Title);
			const string experimentalMode = "*#exp#"; // Experimental Mode: Can load tags in xml
			if(package.Title.EndsWith(experimentalMode))
			{
				package.Title = package.Title.Substring(0, package.Title.Length - experimentalMode.Length);
				Startup.ExperimentalMode = true;
				Startup.Warning("Experimental Mode activated");
			}
			list.GapLine();

			list.Label(Lang.Get("Description").Bold() + (package.IsNewCreation ? null : Lang.Get("DescriptionLocked")));
			string description = list.TextEntry(package.Description, 6);
			if(package.IsNewCreation)
			{
				package.Description = description;
			}
			list.GapLine();

			list.Label(Lang.Get("Tags").Bold());
			list.Label(package.Tags.ToCommaList().Italic());
			list.GapLine();

			list.Label(Lang.Get("PreviewFile").Bold() + (package.PreviewExists ? null : Lang.Get("PreviewNotFound").Italic()));
			package.Preview = list.TextEntry(package.Preview);

			list.End();
		}
	}
}
