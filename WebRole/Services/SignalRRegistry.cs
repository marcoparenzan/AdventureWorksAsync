using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRole.Services
{
    public class SignalRRegistry : ISignalRRegistry
    {
        private Dictionary<string, string> _usernamesToClientIds = 
            new Dictionary<string, string>();

        public string ClientIdFromUsername(string username)
        {
            if (_usernamesToClientIds.ContainsKey(username))
                return _usernamesToClientIds[username];
            else
                return string.Empty;
        }

        public void Register(string clientId, string username)
        {
            if (_usernamesToClientIds.ContainsKey(username))
            {
                if (_usernamesToClientIds[username] != clientId)
                {
                    _usernamesToClientIds.Remove(username);
                }
            }
            if (!_usernamesToClientIds.ContainsKey(username))
            {
                _usernamesToClientIds.Add(username, clientId);
            }
        }
    }
}
