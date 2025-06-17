using System.IO;

namespace PublisherPlus.Data
{
	public abstract class FileFilter
	{
		protected WorkshopPackage package;
		public FileFilter(WorkshopPackage package)
		{
			this.package = package;
		}

		public abstract string FilterReason { get; }
		public abstract bool AllowsPublishing(FileSystemInfo file);
	}
}
