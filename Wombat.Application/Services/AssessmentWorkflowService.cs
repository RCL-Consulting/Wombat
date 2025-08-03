using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;        // for Request
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Application.Services
{
    public class AssessmentWorkflowService : IAssessmentWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssessmentRequestRepository _assessmentRequestRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly UserManager<WombatUser> _userManager;
        private readonly IEPARepository _epaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _environment;
        private readonly LinkGenerator _linkGenerator;

        public AssessmentWorkflowService( ApplicationDbContext context, 
                                          IAssessmentRequestRepository assessmentRequestRepository,
                                          INotificationService notificationService,
                                          IMapper mapper,
                                          UserManager<WombatUser> userManager,
                                          IEPARepository epaRepository,
                                          IWebHostEnvironment environment,
                                          LinkGenerator linkGenerator )
        {
            _context = context;
            _assessmentRequestRepository = assessmentRequestRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _userManager = userManager;
            _epaRepository = epaRepository;
            _environment = environment;
            _linkGenerator = linkGenerator;
        }

        private string LoadCreateTemplateAndInsertValues( AssessmentRequestVM assessmentRequestVM,
                                                          string url )
        {
            var templatePath = Path.Combine(_environment.WebRootPath, "Templates", "AssessmentRequest.html");
            var emailTemplate = System.IO.File.ReadAllText(templatePath);
            return emailTemplate
                .Replace("{{assessorName}}", assessmentRequestVM.Assessor?.Name)
                .Replace("{{traineeName}}", assessmentRequestVM.Trainee?.Name ?? "a trainee")
                .Replace("{{epaName}}", assessmentRequestVM.EPA?.Name)
                .Replace("{{link}}", url);
        }

        public async Task<AssessmentRequest> CreateRequestAsync( AssessmentRequestVM model, 
                                                                 string actorId,
                                                                 HttpRequest httpRequest)

        {
            // Map view model to entity
            var request = _mapper.Map<AssessmentRequest>(model);
            request.DateRequested = DateTime.UtcNow;

            // Save request
            await _assessmentRequestRepository.AddAsync(request);
            model.Id = request.Id;           

            // Log event
            var actor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(actorId));
            var evt = new AssessmentEvent
            {
                ActorId = actorId,
                Type = AssessmentEventType.RequestCreated,
                AssessmentRequestId = request.Id,
                Message = string.IsNullOrWhiteSpace(model.ActionComment)
                ? $"Request for EPA '{request.EPA?.Name}' was created."
                : model.ActionComment
            };
            _context.AssessmentEvents.Add(evt);
            await _context.SaveChangesAsync();

            model.Assessor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.AssessorId));
            if (model.Assessor != null && !string.IsNullOrEmpty(model.Assessor.Email))
            {
                model.Trainee = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.TraineeId));
                model.EPA = _mapper.Map<EPAVM>(await _epaRepository.GetAsync(model.EPAId));

                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "Details",
                    controller: "AssessmentRequests",
                    values: new { id = request.Id, requestStatus = AssessmentRequestStatus.Requested }
                );

                string htmlContent = LoadCreateTemplateAndInsertValues(model, url);

                 // Notify assessor
                await _notificationService.NotifyAsync(
                    request.AssessorId,
                    "New Assessment Request",
                     htmlContent);
            }           

            return request;
        }

        private string LoadAcceptTemplateAndInsertValues( AssessmentRequestVM vm,
                                                          string url )
        {
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "AssessmentAccepted.html");
            var html = System.IO.File.ReadAllText(templatePath);
            return html
                .Replace("{{assessorName}}", vm.Assessor?.Name)
                .Replace("{{traineeName}}", vm.Trainee?.Name ?? "you")
                .Replace("{{epaName}}", vm.EPA?.Name)
                .Replace("{{assessorNotes}}", string.IsNullOrWhiteSpace(vm.ActionComment) ? "No comments provided." : vm.ActionComment)
                .Replace("{{link}}",url);
        }

        public async Task<AssessmentRequest> AcceptRequestAsync( AssessmentRequestVM model,
                                                                 string actorId, 
                                                                 HttpRequest httpRequest )
        {
            int requestId = model.Id;

            // Retrieve request
            var request = await _assessmentRequestRepository.GetAsync(requestId);
            if (request == null)
                throw new InvalidOperationException("Assessment request not found.");

            // Mark as accepted
            request.DateAccepted = DateTime.UtcNow;
            await _assessmentRequestRepository.UpdateAsync(request);

            model.Assessor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.AssessorId));
            model.Trainee = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.TraineeId));
            model.EPA = _mapper.Map<EPAVM>(await _epaRepository.GetAsync(model.EPAId));

            // Log event
            var actor = await _userManager.FindByIdAsync(actorId);
            var actorVM = _mapper.Map<WombatUserVM>(actor);
            var epa = await _epaRepository.GetAsync(request.EPAId);

            var evt = new AssessmentEvent
            {
                ActorId = actorId,
                Type = AssessmentEventType.RequestAccepted,
                AssessmentRequestId = request.Id,
                Message = string.IsNullOrWhiteSpace(model.ActionComment)
                ? $"Request for EPA '{request.EPA?.Name}' was accepted."
                : model.ActionComment
            };
            _context.AssessmentEvents.Add(evt);
            await _context.SaveChangesAsync();

            // Notify trainee
            if (!string.IsNullOrEmpty(request.TraineeId))
            {
                var trainee = await _userManager.FindByIdAsync(request.TraineeId);

                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "Details",
                    controller: "AssessmentRequests",
                    values: new { id = request.Id, requestStatus = AssessmentRequestStatus.Requested }
                );
                string html = LoadAcceptTemplateAndInsertValues(model, url);

                await _notificationService.NotifyAsync(
                    trainee.Id,
                    "Assessment Request Accepted",
                    html);
            }

            return request;
        }
        private string LoadAcceptDeclineTemplateAndInsertValues( AssessmentRequestVM vm,
                                                                 string url )
        {
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "AssessmentDeclined.html");
            var html = System.IO.File.ReadAllText(templatePath);
            return html
                .Replace("{{assessorName}}", vm.Assessor?.Name)
                .Replace("{{traineeName}}", vm.Trainee?.Name ?? "you")
                .Replace("{{epaName}}", vm.EPA?.Name)
                .Replace("{{assessorNotes}}", string.IsNullOrWhiteSpace(vm.ActionComment) ? "No comments provided." : vm.ActionComment)
                .Replace("{{link}}", url);
        }

        public async Task<AssessmentRequest> DeclineRequestAsync( AssessmentRequestVM model,
                                                                  string actorId,
                                                                  HttpRequest httpRequest )
        {
            // Retrieve the request
            var request = await _assessmentRequestRepository.GetAsync(model.Id);
            if (request == null)
                throw new InvalidOperationException("Assessment request not found.");

            // Mark as declined
            request.DateDeclined = DateTime.UtcNow;
            await _assessmentRequestRepository.UpdateAsync(request);
            model.Assessor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.AssessorId));
            model.Trainee = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(model.TraineeId));
            model.EPA = _mapper.Map<EPAVM>(await _epaRepository.GetAsync(model.EPAId));

            // Log the event
            var actor = await _userManager.FindByIdAsync(actorId);
            var actorVM = _mapper.Map<WombatUserVM>(actor);
            var epa = await _epaRepository.GetAsync(request.EPAId);

            var evt = new AssessmentEvent
            {
                ActorId = actorId,
                Type = AssessmentEventType.RequestDeclined,
                AssessmentRequestId = request.Id,
                Message = string.IsNullOrWhiteSpace(model.ActionComment)
                ? $"Request for EPA '{request.EPA?.Name}' was declined."
                : model.ActionComment
            };
            _context.AssessmentEvents.Add(evt);
            await _context.SaveChangesAsync();

            // Notify trainee
            if (!string.IsNullOrEmpty(request.TraineeId))
            {
                var trainee = await _userManager.FindByIdAsync(request.TraineeId);

                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "Details",
                    controller: "AssessmentRequests",
                    values: new { id = request.Id, requestStatus = AssessmentRequestStatus.Requested }
                );
                string html = LoadAcceptDeclineTemplateAndInsertValues(model, url);

                await _notificationService.NotifyAsync(
                    trainee.Id,
                    "Assessment Request Declined",
                    html);
            }

            return request;
        }

    }

}
