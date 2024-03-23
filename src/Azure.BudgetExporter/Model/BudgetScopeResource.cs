using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BudgetExporter.Model
{
    public class BudgetScopeResource
    {
        public string ResourceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public ResourceType ResourceType { get; set; }

        public List<BudgetInfo> Budgets { get; set; } = new List<BudgetInfo>();

        public BudgetScopeResource? Parent { get; set; }

        public bool HasBudgets
        {
            get
            {
                return Budgets.Any();
            }
        }

        // Tree buidling is not as easy as the list from the API is a flat list. Implement later
        private List<BudgetScopeResource> Children { get; set; } = new List<BudgetScopeResource>();

        public BudgetScopeResource() { }

    }
}
