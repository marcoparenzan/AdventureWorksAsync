using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRole.Services
{
    public class CommandResponsesHub: Hub
    {
        private ISignalRRegistry _registry;

        public CommandResponsesHub(ISignalRRegistry registry)
        {
            _registry = registry;
        }

        public void Register(string id, string username)
        {
            _registry.Register(id, username);
        }
    }
}
