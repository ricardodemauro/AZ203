using ConsoleAppTableStorage.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppTableStorage
{
    public class TableManager
    {
        const string _connectionString = @"DefaultEndpointsProtocol=https;AccountName=myamazingstorageaccount;AccountKey=bPbzZJAPBFZ3kueL85251gs0rzTscYVFMdr4V8DDwNOXmM5pOu3sd8Vd/RdYhsqUTFjCAr3qUrMr7BZfBn2TyQ==;EndpointSuffix=core.windows.net";

        // private property  
        private CloudTable _table;

        public TableManager(string tableName)
            : this(tableName, _connectionString)
        {

        }

        public TableManager(string tableName, string connectionString)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("Table", "Table Name can't be empty");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient client = storageAccount.CreateCloudTableClient();

            CloudTable table = client.GetTableReference(tableName);

            table.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            _table = table;
        }

        public async Task<bool> Add(EmployeeSessionEntity entity,
            CancellationToken cancellationToken = default)
        {
            TableOperation insertOp = TableOperation.Insert(entity);

            var tableResult = await _table.ExecuteAsync(insertOp);
            return (int)HttpStatusCode.NoContent == tableResult.HttpStatusCode;
        }

        public async Task<EmployeeSessionEntity> Get(string partitionKey, string rowkey, CancellationToken cancellationToken = default)
        {
            TableOperation retOp = TableOperation.Retrieve(partitionKey, rowkey);

            TableResult tableResult = await _table.ExecuteAsync(retOp);

            if ((int)HttpStatusCode.OK == tableResult.HttpStatusCode)
            {
                return tableResult.Result as EmployeeSessionEntity;
            }
            return null;
        }

        public async Task<IReadOnlyList<EmployeeSessionEntity>> GetAll(string query = null)
        {
            try
            {
                TableQuerySegment<DynamicTableEntity> segment = null;

                // Create the Table Query Object for Azure Table Storage  
                TableQuery<EmployeeSessionEntity> DataTableQuery = new TableQuery<EmployeeSessionEntity>();
                if (!string.IsNullOrEmpty(query))
                {
                    DataTableQuery = new TableQuery<EmployeeSessionEntity>().Where(query);
                }

                TableQuerySegment<EmployeeSessionEntity> queryResult = await _table.ExecuteQuerySegmentedAsync(DataTableQuery, segment?.ContinuationToken);

                return queryResult.Results;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }
    }
}
