using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Bounteous.Azure.Serialization;
using Bounteous.Core.Extensions;

namespace Bounteous.Azure.Storage
{
    public interface IBlobStorage
    {
        Task<IBlobStorage> ForContainer(string container);
        Task SaveAsync<T>(string name, T toSave) where T : class;
        Task<T> ReadAsync<T>(string name) where T : class;
    }

    public class BlobStorage : IBlobStorage
    {
        private string containerName;
        private Func<string,BlobServiceClient> clientFactory;
        private BlobContainerClient containerClient;

        public BlobStorage() { }

        public BlobStorage(Func<string, BlobServiceClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }
        
        public async Task<IBlobStorage> ForContainer(string container)
        {
            containerClient = await GetContainerClientAsync(container);
            return this;
        }

        public async Task SaveAsync<T>(string name, T toSave) where T : class
        {
            var blobClient = containerClient.GetBlobClient(name);
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(toSave.ToJson()));
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task<T> ReadAsync<T>(string name) where T : class
        {
            var blobClient = containerClient.GetBlobClient(name);
            var response = await blobClient.DownloadAsync();
            await using var stream = response.Value.Content;
            return await stream.FromJsonAsync<T>();
        }

        private BlobServiceClient Client
        {
            get
            {
                clientFactory ??= x => new BlobServiceClient(containerName);
                return clientFactory(containerName);
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync(string container)
        {
            containerName = container.Trim();
            containerClient = Client.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, null, null, CancellationToken.None);
            return containerClient;
        }
    }
}