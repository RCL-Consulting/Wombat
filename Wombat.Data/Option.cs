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
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class Option : BaseEntity
    {
        public Option()
        {
            Description = "";
        }
        public string Description { get; set; }
        public int Rank { get; set; }

        public int OptionSetId { get; set; }

        [ForeignKey("OptionSetId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public OptionSet? OptionSet { get; set; }

    }
}
