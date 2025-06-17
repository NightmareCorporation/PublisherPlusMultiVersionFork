using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace PublisherPlus.Data
{
	public class FileFilter_FileTreeExclusion : IFileFilter
	{
		ManagedWorkshopPackage package;
		List<FileSystemInfo> FileExclusionPaths => package.SerializedData.FileTreeExclusions.ExcludedFiles;

		public void SetWorkshopPackage(ManagedWorkshopPackage package)
		{
			this.package = package;
		}

		public string FilterReason => "FileTreeExclusion";

		public bool AllowsPublishing(FileSystemInfo file)
		{
			return !FileExclusionPaths.Any(exclusionPath => file.FullName.StartsWith(exclusionPath.FullName));
		}
		public bool IsActive => true;

		public void Reset()
		{
			FileExclusionPaths.Clear();
		}

		public void SetExcluded(FileSystemInfo item, bool isExcluded)
		{
			if(isExcluded)
			{
				Log.Message($"Added file exclusion path: {item.FullName}");
				FileExclusionPaths.RemoveAll(path => path.FullName.StartsWith(item.FullName));
				FileExclusionPaths.Add(item);
			}
			else
			{
				Log.Message($"Removed file exclusion path: {item.FullName}");
				FileExclusionPaths.Remove(item);
			}
		}

	}
}
