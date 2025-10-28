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
            if(package.SerializedData.Regex.Patterns.NullOrEmpty())
            {
                return true;
            }

            return !package.SerializedData.Regex.Patterns
                .Any(pattern => Regex.IsMatch(file.Name, pattern));
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
