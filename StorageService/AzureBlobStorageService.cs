using Microsoft.Extensions.Configuration;
using SharedLibrary.Interfaces;
using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;
using Azure;
using Azure.Storage.Blobs.Specialized;
using System.Collections.Generic;
using Azure.Storage.Blobs.Models;
using SharedLibrary;

namespace StorageService
{
    public class AzureBlobStorageService : IStorageService
    {
        private IConfiguration _configuration;

        private static BlobServiceClient blobServiceClient;
        private static BlobContainerClient blobContainerClient;

        private static string _containerName;

        public AzureBlobStorageService(IConfiguration configuration, string containerName)
        {
            _configuration = configuration;
            _containerName = containerName;

            blobServiceClient = new BlobServiceClient(configuration.GetSection("ConnectionString").Value);

            CreateContainerIfNotExists(_containerName);
        }

        public async Task<string> Upload(string blobName, string filePathName)
        {
            Logger.LogDebug($">> Upload {blobName} {filePathName}");

            blobName = $"{blobName.ToLower()}-{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.jpg";

            BlockBlobClient blockBlobClient = new BlockBlobClient(_configuration.GetSection("ConnectionString").Value,
                _containerName,
                blobName);

            var blobInfo = await blockBlobClient.UploadAsync(new MemoryStream(File.ReadAllBytes(filePathName)), httpHeaders: new BlobHttpHeaders() { ContentType = "image/jpeg" });

            if (blobInfo.GetRawResponse().Status == 201)
            {
                return blockBlobClient.Uri.ToString();
            }

            return string.Empty;
        }

        private void CreateContainerIfNotExists(string containerName)
        {
            blobContainerClient = new BlobContainerClient(_configuration.GetSection("ConnectionString").Value, containerName);

            blobContainerClient.CreateIfNotExists(PublicAccessType.BlobContainer);

            blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }
    }
}
