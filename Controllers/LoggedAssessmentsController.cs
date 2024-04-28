using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Data;
using Wombat.Contracts;
using AutoMapper;
using Wombat.Models;
using Wombat.Repositories;
using Microsoft.AspNetCore.Identity;
using Wombat.Constants;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;

namespace Wombat.Controllers
{
    public class LoggedAssessmentsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IAssessmentContextRepository assessmentContextRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public LoggedAssessmentsController( UserManager<WombatUser> userManager, 
                                            ILoggedAssessmentRepository loggedAssessmentRepository,
                                            IAssessmentContextRepository assessmentContextRepository,
                                            IHttpContextAccessor httpContextAccessor,
                                            IMapper mapper )
        {
            this.userManager=userManager;
            this.loggedAssessmentRepository=loggedAssessmentRepository;
            this.assessmentContextRepository=assessmentContextRepository;
            this.httpContextAccessor=httpContextAccessor;
            this.mapper=mapper;
        }

        // GET: LoggedAssessments
        public async Task<IActionResult> Index()
        {
            var loggedAssessment = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAllAsync());

            return View(loggedAssessment);
            
        }

        public async Task<IActionResult> MyAssessments()
        {
            if(httpContextAccessor.HttpContext==null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);

            var roles = await userManager.GetRolesAsync(user);
            if(roles.Contains(Roles.Trainee))
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

        // GET: LoggedAssessments/Create
        public async Task<IActionResult> Create()
        {
            await AddViewDataAsync();
            var loggedAssessmentVM = new LoggedAssessmentVM();
            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

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
            loggedAssessmentVM.Trainee = user;

            if (httpContextAccessor.HttpContext!=null)
            {
                loggedAssessmentVM.AssessorId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
                loggedAssessmentVM.Assessor = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            }
            else
                return NotFound();           

            loggedAssessmentVM.AssessmentDate = DateTime.Now;
            return View(loggedAssessmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            loggedAssessmentVM.AssessmentContext = mapper.Map<AssessmentContextVM>(await assessmentContextRepository.GetAsync(loggedAssessmentVM.AssessmentContextId));
            loggedAssessmentVM.Trainee = await userManager.FindByIdAsync(loggedAssessmentVM.TraineeId);
            loggedAssessmentVM.Assessor = await userManager.FindByIdAsync(loggedAssessmentVM.AssessorId);

            foreach ( var optionCriterion in loggedAssessmentVM.AssessmentContext.AssessmentCategory.OptionCriteria)
            {
                var optionCriterionResponse = new OptionCriterionResponseVM();
                optionCriterionResponse.Criterion = mapper.Map<OptionCriterionVM>(optionCriterion);
                optionCriterionResponse.OptionId = optionCriterion.OptionsSet.Options.First().Id;
                optionCriterionResponse.CriterionId = optionCriterion.Id;
                loggedAssessmentVM.OptionCriterionResponses.Add(optionCriterionResponse);
            }
            await AddViewDataAsync();
            return View(loggedAssessmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssessment(LoggedAssessmentVM loggedAssessmentVM)
        {
            if (ModelState.IsValid)
            {
                var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);
                await loggedAssessmentRepository.AddAsync(loggedAssessment);
                return RedirectToAction(nameof(Index));
            }

            return View(loggedAssessmentVM);
        }

        // POST: LoggedAssessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( LoggedAssessmentVM loggedAssessmentVM )
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

            string Path = "c:/Junk/docx/";
            string Name = loggedAssessment.Trainee.Id;

            var imagePath = @"c:\Junk\docx\logo.jpg";
            var pdfPath = Path + Name + ".pdf";

            // Create a new PDF document
            var document = new Document();
            var section = document.AddSection();

            // Define table dimensions and column count
            const int rowCount = 3;
            const int columnCount = 2;

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
            var imageSource = ImageSource.FromFile(imagePath,100);
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

        // POST: LoggedAssessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await loggedAssessmentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
