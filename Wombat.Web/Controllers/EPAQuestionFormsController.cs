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
using static Wombat.Common.Models.EPAQuestionFormVM;

namespace Wombat.Web.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class EPAQuestionFormsController : Controller
    {
        private readonly IEPAQuestionRepository epaQuestionRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IEPARepository epaRepository;
        private readonly IMapper mapper;

        public EPAQuestionFormsController(IEPAQuestionRepository epaQuestionRepository,
                                           IOptionSetRepository optionSetRepository,
                                           IEPARepository epaRepository,
                                           IMapper mapper)
        {
            this.epaQuestionRepository = epaQuestionRepository;
            this.optionSetRepository = optionSetRepository;
            this.epaRepository = epaRepository;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var epas = await epaQuestionRepository.GetAllWithSpecialitiesAndQuestionCountsAsync();

            var vm = epas.Select(e => new EPAQuestionFormVM
            {
                EPAId = e.Id,
                EPAName = e.Name,
                SpecialityName = e.SubSpeciality?.Speciality?.Name ?? "",
                SubSpecialityId = e.SubSpeciality?.Id,
                SubSpecialityName = e.SubSpeciality?.Name ?? "",
                Questions = Enumerable.Range(1, e.QuestionCount)
                    .Select(i => new EPAQuestionFormVM.EPAQuestionVM { DisplayId = i }).ToList()
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int epaId)
        {
            //var epa = await _epaRepo.GetEPAWithSubspecialityAsync(epaId);
            var epa = await epaRepository.GetAsync(epaId);
            if (epa == null)
                return NotFound();

            var questions = (await epaQuestionRepository.GetByEPAIdAsync(epaId))
                .OrderBy(q => q.Rank)
                .ToList();
            var optionSets = await optionSetRepository.GetAllAsync();

            var selectList = optionSets
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Description })
                .ToList();

            var vm = new EPAQuestionFormVM
            {
                EPAId = epa.Id,
                EPAName = epa.Name,
                SpecialityName = epa.SubSpeciality?.Speciality?.Name ?? "",
                SubSpecialityName = epa.SubSpeciality?.Name ?? "",
                SubSpecialityId = epa.SubSpeciality?.Id,
                Questions = questions.Select((q, i) => new EPAQuestionFormVM.EPAQuestionVM
                {
                    Id = q.Id,
                    DisplayId = i + 1,
                    Heading = q.Heading,
                    Description = q.Description,
                    OptionSetId = q.OptionSetId,
                    AvailableOptionSets = selectList
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EPAQuestionFormVM model)
        {
            var existingQuestions = await epaQuestionRepository.GetByEPAIdAsync(model.EPAId);
            var existingIds = existingQuestions.Select(q => q.Id).ToHashSet();

            foreach (var q in model.Questions)
            {
                if (q.Id.HasValue)
                {
                    var entity = existingQuestions.First(x => x.Id == q.Id);
                    entity.Rank = q.DisplayId; // Update rank based on display ID
                    entity.Heading = q.Heading;
                    entity.Description = q.Description;
                    entity.OptionSetId = q.OptionSetId;

                    await epaQuestionRepository.UpdateAsync(entity);
                }
                else
                {
                    await epaQuestionRepository.AddAsync(new STARItem
                    {
                        EPAId = model.EPAId,
                        Heading = q.Heading,
                        Description = q.Description,
                        OptionSetId = q.OptionSetId,
                        Rank = q.DisplayId
                    });
                }
            }

            var updatedIds = model.Questions.Where(q => q.Id.HasValue).Select(q => q.Id!.Value).ToHashSet();
            var toRemove = existingQuestions.Where(q => !updatedIds.Contains(q.Id)).ToList();
            foreach (var q in toRemove)
            {
                await epaQuestionRepository.DeleteAsync(q.Id);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(EPAQuestionFormVM model)
        {
            var nextId = model.Questions.Any()
                ? model.Questions.Max(q => q.DisplayId) + 1
                : 1;

            var optionSets = await optionSetRepository.GetAllAsync();
            var selectList = optionSets.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Description
            }).ToList();

            model.Questions.Add(new EPAQuestionFormVM.EPAQuestionVM
            {
                DisplayId = nextId,
                AvailableOptionSets = selectList
            });

            foreach (var q in model.Questions)
                q.AvailableOptionSets = selectList;

            return PartialView("~/Views/EPAQuestionForms/_EPAQuestionsPartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(EPAQuestionFormVM model, int displayId)
        {
            model.Questions.RemoveAll(q => q.DisplayId == displayId);

            var optionSets = await optionSetRepository.GetAllAsync();
            var selectList = optionSets.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Description
            }).ToList();

            foreach (var q in model.Questions)
                q.AvailableOptionSets = selectList;

            return PartialView("~/Views/EPAQuestionForms/_EPAQuestionsPartial.cshtml", model);
        }
    }
}
