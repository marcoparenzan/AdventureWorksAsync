using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using WorkerRole.Models;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WorkerRole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile(
                "appsettings.json", 
                optional: true, 
                reloadOnChange: true);
            var configuration = builder.Build();

            var storageAccount =
                CloudStorageAccount.Parse(
                    configuration["StorageConnectionString"]);

            var queueClient =
                storageAccount.CreateCloudQueueClient();

            var commandsQueue =
                queueClient.GetQueueReference("webrolecommands");
            await commandsQueue.CreateIfNotExistsAsync();

            var commandsResponsesQueue =
                queueClient.GetQueueReference("commandresponses");
            await commandsResponsesQueue.CreateIfNotExistsAsync();

            while (true)
            {
                var message = await commandsQueue.GetMessageAsync();
                if (message == null)
                {
                    await Task.Delay(1000);
                    continue;
                }

                var command = JsonConvert.DeserializeObject<JObject>(message.AsString);
                var type = command.Value<string>("Type");

                switch (type)
                {
                    case "GetCustomersList":
                        using (var conn = new SqlConnection(configuration["SqlConnectionString"]))
                        {
                            conn.Open();

                            var query = conn.Query<GetCustomerListDto>(
                                "SELECT TOP 10 CustomerId AS Id, CompanyName AS Name FROM salesLT.Customer ORDER BY Name"
                            );

                            var response = new
                            {
                                Type = "GetCustomersListResponse",
                                Result = query.ToList()
                            };

                            var jsonResponse = JsonConvert.SerializeObject(response);

                            var responseMessage = new CloudQueueMessage(jsonResponse);

                            await commandsResponsesQueue.AddMessageAsync(responseMessage);

                            conn.Close();
                        }
                        break;
                    default:
                        break;
                }


                await commandsQueue.DeleteMessageAsync(message);
            }
        }
    }
}
