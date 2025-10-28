using System.IO;

namespace PublisherPlus.Data
{
    public abstract class FileFilter
    {
        protected ManagedWorkshopPackage package;
        public FileFilter(ManagedWorkshopPackage package)
        {
            this.package = package;
        }
        virtual public bool IsActive { get; }
        abstract public string FilterReason { get; }
        abstract public bool AllowsPublishing(FileSystemInfo file);

        virtual public void Reset() { }

        virtual public void StartSaving() { }
        virtual public void FinishLoading() { }
    }
}
