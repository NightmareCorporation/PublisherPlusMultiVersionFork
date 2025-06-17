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

		public abstract bool IsActive { get; }
		public abstract string FilterReason { get; }
		public abstract void Reset();
		public abstract bool AllowsPublishing(FileSystemInfo file);
	}
}
