using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace PublisherPlus.Data
{
	[XmlRoot("Configuration")]
	public class SerializedData
	{
		[XmlElement]
		public UploadablePackage UploadablePackage = new UploadablePackage();
		[XmlElement]
		public FileFilter_FileTreeExclusionData FileTreeExclusions = new FileFilter_FileTreeExclusionData();
		[XmlElement]
		public FileFilter_GitIgnoreData GitIgnore = new FileFilter_GitIgnoreData();
	}

	public class FileFilter_FileTreeExclusionData
	{
		[XmlArray, XmlArrayItem(typeof(string), ElementName = "Path")]
		public List<string> ExcludedFilePaths
		{
			get => ExcludedFiles.Select(f => f.FullName).ToList();
			set => ExcludedFiles = value.Select(path =>
			{
				if(File.Exists(path))
				{
					return new FileInfo(path) as FileSystemInfo;
				}
				else
				{
					return new DirectoryInfo(path) as FileSystemInfo;
				}
			}).ToList();
		}
		[XmlIgnore]
		public List<FileSystemInfo> ExcludedFiles = new List<FileSystemInfo>();
	}

	public class FileFilter_GitIgnoreData
	{
		[XmlElement]
		public bool UseGitIgnore;
	}
}
