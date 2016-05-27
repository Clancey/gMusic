using System;
using System.IO;
using System.Collections.Generic;
namespace OneDrive
{
	public class OneDriveItem
	{
		
		public Stream Content { get; set; }

		public OneDriveIdentitySet CreatedBy { get; set; }

		public DateTimeOffset? CreatedDateTime { get; set; }

		public string CTag { get; set; }

		public string Description { get; set; }

		public string ETag { get; set; }

		public string Id { get; set; }

		public OneDriveIdentitySet LastModifiedBy { get; set; }

		public DateTimeOffset? LastModifiedDateTime { get; set; }

		public string Name { get; set; }

		//public OneDriveItemReference ParentReference { get; set; }

		public Int64? Size { get; set; }

		public string WebUrl { get; set; }

		public OneDriveAudio Audio { get; set; }

		//public OneDriveDeleted Deleted { get; set; }

		public OneDriveFile File { get; set; }

		//public OneDriveFileSystemInfo FileSystemInfo { get; set; }

		public OneDriveFolder Folder { get; set; }

		public OneDriveImage Image { get; set; }

		//public OneDriveLocation Location { get; set; }

		//public OpenWithSet OpenWith { get; set; }

		//public OneDrivePhoto Photo { get; set; }

		//public SearchResult SearchResult { get; set; }

		public OneDriveSpecialFolder SpecialFolder { get; set; }

		public OneDriveVideo Video { get; set; }

		//public IPermissionsCollectionPage Permissions { get; set; }

		//public IVersionsCollectionPage Versions { get; set; }

		//public IChildrenCollectionPage Children { get; set; }

		//public IThumbnailsCollectionPage Thumbnails { get; set; }

		public Dictionary<string, object> AdditionalData { get; set; }

	}
}

