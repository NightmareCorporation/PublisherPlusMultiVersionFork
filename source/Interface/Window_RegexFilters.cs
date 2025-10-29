using PublisherPlus.Data;
using RimWorld;
using System.Collections.Generic;
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
            draggable = true;
            resizeable = true;
            closeOnAccept = false;  // prevents closing window on hitting ENTER

            Find.WindowStack.Add(this);
        }

        string directoryText;
        string fileText;

        private void LoadTextFromSerializedData()
        {
            directoryText = string.Join("\n", package.SerializedData.Regex.DirectoryPatterns);
            fileText = string.Join("\n", package.SerializedData.Regex.FilePatterns);
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
            DoPatternInput(textRect.LeftHalf(), Language.Get("Regex.Folders"), ref directoryText);
            DoPatternInput(textRect.RightHalf(), Language.Get("Regex.Files"), ref fileText);
        }

        private void DoPatternInput(Rect inRect, string title, ref string text)
        {
            Rect titleRect = inRect.TopPartPixels(Text.LineHeight);
            Rect textRect = inRect.BottomPartPixels(inRect.height - titleRect.height);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, title);
            text = Widgets.TextArea(textRect, text, readOnly: false);
            Text.Anchor = anchor;
        }

        private void SavePatterns()
        {
            package.SerializedData.Regex.FilePatterns = ProcessPatterns(fileText);
            package.SerializedData.Regex.DirectoryPatterns = ProcessPatterns(directoryText);

            List<string> ProcessPatterns(string input)
            {
                return input
                    .Split("\n")
                    .Except("")
                    .ToList();
            }
        }
    }
}
