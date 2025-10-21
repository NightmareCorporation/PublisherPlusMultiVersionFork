using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace PublisherPlus.Data
{
    public class FileTree
    {
        FileTreeNode root;
        ManagedWorkshopPackage package;
        Dictionary<FileSystemInfo, FileTreeNode> fileInfoToTreeLookup = new Dictionary<FileSystemInfo, FileTreeNode>();

        public FileTreeNode Root => root;

        List<FileInfo> allFiles = new List<FileInfo>();
        public IReadOnlyList<FileInfo> AllFiles => allFiles;

        public FileTree(ManagedWorkshopPackage package)
        {
            this.package = package;
            root = new FileTreeNode(package.ModRootDirectory, null, package);
        }

        public void Notify_NodeAdded(FileSystemInfo fileInfo, FileTreeNode node)
        {
            fileInfoToTreeLookup.SetOrAdd(fileInfo, node);
        }

        public void RefetchFiles()
        {
            HashSet<string> previousList = root.GetExcludedPaths()
                .Select(p => p.FullName)
                .ToHashSet();
            root = new FileTreeNode(package.ModRootDirectory, null, package);
            root.ApplyExcludedPaths(previousList);
        }

        public void Reset()
        {
            root = new FileTreeNode(package.ModRootDirectory, null, package);
            root.ApplyExcludedPaths(package.SerializedData.FileTree.ExcludedFilePaths);
        }
    }
}
