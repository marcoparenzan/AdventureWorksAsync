using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebRole.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IConfiguration _configuration;

        public CustomersController(IConfiguration configuration)
        {
            (_configuration) = (configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomersListCommand()
        {
            var storageAccount =
                CloudStorageAccount.Parse(
                    _configuration["StorageConnectionString"]);

            var queueClient = 
                storageAccount.CreateCloudQueueClient();

            var commandsQueue = 
                queueClient.GetQueueReference("webrolecommands");
            await commandsQueue.CreateIfNotExistsAsync();

            var command = new
            {
                Type = "GetCustomersList"
            };
            var jsonCommand = JsonConvert.SerializeObject(command);

            var message = new CloudQueueMessage(jsonCommand);

            await commandsQueue.AddMessageAsync(message);

            return Json(new
            {
                Success = true
            });
        }
    }
}