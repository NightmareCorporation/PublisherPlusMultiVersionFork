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
    /// <summary>
    /// A mod folder to be uploaded to the workshop. Handles most of the logic for file IO, aggregation, and filtering.
    /// </summary>
    public class ManagedWorkshopPackage
    {
        public static readonly string separator = Path.DirectorySeparatorChar.ToString();

        private const string TempFolderName = "PublisherPlus\\Temp";
        private const string ConfigFileName = "_PublisherPlusV2.xml";

        private static ManagedWorkshopPackage _current;
        public SerializedData SerializedData { get; set; }
        public UploadablePackage UploadablePackage => SerializedData.UploadablePackage;
        public bool IsNewCreation => SerializedData.UploadablePackage.PublishedFileId == PublishedFileId_t.Invalid;

        private readonly WorkshopItemHook workshopItemHook;

        private List<IFileFilter> filters;
        public FileFilter_GitIgnore gitIgnoreFilter;
        public FileFilter_FileTree fileTreeFilter;
        public FileFilter_Regex regexFilter;

        public ManagedWorkshopPackage(WorkshopItemHook hook)
        {
            workshopItemHook = hook;
            ModRootDirectory = hook.Directory;

            SetFilters();

            LoadFromConfigFile();
            UploadablePackage.OriginalPackageHook = hook;
            UploadablePackage.ResetToOriginalHookData();
        }

        #region Serialization
        readonly XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));
        private void LoadFromConfigFile()
        {
            string configFile = Path.Combine(ModRootDirectory.FullName, ConfigFileName);
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
            string configFile = Path.Combine(ModRootDirectory.FullName, ConfigFileName);

            SerializedData.FileTree.ExcludedFilePaths = fileTreeFilter.ExcludedPaths;
            FileStream fileStream = new FileStream(configFile, FileMode.Create);    // using Create overwrites already existing content in the file. CreateOrOpen can lead to "trailing" old data at the end of the newly written data
            serializer.Serialize(fileStream, SerializedData);
        }

        public void ResetConfig()
        {
            UploadablePackage.ResetToOriginalHookData();
            RefetchFiles();
            filters.ForEach(filter => filter.Reset());
        }
        #endregion

        public bool PreviewExists => UploadablePackage.PreviewFile.ExistsNow();
        public IEnumerable<FileInfo> AllFiles => UploadableFiles;

        public DirectoryInfo ModRootDirectory { get; private set; }

        private void SetFilters()
        {
            // file tree needs to init and set package first, as it provides the file list used by other filters
            fileTreeFilter = new FileFilter_FileTree();
            gitIgnoreFilter = new FileFilter_GitIgnore();
            regexFilter = new FileFilter_Regex();

            filters = new List<IFileFilter>()
            {
                fileTreeFilter,
                gitIgnoreFilter,
                regexFilter,
            };
            filters.ForEach(filter => filter.SetWorkshopPackage(this));
        }

        public bool AllowsPublishing(FileSystemInfo item, out string reason)
        {
            List<string> reasons = new List<string>();
            bool isPublishingAllowed = true;
            foreach(IFileFilter filter in filters)
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

        public void RefetchFiles()
        {
            fileTreeFilter.RefetchFiles();
        }

        List<FileInfo> UploadableFiles = new List<FileInfo>();
        /// <summary>
        /// I should be doing try-catching in this section, but I would not know what the expected behavior would be if a specific file creation 
        /// failed. Ultimately the file upload should be cancelled. It makes more sense to me to throw whatever exception and then let the user 
        /// figure it out rather than trying to catch every possible issue
        /// </summary>
        private void PrepareTempFolder()
        {
            if(!PreviewExists)
            {
                UploadablePackage.PreviewFilePath = workshopItemHook.PreviewImagePath;
            }
            UploadableFiles.Clear();

            DirectoryInfo uploadDirectory = new DirectoryInfo(PublisherPlusSettings.TempFolderPath).CreateSubdirectory(ModRootDirectory.Name);
            if(uploadDirectory.ExistsNow())
            {
                uploadDirectory.Delete(true);
            }
            uploadDirectory.Create();

            Log.Message($"processing {fileTreeFilter.root.FilesInThisNode.Count()} entries");
            ProcessNode(fileTreeFilter.root);

            void ProcessNode(FileTreeNode node)
            {
                if(!AllowsPublishing(node.entryInfo, out string reason))
                {
                    Log.Message($"Skipping entry {node.entryInfo}: {reason}");
                    return;
                }
                string targetPath = Path.Combine(uploadDirectory.FullName, Utility.GetRelativePathTo(node.entryInfo, ModRootDirectory));
                if(node.entryInfo is DirectoryInfo)
                {
                    Directory.CreateDirectory(targetPath);
                }
                else if(node.entryInfo is FileInfo file)
                {
                    FileInfo createdFile = file.CopyTo(targetPath);
                    UploadableFiles.Add(createdFile);
                }

                foreach(FileTreeNode child in node.children)
                {
                    ProcessNode(child);
                }
            }
        }

        public void UploadToWorkshop()
        {
            if(_current == this)
            {
                Startup.Error("This workshop package is still being uploaded");
                return;
            }
            _current = this;

            PrepareTempFolder();

            Access.Method_Verse_Steam_Workshop_Upload_Call(UploadablePackage);
        }

        public static void OnUploaded()
        {
            if(_current == null)
            {
                return;
            }

            Startup.Log($"Finished uploading '{_current.UploadablePackage.Title}'");

            _current = null;
        }
    }
}
