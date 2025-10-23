using PublisherPlus.Patch;
using PublisherPlus.Settings;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Verse;
using Verse.Steam;

namespace PublisherPlus.Data
{
    public class ManagedWorkshopPackage
    {
        public SerializedData SerializedData { get; set; }

        public readonly ModMetaData metaData;
        private readonly WorkshopItemHook workshopItemHook;

        public FileTree fileTree;

        private List<FileFilter> filters;
        public FileFilter_GitIgnore gitIgnoreFilter;
        public FileFilter_FileTree fileTreeFilter;
        public FileFilter_Regex regexFilter;

        private static ManagedWorkshopPackage _current;
        private string _currentCommitHash;
        private string _changeLog;

        public static ManagedWorkshopPackage Current => _current;
        public string HumanReadablePackageId => metaData.GetPublishedFileId() == PublishedFileId_t.Invalid ? "-" : metaData.GetPublishedFileId().ToString();

        public string CurrentCommitHash
        {
            get => _currentCommitHash;
            set => _currentCommitHash = value;
        }
        public string ChangeLog
        {
            get => _changeLog;
            set => _changeLog = value;
        }

        public DirectoryInfo ModRootDirectory { get; private set; }

        #region Serialization
        private const string configFileName = "_PublisherPlusV2.xml";
        readonly XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));
        private void LoadFromConfigFile()
        {
            string configFile = Path.Combine(ModRootDirectory.FullName, configFileName);
            if(!File.Exists(configFile))
            {
                SerializedData = new SerializedData();
                return;
            }

            FileStream fileStream = new FileStream(configFile, FileMode.Open);
            SerializedData = (SerializedData)serializer.Deserialize(fileStream);
            SerializedData.FinishLoading(this);
        }

        public void SaveToConfigFile()
        {
            string configFile = Path.Combine(ModRootDirectory.FullName, configFileName);

            SerializedData.StartSaving(this);

            FileStream fileStream = new FileStream(configFile, FileMode.Create);    // using Create overwrites already existing content in the file. CreateOrOpen can lead to "trailing" old data at the end of the newly written data
            serializer.Serialize(fileStream, SerializedData);
        }

        public void ResetConfig()
        {
            fileTree.Reset();
            filters.ForEach(filter => filter.Reset());
        }
        #endregion

        public ManagedWorkshopPackage(ModMetaData metaData)
        {
            this.metaData = metaData;

            WorkshopItemHook hook = metaData.GetWorkshopItemHook();
            workshopItemHook = hook;
            ModRootDirectory = hook.Directory;

            InitiateFileTree();
            SetFilters();

            LoadFromConfigFile();
        }

        private void InitiateFileTree()
        {
            fileTree = new FileTree(this);
        }

        private void SetFilters()
        {
            fileTreeFilter = new FileFilter_FileTree(this);
            gitIgnoreFilter = new FileFilter_GitIgnore(this);
            regexFilter = new FileFilter_Regex(this);

            filters = new List<FileFilter>()
            {
                fileTreeFilter,
                gitIgnoreFilter,
                regexFilter,
            };
        }

        public bool AllowsPublishing(FileSystemInfo item, out string reason)
        {
            List<string> reasons = new List<string>();
            bool isPublishingAllowed = true;
            foreach(FileFilter filter in filters)
            {
                if(!filter.IsActive)
                {
                    continue;
                }
                if(!filter.AllowsPublishing(item))
                {
                    isPublishingAllowed = false;
                    reasons.Add(filter.FilterReason);
                }
            }
            reason = reasons.Any() ? String.Join(", ", reasons) : null;
            return isPublishingAllowed;
        }

        /// <summary>
        /// I should be doing try-catching in this section, but I would not know what the expected behavior would be if a specific file creation 
        /// failed. Ultimately the file upload should be cancelled. It makes more sense to me to throw whatever exception and then let the user 
        /// figure it out rather than trying to catch every possible issue
        /// </summary>
        private UploadablePackage PrepareUploadPackage()
        {
            DirectoryInfo uploadDirectory = new DirectoryInfo(PublisherPlusSettings.TempFolderPath).CreateSubdirectory(metaData.Name);

            if(uploadDirectory.ExistsNow())
            {
                uploadDirectory.Delete(true);
            }
            uploadDirectory.Create();

            return new UploadablePackage(this, uploadDirectory);

        }

        public void UploadToWorkshop()
        {
            if(_current == this)
            {
                Startup.Error("This workshop package is still being uploaded");
                return;
            }
            _current = this;

            UploadablePackage package = PrepareUploadPackage();

            Access.Method_Verse_Steam_Workshop_Upload_Call(package);
        }

        public static void OnUploaded(string fileId)
        {
            if(_current == null)
            {
                return;
            }

            _current.EnsurePublishedFileId(fileId);
            _current.SetLastPublishedCommit();

            Startup.Log($"Finished uploading '{_current.metaData.Name}'");

            _current = null;
        }

        private void EnsurePublishedFileId(string fileId)
        {
            const string fileName = "PublishedFileId.txt";
            const string aboutFolderName = "About";
            string filePath = Path.Combine(metaData.RootDir.FullName, aboutFolderName, fileName);
            if(File.Exists(filePath))
            {
                Log.Message($"file id already exists");
                return;
            }
            File.WriteAllText(filePath, fileId);
        }

        private void SetLastPublishedCommit()
        {
            SerializedData.lastPublishedCommit = CurrentCommitHash;
            SaveToConfigFile();
        }
    }
}
