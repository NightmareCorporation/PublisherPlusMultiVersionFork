using PublisherPlus.Data;
using UnityEngine;

namespace PublisherPlus.Interface
{
	public abstract class Page
	{
		protected WorkshopPackage package;

		public Page(WorkshopPackage package)
		{
			this.package = package;
		}
		public abstract string Title { get; }
		public abstract void DoWindowContents(Rect inRect);
	}
}
