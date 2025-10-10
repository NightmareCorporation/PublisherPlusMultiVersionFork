using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using Verse.Steam;

namespace PublisherPlus.Data
{
    public class UploadablePackage : WorkshopUploadable
    {
        #region Interface
        public string Title { get; set; }
        public string GetWorkshopName() => Title;

        public string Description { get; set; }
        public string GetWorkshopDescription() => Description;

        public List<string> Tags { get; set; }
        public IList<string> GetWorkshopTags() => Tags;

        List<System.Version> _supportedVersions { get; set; }
        public IEnumerable<System.Version> SupportedVersions
        {
            get => _supportedVersions;
            set => _supportedVersions = value.ToList();
        }

        public PublishedFileId_t PublishedFileId { get; set; }
        public void SetPublishedFileId(Steamworks.PublishedFileId_t pfid)
        {
            PublishedFileId = pfid;
            Log.Message($"called setter");
        }
        public PublishedFileId_t GetPublishedFileId() => PublishedFileId;
        public string ReadablePublishedFileId
        {
            get
            {
                if(PublishedFileId == PublishedFileId_t.Invalid)
                {
                    return Language.Get("NewFileId");
                }
                return PublishedFileId.ToString();
            }
        }

        private FileInfo _previewFile;
        public FileInfo PreviewFile => _previewFile;
        public string PreviewFilePath
        {
            get => _previewFile.FullName;
            set
            {
                if(_previewFile?.FullName == value)
                {
                    return;
                }
                _previewFile = new FileInfo(value);
            }
        }
        public string GetWorkshopPreviewImagePath() => PreviewFilePath;

        public bool CanToUploadToWorkshop() => true;
        public DirectoryInfo UploadDirectory { get; set; }
        public DirectoryInfo GetWorkshopUploadDirectory() => UploadDirectory;


        public UploadablePackage(ManagedWorkshopPackage managedPackage, DirectoryInfo targetDirectory)
        {
            Title = managedPackage.metaData.Name;
            Description = managedPackage.metaData.Description;
            Tags = managedPackage.metaData.GetWorkshopTags().ToList();
            SupportedVersions = managedPackage.metaData.SupportedVersionsReadOnly;
            PublishedFileId = managedPackage.metaData.GetPublishedFileId();
            PreviewFilePath = managedPackage.metaData.PreviewImagePath;

            CreateUploadableMirror(managedPackage.fileTreeFilter.root);
            UploadDirectory = targetDirectory;

            void CreateUploadableMirror(FileTreeNode node)
            {
                if(!managedPackage.AllowsPublishing(node.entryInfo, out string reason))
                {
                    Log.Message($"Skipping entry {node.entryInfo}: {reason}");
                    return;
                }
                string targetPath = Path.Combine(targetDirectory.FullName, Utility.GetRelativePathTo(node.entryInfo, managedPackage.ModRootDirectory));
                if(node.entryInfo is DirectoryInfo)
                {
                    Directory.CreateDirectory(targetPath);
                }
                else if(node.entryInfo is FileInfo file)
                {
                    FileInfo createdFile = file.CopyTo(targetPath);
                }

                foreach(FileTreeNode child in node.children)
                {
                    CreateUploadableMirror(child);
                }
            }
        }
        public void PrepareForWorkshopUpload() { }

        public WorkshopItemHook GetWorkshopItemHook() => new WorkshopItemHook(this);
        #endregion
    }
}
