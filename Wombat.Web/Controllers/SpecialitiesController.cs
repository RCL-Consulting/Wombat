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
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel.Tables;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize]
    public class SpecialitiesController : Controller
    {
        private readonly ISpecialityRepository specialityRepository;
        private readonly IMapper mapper;

        public SpecialitiesController( ISpecialityRepository specialityRepository,
                                       IMapper mapper )
        {
            this.specialityRepository = specialityRepository;
            this.mapper = mapper;
        }

        // GET: Specialities
        public async Task<IActionResult> Index()
        {
            var specialities = mapper.Map<List<SpecialityVM>>(await specialityRepository.GetAllAsync());
            return View(specialities);
        }

        // GET: Specialities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var speciality = await specialityRepository.GetAsync(id);
            if (speciality == null)
            {
                return NotFound();
            }

            var specialityVM = mapper.Map<SpecialityVM>(speciality);
            return View(specialityVM);
        }

        // GET: Specialities/Create
        [Authorize(Policy = Claims.ManageSpecialities)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specialities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageSpecialities)]
        public async Task<IActionResult> Create(SpecialityVM specialityVM)
        {
            if (ModelState.IsValid)
            {
                var speciality = mapper.Map<Speciality>(specialityVM);

                if (speciality.SubSpecialities == null)
                    speciality.SubSpecialities = new List<SubSpeciality>();

                var General = new SubSpeciality();
                General.Name = "General";
                General.Speciality = speciality;
                General.CanEditAndDelete = false;
                speciality.SubSpecialities.Insert(0, General);

                foreach (var item in speciality.SubSpecialities)
                {
                    item.Speciality = speciality;
                }

                await specialityRepository.AddAsync(speciality);
                return RedirectToAction(nameof(Index));
            }
            return View(specialityVM);
        }

        // GET: Specialities/Edit/5
        [Authorize(Policy = Claims.ManageSpecialities)]
        public async Task<IActionResult> Edit(int? id)
        {
            var speciality = await specialityRepository.GetAsync(id);
            if (speciality == null)
            {
                return NotFound();
            }

            var specialityVM = mapper.Map<SpecialityVM>(speciality);
            return View(specialityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageSpecialities)]
        public IActionResult DeleteSubSpeciality(SpecialityVM specialityVM, int DisplayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = specialityVM.SubSpecialities?.FirstOrDefault(s => s.DisplayId == DisplayId);
            if (Item != null && Item.CanEditAndDelete)
            {
                specialityVM.SubSpecialities?.RemoveAll(s => s.DisplayId == DisplayId);
            }
            return PartialView("SubSpeciality", specialityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageSpecialities)]
        public IActionResult AddSubSpeciality(SpecialityVM specialityVM)
        {
            var Item = new SubSpecialityVM();
            Item.DisplayId = SubSpecialityVM.NextDisplayId++;
            Item.Speciality = specialityVM;
            specialityVM.SubSpecialities?.Add(Item);
            return PartialView("SubSpeciality", specialityVM);
        }

        // POST: Specialities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageSpecialities)]
        public async Task<IActionResult> Edit(int id, SpecialityVM specialityVM)
        {
            if (id != specialityVM.Id)
            {
                return NotFound();
            }

            var speciality = await specialityRepository.GetAsync(id);

            if (speciality == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(specialityVM, speciality);
                    await specialityRepository.UpdateAsync(speciality);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await specialityRepository.Exists(specialityVM.Id))
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
            return View(specialityVM);
        }

        // POST: OptionSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageSpecialities)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await specialityRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
