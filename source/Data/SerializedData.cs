using System.Collections.Generic;
using System.Xml.Serialization;

namespace PublisherPlus.Data
{
    [XmlRoot("Configuration")]
    public class SerializedData
    {
        [XmlElement]
        public FileFilter_FileTreeData FileTree = new FileFilter_FileTreeData();
        [XmlElement]
        public FileFilter_GitIgnoreData GitIgnore = new FileFilter_GitIgnoreData();
        [XmlElement]
        public FileFilter_RegexData Regex = new FileFilter_RegexData();

        public void FinishLoading(ManagedWorkshopPackage package)
        {
            package.fileTreeFilter.ExcludedPaths = FileTree.ExcludedFilePaths;
            if(!Regex.HasInitializedDefaultValues)
            {
                Regex.SetDefaultValues();
            }
        }
    }

    public class FileFilter_FileTreeData
    {
        [XmlArray, XmlArrayItem(typeof(string), ElementName = "Path")]
        public HashSet<string> ExcludedFilePaths { get; set; }
    }

    public class FileFilter_GitIgnoreData
    {
        [XmlElement]
        public bool UseGitIgnore;
    }

    public class FileFilter_RegexData
    {
        [XmlElement]
        public bool UseRegex = true;
        [XmlArray, XmlArrayItem(typeof(string), ElementName = "Pattern")]
        public List<string> Patterns = new List<string>();
        [XmlElement]
        public bool HasInitializedDefaultValues = false;

        public void SetDefaultValues()
        {
            HasInitializedDefaultValues = true;
            Patterns = new List<string>()
            {
                "\\.gitignore",
                "_PublisherPlusV2.xml",
                "_PublisherPlus.xml",
            };
        }
    }
}
