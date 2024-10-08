﻿using System;
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
    [Authorize(Roles = Roles.Administrator)]
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specialities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecialityVM specialityVM)
        {
            if (ModelState.IsValid)
            {
                var speciality = mapper.Map<Speciality>(specialityVM);

                if (speciality.SubSpecialities != null)
                {
                    foreach (var item in speciality.SubSpecialities)
                    {
                        item.Speciality = speciality;
                    }
                }

                await specialityRepository.AddAsync(speciality);
                return RedirectToAction(nameof(Index));
            }
            return View(specialityVM);
        }

        // GET: Specialities/Edit/5
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
        public async Task<IActionResult> DeleteSubSpeciality(SpecialityVM specialityVM, int id)
        {
            specialityVM.SubSpecialities?.RemoveAll(s => s.DisplayId == id);
            return PartialView("SubSpeciality", specialityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubSpeciality( SpecialityVM specialityVM )
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await specialityRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}