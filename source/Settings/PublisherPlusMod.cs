using UnityEngine;
using Verse;

namespace PublisherPlus.Settings
{
	public class PublisherPlusMod : Mod
	{
		public PublisherPlusMod(ModContentPack content) : base(content)
		{
			GetSettings<PublisherPlusSettings>();
		}

		public override string SettingsCategory()
		{
			return Language.Get("Settings.Title");
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(inRect);

			list.CheckboxLabeled(Language.Get("Settings.UseRelativePathToParent"), ref PublisherPlusSettings.UseRelativePathToParentForFileTree, Language.Get("Settings.UseRelativePathToParent.Tip"));
			list.CheckboxLabeled(Language.Get("Settings.ShowFileSize"), ref PublisherPlusSettings.ShowFileSizeInFileTree);
			list.CheckboxLabeled(Language.Get("Settings.IndentButtonsWithTree"), ref PublisherPlusSettings.IndentButtonsWithTree, Language.Get("Settings.IndentButtonsWithTree.Tip"));

			list.End();
		}
	}

	public class PublisherPlusSettings : ModSettings
	{
		public static bool UseRelativePathToParentForFileTree = true;
		public static bool ShowFileSizeInFileTree = true;
		public static bool IndentButtonsWithTree = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref UseRelativePathToParentForFileTree, nameof(UseRelativePathToParentForFileTree));
			Scribe_Values.Look(ref ShowFileSizeInFileTree, nameof(ShowFileSizeInFileTree));
			Scribe_Values.Look(ref IndentButtonsWithTree, nameof(IndentButtonsWithTree));
		}
	}
}
