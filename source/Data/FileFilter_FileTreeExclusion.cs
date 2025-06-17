using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace PublisherPlus.Data
{
	public class FileFilter_FileTreeExclusion : FileFilter
	{
		List<FileSystemInfo> FileExclusionPaths => package.SerializedData.FileTreeExclusions.ExcludedFiles;
		public FileFilter_FileTreeExclusion(ManagedWorkshopPackage package) : base(package) { }

		public override string FilterReason => "FileTreeExclusion";

		public override bool AllowsPublishing(FileSystemInfo file)
		{
			return !FileExclusionPaths.Any(exclusionPath => file.FullName.StartsWith(exclusionPath.FullName));
		}
		public override bool IsActive => true;

		public override void Reset()
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
