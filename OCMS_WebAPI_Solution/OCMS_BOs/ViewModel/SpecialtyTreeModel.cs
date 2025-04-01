using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.ViewModel
{
    public class SpecialtyTreeModel
    {
        public string SpecialtyId { get; set; }

        public string SpecialtyName { get; set; }

        public string Description { get; set; }

        public string ParentSpecialtyId { get; set; }

        public int Status { get; set; }

        public ICollection<SpecialtyTreeModel> Children { get; set; } = new List<SpecialtyTreeModel>();
    }
}
