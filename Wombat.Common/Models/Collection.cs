using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class Collection
    {
        private int _id = 0;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                DisplayId = _id;
            }
        }
        public bool CanEditAndDelete { get; set; } = true;

        public int DisplayId { get; set; }
        static public int NextDisplayId { get; set; } = 1;
    }
}
