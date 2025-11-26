using Azure.Storage.Blobs;

namespace magic_portal_class_library;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(string accountUrl, string sasToken, string containerName)
    {
        var containerSasUri = new Uri($"{accountUrl}{containerName}{sasToken}");
        _container = new BlobContainerClient(containerSasUri);
    }

    public async Task<List<string>> ListBlobsAsync()
    {
        var blobs = new List<string>();
        await foreach (var item in _container.GetBlobsAsync())
            blobs.Add(item.Name);
        return blobs;
    }

    public async Task UploadFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var blob = _container.GetBlobClient(fileName);

        await using var stream = File.OpenRead(filePath);
        await blob.UploadAsync(stream, overwrite: true);
    }

    public async Task DownloadFileAsync(string blobName, string downloadPath)
    {
        var blob = _container.GetBlobClient(blobName);

        await using var fs = File.OpenWrite(downloadPath);
        await blob.DownloadToAsync(fs);
    }
}
