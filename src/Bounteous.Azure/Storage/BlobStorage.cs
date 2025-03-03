using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Bounteous.Azure.Serialization;
using Bounteous.Core.Extensions;
using Bounteous.Core.Validations;

namespace Bounteous.Azure.Storage
{
    public interface IBlobStorage
    {
        IBlobStorage ForAccount(string accountName);
        Task<IBlobStorage> ForContainer(string container);
        Task SaveAsync<T>(string name, T toSave) where T : class;
        Task<T> ReadAsync<T>(string name) where T : class;
    }

    public class BlobStorage : IBlobStorage
    {
        private Uri blobClientUri;
        private string containerName;
        private readonly Func<string,BlobServiceClient> clientFactory;
        private BlobContainerClient containerClient;
        private TokenCredential credentials;

        public BlobStorage() { }

        public BlobStorage(Func<string, BlobServiceClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public IBlobStorage ForAccount(string accountName)
        {
            Validate.Begin().IsNotEmpty(accountName, nameof(accountName)).Check();
            blobClientUri = new Uri($"https://{accountName}.blob.core.windows.net");
            return this;
        }
        
        public BlobStorage WithCredentials(TokenCredential credential)
        {
            Validate.Begin().IsNotNull(credential, nameof(credential)).Check();
            credentials = credential;
            return this;
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
            => clientFactory != null
                ? clientFactory(containerName)
                : new BlobServiceClient(blobClientUri, Credentials);

        private TokenCredential Credentials => credentials ?? new DefaultAzureCredential();

        private async Task<BlobContainerClient> GetContainerClientAsync(string container)
        {
            containerName = container.Trim();
            containerClient = Client.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, null, null, CancellationToken.None);
            return containerClient;
        }
    }
}