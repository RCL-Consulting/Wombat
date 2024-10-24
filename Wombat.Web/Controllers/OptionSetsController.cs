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

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;
using Wombat.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Wombat.Common.Constants;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class OptionSetsController : Controller
    {
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;

        public OptionSetsController( IOptionSetRepository optionSetRepository,
                                     IMapper mapper )
        {
            this.optionSetRepository=optionSetRepository;
            this.mapper=mapper;
        }

        // GET: OptionSets
        public async Task<IActionResult> Index()
        {
            var optionSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());
            return View(optionSets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOption(OptionSetVM optionSetVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = optionSetVM.Options?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null && Item.CanEditAndDelete)
            {
                optionSetVM.Options?.RemoveAll(s => s.DisplayId == displayId);
            }
            return PartialView("OptionSet", optionSetVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOption(OptionSetVM optionSetVM)
        {
            var Item = new OptionVM();
            Item.DisplayId = OptionVM.NextDisplayId++;
            Item.Rank = optionSetVM.Options.Count;
            optionSetVM.Options?.Add(Item);
            return PartialView("OptionSet", optionSetVM);
        }

        // GET: OptionSets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var optionSet = await optionSetRepository.GetAsync(id);
            if (optionSet == null)
            {
                return NotFound();
            }

            var optionSetVM = mapper.Map<OptionSetVM>(optionSet);
            return View(optionSetVM);
        }

        // GET: OptionSets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OptionSets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OptionSetVM optionSetVM)
        {
            if (ModelState.IsValid)
            {
                var optionSet = mapper.Map<OptionSet>(optionSetVM);

                if (optionSet.Options!=null)
                {
                    foreach (var option in optionSet.Options)
                    {
                        option.OptionSet = optionSet;
                    }
                }

                await optionSetRepository.AddAsync(optionSet);
                return RedirectToAction(nameof(Index));
            }
            return View(optionSetVM);
        }

        // GET: OptionSets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var optionSet = await optionSetRepository.GetAsync(id);
            if (optionSet == null)
            {
                return NotFound();
            }

            var optionSetVM = mapper.Map<OptionSetVM>(optionSet);
            return View(optionSetVM);
        }

        // POST: OptionSets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OptionSetVM optionSetVM)
        {
            if (id != optionSetVM.Id)
            {
                return NotFound();
            }

            var optionSet = await optionSetRepository.GetAsync(id);

            if (optionSet==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(optionSetVM, optionSet);
                    await optionSetRepository.UpdateAsync(optionSet);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await optionSetRepository.Exists(optionSetVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(optionSetVM);
        }

        // POST: OptionSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await optionSetRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
