using PublisherPlus.Patch;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Verse;
using Verse.Steam;
using Version = System.Version;

namespace PublisherPlus.Data
{
	/// <summary>
	/// A mod folder to be uploaded to the workshop. Handles most of the logic for file IO, aggregation, and filtering.
	/// </summary>
	[XmlRootAttribute("WorkshopPackage")]
	public class ManagedWorkshopPackage : WorkshopUploadable
	{
		public static readonly string separator = Path.DirectorySeparatorChar.ToString();

		private const string TempFolderName = "PublisherPlus\\Temp";
		private const string ConfigFileName = "_PublisherPlus.xml";
		private const string PublishedFileIdFilePath = "About\\PublishedFileId.txt";

		private static readonly DirectoryInfo TempDirectory = new DirectoryInfo(Path.Combine(GenFilePaths.ConfigFolderPath, TempFolderName));

		private static ManagedWorkshopPackage _current;

		private readonly WorkshopItemHook workshopItemHook;
		private List<FileSystemInfo> allFiles = new List<FileSystemInfo>();
		public IReadOnlyCollection<FileSystemInfo> AllFiles => allFiles;

		private List<FileFilter> filters;
		public FileFilter_GitIgnore gitIgnoreFilter;
		public FileFilter_FileTreeExclusion fileTreeExclusionFilter;

		private PublishedFileId_t publishedFileId;
		public string ReadableId => publishedFileId == PublishedFileId_t.Invalid ? Language.Get("NewFileId") : publishedFileId.ToString();

		public ManagedWorkshopPackage(WorkshopItemHook hook)
		{
			workshopItemHook = hook;
			publishedFileId = hook.PublishedFileId;

			CopyMetadataFromHook();

			SetAllFiles();
			GetConfig();
			SetFilters();

			_uploadDirectory = TempDirectory.CreateSubdirectory(SourceDirectory.Name);
			if(_uploadDirectory.ExistsNow())
			{
				_uploadDirectory.Delete(true);
			}
			_uploadDirectory.Create();
		}

		#region Serialization
		[XmlAttribute("Title")]
		public string Title { get; set; }
		[XmlAttribute("Description")]
		public string Description { get; set; }
		[XmlAttribute("Tags")]
		public List<string> Tags { get; set; }
		[XmlArray("SupportedVersions"), XmlArrayItem(typeof(Version), ElementName = "Version")]
		public IEnumerable<Version> SupportedVersions { get; set; }
		[XmlAttribute("PreviewFile")]
		private FileInfo _previewFile;
		public string Preview
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

		XmlSerializer serializer = new XmlSerializer(typeof(ManagedWorkshopPackage));
		private void GetConfig()
		{
			string configFile = Path.Combine(SourceDirectory.FullName, ConfigFileName);
			if(!File.Exists(configFile))
			{
				return;
			}

			FileStream fileStream = new FileStream(configFile, FileMode.OpenOrCreate);
			ManagedWorkshopPackage package = (ManagedWorkshopPackage)serializer.Deserialize(fileStream);

		}

		private void CopyFromWorkshopPackage(ManagedWorkshopPackage other)
		{

		}

		public void SaveConfig()
		{
			string configFile = Path.Combine(SourceDirectory.FullName, ConfigFileName);

			FileStream fileStream = new FileStream(configFile, FileMode.OpenOrCreate);
			serializer.Serialize(fileStream, this);
		}

		public void ResetConfig()
		{
			CopyMetadataFromHook();
			SetAllFiles();
		}

		#endregion
		public bool PreviewExists => _previewFile.ExistsNow();
		public bool IsNewCreation => publishedFileId == PublishedFileId_t.Invalid;

		public DirectoryInfo SourceDirectory { get; private set; }

		private readonly DirectoryInfo _uploadDirectory;
		public bool useGitIgnore = false;



