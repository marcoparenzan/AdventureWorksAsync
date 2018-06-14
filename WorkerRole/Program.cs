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

                            var args1 = command.Value<JObject>("Args");
                            var pageNumber = args1.Value<int>("pageNumber");
                            var pageSize = args1.Value<int>("pageSize");

                            var rows = conn.Query<GetCustomerListDto>(
                                $"SELECT CustomerId AS Id, FirstName, LastName FROM salesLT.Customer ORDER BY LastName, FirstName OFFSET {pageSize*(pageNumber-1)} ROWS FETCH NEXT {pageSize} ROWS ONLY"
                            );

                            var rowCount = conn.QuerySingle<int>("SELECT COUNT(*) FROM salesLT.Customer");

                            var response = new
                            {
                                Type = "GetCustomersListResponse",
                                Rows = rows,
                                PageSize = pageSize,
                                PageNumber = pageNumber,
                                PageCount = Math.Ceiling(rowCount/(decimal) pageSize)
                            };

                            var jsonResponse = JsonConvert.SerializeObject(response);

                            var responseMessage = new CloudQueueMessage(jsonResponse);

                            await commandsResponsesQueue.AddMessageAsync(responseMessage);

                            conn.Close();
                        }
                        break;
                    case "GetCustomer":
                        using (var conn = new SqlConnection(configuration["SqlConnectionString"]))
                        {
                            conn.Open();

                            var result = conn.QuerySingle<GetCustomerDto>(
                                "SELECT CustomerId AS Id, FirstName, LastName FROM salesLT.Customer WHERE CustomerId = @id",
                                new { id = command.Value<int>("Id") }
                            );

                            var response = new
                            {
                                Type = "GetCustomerResponse",
                                Result = result
                            };

                            var jsonResponse = JsonConvert.SerializeObject(response);

                            var responseMessage = new CloudQueueMessage(jsonResponse);

                            await commandsResponsesQueue.AddMessageAsync(responseMessage);

                            conn.Close();
                        }
                        break;
                    case "UpdateCustomer":
                        using (var conn = new SqlConnection(configuration["SqlConnectionString"]))
                        {
                            conn.Open();

                            var id = command.Value<int>("Id");
                            var customerInfo = command.Value<JObject>("CustomerInfo");
                            var firstName = customerInfo.Value<string>("FirstName");
                            var lastName = customerInfo.Value<string>("LastName");

                            var updated = await conn.ExecuteAsync(
                                "UPDATE salesLT.Customer SET FirstName = @FirstName, LastName = @LastName WHERE CustomerId = @Id",
                                new { id, firstName, lastName });

                            var response = new
                            {
                                Type = "UpdateCustomerResponse",
                                Id = id
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
