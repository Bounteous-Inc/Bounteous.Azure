using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Bounteous.Azure.Storage;
using Bounteous.xUnit.Accelerator;
using Moq;
using Xunit;

namespace Bounteous.Azure.Test.Storage
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class BlobStorageTests : MockBase
    {
        private readonly Mock<BlobServiceClient> blobServiceClientMock;
        private readonly Mock<BlobContainerClient> blobContainerClientMock;
        private readonly Mock<BlobClient> blobClientMock;
        private readonly BlobStorage blobStorage;

        private const string containerName = "container";
        private const string blobName = "test-blob.json";
    
        public BlobStorageTests()
        {
            blobServiceClientMock = Strict<BlobServiceClient>();
            blobContainerClientMock = Strict<BlobContainerClient>();
            blobClientMock = Strict<BlobClient>();
            blobStorage = new BlobStorage(container => blobServiceClientMock.Object);
        }

        [Fact]
        public async Task ForContainer_ShouldInitializeContainerClient()
        {
            SetupBlobContainerClient(containerName);
            SetupCreateIfNotExists();
            
            await blobStorage.ForContainer(containerName);
            blobServiceClientMock.Verify(x => x.GetBlobContainerClient(containerName), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_ShouldUploadJsonData()
        {
            // Arrange
            SetupBlobContainerClient(containerName);
            SetupCreateIfNotExists();
            SetupBlobClient(blobName);
            
            var data = new { Name = "John Doe", Age = 30 };

            await blobStorage.ForContainer(containerName);

            blobClientMock
                .Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, CancellationToken.None))
                .ReturnsAsync((Response<BlobContentInfo>)null);

            await blobStorage.SaveAsync(blobName, data);
        }

        private void SetupBlobClient(string name)
            => blobContainerClientMock
                .Setup(x => x.GetBlobClient(name))
                .Returns(blobClientMock.Object);
        
        private void SetupBlobContainerClient(string container)
            => blobServiceClientMock
                .Setup(x => x.GetBlobContainerClient(container))
                .Returns(blobContainerClientMock.Object);
        
        private void SetupCreateIfNotExists()
            => blobContainerClientMock.Setup(x =>
                    x.CreateIfNotExistsAsync(PublicAccessType.None, null, null, CancellationToken.None))
                .ReturnsAsync((Response<BlobContainerInfo>)null);
    }
}