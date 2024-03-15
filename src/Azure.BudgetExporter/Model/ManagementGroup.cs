using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BudgetExporter.Model
{
    public class ManagementGroup
    {
        public string ResourceId { get; set; } = string.Empty;

        public string ManagementGroupId { get; set; }

        public string DisplayName { get; set; }
    }
}


