using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleAppCosmosDbDocumentApi
{
    public class News
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public DateTime Created { get; set; }

        public bool Active { get; set; }

        public string Location { get; set; }

        public int Position { get; set; }
    }


    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            Console.WriteLine(configuration.GetConnectionString("ConnectionStrings"));

            DocumentClient client = new DocumentClient(new Uri("[endpoint]"), "[key]");
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

            var document = new
            {
                firstName = "Alex",
                lastName = "Leh"
            };
            await client.CreateDocumentAsync(collectionUri, document);

            var query = client.CreateDocumentQuery<News>(collectionUri,
                new SqlQuerySpec()
                {
                    QueryText = "SELECT * FROM f WHERE (f.surname = @lastName)",
                    Parameters = new SqlParameterCollection()
                {
                new SqlParameter("@lastName", "Andt")
                }
                }, DefaultOptions);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
