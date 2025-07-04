using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PublisherPlus.Data
{
	public class FileFilter_FileTree : IFileFilter
	{
		ManagedWorkshopPackage package;
		public FileTreeNode root;
		public Dictionary<FileSystemInfo, FileTreeNode> fileInfoToTreeLookup = new Dictionary<FileSystemInfo, FileTreeNode>();

		public void SetWorkshopPackage(ManagedWorkshopPackage package)
		{
			this.package = package;
			root = new FileTreeNode(package.ModRootDirectory, null, package);
		}
		public string FilterReason => "FileTreeExclusion";

		public bool AllowsPublishing(FileSystemInfo file)
		{
			return fileInfoToTreeLookup[file].IsIncluded;
		}
		public bool IsActive => true;
		public HashSet<string> ExcludedPaths
		{
			get
			{
				return root.GetExcludedPaths()
					.Select(p => p.FullName)
					.ToHashSet();
			}
			set
			{
				Reset();
				root.ApplyExcludedPaths(value);
			}
		}

		public void Reset()
		{
			root = new FileTreeNode(package.ModRootDirectory, null, package);
			root.ApplyExcludedPaths(package.SerializedData.FileTree.ExcludedFilePaths);
		}

		public void RefetchFiles()
		{
			HashSet<string> previousList = root.GetExcludedPaths()
				.Select(p => p.FullName)
				.ToHashSet();
			root = new FileTreeNode(package.ModRootDirectory, null, package);
			root.ApplyExcludedPaths(previousList);

		}
	}
}
