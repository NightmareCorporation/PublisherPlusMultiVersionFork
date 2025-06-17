using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Verse.Steam;

namespace PublisherPlus.Data
{
	public class UploadablePackage : WorkshopUploadable
	{
		/// <summary>
		/// Used to restore original values if requested by user
		/// </summary>
		[XmlIgnore]
		public WorkshopItemHook OriginalPackageHook { get; set; }

		public void ResetToOriginalHookData()
		{
			if(OriginalPackageHook == null)
			{
				throw new InvalidOperationException("Could not reset package to original hook data because it was never set");
			}

			Title = OriginalPackageHook.Name;
			Description = OriginalPackageHook.Description;
			Tags = OriginalPackageHook.Tags?.ToList() ?? new List<string>();
			SupportedVersions = OriginalPackageHook.SupportedVersions.ToList();
			PreviewFilePath = OriginalPackageHook.PreviewImagePath;
			UploadDirectory = new DirectoryInfo(OriginalPackageHook.Directory.FullName);
		}

		#region Interface
		[XmlElement]
		public string Title { get; set; }
		public string GetWorkshopName() => Title;

		[XmlElement]
		public string Description { get; set; }
		public string GetWorkshopDescription() => Description;

		[XmlElement]
		public List<string> Tags { get; set; }
		public IList<string> GetWorkshopTags() => Tags;

		[XmlArray, XmlArrayItem(typeof(System.Version), ElementName = "Version")]
		List<System.Version> _supportedVersions { get; set; }
		[XmlIgnore]
		public IEnumerable<System.Version> SupportedVersions
		{
			get => _supportedVersions;
			set => _supportedVersions = value.ToList();
		}

		[XmlIgnore]
		public PublishedFileId_t PublishedFileId { get; set; }
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
		public void SetPublishedFileId(Steamworks.PublishedFileId_t pfid) => PublishedFileId = pfid;

		private FileInfo _previewFile;
		public FileInfo PreviewFile => _previewFile;
		[XmlElement]
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
		[XmlIgnore]
		DirectoryInfo UploadDirectory { get; set; }
		public DirectoryInfo GetWorkshopUploadDirectory() => UploadDirectory;

		public void PrepareForWorkshopUpload() { }

		public WorkshopItemHook GetWorkshopItemHook() => new WorkshopItemHook(this);
		#endregion
	}
}
