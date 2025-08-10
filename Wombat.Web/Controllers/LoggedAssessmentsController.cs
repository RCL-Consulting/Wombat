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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;
using System;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Application.Services;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize]
    public class LoggedAssessmentsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext context;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly IEPARepository EPARepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAssessmentWorkflowService assessmentWorkflowService;
        private readonly IInstitutionRepository institutionRepository;
        private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMapper mapper;

        public LoggedAssessmentsController(UserManager<WombatUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            ApplicationDbContext context,
                                            ILoggedAssessmentRepository loggedAssessmentRepository,
                                            IEPARepository EPARepository,
                                            IAssessmentFormRepository assessmentFormRepository,
                                            IAssessmentRequestRepository assessmentRequestRepository,
                                            ISpecialityRepository specialityRepository,
                                            ISubSpecialityRepository subSpecialityRepository,
                                            IHttpContextAccessor httpContextAccessor,
                                            IAssessmentWorkflowService assessmentWorkflowService,
                                            IInstitutionRepository institutionRepository,
                                            IEmailSender emailSender,
                                            IWebHostEnvironment webHostEnvironment,
                                            IMapper mapper)
        {
            this.userManager=userManager;
            this.roleManager = roleManager;
            this.context = context;
            this.loggedAssessmentRepository=loggedAssessmentRepository;
            this.EPARepository = EPARepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.httpContextAccessor=httpContextAccessor;
            this.assessmentWorkflowService = assessmentWorkflowService;
            this.institutionRepository = institutionRepository;
            this.emailSender=emailSender;
            this.webHostEnvironment=webHostEnvironment;
            this.mapper=mapper;
            this.subSpecialityRepository = subSpecialityRepository;
            this.specialityRepository = specialityRepository;
        }

        public async Task<IActionResult> PortfolioByEPA(int id, string traineeId)
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(Role.Trainee.ToStringValue()))
            {
                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmentsByEPAAndTraineeAsync(id,userId));
                foreach(var assessment in loggedAssessments)
                {
                    assessment.SetScore();
                }
                return View(loggedAssessments);
            }
            else if (roles.Contains(Role.Coordinator.ToStringValue()))
            {
                var trainee = await userManager.FindByIdAsync(traineeId);
                if( trainee==null || trainee.InstitutionId != user.InstitutionId)
                    return NotFound();

                var epa = await EPARepository.GetAsync(id);
                ViewBag.EPA = epa.Name;
                ViewBag.TraineeName = mapper.Map<WombatUserVM>(trainee).DisplayName;

                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetVisibleAssessmentsPerEPAByTrainee(id, trainee.Id));
                foreach (var assessment in loggedAssessments)
                {
                    assessment.SetScore();
                }
                return View(loggedAssessments);
            }

            return NotFound();
        }

        [Authorize(Roles = RoleStrings.Trainee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PortfolioByEPA(bool isPublic, int assessmentId)
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var assessment = await loggedAssessmentRepository.GetAsync(assessmentId);
            if(assessment == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            int epaId = assessment.EPAId;
            assessment.AssessmentIsPublic = isPublic;
            try
            {
                await loggedAssessmentRepository.UpdateAsync(assessment);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await loggedAssessmentRepository.Exists(assessmentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }                        

            var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmentsByEPAAndTraineeAsync(epaId, userId));
            foreach (var item in loggedAssessments)
            {
                item.SetScore();
            }
            return View(loggedAssessments);
        }

        public async Task<IActionResult> PortfolioByTrainee(string id)
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var trainee = await userManager.FindByIdAsync(id);
            if (trainee == null)
                return NotFound();

            var SubSpeciality = mapper.Map<SubSpecialityVM>(await subSpecialityRepository.GetAsync(trainee.SubSpecialityId));
            if (SubSpeciality == null)
                return NotFound();

            var Speciality = SubSpeciality.Speciality;
            var EPAList = await EPARepository.GetEPAListBySubspeciality(SubSpeciality.Id);
            if (EPAList == null)
                return NotFound();

            var portfolio = new PortfolioVM();

            portfolio.Trainee = mapper.Map<WombatUserVM>(trainee);
            portfolio.EPAList = mapper.Map<List<EPAVM>>(EPAList);
            List<int> EPAIds = EPAList.Select(e => e.Id).ToList();
            
            portfolio.AssessmentsPerEPA = await loggedAssessmentRepository.GetVisibleAssessmentsPerEPAByTrainee(EPAIds, trainee.Id);
            portfolio.ScorePerEPA = await loggedAssessmentRepository.GetVisibleScorePerEPAByTrainee(EPAIds, trainee.Id);

            return View(portfolio);
        }

        public async Task<IActionResult> PortfolioIndex()
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            if(user == null) return NotFound();

            var institutionId = user.InstitutionId;

            var trainees = await userManager.GetUsersInRoleAsync(Role.Trainee.ToStringValue());

            List<WombatUserVM> traineesHere = new List<WombatUserVM>();
            foreach( var item in trainees)
            {
                if (item.InstitutionId == institutionId)
                {
                    traineesHere.Add(mapper.Map<WombatUserVM>(item));
                }
            }

            return View(traineesHere);
        }

        public async Task<IActionResult> AssessorsIndex()
        {
            return View();
        }

        public async Task<IActionResult> MyAssessments()
        {
            if (httpContextAccessor.HttpContext==null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(Role.Trainee.ToStringValue()))
            {
                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmentsByTraineeAsync(userId));
                return View(loggedAssessments);
            }
            else if (roles.Contains(Role.Assessor.ToStringValue()))
            {
                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmentsByAssessorAsync(userId));
                return View(loggedAssessments);
            }
            else
                return NotFound();
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        public async Task<IActionResult> CreateFromList( bool sameSpeciality = false,
                                                         bool sameSubSpeciality = false )
        {
            // who’s asking?
            var assessorId = userManager.GetUserId(User);
            var me = await userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == assessorId);

            if (me == null)
                return Forbid();

            if (me.InstitutionId==null || me.InstitutionId == 0)
                return BadRequest("Your profile is missing an institution; cannot filter trainees.");

            // resolve trainee role id
            var traineeRoleId = await roleManager.Roles
                .Where(r => r.Name == Role.Trainee.ToStringValue())
                .Select(r => r.Id)
                .SingleAsync();

            // base query: trainees in my institution
            var q =
                from u in context.Users.AsNoTracking()
                join ur in context.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                where ur.RoleId == traineeRoleId
                      && u.InstitutionId == me.InstitutionId
                select u;

            // optional speciality / subspeciality narrowing
            if (sameSpeciality && (me.SpecialityId!=null && me.SpecialityId!=0))
                q = q.Where(u => u.SpecialityId == me.SpecialityId);

            if (sameSubSpeciality && (me.SubSpecialityId!=null && me.SubSpecialityId!=0))
                q = q.Where(u => u.SubSpecialityId == me.SubSpecialityId);

            // (optional) skip inactive/locked users if you track that
            // q = q.Where(u => u.IsActive);

            var trainees = await q
                .OrderBy(u => u.Surname).ThenBy(u => u.Name)
                .ToListAsync();

            var vms = mapper.Map<List<WombatUserVM>>(trainees);
            return View(vms);
        }

        // GET: LoggedAssessments/Details/5
        public async Task<IActionResult> Details(int? id, string returnAction, int returnId )
        {
            ViewBag.ReturnAction = returnAction;
            ViewBag.ReturnId = returnId;

            var loggedAssessment = await loggedAssessmentRepository.GetAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            var loggedAssessmentVM = mapper.Map<LoggedAssessmentVM>(loggedAssessment);
            return View(loggedAssessmentVM);
        }

        public async Task<IActionResult> DetailsFromRequest(int? id)
        {
            var loggedAssessment = await loggedAssessmentRepository.GetAssessmentByRequestAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            var loggedAssessmentVM = mapper.Map<LoggedAssessmentVM>(loggedAssessment);
            return View(loggedAssessmentVM);
        }        

        public async Task AddViewDataAsync()
        {
            var EPAs = mapper.Map<List<EPAVM>>(await EPARepository.GetAllAsync());
            ViewData["EPA"] = new SelectList(EPAs, "Id", "Description");

            var assessors = await userManager.GetUsersInRoleAsync(Role.Assessor.ToStringValue());
            ViewData["Assessor"] = new SelectList(assessors, "Id", "Email");

            var trainees = await userManager.GetUsersInRoleAsync(Role.Trainee.ToStringValue());
            ViewData["Trainee"] = new SelectList(trainees, "Id", "Email");
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        // GET: LoggedAssessments/Create
        public async Task<IActionResult> Create()
        {
            await AddViewDataAsync();
            var loggedAssessmentVM = new LoggedAssessmentVM();
            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

        public async Task<IActionResult> GetSubOptions(int mainOptionId)
        {
            var epa = await EPARepository.GetAsync(mainOptionId);

            List<SelectVM> subOptions = new List<SelectVM>();
            foreach( var item in epa?.Forms)
            {
                subOptions.Add(new SelectVM { Id = item.FormId, Name = item.Form.Name });
            }

            return Json(subOptions);
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        public async Task<IActionResult> LogAssessmentFor(string? id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await AddViewDataAsync();
            var loggedAssessmentVM = new LoggedAssessmentVM();
            loggedAssessmentVM.TraineeId = id;
            loggedAssessmentVM.Trainee = mapper.Map<WombatUserVM>(user);

            if (httpContextAccessor.HttpContext!=null)
            {
                loggedAssessmentVM.AssessorId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
                loggedAssessmentVM.Assessor = mapper.Map<WombatUserVM>(await userManager.GetUserAsync(httpContextAccessor.HttpContext.User));
            }
            else
                return NotFound();

            ViewData["EPAList"] = mapper.Map<List<EPAVM>>(await EPARepository.GetAllAsync());

            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        public async Task<IActionResult> LogRequestedAssessment(int id)
        {
            try
            {
                var assessorId = userManager.GetUserId(User);
                var vm = await assessmentWorkflowService.PrepareLogRequestedAssessmentAsync(id, assessorId);

                // Your existing helpers remain in the controller:
                await PopulateAssessment(vm);   // e.g., hydrate item responses, defaults, etc.
                await AddViewDataAsync();       // e.g., dropdowns/select lists for the view

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                // If it's "already completed", you were redirecting Home/Index before
                if (ex.Message.Contains("completed", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Home");

                return NotFound();
            }
        }

        public async Task PopulateAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            loggedAssessmentVM.EPA = mapper.Map<EPAVM>(await EPARepository.GetAsync(loggedAssessmentVM.EPAId));
            loggedAssessmentVM.Trainee = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(loggedAssessmentVM.TraineeId));
            loggedAssessmentVM.Assessor = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(loggedAssessmentVM.AssessorId));

            foreach (var optionCriterion in loggedAssessmentVM.Form.OptionCriteria)
            {
                var optionCriterionResponse = loggedAssessmentVM.OptionCriterionResponses.FirstOrDefault(x => x.CriterionId == optionCriterion.Id);
                if (optionCriterionResponse == null)
                {
                    optionCriterionResponse = new OptionCriterionResponseVM();
                    optionCriterionResponse.Criterion = mapper.Map<OptionCriterionVM>(optionCriterion);
                    if (optionCriterion.OptionsSet.Options.Count > 0)
                        optionCriterionResponse.OptionId = optionCriterion.OptionsSet.Options.First().Id;
                    optionCriterionResponse.CriterionId = optionCriterion.Id;
                    loggedAssessmentVM.OptionCriterionResponses.Add(optionCriterionResponse);
                }
                else
                {
                    optionCriterionResponse.Criterion = mapper.Map<OptionCriterionVM>(optionCriterion);
                    optionCriterionResponse.Option = optionCriterion.OptionsSet.Options.FirstOrDefault(x => x.Id == optionCriterionResponse.OptionId);
                }
            }
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            loggedAssessmentVM.Form = mapper.Map<AssessmentFormVM>(await assessmentFormRepository.GetAsync(loggedAssessmentVM.FormId));

            await PopulateAssessment(loggedAssessmentVM);
            await AddViewDataAsync();
            return View(loggedAssessmentVM);
        }

        [Authorize(Roles = RoleStrings.Assessor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAssessment(loggedAssessmentVM);
                return View(loggedAssessmentVM);
            }

            try
            {
                var assessorId = userManager.GetUserId(User);
                var loggedId = await assessmentWorkflowService.SubmitAssessmentAsync(loggedAssessmentVM, assessorId, Request);
                return RedirectToAction(nameof(MyAssessments));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateAssessment(loggedAssessmentVM);
                return View(loggedAssessmentVM);
            }
            
        }

        // POST: LoggedAssessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleStrings.Assessor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoggedAssessmentVM loggedAssessmentVM)
        {
            if (ModelState.IsValid)
            {
                var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);

                await loggedAssessmentRepository.AddAsync(loggedAssessment);
                return RedirectToAction(nameof(Index));
            }

            await AddViewDataAsync();
            return View(loggedAssessmentVM);
        }

        // GET: LoggedAssessments/Edit/5
        [HttpGet]
        public async Task<IActionResult> CreatePDFFromRequest(int? id)
        {
            var loggedAssessment = await loggedAssessmentRepository.GetAssessmentByRequestAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            return await CreatePDF(loggedAssessment.Id);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePDF(int? id)
        {
            var la = await loggedAssessmentRepository.GetAsync(id);
            if (la == null) return NotFound();

            // ---------- Document + page setup ----------
            var doc = new Document();
            doc.Info.Title = "Work-based Assessment";
            doc.Info.Author = "Wombat";
            var section = doc.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            // Styles
            var normal = doc.Styles["Normal"];
            normal.Font.Name = "Segoe UI";
            normal.Font.Size = 10;

            var h1 = doc.Styles.AddStyle("Heading1", "Normal");
            h1.Font.Size = 16;
            h1.Font.Bold = true;

            var h2 = doc.Styles.AddStyle("Heading2", "Normal");
            h2.Font.Size = 12;
            h2.Font.Bold = true;

            var label = doc.Styles.AddStyle("Label", "Normal");
            label.Font.Bold = true;
            label.Font.Color = Colors.DimGray;

            // ---------- Header (logo + title) ----------
            var headerTable = section.AddTable();
            headerTable.AddColumn(Unit.FromCentimeter(3.5));  // logo column
            headerTable.AddColumn(Unit.FromCentimeter(11.5)); // text column
            headerTable.Rows.LeftIndent = 0;

            var headerRow = headerTable.AddRow();
            headerRow.TopPadding = 2;
            headerRow.BottomPadding = 2;
            headerRow.Cells[0].Borders.Visible = false;
            headerRow.Cells[1].Borders.Visible = false;
            headerRow.BottomPadding = 6; // adds inside-table breathing room

            // Fetch meta
            var institution = await institutionRepository.GetAsync(la.Trainee?.InstitutionId ?? la.Assessor?.InstitutionId);
            var subSpeciality = await subSpecialityRepository.GetAsync(la.Trainee?.SubSpecialityId);
            var speciality = await specialityRepository.GetAsync(subSpeciality?.SpecialityId);

            // --- LOGO (fixed size) ---
            {
                if (ImageSource.ImageSourceImpl == null)
                    ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();

                Paragraph logoPara = headerRow.Cells[0].AddParagraph();

                if (institution?.Logo is { Length: > 0 })
                {
                    var imageSource = ImageSource.FromStream(
                        "institutionLogo", () => new MemoryStream(institution.Logo), 100);
                    var img = logoPara.AddImage(imageSource);
                    img.LockAspectRatio = true;
                    img.Width = Unit.FromCentimeter(3.0);
                    img.Height = Unit.FromCentimeter(3.0);
                }
                else
                {
                    var fallback = Path.Combine(webHostEnvironment.WebRootPath, "pdf", "logo.jpg");
                    if (System.IO.File.Exists(fallback))
                    {
                        var img = logoPara.AddImage(ImageSource.FromFile(fallback, 100));
                        img.LockAspectRatio = true;
                        img.Width = Unit.FromCentimeter(3.0);
                        img.Height = Unit.FromCentimeter(3.0);
                    }
                }
            }

            // --- TITLES (larger fonts) ---
            {
                var institutionName = institution?.Name ?? "Institution";
                var spec = speciality?.Name;
                var subs = subSpeciality?.Name;
                var specLine = (spec, subs) switch
                {
                    (not null, not null) => $"{spec} — {subs}",
                    (not null, null) => spec,
                    (null, not null) => subs,
                    _ => null
                };

                // Institution name (20pt, bold)
                var pInst = headerRow.Cells[1].AddParagraph();
                pInst.Format.Alignment = ParagraphAlignment.Right;   // or Left if you prefer
                pInst.Format.SpaceAfter = Unit.FromPoint(2);
                pInst.AddFormattedText(institutionName, new Font("Segoe UI", 20) { Bold = true });

                // Speciality line (14pt)
                if (!string.IsNullOrWhiteSpace(specLine))
                {
                    var pSpec = headerRow.Cells[1].AddParagraph();
                    pSpec.Format.Alignment = ParagraphAlignment.Right;
                    pSpec.Format.SpaceAfter = Unit.FromPoint(4);
                    pSpec.AddFormattedText(specLine, new Font("Segoe UI", 14));
                }

                // Form title (11pt bold)
                var pForm = headerRow.Cells[1].AddParagraph();
                pForm.Format.Alignment = ParagraphAlignment.Right;
                pForm.Format.SpaceBefore = Unit.FromPoint(2);
                pForm.AddFormattedText("Work‑based Assessment – Feedback Form",
                    new Font("Segoe UI", 11) { Bold = true });
            }

            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = Unit.FromPoint(10);

            // ---------- Info grid ----------
            var info = section.AddTable();
            info.Borders.Width = 0.25;
            info.Borders.Color = Colors.Gainsboro;
            info.LeftPadding = 4;
            info.RightPadding = 4;
            info.TopPadding = 3;
            info.BottomPadding = 3;
            info.Rows.LeftIndent = 0;

            info.AddColumn(Unit.FromCentimeter(4.0));
            info.AddColumn(Unit.FromCentimeter(11.0));

            void InfoRow(string lbl, string? value)
            {
                var r = info.AddRow();
                r.TopPadding = 2;
                r.BottomPadding = 2;
                r.Cells[0].AddParagraph(lbl).Style = "Label";
                r.Cells[1].AddParagraph(value ?? "—");
            }

            InfoRow("Date", la.AssessmentDate.ToString("yyyy-MM-dd HH:mm"));
            var trainee = la.Trainee != null
                ? $"{la.Trainee.Name} {la.Trainee.Surname} ({la.Trainee.Email})"
                : la.TraineeId;
            InfoRow("Trainee", trainee);

            var assessor = la.Assessor != null
                ? $"{la.Assessor.Name} {la.Assessor.Surname} ({la.Assessor.Email})"
                : la.AssessorId;
            InfoRow("Assessor", assessor);

            var epaLabel = la.EPA?.Name ?? "";
            if (!string.IsNullOrWhiteSpace(la.EPA?.Description))
                epaLabel += $" — {la.EPA.Description}";
            InfoRow("EPA", epaLabel);

            section.AddParagraph().AddLineBreak();

            // ---------- Responses heading ----------
            var pHead = section.AddParagraph("Responses");
            pHead.Style = "Heading2";
            pHead.Format.SpaceBefore = 6;
            pHead.Format.SpaceAfter = 4;
            pHead.Format.KeepWithNext = true;


            // ---------- Responses table (zebra, padding, wrapping) ----------
            var resp = section.AddTable();
            resp.Borders.Width = 0.25;
            resp.Borders.Color = Colors.Gainsboro;
            resp.LeftPadding = 4;
            resp.RightPadding = 4;
            resp.TopPadding = 3;
            resp.BottomPadding = 3;
            resp.Rows.LeftIndent = 0;

            resp.AddColumn(Unit.FromCentimeter(7.0));  // Criterion
            resp.AddColumn(Unit.FromCentimeter(8.0));  // Value / Comment

            // header row
            var rh = resp.AddRow();
            rh.Shading.Color = Colors.AliceBlue;
            rh.Format.Font.Bold = true;
            rh.TopPadding = 4;
            rh.BottomPadding = 4;
            rh.Cells[0].AddParagraph("Criterion");
            rh.Cells[1].AddParagraph("Response");
            rh.HeadingFormat = true;

            if (la.OptionCriterionResponses != null)
            {
                bool zebra = false;
                foreach (var r in la.OptionCriterionResponses)
                {
                    string valueText;
                    if (r?.Option != null)
                    {
                        if (r.Criterion?.OptionsSet?.DisplayRank == true)
                            valueText = $"{r.Option.Rank} — {r.Option.Description}";
                        else
                            valueText = r.Option.Description;
                    }
                    else
                    {
                        valueText = string.IsNullOrWhiteSpace(r?.Comment) ? "—" : r!.Comment!;
                    }

                    var row = resp.AddRow();
                    row.TopPadding = 3;
                    row.BottomPadding = 3;
                    if (zebra) row.Shading.Color = Colors.WhiteSmoke;
                    zebra = !zebra;

                    var c0 = row.Cells[0];
                    c0.VerticalAlignment = MigraDocCore.DocumentObjectModel.Tables.VerticalAlignment.Top;
                    var c1 = row.Cells[1];
                    c1.VerticalAlignment = MigraDocCore.DocumentObjectModel.Tables.VerticalAlignment.Top;

                    var p0 = c0.AddParagraph(r?.Criterion?.Description ?? "—");
                    p0.Format.KeepTogether = true;

                    var p1 = c1.AddParagraph(valueText);
                    p1.Format.KeepTogether = true;

                    if (r?.Option != null && !string.IsNullOrWhiteSpace(r?.Comment))
                    {
                        var pc = c1.AddParagraph(r.Comment);
                        pc.Format.Font.Color = Colors.Gray;
                        pc.Format.Font.Size = 9;
                        pc.Format.SpaceBefore = 1;
                        pc.Format.KeepTogether = true; // keep comment with its value
                    }
                }
            }

            // ---------- Footer: page numbers ----------
            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.Alignment = ParagraphAlignment.Center;
            footer.Format.Font.Size = 9;
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();

            // ---------- Render (stream back; avoid temp files) ----------
            var renderer = new PdfDocumentRenderer(unicode: true) { Document = doc };
            renderer.RenderDocument();

            // Security (optional): keep only print
            var pdf = renderer.PdfDocument;
            var sec = pdf.SecuritySettings;
            
            //Locked forever
            sec.OwnerPassword = Guid.NewGuid().ToString("N");

            // Disable all modification options
            sec.PermitAccessibilityExtractContent = false;
            sec.PermitAnnotations = false;
            sec.PermitAssembleDocument = false;
            sec.PermitExtractContent = false;
            sec.PermitFormsFill = false;
            sec.PermitFullQualityPrint = false;
            sec.PermitModifyDocument = false;

            // Allow printing? (true = allow low-res print; false = block printing)
            sec.PermitPrint = true;

            using var stream = new MemoryStream();
            pdf.Save(stream, closeStream: false);
            stream.Position = 0;

            var safeName = MakeSafeFileName($"{la.Trainee?.Surname}_{la.AssessmentDate:yyyyMMdd_HHmm}_Assessment.pdf");
            return File(stream.ToArray(), "application/pdf", safeName);
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }


    }
}
