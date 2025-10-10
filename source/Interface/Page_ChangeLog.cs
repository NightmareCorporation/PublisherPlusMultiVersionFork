using PublisherPlus.Data;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
    public class Page_ChangeLog : Page
    {
        public Page_ChangeLog(ManagedWorkshopPackage package) : base(package) { }

        public override string Title => Language.Get("Title.ChangeLog");

        public override void DoWindowContents(Rect inRect)
        {
            RectDivider divider = new RectDivider(inRect, this.GetHashCode());
            RectDivider labelRect = divider.NewRow(Text.LineHeight);
            Widgets.Label(labelRect, Language.Get("ChangeLog.Info"));

            package.ChangeLog = Widgets.TextArea(divider, package.ChangeLog);
        }
    }
}
