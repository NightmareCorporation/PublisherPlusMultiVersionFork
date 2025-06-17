using System.Collections.Generic;
using System.IO;

namespace PublisherPlus.Data
{
	public class FileFilter_FileTreeExclusion : FileFilter
	{
		List<FileSystemInfo> fileExclusionPaths = new List<FileSystemInfo>();
		public FileFilter_FileTreeExclusion(WorkshopPackage package) : base(package) { }

		public override string FilterReason => "FileTreeExclusion";

		public override bool AllowsPublishing(FileSystemInfo file)
		{
			return fileExclusionPaths.Contains(file);
		}

		public void SetExcluded(FileSystemInfo item, bool isExcluded)
		{
			if(isExcluded)
			{
				fileExclusionPaths.Add(item);
				fileExclusionPaths.RemoveAll(path => path.FullName.StartsWith(item.FullName));
			}
			else
			{
				fileExclusionPaths.Remove(item);
			}
		}
	}
}
