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

namespace Wombat.Controllers
{
    public class OptionSetsController : Controller
    {
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;

        public OptionSetsController(IOptionSetRepository optionSetRepository,
                                       IMapper mapper)
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

            if (ModelState.IsValid)
            {
                try
                {
                    var optionSet = mapper.Map<OptionSet>(optionSetVM);
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
