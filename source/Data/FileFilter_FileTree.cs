using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PublisherPlus.Data
{
    public class FileFilter_FileTree : IFileFilter
    {
        ManagedWorkshopPackage package;
        public void SetWorkshopPackage(ManagedWorkshopPackage package)
        {
            this.package = package;
        }
        public string FilterReason => "FileTreeExclusion";

        public bool AllowsPublishing(FileSystemInfo file)
        {
            return fileInfoToTreeLookup[file].IsIncluded;
        }

        public void Reset()
        {

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

    }
}
