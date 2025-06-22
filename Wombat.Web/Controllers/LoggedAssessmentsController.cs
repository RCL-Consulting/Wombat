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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.Rendering;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;
using Wombat.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Wombat.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Wombat.Controllers
{
    [Authorize]
    public class LoggedAssessmentsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly IEPARepository EPARepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMapper mapper;

        public LoggedAssessmentsController(UserManager<WombatUser> userManager,
                                            ILoggedAssessmentRepository loggedAssessmentRepository,
                                            IEPARepository EPARepository,
                                            IAssessmentFormRepository assessmentFormRepository,
                                            IAssessmentRequestRepository assessmentRequestRepository,
                                            ISpecialityRepository specialityRepository,
                                            ISubSpecialityRepository subSpecialityRepository,
                                            IHttpContextAccessor httpContextAccessor,
                                            IEmailSender emailSender,
                                            IWebHostEnvironment webHostEnvironment,
                                            IMapper mapper)
        {
            this.userManager=userManager;
            this.loggedAssessmentRepository=loggedAssessmentRepository;
            this.EPARepository = EPARepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.httpContextAccessor=httpContextAccessor;
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
        public async Task<IActionResult> CreateFromList()
        {
            var trainees = mapper.Map<List<WombatUserVM>>(await userManager.GetUsersInRoleAsync(Role.Trainee.ToStringValue()));

            return View(trainees);
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
            var assessmentRequest = await assessmentRequestRepository.GetAsync(id);
            if (assessmentRequest == null)
            {
                return NotFound();
            }

            if (assessmentRequest.CompletionDate != null)
                return RedirectToAction("Home","Index");

            var user = assessmentRequest.Trainee;
            if (user == null)
            {
                return NotFound();
            }

            await AddViewDataAsync();
            var loggedAssessmentVM = new LoggedAssessmentVM();
            loggedAssessmentVM.TraineeId = assessmentRequest.TraineeId;
            loggedAssessmentVM.Trainee = mapper.Map<WombatUserVM>(user);
            loggedAssessmentVM.FormId = assessmentRequest.AssessmentFormId;
            loggedAssessmentVM.AssessorId = assessmentRequest.AssessorId;
            loggedAssessmentVM.EPAId = assessmentRequest.EPAId;
            loggedAssessmentVM.AssessmentRequestId = id;

            if (httpContextAccessor.HttpContext != null)
            {
                loggedAssessmentVM.AssessorId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
                loggedAssessmentVM.Assessor = mapper.Map<WombatUserVM>(await userManager.GetUserAsync(httpContextAccessor.HttpContext.User));
            }
            else
                return NotFound();

            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            loggedAssessmentVM.Form = mapper.Map<AssessmentFormVM>(await assessmentFormRepository.GetAsync(loggedAssessmentVM.FormId));

            await PopulateAssessment(loggedAssessmentVM);
            await AddViewDataAsync();

            return View(loggedAssessmentVM);
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
            if (ModelState.IsValid)
            {
                foreach (var optionCriterionResponse in loggedAssessmentVM.OptionCriterionResponses)
                {
                    if(optionCriterionResponse.OptionId==0)
                        optionCriterionResponse.OptionId = null;
                    
                    if (optionCriterionResponse.Comment==null)
                    {
                        optionCriterionResponse.Comment = "";
                    }
                }
                var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);
                await loggedAssessmentRepository.AddAsync(loggedAssessment);

                var assessmentRequest = await assessmentRequestRepository.GetAsync(loggedAssessmentVM.AssessmentRequestId);
                assessmentRequest.CompletionDate = DateTime.Now;
                await assessmentRequestRepository.UpdateAsync(assessmentRequest);

                var user = await userManager.FindByIdAsync(loggedAssessment.TraineeId);

                await emailSender.SendEmailAsync(user.Email, "Assessment Submitted", "Your assessment has been submitted.");

                return RedirectToAction(nameof(MyAssessments));
            }

            await PopulateAssessment(loggedAssessmentVM);
            return View(loggedAssessmentVM);
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
            var loggedAssessment = await loggedAssessmentRepository.GetAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            string Name = loggedAssessment.Trainee.Id;

            var imagePath = Path.Combine(webHostEnvironment.WebRootPath, "pdf", "logo.jpg");
            var pdfPath = Path.Combine(webHostEnvironment.WebRootPath, "pdf", Name+".pdf");

            // Create a new PDF document
            var document = new Document();
            var section = document.AddSection();

            // Create a table
            var table = section.AddTable();
            table.Borders.Width = 0.75; // Set border width

            // Add columns
            table.AddColumn(Unit.FromCentimeter(7)); // Adjust column width as needed
            table.AddColumn(Unit.FromCentimeter(10));

            // Add headers
            var Row = table.AddRow();
            Row.Height = Unit.FromCentimeter(5);
            var cell = Row.Cells[0];
            cell.Format.Font.Bold = true;
            cell.Borders.Right.Visible = false;
            if (ImageSource.ImageSourceImpl == null)
            {
                ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();
            }
            var imageSource = ImageSource.FromFile(imagePath, 100);
            var paragraph = cell.AddParagraph();
            var image = paragraph.AddImage(imageSource);
            image.LockAspectRatio = true;
            image.Width = Unit.FromCentimeter(5); // Adjust image width as needed
            image.Height = Unit.FromCentimeter(5);
            image.WrapFormat.Style = MigraDocCore.DocumentObjectModel.Shapes.WrapStyle.Through;

            cell = Row.Cells[1];
            cell.VerticalAlignment = MigraDocCore.DocumentObjectModel.Tables.VerticalAlignment.Center;
            paragraph = cell.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.AddFormattedText("ABC University\r\nDepartment of Paediatrics and Child Health\r\n\r\nWork based assessment Feedback Form", TextFormat.Bold);

            var Cells = table.AddRow().Cells;
            Cells[0].AddParagraph("Date");
            Cells[1].AddParagraph(loggedAssessment.AssessmentDate.ToString("dd/MM/yyyy"));

            Cells = table.AddRow().Cells;
            Cells[0].AddParagraph("Trainee");
            Cells[1].AddParagraph(loggedAssessment.Trainee.Email);

            Cells = table.AddRow().Cells;
            Cells[0].AddParagraph("Assessor");
            Cells[1].AddParagraph(loggedAssessment.Assessor.Email);

            Cells = table.AddRow().Cells;
            Cells[0].AddParagraph("Type of assessment");
            Cells[1].AddParagraph(loggedAssessment.EPA.Description);

            Cells = table.AddRow().Cells;
            Cells[0].MergeRight = 1;

            // Add content
            foreach (var optionCriterionResponse in loggedAssessment.OptionCriterionResponses)
            {
                string Value = "";
                if (optionCriterionResponse.Option!=null)
                {
                    if (optionCriterionResponse.Criterion.OptionsSet.DisplayRank)
                    {
                        Value = optionCriterionResponse.Option.Rank + "-" + optionCriterionResponse.Option.Description;
                    }
                    else
                    {
                        Value = optionCriterionResponse.Option.Description;
                    }
                }
                else
                    Value = optionCriterionResponse.Comment; 

                Cells = table.AddRow().Cells;
                Cells[0].AddParagraph(optionCriterionResponse.Criterion.Description);
                Cells[1].AddParagraph(Value);
            }

            // Render the PDF document
            var pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            // Save the PDF document to a file or stream
            var pdfdoc = pdfRenderer.PdfDocument;
            PdfSharpCore.Pdf.Security.PdfSecuritySettings securitySettings = pdfdoc.SecuritySettings;

            securitySettings.OwnerPassword = "iowurhf3w74538q9475432qythiwl";

            securitySettings.PermitAccessibilityExtractContent = false;
            securitySettings.PermitAnnotations = false;
            securitySettings.PermitAssembleDocument = false;
            securitySettings.PermitExtractContent = false;
            securitySettings.PermitFormsFill = false;
            securitySettings.PermitFullQualityPrint = false;
            securitySettings.PermitModifyDocument = false;
            securitySettings.PermitPrint = true;

            pdfdoc.Save(pdfPath);

            // Return the PDF file
            var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            return File(pdfBytes, "application/pdf", pdfPath);
        }

    }
}
