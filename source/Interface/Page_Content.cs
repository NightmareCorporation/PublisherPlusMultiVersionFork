using PublisherPlus.Data;
using System.Linq;
using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
	public class Page_Content : Page
	{
		private Vector2 _scroll;
		private const float ScrollBarWidth = 20f;

		public Page_Content(WorkshopPackage package) : base(package) { }

		public override string Title => throw new System.NotImplementedException();

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.Begin(inRect);
			list.Gap();
			list.Label(Lang.Get("ContentDirectory").Bold());
			list.Label(package.SourceDirectory.FullName.Italic());
			list.GapLine();
			list.End();

			Listing_Standard filterList = new Listing_Standard();
			Rect filterRect = new Rect(inRect.x, inRect.y + list.CurHeight, inRect.width, inRect.height - list.CurHeight);

			float listingSize = Text.LineHeight + filterList.verticalSpacing;
			int listingCount = package.AllContent.Count();
			Rect filterViewRect = new Rect(0f, 0f, inRect.width - ScrollBarWidth, listingCount * listingSize);

			Widgets.BeginScrollView(filterRect, ref _scroll, filterViewRect);
			filterList.Begin(new Rect(0, _scroll.y, filterViewRect.width, filterRect.height));

			int startIndex = (int)(_scroll.y / listingSize);
			int indexRange = Mathf.Min((int)(filterRect.height / listingSize) + 1, listingCount);
			int endIndex = startIndex + indexRange;

			if(startIndex >= 0 && endIndex <= listingCount)
			{
				for(int i = startIndex; i < endIndex; i++)
				{
					System.IO.FileSystemInfo item = package.AllContent.ElementAt(i);
					string path = package.GetRelativePath(item);

					bool isIncluded = package.IsIncluded(item);
					bool include = isIncluded;
					filterList.CheckboxLabeled(item.IsDirectory() ? path.Bold() : path, ref include, item.FullName, isIncluded ? (Color?)null : Color.red);

					if(include != isIncluded)
					{
						package.SetIncluded(item, include);
					}
				}
			}

			filterList.End();
			Widgets.EndScrollView();
		}
	}
}
