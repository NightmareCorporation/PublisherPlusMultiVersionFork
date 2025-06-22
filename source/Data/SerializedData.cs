using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Verse;

namespace PublisherPlus.Data
{
	[XmlRoot("Configuration")]
	public class SerializedData
	{
		[XmlElement]
		public UploadablePackage UploadablePackage = new UploadablePackage();
		[XmlElement]
		public FileFilter_FileTreeData FileTree = new FileFilter_FileTreeData();
		[XmlElement]
		public FileFilter_GitIgnoreData GitIgnore = new FileFilter_GitIgnoreData();
	}

	public class FileFilter_FileTreeData
	{
		[XmlArray, XmlArrayItem(typeof(string), ElementName = "Path")]
		public HashSet<string> ExcludedFilePaths { get; set; }

		private FileSystemInfo ToInfo(string path)
		{
			if(File.Exists(path))
			{
				return new FileInfo(path);
			}
			else
			{
				return new DirectoryInfo(path);
			}
		}
	}

	public class FileFilter_GitIgnoreData
	{
		[XmlElement]
		public bool UseGitIgnore;
	}
}
