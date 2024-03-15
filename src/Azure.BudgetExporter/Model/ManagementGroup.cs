using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BudgetExporter.Model
{
    public class ManagementGroup
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }

    public class Subscription
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }

    public class ResourceGroup
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }
}


