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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    public class InstitutionsController : Controller
    {
        private readonly IInstitutionRepository institutionRepository;
        private readonly IMapper mapper;

        public InstitutionsController( IInstitutionRepository institutionRepository,
                                       IMapper mapper )
        {
            this.institutionRepository = institutionRepository;
            this.mapper = mapper;
        }

        // GET: Institutions
        public async Task<IActionResult> Index()
        {
            var institutions = mapper.Map<List<InstitutionVM>>(await institutionRepository.GetAllAsync());
            return View(institutions);
        }

        // GET: Institutions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var institution = await institutionRepository.GetAsync(id);
            if (institution == null)
            {
                return NotFound();
            }

            var institutionVM = mapper.Map<InstitutionVM>(institution);
            return View(institutionVM);
        }

        // GET: Institutions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Institutions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstitutionVM institutionVM, IFormFile? logoFile)
        {
            if (logoFile != null && logoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    logoFile.CopyTo(memoryStream);
                    institutionVM.Logo = memoryStream.ToArray(); // Convert image to byte array
                }
            }

            if (ModelState.IsValid)
            {
                var institution = mapper.Map<Institution>(institutionVM);

                await institutionRepository.AddAsync(institution);
                return RedirectToAction(nameof(Index));
            }
            
            return View(institutionVM);
        }

        // GET: Institutions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var institution = await institutionRepository.GetAsync(id);
            if (institution == null)
            {
                return NotFound();
            }

            var institutionVM = mapper.Map<InstitutionVM>(institution);
            
            return View(institutionVM);
        }

        // POST: Institutions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InstitutionVM institutionVM, IFormFile? logoFile)
        {
            if (id != institutionVM.Id)
            {
                return NotFound();
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    logoFile.CopyTo(memoryStream);
                    institutionVM.Logo = memoryStream.ToArray(); // Convert image to byte array
                }
            }

            var institution = await institutionRepository.GetAsync(id);

            if (institution == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(institutionVM, institution);
                    await institutionRepository.UpdateAsync(institution);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await institutionRepository.Exists(institutionVM.Id))
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
            
            return View(institutionVM);
        }

        // POST: OptionSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await institutionRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
