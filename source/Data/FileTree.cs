using System.Collections.Generic;
using System.IO;
using Verse;

namespace PublisherPlus.Data
{
    public class FileTree
    {
        ManagedWorkshopPackage package;
        Dictionary<FileSystemInfo, FileTreeNode> fileInfoToTreeLookup = new Dictionary<FileSystemInfo, FileTreeNode>();

        FileTreeNode _root;
        public FileTreeNode Root
        {
            get
            {
                if(_root == null)
                {
                    _root = new FileTreeNode(package.ModRootDirectory, null, package);
                }
                return _root;
            }
        }

        List<FileInfo> allFiles = new List<FileInfo>();
        public IReadOnlyList<FileInfo> AllFiles => allFiles;

        public FileTree(ManagedWorkshopPackage package)
        {
            this.package = package;
        }

        public void Notify_NodeAdded(FileSystemInfo fileInfo, FileTreeNode node)
        {
            fileInfoToTreeLookup.SetOrAdd(fileInfo, node);
        }

        public FileTreeNode NodeForEntry(FileSystemInfo info)
        {
            return fileInfoToTreeLookup[info];
        }

        public void Reset()
        {
            _root = new FileTreeNode(package.ModRootDirectory, null, package);
        }
    }
}
