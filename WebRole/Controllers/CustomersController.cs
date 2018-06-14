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
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomersListCommand([FromBody] JObject args)
        {
            await SendCommand(new
            {
                Type = "GetCustomersList",
                Args = args
            });

            return Json(new
            {
                Success = true
            });
        }

        private async Task SendCommand<TCommand>(TCommand command)
        {
            var storageAccount =
                CloudStorageAccount.Parse(
                    _configuration["StorageConnectionString"]);

            var queueClient =
                storageAccount.CreateCloudQueueClient();

            var commandsQueue =
                queueClient.GetQueueReference("webrolecommands");
            await commandsQueue.CreateIfNotExistsAsync();

            var jsonCommand = JsonConvert.SerializeObject(command);

            var message = new CloudQueueMessage(jsonCommand);

            await commandsQueue.AddMessageAsync(message);
        }

        [HttpGet]
        [ActionName("Customer")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            await SendCommand(new
            {
                Type = "GetCustomer",
                Id = id
            });

            return Json(new
            {
                Success = true
            });
        }


        [HttpPost]
        [ActionName("Customer")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] JObject customer)
        {
            await SendCommand(new
            {
                Type = "UpdateCustomer",
                Id = id,
                CustomerInfo = customer
            });

            return Json(new
            {
                Success = true
            });
        }
    }
}