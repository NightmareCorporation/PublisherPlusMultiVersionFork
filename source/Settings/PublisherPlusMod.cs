using RimWorld;
using System.IO;
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

			DoTempFolder(list);

			list.End();
		}

		static string tempFolderText = PublisherPlusSettings.TempFolderPath;
		private void DoTempFolder(Listing_Standard list)
		{
			list.Label(Language.Get("Settings.CurrentTempPath", tempFolderText.Italic()));
			RectDivider divider = new RectDivider(list.GetRect(Text.LineHeight), this.GetType().GetHashCode());
			DoSave();
			DoReset();
            tempFolderText = Widgets.TextArea(divider.Rect, tempFolderText);

			void DoSave()
			{
				string label = Language.Get("Settings.Save");
				Rect labelRect = divider.NewCol(Text.CalcSize(label + "    ").x, HorizontalJustification.Right, 2).Rect;
				if(Widgets.ButtonText(labelRect, label))
				{
					TrySetTempFolder();
				}
			}

			void DoReset()
            {
                string label = Language.Get("Settings.Reset");
                Rect labelRect = divider.NewCol(Text.CalcSize(label + "    ").x, HorizontalJustification.Right, 2).Rect;
                if(Widgets.ButtonText(labelRect, label))
                {
					tempFolderText = GenFilePaths.TempFolderPath;
					TrySetTempFolder();
                }
            }
		}

		private void TrySetTempFolder()
		{
			if(!Directory.Exists(tempFolderText))
			{
                Dialog_MessageBox messageBox = new Dialog_MessageBox(Language.Get("Settings.FolderDoesNotExist"),
					Language.Get("Settings.FolderDoesNotExist.CreateFolder"), () =>
					{
						Directory.CreateDirectory(tempFolderText);
						TrySetTempFolder();
					},
					Language.Get("Settings.Cancel"), () => { });
				Find.WindowStack.Add(messageBox);
				return;
			}
			PublisherPlusSettings.TempFolderPath = tempFolderText;
		}
	}

	public class PublisherPlusSettings : ModSettings
	{
		public static bool UseRelativePathToParentForFileTree = true;
		public static bool ShowFileSizeInFileTree = true;
		public static bool IndentButtonsWithTree = true;
		public static string TempFolderPath = GenFilePaths.TempFolderPath;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref UseRelativePathToParentForFileTree, nameof(UseRelativePathToParentForFileTree));
			Scribe_Values.Look(ref ShowFileSizeInFileTree, nameof(ShowFileSizeInFileTree));
			Scribe_Values.Look(ref IndentButtonsWithTree, nameof(IndentButtonsWithTree));
			Scribe_Values.Look(ref TempFolderPath, nameof(TempFolderPath));
		}
	}
}
