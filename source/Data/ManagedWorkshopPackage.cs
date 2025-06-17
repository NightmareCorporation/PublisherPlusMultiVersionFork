using PublisherPlus.Patch;
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
		private const string PublishedFileIdFilePath = "About\\PublishedFileId.txt";

		private static readonly DirectoryInfo TempDirectory = new DirectoryInfo(Path.Combine(GenFilePaths.ConfigFolderPath, TempFolderName));

		private static ManagedWorkshopPackage _current;
		public SerializedData SerializedData { get; set; }
		public UploadablePackage UploadablePackage => SerializedData.UploadablePackage;

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
			SourceDirectory = hook.Directory;

			LoadFromConfigFile();
			UploadablePackage.OriginalPackageHook = hook;
			UploadablePackage.ResetToOriginalHookData();

			SetAllFiles();
			SetFilters();

			_uploadDirectory = TempDirectory.CreateSubdirectory(SourceDirectory.Name);
			if(_uploadDirectory.ExistsNow())
			{
				_uploadDirectory.Delete(true);
			}
			_uploadDirectory.Create();
		}

		#region Serialization
		readonly XmlSerializer serializer = new XmlSerializer(typeof(SerializedData));
		private void LoadFromConfigFile()
		{
			string configFile = Path.Combine(SourceDirectory.FullName, ConfigFileName);
			if(!File.Exists(configFile))
			{
				SerializedData = new SerializedData();
				return;
			}

			FileStream fileStream = new FileStream(configFile, FileMode.Open);
			SerializedData = (SerializedData)serializer.Deserialize(fileStream);
		}

		public void SaveToConfigFile()
		{
			string configFile = Path.Combine(SourceDirectory.FullName, ConfigFileName);

			FileStream fileStream = new FileStream(configFile, FileMode.OpenOrCreate);
			serializer.Serialize(fileStream, SerializedData);
		}

		public void ResetConfig()
		{
			UploadablePackage.ResetToOriginalHookData();
			SetAllFiles();
		}

		#endregion
		public bool PreviewExists => UploadablePackage.PreviewFile.ExistsNow();
		public bool IsNewCreation => publishedFileId == PublishedFileId_t.Invalid;

		public DirectoryInfo SourceDirectory { get; private set; }

		private readonly DirectoryInfo _uploadDirectory;

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

		public bool AllowsPublishing(FileSystemInfo item, out string reason)
		{
			List<string> reasons = new List<string>();
			bool isPublishingAllowed = true;
			foreach(FileFilter filter in filters)
			{
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
				UploadablePackage.PreviewFilePath = workshopItemHook.PreviewImagePath;
			}

			foreach(FileSystemInfo file in AllFiles)
			{
				if(!AllowsPublishing(file, out _))
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
			TempDirectory.Delete(true);
		}

		#region IWorkshopUploadable
		public void SetPublishedFileId(PublishedFileId_t pfid)
		{
			publishedFileId = pfid;
			TrackPublishedIdFile();
		}
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
