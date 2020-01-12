using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StorageApp
{
    class Program
    {
        const int LIST_FILES = 1;
        const int UPLOAD_FILE = 2;
        const int DOWNLOAD_FILE = 3;
        const int SAS_KEY = 4;
        const int SHOW_CONNECTION_STRING = 5;
        const int EXIT = 6;

        static readonly string _connectionString;
        static readonly string _accountName;
        static readonly string _accountKey;

        static readonly App app;

        static Program()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("secrets.json", optional: true);

            var configuration = builder.Build();

            _connectionString = configuration["AZURE_STORAGE_CONNECTION_STRING"];
            _accountKey = configuration["Key"];
            _accountName = configuration["AccountName"];


            var svcColl = new ServiceCollection();
            _ = svcColl.AddLogging(opt => opt.AddConsole().SetMinimumLevel(LogLevel.Trace));
            _ = svcColl.AddScoped(x => new App(_connectionString, _accountKey, _accountName, x.GetService<ILogger<App>>()));

            var serviceProvider = svcColl.BuildServiceProvider();

            app = serviceProvider.GetService<App>();
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Demo Azure Storage - AZ203");

            if (string.IsNullOrEmpty(_connectionString))
            {
                Console.WriteLine("CONNECTION STRING IS EMPTY... ENDING PROGRAM");
                Environment.Exit(0);
                return;
            }

            Opt:
            ShowOptions();

            string option = Console.ReadKey().KeyChar.ToString();

            int parsedOption = int.Parse(option);
            Console.WriteLine();

            switch (parsedOption)
            {
                case EXIT:
                    return;
                case LIST_FILES:
                    await app.ListFiles();
                    break;
                case DOWNLOAD_FILE:
                    Console.WriteLine("type the file name with extension");
                    string filename = Console.ReadLine();
                    await app.Download(filename);
                    break;
                case SAS_KEY:
                    Console.WriteLine("type the file name with extension");
                    string sasFilename = Console.ReadLine();
                    await app.CreateSASUrl(sasFilename);
                    break;
                case UPLOAD_FILE:
                    await app.Upload();
                    break;
                case SHOW_CONNECTION_STRING:
                    Console.WriteLine(_connectionString);
                    break;
            }

            goto Opt;
        }

        private static void ShowOptions()
        {
            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}Available options...");
            Console.WriteLine($"{LIST_FILES}. List files");
            Console.WriteLine($"{UPLOAD_FILE}. Upload file");
            Console.WriteLine($"{DOWNLOAD_FILE}. Download file");
            Console.WriteLine($"{SAS_KEY}. Generate SAS Key");
            Console.WriteLine($"{SHOW_CONNECTION_STRING}. Show connection string");
            Console.WriteLine($"{EXIT}. Exit");
        }
    }

    public class App
    {
        private readonly string _connectionString;
        private readonly string _accountKey;
        private readonly string _accountName;

        private readonly ILogger<App> _logger;

        private const string CONTAINER_NAME = "quickstartblobs";
        private const string FILE_NAME = "sample.txt";

        public App(string connectionString, string accountKey, string accountName, ILogger<App> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _accountKey = accountKey ?? throw new ArgumentNullException(nameof(accountKey));
            _accountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<BlobContainerClient> GetContainer()
        {
            _logger.LogInformation("Getting Container Reference");

            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            return containerClient;
        }

        public async Task Upload()
        {
            string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sample.txt");

            var containerClient = await GetContainer();

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient($"{Guid.NewGuid().ToString()}-{FILE_NAME}");

            _logger.LogInformation("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
        }

        public async Task ListFiles()
        {
            _logger.LogInformation("Listing blobs...");

            var containerClient = await GetContainer();

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                _logger.LogInformation("\t" + blobItem.Name);
            }
        }

        public async Task Download(string name)
        {
            // Download the blob to a local file
            // Append the string "DOWNLOAD" before the .txt extension so you can see both files in MyDocuments
            string downloadFilePath = Path.Combine(Directory.GetCurrentDirectory(), name.Replace(".txt", "DOWNLOAD.txt"));

            _logger.LogInformation("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            var container = await GetContainer();

            var blobClient = container.GetBlobClient(name);

            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using FileStream downloadFileStream = File.OpenWrite(downloadFilePath);
            await download.Content.CopyToAsync(downloadFileStream);
            downloadFileStream.Close();
        }

        public async Task CreateSASUrl(string filename)
        {
            BlobContainerClient containerClient = await GetContainer();
            BlobClient blobClient = containerClient.GetBlobClient(filename);

            var builder = new BlobSasBuilder
            {
                BlobContainerName = CONTAINER_NAME,
                StartsOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddMinutes(4),
                BlobName = filename
            };

            builder.SetPermissions(BlobAccountSasPermissions.Read);

            //  Builds an instance of StorageSharedKeyCredential      
            var storageSharedKeyCredential = new StorageSharedKeyCredential(_accountName, _accountKey);

            var sasQueryParameters = builder.ToSasQueryParameters(storageSharedKeyCredential);

            //  Builds the URI to the blob storage.
            UriBuilder fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", _accountName),
                Path = string.Format("{0}/{1}", CONTAINER_NAME, filename),
                Query = sasQueryParameters.ToString()
            };

            _logger.LogInformation($"Uri with SAS Token {fullUri.ToString()}");
        }
    }
}
