using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace PublisherPlus.Data
{
    public class FileFilter_Regex : FileFilter
    {
        public FileFilter_Regex(ManagedWorkshopPackage package) : base(package) { }

        public override string FilterReason => "Regex";
        public override bool IsActive => package.SerializedData.Regex.UseRegex;

        public override bool AllowsPublishing(FileSystemInfo file)
        {
            List<string> patterns = new List<string>();

            if(file is FileInfo)
            {
                patterns = package.SerializedData.Regex.FilePatterns;
            }
            else if(file is DirectoryInfo)
            {
                patterns = package.SerializedData.Regex.DirectoryPatterns;
            }
            else
            {
                Log.Warning($"Unhandled edge case: FileSystemInfo is not file or directory: {file}");
            }

            if(patterns.NullOrEmpty())
            {
                return true;
            }

            return !patterns
                .Any(pattern => Regex.IsMatch(file.Name, pattern))
                && IsParentAllowed(file);
        }

        private bool IsParentAllowed(FileSystemInfo child)
        {
            FileSystemInfo parent = null;
            if(child is FileInfo file)
            {
                parent = file.Directory;
            }
            else if(child is DirectoryInfo directory)
            {
                parent = directory.Parent;
            }
            if(parent == null)
            {
                return true;
            }
            return AllowsPublishing(parent);
        }

        public override void Reset()
        {
            package.SerializedData.Regex.UseRegex = true;
        }

        public override void FinishLoading()
        {
            base.FinishLoading();
            if(!package.SerializedData.Regex.HasInitializedDefaultValues)
            {
                package.SerializedData.Regex.SetDefaultValues();
            }
        }
    }
}
