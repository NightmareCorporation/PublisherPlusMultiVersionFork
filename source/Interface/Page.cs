using PublisherPlus.Data;
using UnityEngine;

namespace PublisherPlus.Interface
{
    public abstract class Page
    {
        protected ManagedWorkshopPackage package;

        public Page(ManagedWorkshopPackage package)
        {
            this.package = package;
        }
        public abstract string Title { get; }
        public abstract void DoWindowContents(Rect inRect);
    }
}
