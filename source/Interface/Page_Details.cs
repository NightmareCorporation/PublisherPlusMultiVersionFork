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
			list.Label(package.ReadableId.Italic());
			list.GapLine();

			list.Label(Language.Get("Title").Bold());
			package.UploadablePackage.Title = list.TextEntry(package.UploadablePackage.Title);
			const string experimentalMode = "*#exp#"; // Experimental Mode: Can load tags in xml
			if(package.UploadablePackage.Title.EndsWith(experimentalMode))
			{
				package.UploadablePackage.Title = package.UploadablePackage.Title.Substring(0, package.UploadablePackage.Title.Length - experimentalMode.Length);
				Startup.ExperimentalMode = true;
				Startup.Warning("Experimental Mode activated");
			}
			list.GapLine();

			list.Label(Language.Get("Description").Bold() + (package.IsNewCreation ? null : Language.Get("DescriptionLocked")));
			string description = list.TextEntry(package.UploadablePackage.Description, 6);
			if(package.IsNewCreation)
			{
				package.UploadablePackage.Description = description;
			}
			list.GapLine();

			list.Label(Language.Get("Tags").Bold());
			list.Label(package.UploadablePackage.Tags.ToCommaList().Italic());
			list.GapLine();

			list.Label(Language.Get("PreviewFile").Bold() + (package.PreviewExists ? null : Language.Get("PreviewNotFound").Italic()));
			package.UploadablePackage.PreviewFilePath = list.TextEntry(package.UploadablePackage.PreviewFilePath);

			list.End();
		}
	}
}
