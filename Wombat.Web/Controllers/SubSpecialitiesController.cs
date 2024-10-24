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
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class SubSpecialitiesController : Controller
    {
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IMapper mapper;

        public SubSpecialitiesController( ISubSpecialityRepository subSpecialityRepository,
                                          ISpecialityRepository specialityRepository,
                                          IMapper mapper )
        {
            this.subSpecialityRepository = subSpecialityRepository;
            this.specialityRepository = specialityRepository;
            this.mapper = mapper;
        }

        // GET: SubSpecialities
        public async Task<IActionResult> Index()
        {
            var subSpecialities = mapper.Map<List<SubSpecialityVM>>(await subSpecialityRepository.GetAllAsync());
            return View(subSpecialities);
        }

        // GET: SubSpecialities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var subSpeciality = await subSpecialityRepository.GetAsync(id);
            if (subSpeciality == null)
            {
                return NotFound();
            }

            var subSpecialityVM = mapper.Map<SubSpecialityVM>(subSpeciality);
            return View(subSpecialityVM);
        }

        // GET: SubSpecialities/Create
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["SpecialityId"] = new SelectList(await specialityRepository.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: SubSpecialities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubSpecialityVM subSpecialityVM)
        {
            if (ModelState.IsValid)
            {
                var subSpeciality = mapper.Map<SubSpeciality>(subSpecialityVM);

                var speciality = await specialityRepository.GetAsync(subSpecialityVM.SpecialityId);
                if(speciality == null)
                {
                    return NotFound();
                }

                subSpeciality.Speciality = speciality;
                await subSpecialityRepository.AddAsync(subSpeciality);
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpecialityId"] = new SelectList(await specialityRepository.GetAllAsync(), "Id", "Name", subSpecialityVM.SpecialityId);
            return View(subSpecialityVM);
        }

        // GET: SubSpecialities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var subSpeciality = await subSpecialityRepository.GetAsync(id);
            if (subSpeciality == null)
            {
                return NotFound();
            }

            var subSpecialityVM = mapper.Map<SubSpecialityVM>(subSpeciality);
            ViewData["SpecialityId"] = new SelectList(await specialityRepository.GetAllAsync(), "Id", "Name", subSpeciality.SpecialityId);
            return View(subSpecialityVM);
        }

        // POST: SubSpecialities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubSpecialityVM subSpecialityVM)
        {
            if (id != subSpecialityVM.Id)
            {
                return NotFound();
            }

            var subSpeciality = await subSpecialityRepository.GetAsync(id);

            if (subSpeciality == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(subSpecialityVM, subSpeciality);
                    await subSpecialityRepository.UpdateAsync(subSpeciality);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await subSpecialityRepository.Exists(subSpecialityVM.Id))
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
            ViewData["SpecialityId"] = new SelectList(await specialityRepository.GetAllAsync(), "Id", "Name", subSpecialityVM.SpecialityId);
            return View(subSpecialityVM);
        }

        // POST: OptionSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await subSpecialityRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
