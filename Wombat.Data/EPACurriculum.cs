/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class EPACurriculum : BaseEntity
    {
        public EPACurriculum()
        {
        }
        
        public int NumberOfMonths { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public EPA? EPA { get; set; }

        public int EPAScaleId { get; set; }

        [ForeignKey("EPAScaleId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Option? EPAScaleOption { get; set; }

    }
}
