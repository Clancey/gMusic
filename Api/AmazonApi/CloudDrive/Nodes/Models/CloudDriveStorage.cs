using System;

namespace Amazon.CloudDrive
{
	public class CloudDriveStorage
	{
		[Newtonsoft.Json.JsonProperty("s3_bucket")]
		public string Bucket { get; set; }

		[Newtonsoft.Json.JsonProperty("nonBillable")]
		public string NonBillable { get; set; }

		[Newtonsoft.Json.JsonProperty("s3_storageKey")]
		public string StorageKey { get; set; }

		[Newtonsoft.Json.JsonProperty("s3_resume_bucket")]
		public string ResumeBucket { get; set; }

		[Newtonsoft.Json.JsonProperty("s3_resume_uploadId")]
		public string ResumeUploadId { get; set; }

		[Newtonsoft.Json.JsonProperty("s3_resume_storageKey")]
		public string ResumeStorageKey { get; set; }

		public string Processing { get; set; }
	}
}