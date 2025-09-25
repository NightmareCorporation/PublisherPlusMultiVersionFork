using PublisherPlus.Data;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    public class ControlRibbon
    {
        ManagedWorkshopPackage package;
        public ControlRibbon(ManagedWorkshopPackage package)
        {
            this.package = package;
        }

        RectDivider divider;
        public void Draw(Rect inRect)
        {
            divider = new RectDivider(inRect, this.GetHashCode());
            DoGitIgnoreControls();
            DoRegexControls();
        }

        private void DoGitIgnoreControls()
        {
            DoUsageToggle();
            if(package.SerializedData.GitIgnore.UseGitIgnore)
            {
                DoInfo();
            }

            void DoInfo()
            {
                Rect infoIconRect = divider.NewCol(Text.LineHeight, HorizontalJustification.Right);
                Widgets.DrawTextureFitted(infoIconRect, TexButton.Info, 1);
                string gitIgnoreInfoText = package.gitIgnoreFilter.GitIgnoreInfoText;
                TooltipHandler.TipRegion(infoIconRect, gitIgnoreInfoText);
            }

            void DoUsageToggle()
            {
                string label = Language.Get("GitIgnore.UseGitIgnore");
                const float checkboxMarginWidth = 30f;
                float width = Text.CalcSize(label).x + checkboxMarginWidth;
                Rect rect = divider.NewCol(width, HorizontalJustification.Right);
                bool previousValue = package.SerializedData.GitIgnore.UseGitIgnore;
                Widgets.CheckboxLabeled(rect, label, ref package.SerializedData.GitIgnore.UseGitIgnore);
                if(previousValue == false && package.SerializedData.GitIgnore.UseGitIgnore)
                {
                    package.gitIgnoreFilter.ParseGitIgnore();
                }
            }
        }

        private void DoRegexControls()
        {
            DoRegexToggle();
            if(package.SerializedData.Regex.UseRegex)
            {
                DoRegexMenuOpenButton();
            }

            void DoRegexToggle()
            {
                string label = Language.Get("Regex.UseRegexFilter");
                const float checkboxMarginWidth = 30f;
                float width = Text.CalcSize(label).x + checkboxMarginWidth;
                Rect rect = divider.NewCol(width, HorizontalJustification.Left);
                bool previousValue = package.SerializedData.Regex.UseRegex;
                Widgets.CheckboxLabeled(rect, label, ref package.SerializedData.Regex.UseRegex);
            }

            void DoRegexMenuOpenButton()
            {
                const float buttonMargin = 10f;
                string label = Language.Get("Regex.OpenRegexWindow");
                float width = Text.CalcSize(label).x + buttonMargin;
                Rect rect = divider.NewCol(width, HorizontalJustification.Left);
                if(Widgets.ButtonText(rect, label))
                {
                    new Window_RegexFilters(package);
                }
            }
        }
    }
}
