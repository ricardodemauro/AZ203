using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

//based on https://medium.com/@fulviocanducci/mongodb-opera%C3%A7%C3%B5es-crud-com-c-2af4e77c046

namespace ConsoleAppCosmosDb
{
    public class News
    {
        [BsonId()]
        public ObjectId Id { get; set; }

        [BsonElement("title")]
        [BsonRequired()]
        public string Title { get; set; }

        [BsonElement("body")]
        [BsonRequired()]
        public string Body { get; set; }

        [BsonElement("created")]
        [BsonRequired()]
        public DateTime Created { get; set; }

        [BsonElement("active")]
        [BsonRequired()]
        public bool Active { get; set; }

        [BsonElement("location")]
        [BsonRequired()]
        public string Location { get; set; }

        [BsonElement("position")]
        [BsonRequired()]
        public int Position { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            Console.WriteLine(configuration.GetConnectionString("mongodb"));

            var collection = GetMongoCollection(configuration);

            Insert(collection, "BR");
            Insert(collection, "BR");
            Insert(collection, "BR");
            Insert(collection, "BR");
            Insert(collection, "BR");

            Insert(collection, "AR");
            Insert(collection, "AR");
            Insert(collection, "AR");
            Insert(collection, "AR");
            Insert(collection, "AR");
            Insert(collection, "AR");

            var allDocs = GetAll(collection);

            foreach (var doc in allDocs)
            {
                Console.WriteLine($"Id \"{doc.Id}\" Title \"{doc.Title}\" Created \"{doc.Created}\" Location \"{doc.Location}\"");
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static void Insert(IMongoCollection<News> collection, string location)
        {
            News doc = new News();
            doc.Title = $"Titulo {Guid.NewGuid()}";
            doc.Body = "Texto";
            doc.Created = DateTime.Now;
            doc.Active = true;
            doc.Location = location;

            collection.InsertOne(doc);
        }

        static void Update(IMongoCollection<News> collection, ObjectId id)
        {
            Expression<Func<News, bool>> filter =
                x => x.Id.Equals(ObjectId.Parse("594b093325841c1b6cac28ea"));

            News news = collection.Find(filter).FirstOrDefault();
            if (news != null)
            {
                news.Title = "Edited";
                ReplaceOneResult result = collection.ReplaceOne(filter, news);
            }
        }

        static IReadOnlyCollection<News> GetAll(IMongoCollection<News> collection)
        {
            return collection.AsQueryable().ToList();
        }

        static IMongoCollection<News> GetMongoCollection(IConfiguration configuration)
        {
            IMongoClient client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            IMongoDatabase database = client.GetDatabase(configuration["MongoDb:Database"]);

            return database.GetCollection<News>("news");
        }
    }
}
