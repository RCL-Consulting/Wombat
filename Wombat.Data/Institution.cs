using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class Institution : BaseEntity
    {
        public Institution()
        {
            Name = "";
        }
        public string Name { get; set; }

        public byte[] Logo { get; set; }
    }
}
