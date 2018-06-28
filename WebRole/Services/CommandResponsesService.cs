using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebRole.Services
{
    public class CommandResponsesService : IHostedService
    {
        private IConfiguration _configuration;
        private IHubContext<CommandResponsesHub> _hub;
        private ISignalRRegistry _registry;

        public CommandResponsesService(IConfiguration configuration, IHubContext<CommandResponsesHub> hub, ISignalRRegistry registry)
        {
            _configuration = configuration;
            _hub = hub;
            _registry = registry;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            var storageAccount =
            CloudStorageAccount.Parse(
                _configuration["StorageConnectionString"]);

            var queueClient =
                storageAccount.CreateCloudQueueClient();

            var commandsResponsesQueue =
                queueClient.GetQueueReference("commandresponses");
            commandsResponsesQueue.CreateIfNotExistsAsync().Wait();

            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await commandsResponsesQueue.GetMessageAsync();
                    if (message == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    var command = JsonConvert.DeserializeObject<JObject>(message.AsString);
                    var type = command.Value<string>("Type");

                    switch (type)
                    {
                        case "GetCustomersListResponse":
                            //var result = command.Value<JArray>("Result");
                            // to be fast, I use command directly, but it is wrong!
                            // build the correct dto
                            await _hub.Clients.All.SendAsync("GetCustomersListResponse", command);
                            break;
                        case "GetCustomerResponse":
                            var result1 = command.Value<JObject>("Result");
                            var username = command.Value<string>("Username");

                            await _hub.Clients.Client(_registry.ClientIdFromUsername(username)).SendAsync("CustomerAvailable", result1);
                            break;
                        case "UpdateCustomerResponse":
                            var id = command.Value<int>("Id");
                            await _hub.Clients.All.SendAsync("CustomerUpdated", new { Id = id });
                            break;
                        default:
                            break;
                    }

                    await commandsResponsesQueue.DeleteMessageAsync(message);
                }
            });

            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
