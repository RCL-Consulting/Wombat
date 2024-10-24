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
                if (DisplayId>= NextDisplayId)
                {
                    NextDisplayId = DisplayId + 1;
                }
            }
        }
        public bool CanEditAndDelete { get; set; } = true;

        public int DisplayId { get; set; }
        static public int NextDisplayId { get; set; } = 1;
    }
}
