using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Contracts;
using Wombat.Data;
using Wombat.Models;
using Wombat.Repositories;

namespace Wombat.Controllers
{
    public class EnumCriteriaController : Controller
    {
        private readonly IEnumCriteriaRepository enumCriteriaRepository;
        private readonly IMapper mapper;

        public EnumCriteriaController( IEnumCriteriaRepository enumCriteriaRepository,
                                       IMapper mapper )
        {
            this.enumCriteriaRepository=enumCriteriaRepository;
            this.mapper=mapper;
        }

        // GET: EnumCriteria
        public async Task<IActionResult> Index()
        {
            var model = mapper.Map<List<EnumCriterionVM>>(await enumCriteriaRepository.GetAllAsync());
            return View(model);
        }

        // GET: EnumCriteria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var enumCriterion = await enumCriteriaRepository.GetAsync(id);
            if (enumCriterion == null)
            {
                return NotFound();
            }

            var enumCriterionRepositoryVM = mapper.Map<EnumCriterionVM>(enumCriterion);
            return View(enumCriterionRepositoryVM);
        }

        // GET: EnumCriteria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EnumCriteria/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnumCriterionVM enumCriterionVM)
        {
            if (ModelState.IsValid)
            {
                var enumCriterion = mapper.Map<EnumCriterion>(enumCriterionVM);
                await enumCriteriaRepository.AddAsync(enumCriterion);
                return RedirectToAction(nameof(Index));
            }
            return View(enumCriterionVM);
        }

        // GET: EnumCriteria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var enumCriterion = await enumCriteriaRepository.GetAsync(id);
            if (enumCriterion == null)
            {
                return NotFound();
            }

            var model = mapper.Map<EnumCriterionVM>(enumCriterion);
            return View(model);
        }

        // POST: EnumCriteria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnumCriterionVM enumCriterionVM)
        {

            if (id != enumCriterionVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var enumCriterion = mapper.Map<EnumCriterion>(enumCriterionVM);
                    await enumCriteriaRepository.UpdateAsync(enumCriterion);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await enumCriteriaRepository.Exists(enumCriterionVM.Id))
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
            return View(enumCriterionVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await enumCriteriaRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
