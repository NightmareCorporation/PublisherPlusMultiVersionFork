using System.Collections.Generic;
using System.IO;
using Verse;

namespace PublisherPlus.Data
{
    public class FileTree
    {
        ManagedWorkshopPackage package;
        /// <summary>
        /// based on FileSystemInfo.FullName
        /// </summary>
        Dictionary<string, FileTreeNode> fileNamesWithTreeNodes = new Dictionary<string, FileTreeNode>();

        FileTreeNode _root;
        public FileTreeNode Root => _root;

        List<FileInfo> allFiles = new List<FileInfo>();
        public IReadOnlyList<FileInfo> AllFiles => allFiles;

        public FileTree(ManagedWorkshopPackage package)
        {
            this.package = package;
        }

        public void InitFileTree()
        {
            if(_root != null)
            {
                return;
            }
            _root = new FileTreeNode(package.ModRootDirectory, null, package);
        }

        public void Notify_NodeAdded(FileSystemInfo fileInfo, FileTreeNode node)
        {
            fileNamesWithTreeNodes.SetOrAdd(fileInfo.FullName, node);
        }

        public FileTreeNode NodeForEntry(FileSystemInfo info)
        {
            return fileNamesWithTreeNodes.TryGetValue(info.FullName);
        }

        public void Reset()
        {
            _root = new FileTreeNode(package.ModRootDirectory, null, package);
        }
    }
}
