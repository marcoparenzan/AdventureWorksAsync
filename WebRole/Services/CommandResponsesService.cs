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

        public CommandResponsesService(IConfiguration configuration, IHubContext<CommandResponsesHub> hub)
        {
            _configuration = configuration;
            _hub = hub;
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
                            var result = command.Value<JArray>("Result");
                            await _hub.Clients.All.SendAsync("GetCustomersListResponse", result);
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