		private void CopyMetadataFromHook()
		{
			Title = workshopItemHook.Name;
			Description = workshopItemHook.Description;
			Tags = workshopItemHook.Tags?.ToList() ?? new List<string>();
			SupportedVersions = workshopItemHook.SupportedVersions.ToList();
			Preview = workshopItemHook.PreviewImagePath;
			SourceDirectory = new DirectoryInfo(workshopItemHook.Directory.FullName);
		}

		private void SetFilters()
		{
			gitIgnoreFilter = new FileFilter_GitIgnore(this);
			fileTreeExclusionFilter = new FileFilter_FileTreeExclusion(this);
			filters = new List<FileFilter>()
			{
				gitIgnoreFilter,
				fileTreeExclusionFilter
			};
		}

		public bool AllowsPublishing(FileSystemInfo item)
		{
			return filters.All(filter => filter.AllowsPublishing(item));
		}

		/// <summary>
		/// Gets the path relative to the root <see cref="SourceDirectory">, if the item is a directory, appends a trailing slash
		/// </summary>
		public string GetRelativePath(FileSystemInfo item)
		{
			return item.FullName.Substring(SourceDirectory.FullName.Length + 1) + (item.IsDirectory() ? separator : "");
		}

		private void SetAllFiles()
		{
			allFiles.Clear();

			allFiles = SourceDirectory.GetFileSystemInfos("*", SearchOption.AllDirectories)
				.OrderBy(item => item.FullName)
				.Where(item => item.Name != ConfigFileName)
				.ToList();
		}


		public bool HasContent() => AllFiles.Any();

		private void PrepareTempFolder()
		{
			if(!PreviewExists)
			{
				Preview = workshopItemHook.PreviewImagePath;
			}

			foreach(FileSystemInfo file in AllFiles)
			{
				if(!AllowsPublishing(file))
				{
					continue;
				}
				string path = Path.Combine(_uploadDirectory.FullName, GetRelativePath(file));

				if(file is DirectoryInfo)
				{
					new DirectoryInfo(path).Create();
				}

				if(!(file is FileInfo original))
				{
					continue;
				}

				try
				{
					FileInfo destination = new FileInfo(path);
					if(destination.Directory == null)
					{
						throw new Startup.Exception("Destination directory is null");
					}
					destination.Directory.Create();
					original.CopyTo(destination.FullName);
				}
				catch(Exception e)
				{
					string message = $"Skipping package file '{original.FullName}' due to error: {e.Message}";
					Startup.Warning(message);
				}
			}
		}

		public void Upload()
		{
			if(_current == this)
			{
				Startup.Error("This workshop package is still being uploaded");
				return;
			}
			_current = this;

			PrepareTempFolder();

			Access.Method_Verse_Steam_Workshop_Upload_Call(this);
		}

		public static void OnUploaded()
		{
			if(_current == null)
			{
				return;
			}

			Startup.Log($"Finished uploading '{_current.Title}'");

			_current = null;
			TempDirectory.Delete(true);
		}

		#region IWorkshopUploadable
		public string GetWorkshopName() => Title;
		public string GetWorkshopDescription() => Description;
		public IList<string> GetWorkshopTags() => Tags;
		public bool CanToUploadToWorkshop() => true;
		public PublishedFileId_t GetPublishedFileId() => publishedFileId;
		public void SetPublishedFileId(PublishedFileId_t pfid)
		{
			publishedFileId = pfid;
			TrackPublishedIdFile();
		}
		public WorkshopItemHook GetWorkshopItemHook() => new WorkshopItemHook(this);
		public string GetWorkshopPreviewImagePath() => Preview;
		public DirectoryInfo GetWorkshopUploadDirectory() => _uploadDirectory;
		public void PrepareForWorkshopUpload() { }

		private void TrackPublishedIdFile()
		{
			FileInfo file = new FileInfo(Path.Combine(SourceDirectory.FullName, PublishedFileIdFilePath));
			workshopItemHook.PublishedFileId = publishedFileId;

			if(allFiles.Contains(file))
			{
				return;
			}
			allFiles.Add(file);
		}
		#endregion


	}
}
