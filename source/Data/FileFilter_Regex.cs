using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace PublisherPlus.Data
{
	public class FileFilter_Regex : IFileFilter
	{
		ManagedWorkshopPackage package;

		public void SetWorkshopPackage(ManagedWorkshopPackage package)
		{
			this.package = package;
		}

		public string FilterReason => "Regex";

		public bool AllowsPublishing(FileSystemInfo file)
		{
			if(package.SerializedData.Regex.Patterns.NullOrEmpty())
			{
				return true;
			}

			return !package.SerializedData.Regex.Patterns
				.Any(pattern => Regex.IsMatch(file.Name, pattern));
		}

		public bool IsActive => package.SerializedData.Regex.UseRegex;

		public void Reset()
		{
			package.SerializedData.Regex.UseRegex = true;
		}
	}
}
