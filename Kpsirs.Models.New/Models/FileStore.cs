namespace Dt.File.Store
{
    using System.Net;
    using Dt.File.MinioConfig;
    using Dt.Kpsirs.Common.File.Dto;
    using Minio;
    using Minio.DataModel.Args;

     public class FileStore
    {
        protected string bucketName;
        protected string rootFolderName;
        private string location;

        private IMinioClient minio;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStore"/> class.
        /// </summary>
        /// <param name="minioConfig">Файл конфигурации настроек минио.</param>
        public FileStore(MinioConfiguration minioConfig)
        {
            var endpoint = minioConfig.endpoint;
            var accessKey = minioConfig.accessKey;
            var secretKey = minioConfig.secretKey;
            this.rootFolderName = minioConfig.rootFolderName;
            this.bucketName = minioConfig.bucketName;
            this.location = minioConfig.location;


            // При создании "хранилища" подключаемся к api MinIO Object Store, используя логин и пароль
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;

            // Инициализируем MinIO Client.
            this.minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objectName">Имя файла.</param>
        /// <param name="folderName">Имя папки.</param>
        /// <param name="minioFileName">Имя файла в минио.</param>
        /// <param name="filePath">Путь до файла.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<FileContentDto> LoadFile_(string objectName, string folderName, string minioFileName, string filePath)
        {
            try
            {
                byte[] buffer;

                // Проверяем, существует ли объект, используя statObject()
                // Если объект не найден, statObject() генерирует исключение
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(this.bucketName)
                                                    .WithObject(minioFileName);
                _ = await this.minio.StatObjectAsync(statObjectArgs);

                // Получаем входной поток, чтобы узнать содержимое 'minioFileName' из 'bucketName'
                using (var memoryStream = new MemoryStream())
                {
                    GetObjectArgs getObjectArgs = new GetObjectArgs()
                        .WithBucket(this.bucketName)
                        .WithObject(minioFileName)
                        .WithCallbackStream((stream) =>
                        {
                            stream.CopyTo(memoryStream);
                        });
                    await minio.GetObjectAsync(getObjectArgs);
                    buffer = memoryStream.ToArray();
                    Console.WriteLine($"Успешно скачали файл \"{minioFileName}\" из bucket: \"{bucketName}\"");
                    return new FileContentDto(filePath, buffer);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Файла с именем \"{minioFileName}\" не существует в \"{bucketName}\"" + e);
            }
        }
        public async Task CreateFile_(string objectName, string folderName, string minioFileName, string filePath, byte[] fileContent)
        {
            using (MemoryStream fileStream = new MemoryStream(fileContent))
            {
                try
                {
                    // Создаем новый bucket с именем bucketName, если он не существует
                    var bktExistArgs = new BucketExistsArgs()
                        .WithBucket(this.bucketName);
                    var found = await this.minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);
                    if (!found)
                    {
                        var mkBktArgs = new MakeBucketArgs()
                            .WithBucket(this.bucketName)
                            .WithLocation(this.location);
                        await this.minio.MakeBucketAsync(mkBktArgs).ConfigureAwait(false);
                    }

                    // загружаем файл в bucket
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(this.bucketName)
                        .WithObject(minioFileName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(fileContent.Length);
                    _ = await this.minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                    Console.WriteLine($"\nУспешно загрузили файл \"{minioFileName}\" в bucket: \"{this.bucketName}\"");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Не удалось найти файл {filePath}" + e);
                }
            }
        }
        public async void DeleteFile_(string objectName, string folderName, string minioFileName, string filePath)
        {
            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket(this.bucketName)
                    .WithObject(minioFileName);

                await this.minio.RemoveObjectAsync(args).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось удалить файл \"{minioFileName}\" из \"{this.bucketName}\"" + e);
            }
            Console.WriteLine($"Успешно удалили \"{minioFileName}\" из bucket: \"{this.bucketName}\"");

        }

        public string GetDirectory()
        {
            return this.rootFolderName;
        }

    }
}
