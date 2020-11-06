using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FlyJetsV2.Services
{
    public class StorageManager
    {
        private IConfiguration _config;

        public StorageManager(IConfiguration config)
        {
            _config = config;
        }

        public string UploadImage(string containerName, byte[] content, string extention, bool createThumbnail, string fileName = null)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());

            var imageFileName = (String.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName) + (extention.StartsWith(".") ? extention : "." + extention);

            if (content != null && content.Length != 0)
            {
                MemoryStream ms = new MemoryStream(content);

                if (!createThumbnail)
                {
                    container.CreateIfNotExists();

                    CloudBlockBlob imageBlob = container.GetBlockBlobReference(imageFileName);

                    imageBlob.UploadFromStream(ms);
                }
                else
                {
                    string imageFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config["TempFilesFolder"], imageFileName);

                    if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config["TempFilesFolder"])))
                    {
                        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config["TempFilesFolder"]));
                    }

                    Image imageToSave = Image.FromStream(ms);
                    imageToSave.Save(imageFullPath);

                    string thumbnailImageFileName = ImageHelper.GetThumbNailImage(imageFullPath);
                    string thumbnailFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config["TempFilesFolder"], thumbnailImageFileName);

                    container.CreateIfNotExists();

                    if (System.IO.File.Exists(imageFullPath))
                    {
                        CloudBlockBlob imageBlob = container.GetBlockBlobReference(imageFileName);

                        using (var fileStream1 = System.IO.File.OpenRead(imageFullPath))
                        {
                            imageBlob.UploadFromStream(fileStream1);
                        }

                        CloudBlockBlob thumbnailImageBlob = container.GetBlockBlobReference(thumbnailImageFileName);

                        using (var fileStream2 = System.IO.File.OpenRead(thumbnailFullPath))
                        {
                            thumbnailImageBlob.UploadFromStream(fileStream2);
                        }

                    }
                }

                return imageFileName;
            }

            return string.Empty;
        }

        public string UploadFile(string containerName, byte[] content, string extention, string fileName = null)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config["StorageConnectionString"]);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());

            var newFileName = (String.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName) + (extention.StartsWith(".") ? extention : "." + extention);

            if (content != null && content.Length != 0)
            {
                MemoryStream ms = new MemoryStream(content);

                container.CreateIfNotExists();

                CloudBlockBlob fileBlob = container.GetBlockBlobReference(newFileName);

                fileBlob.UploadFromStream(ms);

                return newFileName;
            }

            return string.Empty;
        }
    }
}
