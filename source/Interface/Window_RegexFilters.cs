using PublisherPlus.Data;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PublisherPlus.Interface
{
	public class Window_RegexFilters : Window
	{
		ManagedWorkshopPackage package;

		public Window_RegexFilters(ManagedWorkshopPackage package)
		{
			this.package = package;
			LoadTextFromSerializedData();

			doCloseButton = false;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			draggable = true;
			resizeable = true;

			Find.WindowStack.Add(this);
		}

		string currentText;

		private void LoadTextFromSerializedData()
		{
			currentText = string.Join("\n", package.SerializedData.Regex.Patterns);
		}

		public override void DoWindowContents(Rect inRect)
		{
			RectDivider divider = new RectDivider(inRect, this.GetHashCode());
			GameFont font = Text.Font;
			Text.Font = GameFont.Medium;
			Rect titleRect = divider.NewRow(Text.LineHeight, VerticalJustification.Top);
			Widgets.Label(titleRect, Language.Get("Regex.Title"));
			Text.Font = font;

			Rect loadDefaultsRect = divider.NewRow(Text.LineHeight, VerticalJustification.Top);
			if(Widgets.ButtonText(loadDefaultsRect, Language.Get("Regex.LoadDefaults")))
			{
				package.SerializedData.Regex.SetDefaultValues();
				LoadTextFromSerializedData();
				SoundDefOf.Click.PlayOneShotOnCamera();
			}

			Rect saveRect = divider.NewRow(Text.LineHeight * 2, VerticalJustification.Bottom);
			if(Widgets.ButtonText(saveRect, Language.Get("Regex.Save")))
			{
				SavePatterns();
				SoundDefOf.Click.PlayOneShotOnCamera();
			}

			Rect textRect = divider.Rect;
			currentText = Widgets.TextArea(textRect, currentText, false);
		}

		private void SavePatterns()
		{
			package.SerializedData.Regex.Patterns = currentText
				.Split("\n")
				.Except("")
				.ToList();
		}
	}
}
