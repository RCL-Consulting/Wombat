using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Constants;
using Wombat.Contracts;
using Wombat.Data;
using Wombat.Models;
using Wombat.Repositories;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class TextCriteriaController : Controller
    {
        private readonly ITextCriteriaRepository textCriteriaRepository;
        private readonly IMapper mapper;

        public TextCriteriaController( ITextCriteriaRepository textCriteriaRepository,
                                       IMapper mapper )
        {
            this.textCriteriaRepository=textCriteriaRepository;
            this.mapper=mapper;
        }

        // GET: TextCriteria
        public async Task<IActionResult> Index()
        {
            var model = mapper.Map<List<TextCriterionVM>>(await textCriteriaRepository.GetAllAsync());
            return View(model);
        }

        // GET: TextCriteria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var textCriterion = await textCriteriaRepository.GetAsync(id);
            if (textCriterion == null)
            {
                return NotFound();
            }

            var textCriterionRepositoryVM = mapper.Map<TextCriterionVM>(textCriterion);
            return View(textCriterionRepositoryVM);
        }

        // GET: TextCriteria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TextCriteria/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TextCriterionVM textCriterionVM)
        {
            if (ModelState.IsValid)
            {
                var textCriterion = mapper.Map<TextCriterion>(textCriterionVM);
                await textCriteriaRepository.AddAsync(textCriterion);
                return RedirectToAction(nameof(Index));
            }
            return View(textCriterionVM);
        }

        // GET: TextCriteria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var textCriterion = await textCriteriaRepository.GetAsync(id);
            if (textCriterion == null)
            {
                return NotFound();
            }

            var textCriterionVM = mapper.Map<TextCriterionVM>(textCriterion);
            return View(textCriterionVM);
        }

        // POST: TextCriteria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TextCriterionVM textCriterionVM)
        {

            if (id != textCriterionVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var textCriterion = mapper.Map<TextCriterion>(textCriterionVM);
                    await textCriteriaRepository.UpdateAsync(textCriterion);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await textCriteriaRepository.Exists(textCriterionVM.Id))
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
            return View(textCriterionVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await textCriteriaRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
