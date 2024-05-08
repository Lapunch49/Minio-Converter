namespace Dt.Kpsirs.Common.File.Files
{
    using System;
    using System.Threading.Tasks;
    using Dt.File.MinioConfig;
    using Dt.File.Store;
    using Dt.Kpsirs.Common.File.Dto;

    public class FileStoreKpsirs : FileStore, IFileStore
    {
        public FileStoreKpsirs(MinioConfiguration minioConfig) : base(minioConfig)
        {
        }
        /// <inheritdoc/>
        public Task<FileContentDto> LoadFile(Guid fileId, Guid drillingProjectId, string fileName, FileType fileType)
        {
            var objectName = $"{fileId}.{fileName}";
            var folderName = drillingProjectId.ToString();
            var minioFileName = $"{folderName}\\{fileType}\\{objectName}";
            var filePath = $"{this.rootFolderName}\\{minioFileName}";

            Task<FileContentDto> task = LoadFile_(objectName, folderName, minioFileName, filePath);
            return Task.FromResult(task.Result);
        }
        public async Task CreateFile(Guid fileId, Guid drillingProjectId, byte[] fileContent, string fileName, FileType fileType)
        {
            var objectName = $"{fileId}.{fileName}";
            var folderName = drillingProjectId.ToString();
            var minioFileName = $"{folderName}\\{fileType}\\{objectName}";
            var filePath = $"{this.rootFolderName}\\{minioFileName}";

            CreateFile_(objectName, folderName, minioFileName, filePath, fileContent);
          
        }
        public async void DeleteFile(Guid fileId, Guid drillingProjectId, string fileName, FileType fileType)
        {
            var objectName = $"{fileId}.{fileName}";
            var folderName = drillingProjectId.ToString();
            var minioFileName = $"{folderName}\\{fileType}\\{objectName}";
            var filePath = $"{this.rootFolderName}\\{minioFileName}";

            DeleteFile_(objectName, folderName, minioFileName, filePath);
        }
    }
}