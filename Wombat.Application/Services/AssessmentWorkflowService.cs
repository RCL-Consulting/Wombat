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

        private string LoadRequestCommentTemplateAndInsertValues( string commenterName,
                                                                  string recipientName,
                                                                  string epaName,
                                                                  string comment,
                                                                  string url )
        {
            var templatePath = Path.Combine(_environment.WebRootPath, "Templates", "RequestCommentAdded.html");
            var html = System.IO.File.ReadAllText(templatePath);

            return html
                .Replace("{{commenterName}}", commenterName ?? "Someone")
                .Replace("{{recipientName}}", recipientName ?? "there")
                .Replace("{{epaName}}", epaName ?? "the EPA")
                .Replace("{{comment}}", string.IsNullOrWhiteSpace(comment) ? "(no message provided)" : comment)
                .Replace("{{link}}", url ?? "#");
        }

        // Add comment to AssessmentRequest + notify the "other side"
        public async Task<AssessmentEvent> AddCommentToRequestAsync( int requestId,
                                                                     string actorId,
                                                                     string comment,
                                                                     HttpRequest httpRequest )
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment must not be empty.", nameof(comment));

            var request = await _assessmentRequestRepository.GetAsync(requestId);
            if (request == null)
                throw new InvalidOperationException("Assessment request not found.");

            // Persist event
            var evt = new AssessmentEvent
            {
                ActorId = actorId,
                Type = AssessmentEventType.CommentAdded, // ensure this enum value exists
                AssessmentRequestId = request.Id,
                Message = comment.Trim(),
                Timestamp = DateTime.UtcNow
            };

            _context.AssessmentEvents.Add(evt);
            await _context.SaveChangesAsync();

            // Build notification
            var actor = _mapper.Map<WombatUserVM> (await _userManager.FindByIdAsync(actorId));
            var commenterName = actor?.DisplayName ?? actor?.Email ?? "Someone";

            // Notify the opposite party if present
            string recipientId = null;
            if (!string.IsNullOrEmpty(request.TraineeId) && !string.Equals(request.TraineeId, actorId, StringComparison.Ordinal))
                recipientId = request.TraineeId;
            else if (!string.IsNullOrEmpty(request.AssessorId) && !string.Equals(request.AssessorId, actorId, StringComparison.Ordinal))
                recipientId = request.AssessorId;

            if (!string.IsNullOrEmpty(recipientId))
            {
                var recipient = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(recipientId));

                // Resolve EPA name (if not loaded on request)
                var epaName = request.EPA?.Name;
                if (string.IsNullOrEmpty(epaName) && request.EPAId != 0)
                {
                    var epa = await _epaRepository.GetAsync(request.EPAId);
                    epaName = epa?.Name;
                }

                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "Details",
                    controller: "AssessmentRequests",
                    values: new { id = request.Id, anchor = "events" }
                );

                var html = LoadRequestCommentTemplateAndInsertValues(
                    commenterName: commenterName,
                    recipientName: recipient?.DisplayName ?? recipient?.Email,
                    epaName: epaName,
                    comment: comment,
                    url: url
                );

                await _notificationService.NotifyAsync(
                    recipient.Id,
                    "New comment on your assessment request",
                    html);
            }

            return evt;
        }

        private string LoadCancelTemplateAndInsertValues(AssessmentRequestVM vm, string url, string actorName)
        {
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "AssessmentCancelled.html");
            var html = System.IO.File.ReadAllText(templatePath);
            return html
                .Replace("{{actorName}}", actorName ?? "The trainee")
                .Replace("{{assessorName}}", vm.Assessor?.Name)
                .Replace("{{traineeName}}", vm.Trainee?.Name ?? "you")
                .Replace("{{epaName}}", vm.EPA?.Name)
                .Replace("{{comment}}", string.IsNullOrWhiteSpace(vm.ActionComment) ? "No comments provided." : vm.ActionComment)
                .Replace("{{link}}", url);
        }

        public async Task<AssessmentRequest> CancelRequestAsync( AssessmentRequestVM model,
                                                                 string actorId,
                                                                 HttpRequest httpRequest )
        {
            var request = await _assessmentRequestRepository.GetAsync(model.Id)
                  ?? throw new InvalidOperationException("Assessment request not found.");

            if (request.CompletionDate != null) throw new InvalidOperationException("Completed requests cannot be cancelled.");
            if (request.DateAccepted == null) throw new InvalidOperationException("Only accepted requests can be cancelled.");

            // Treat cancel as decline (fast path)
            request.DateCancelled = DateTime.UtcNow;
            await _assessmentRequestRepository.UpdateAsync(request);

            // Event logs who cancelled (assessor or trainee)
            _context.AssessmentEvents.Add(new AssessmentEvent
            {
                ActorId = actorId,
                Type = AssessmentEventType.RequestCancelled,
                AssessmentRequestId = request.Id,
                Message = string.IsNullOrWhiteSpace(model.ActionComment)
                    ? $"Request for EPA '{request.EPA?.Name}' was cancelled."
                    : model.ActionComment,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Notify the other side
            var recipientId = actorId == request.TraineeId ? request.AssessorId : request.TraineeId;
            if (!string.IsNullOrEmpty(recipientId))
            {
                model.Assessor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(request.AssessorId));
                model.Trainee = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(request.TraineeId));
                model.EPA = _mapper.Map<EPAVM>(await _epaRepository.GetAsync(request.EPAId));

                var actor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(actorId));
                var url = _linkGenerator.GetUriByAction(httpRequest.HttpContext, "Details", "AssessmentRequests",
                                                        new { id = request.Id, anchor = "events" });

                var html = LoadCancelTemplateAndInsertValues(model, url, actor?.DisplayName ?? actor?.Email);
                await _notificationService.NotifyAsync(recipientId, "Assessment Request Cancelled", html);
            }

            return request;
        }
    }

}
