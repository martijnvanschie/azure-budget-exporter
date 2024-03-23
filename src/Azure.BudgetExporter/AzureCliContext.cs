using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BudgetExporter
{
    internal class AzureCliContext
    {
        static ArmClient _client;

        internal static ArmClient Client
        { 
            get 
            {
                return _client;
            } 
        }

        static AzureCliContext()
        {
            _client = new ArmClient(new AzureCliCredential());
        }

        public static void SetAzureCliCredential(TokenCredential credential)
        {
            _client = new ArmClient(credential);
        }
    }
}
