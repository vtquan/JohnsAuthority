using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JohnsAuthority.Services
{
    public class FileManager
    {
        private CloudStorageAccount StorageAccount;
        private CloudBlobClient BlobClient;
        private CloudBlobContainer Container;
        private CloudBlockBlob BlockBlob;

        public FileManager()
        {
            StorageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "johnsauthority",
                "0uOGQKDzExR1lTAkZPYot5gihlh0KqD0Jh9myRXw1TUdpNHPmow/cr4BuVh6dGrwtA3TRJoKksD5dhMc+giTtQ=="), true);

            // Create a blob client.
            BlobClient = StorageAccount.CreateCloudBlobClient();
            
            Container = BlobClient.GetContainerReference("images");
        }

        public async Task<string> UploadFile(IFormFile file)
        {
            BlockBlob = Container.GetBlockBlobReference(file.FileName);

            using (var fileStream = file.OpenReadStream())
            {
                var read = fileStream.CanRead;
                await BlockBlob.UploadFromStreamAsync(fileStream);
            }

            return BlockBlob.Uri.AbsoluteUri;
        }

        public async Task<string> UploadFile(IFormFile file, string newFileName)
        {
            BlockBlob = Container.GetBlockBlobReference(newFileName);

            using (var fileStream = file.OpenReadStream())
            {
                var read = fileStream.CanRead;
                await BlockBlob.UploadFromStreamAsync(fileStream);
            }

            return BlockBlob.Uri.AbsoluteUri;
        }

        public async void DeleteFile(string filename)
        {
            BlockBlob = Container.GetBlockBlobReference(filename);

            await BlockBlob.DeleteIfExistsAsync();
        }
    }
}
