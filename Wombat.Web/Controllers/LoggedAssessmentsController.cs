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

namespace Wombat.Controllers
{
    [Authorize]
    public class LoggedAssessmentsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IAssessmentContextRepository assessmentContextRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IMapper mapper;

        public LoggedAssessmentsController(UserManager<WombatUser> userManager,
                                            ILoggedAssessmentRepository loggedAssessmentRepository,
                                            IAssessmentContextRepository assessmentContextRepository,
                                            IHttpContextAccessor httpContextAccessor,
                                            IEmailSender emailSender,
                                            IWebHostEnvironment webHostEnvironment,
                                            IMapper mapper)
        {
            this.userManager=userManager;
            this.loggedAssessmentRepository=loggedAssessmentRepository;
            this.assessmentContextRepository=assessmentContextRepository;
            this.httpContextAccessor=httpContextAccessor;
            this.emailSender=emailSender;
            this.webHostEnvironment=webHostEnvironment;
            this.mapper=mapper;
        }

        public async Task<IActionResult> MyAssessments()
        {
            if (httpContextAccessor.HttpContext==null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Trainee))
            {
                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmntsbyTraineeAsync(userId));
                return View(loggedAssessments);
            }
            else if (roles.Contains(Roles.Assessor))
            {
                var loggedAssessments = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAssessmntsbyAssessorAsync(userId));
                return View(loggedAssessments);
            }
            else
                return NotFound();

            return NotFound();
        }

        [Authorize(Roles = Roles.Assessor)]
        public async Task<IActionResult> CreateFromList()
        {
            var trainees = mapper.Map<List<WombatUserVM>>(await userManager.GetUsersInRoleAsync(Roles.Trainee));

            return View(trainees);
        }

        // GET: LoggedAssessments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var loggedAssessment = await loggedAssessmentRepository.GetAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            var loggedAssessmentVM = mapper.Map<LoggedAssessmentVM>(loggedAssessment);
            return View(loggedAssessmentVM);
        }

        public async Task AddViewDataAsync()
        {
            var assessmentContexts = mapper.Map<List<AssessmentContextVM>>(await assessmentContextRepository.GetAllAsync());
            ViewData["AssessmentContext"] = new SelectList(assessmentContexts, "Id", "Description");

            var assessors = await userManager.GetUsersInRoleAsync(Roles.Assessor);
            ViewData["Assessor"] = new SelectList(assessors, "Id", "Email");

            var trainees = await userManager.GetUsersInRoleAsync(Roles.Trainee);
            ViewData["Trainee"] = new SelectList(trainees, "Id", "Email");
        }

        [Authorize(Roles = Roles.Assessor)]
        // GET: LoggedAssessments/Create
        public async Task<IActionResult> Create()
        {
            await AddViewDataAsync();
            var loggedAssessmentVM = new LoggedAssessmentVM();
            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

        [Authorize(Roles = Roles.Assessor)]
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

            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

        public async Task PopulateAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            loggedAssessmentVM.AssessmentContext = mapper.Map<AssessmentContextVM>(await assessmentContextRepository.GetAsync(loggedAssessmentVM.AssessmentContextId));
            loggedAssessmentVM.Trainee = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(loggedAssessmentVM.TraineeId));
            loggedAssessmentVM.Assessor = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(loggedAssessmentVM.AssessorId));
            
            foreach (var optionCriterion in loggedAssessmentVM.AssessmentContext.AssessmentCategory.OptionCriteria)
            {
                var optionCriterionResponse = loggedAssessmentVM.OptionCriterionResponses.FirstOrDefault(x => x.CriterionId == optionCriterion.Id);
                if (optionCriterionResponse == null)
                {
                    optionCriterionResponse = new OptionCriterionResponseVM();
                    optionCriterionResponse.Criterion = mapper.Map<OptionCriterionVM>(optionCriterion);
                    if (optionCriterion.OptionsSet.Options.Count>0)
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

        [Authorize(Roles = Roles.Assessor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            await PopulateAssessment(loggedAssessmentVM);
            await AddViewDataAsync();
            return View(loggedAssessmentVM);
        }

        [Authorize(Roles = Roles.Assessor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            if (ModelState.IsValid)
            {
                if(loggedAssessmentVM.Comment==null)
                {
                    loggedAssessmentVM.Comment = "";
                }
                foreach(var optionCriterionResponse in loggedAssessmentVM.OptionCriterionResponses)
                {
                    if (optionCriterionResponse.Comment==null)
                    {
                        optionCriterionResponse.Comment = "";
                    }
                }
                var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);
                await loggedAssessmentRepository.AddAsync(loggedAssessment);

                var user = await userManager.FindByIdAsync(loggedAssessment.TraineeId);

                await emailSender.SendEmailAsync(user.Email, "Assessment Submitted", "Your assessment has been submitted.");

                return RedirectToAction(nameof(Index));
            }

            await PopulateAssessment(loggedAssessmentVM);
            return View(loggedAssessmentVM);
        }

        // POST: LoggedAssessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = Roles.Assessor)]
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
            paragraph.AddFormattedText("University of Pretoria\r\nDepartment of Paediatrics and Child Health\r\nSteve Biko Academic Hospital\r\nWork based assessment Feedback Form", TextFormat.Bold);

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
            Cells[1].AddParagraph(loggedAssessment.AssessmentContext.Description);

            Cells = table.AddRow().Cells;
            Cells[0].MergeRight = 1;

            // Add content
            foreach (var optionCriterionResponse in loggedAssessment.OptionCriterionResponses)
            {
                string Value = "";
                if (optionCriterionResponse.Criterion.OptionsSet.DisplayRank)
                {
                    Value = optionCriterionResponse.Option.Rank + "-" + optionCriterionResponse.Option.Description;
                }
                else
                {
                    Value = optionCriterionResponse.Option.Description;
                }

                Cells = table.AddRow().Cells;
                Cells[0].AddParagraph(optionCriterionResponse.Criterion.Description);
                Cells[1].AddParagraph(Value);
            }

            Cells = table.AddRow().Cells;
            Cells[0].AddParagraph("Comments");
            Cells[1].AddParagraph(loggedAssessment.Comment);

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
