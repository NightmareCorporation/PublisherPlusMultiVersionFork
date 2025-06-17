using System.IO;

namespace PublisherPlus.Data
{
	public interface IFileFilter
	{
		void SetWorkshopPackage(ManagedWorkshopPackage package);
		bool IsActive { get; }
		string FilterReason { get; }
		void Reset();
		bool AllowsPublishing(FileSystemInfo file);
	}
}
