using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;        // for Request
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using System.Security.Policy;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
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
        private readonly IAssessmentFormRepository _assessmentFormRepository;
        private readonly ILoggedAssessmentRepository _loggedAssessmentRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly LinkGenerator _linkGenerator;

        public AssessmentWorkflowService( ApplicationDbContext context, 
                                          IAssessmentRequestRepository assessmentRequestRepository,
                                          INotificationService notificationService,
                                          IMapper mapper,
                                          UserManager<WombatUser> userManager,
                                          IEPARepository epaRepository,
                                          IAssessmentFormRepository assessmentFormRepository,
                                          ILoggedAssessmentRepository loggedAssessmentRepository,
                                          IWebHostEnvironment environment,
                                          LinkGenerator linkGenerator )
        {
            _context = context;
            _assessmentRequestRepository = assessmentRequestRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _userManager = userManager;
            _epaRepository = epaRepository;
            _assessmentFormRepository = assessmentFormRepository;
            _loggedAssessmentRepository = loggedAssessmentRepository;
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
                                                                 HttpRequest httpRequest )
        {
            var request = _mapper.Map<AssessmentRequest>(model);

            // initial state
            request.Status = AssessmentRequestStatus.Requested;
            request.StatusChangedAt = DateTime.UtcNow;

            await _assessmentRequestRepository.AddAsync(request);

            // Log "created"
            _context.AssessmentEvents.Add(new AssessmentEvent
            {
                ActorId = actorId,
                AssessmentRequestId = request.Id,
                Type = AssessmentEventType.RequestCreated,
                Message = string.IsNullOrWhiteSpace(model.ActionComment) ? null : model.ActionComment.Trim(),
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Notify assessor (reuse your existing template)
            var assessor = await _userManager.FindByIdAsync(request.AssessorId);
            if (assessor != null)
            {
                var trainee = await _userManager.FindByIdAsync(request.TraineeId);
                var epa = await _epaRepository.GetAsync(request.EPAId);

                var vm = new AssessmentRequestVM
                {
                    Id = request.Id,
                    Assessor = _mapper.Map<WombatUserVM>(assessor),
                    Trainee = _mapper.Map<WombatUserVM>(trainee),
                    EPA = _mapper.Map<EPAVM>(epa),
                    ActionComment = model.ActionComment
                };

                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "Details",
                    controller: "AssessmentRequests",
                    values: new { id = request.Id, anchor = "events" });

                var html = LoadCreateTemplateAndInsertValues(vm, url);
                await _notificationService.NotifyAsync(assessor.Id, "New Assessment Request", html);
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

        public Task AcceptRequestAsync( AssessmentRequestVM model, string actorId, HttpRequest req)
            => TransitionAsync(model.Id, AssessmentRequestStatus.Accepted, actorId, model.ActionComment, req);

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

        public Task DeclineRequestAsync( AssessmentRequestVM model, string actorId, HttpRequest req )
            => TransitionAsync(model.Id, AssessmentRequestStatus.Declined, actorId, model.ActionComment, req);

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

        public Task CancelRequestAsync( AssessmentRequestVM model, string actorId, HttpRequest req )
            => TransitionAsync(model.Id, AssessmentRequestStatus.Cancelled, actorId, model.ActionComment, req);

        private static readonly Dictionary<AssessmentRequestStatus, AssessmentRequestStatus[]> Allowed =
            new()
            {
                { AssessmentRequestStatus.Requested, new[] { AssessmentRequestStatus.Accepted, AssessmentRequestStatus.Declined, AssessmentRequestStatus.Cancelled } },
                { AssessmentRequestStatus.Accepted,  new[] { AssessmentRequestStatus.Cancelled, AssessmentRequestStatus.Declined, AssessmentRequestStatus.Completed, AssessmentRequestStatus.Requested } },
                { AssessmentRequestStatus.Declined,  new[] { AssessmentRequestStatus.Requested, AssessmentRequestStatus.Accepted } }, // <-- add Accepted
                { AssessmentRequestStatus.Cancelled, Array.Empty<AssessmentRequestStatus>() },
                { AssessmentRequestStatus.Completed, Array.Empty<AssessmentRequestStatus>() }
            };


        public async Task TransitionAsync( int requestId,
                                           AssessmentRequestStatus newStatus,
                                           string actorId,
                                           string? message,
                                           HttpRequest httpRequest )
        {
            var request = await _assessmentRequestRepository.GetAsync(requestId)
                         ?? throw new InvalidOperationException("Assessment request not found.");

            // Guardrails
            if (!Allowed.TryGetValue(request.Status, out var next) || !next.Contains(newStatus))
                throw new InvalidOperationException($"Illegal transition: {request.Status} → {newStatus}");

            if (request.Status == AssessmentRequestStatus.Completed)
                throw new InvalidOperationException("Completed requests are immutable.");

            // Apply
            request.Status = newStatus;
            request.StatusChangedAt = DateTime.UtcNow;

            if (newStatus == AssessmentRequestStatus.Completed && request.CompletionDate == null)
                request.CompletionDate = DateTime.UtcNow;

            await _assessmentRequestRepository.UpdateAsync(request);

            // Log event
            var evtType = newStatus switch
            {
                AssessmentRequestStatus.Requested => AssessmentEventType.RequestCreated,
                AssessmentRequestStatus.Accepted => AssessmentEventType.RequestAccepted,
                AssessmentRequestStatus.Declined => AssessmentEventType.RequestDeclined,
                AssessmentRequestStatus.Cancelled => AssessmentEventType.RequestCancelled,
                AssessmentRequestStatus.Completed => AssessmentEventType.AssessmentLogged,
                _ => AssessmentEventType.CommentAdded
            };

            _context.AssessmentEvents.Add(new AssessmentEvent
            {
                ActorId = actorId,
                AssessmentRequestId = request.Id,
                Type = evtType,
                Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await NotifyOtherPartyAsync(request, actorId, newStatus, httpRequest, message);
        }


        private async Task NotifyOtherPartyAsync( AssessmentRequest request,
                                                  string actorId,
                                                  AssessmentRequestStatus newStatus,
                                                  HttpRequest httpRequest,
                                                  string? message )
        {
            // Resolve actor/recipient
            var actor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(actorId));
            var recipientId = actorId == request.TraineeId ? request.AssessorId : request.TraineeId;
            if (string.IsNullOrEmpty(recipientId)) return;

            var recipient = await _userManager.FindByIdAsync(recipientId);
            var assessor = await _userManager.FindByIdAsync(request.AssessorId);
            var trainee = await _userManager.FindByIdAsync(request.TraineeId);
            var epa = request.EPA ?? await _epaRepository.GetAsync(request.EPAId);

            var vm = new AssessmentRequestVM
            {
                Id = request.Id,
                Assessor = _mapper.Map<WombatUserVM>(assessor),
                Trainee = _mapper.Map<WombatUserVM>(trainee),
                EPA = _mapper.Map<EPAVM>(epa),
                ActionComment = message
            };

            var url = _linkGenerator.GetUriByAction(
                httpContext: httpRequest.HttpContext,
                action: "Details",
                controller: "AssessmentRequests",
                values: new { id = request.Id, anchor = "events" });

            string subject, html;

            switch (newStatus)
            {
                case AssessmentRequestStatus.Accepted:
                    subject = "Assessment Request Accepted";
                    html = LoadAcceptTemplateAndInsertValues(vm, url);
                    break;

                case AssessmentRequestStatus.Declined:
                    subject = "Assessment Request Declined";
                    html = LoadAcceptDeclineTemplateAndInsertValues(vm, url);
                    break;

                case AssessmentRequestStatus.Cancelled:
                    subject = "Assessment Request Cancelled";
                    html = LoadCancelTemplateAndInsertValues(vm, url, actor?.DisplayName ?? actor?.Email);
                    break;

                case AssessmentRequestStatus.Requested:
                    subject = "New Assessment Request";
                    html = LoadCreateTemplateAndInsertValues(vm, url);
                    break;

                case AssessmentRequestStatus.Completed:
                    subject = "Assessment Logged";
                    // (Optional) add a Completed template if you want email on log.
                    html = $"<p>The assessment for <strong>{vm.EPA?.Name}</strong> has been logged.</p><p><a href=\"{url}\">Open</a></p>";
                    break;

                default:
                    return;
            }

            await _notificationService.NotifyAsync(recipient.Id, subject, html);
        }

        // AssessmentWorkflowService.cs
        public async Task<LoggedAssessmentVM> PrepareLogRequestedAssessmentAsync(int requestId, string assessorId)
        {
            var request = await _assessmentRequestRepository.GetAsync(requestId)
                         ?? throw new InvalidOperationException("Assessment request not found.");

            // Domain guards
            if (request.CompletionDate != null)
                throw new InvalidOperationException("Assessment already completed.");

            // Only the assigned assessor (or an admin, if you want to allow that) may log it
            var isAssignedAssessor = !string.IsNullOrEmpty(request.AssessorId) &&
                                      string.Equals(request.AssessorId, assessorId, StringComparison.Ordinal);

            if (!isAssignedAssessor)
                throw new UnauthorizedAccessException("Only the assigned assessor can log this assessment.");

            // Load related data
            var trainee = request.Trainee ?? await _userManager.FindByIdAsync(request.TraineeId);
            var assessor = await _userManager.FindByIdAsync(assessorId);
            var form = request.AssessmentFormId != 0
                            ? await _assessmentFormRepository.GetAsync(request.AssessmentFormId)
                            : null;
            var epa = request.EPA ?? await _epaRepository.GetAsync(request.EPAId);

            if (trainee == null || assessor == null || epa == null)
                throw new InvalidOperationException("Request is missing required links (trainee/assessor/EPA).");

            // Build VM for the view
            var vm = new LoggedAssessmentVM
            {
                AssessmentRequestId = request.Id,

                TraineeId = request.TraineeId,
                Trainee = _mapper.Map<WombatUserVM>(trainee),

                AssessorId = assessorId,
                Assessor = _mapper.Map<WombatUserVM>(assessor),

                EPAId = request.EPAId,
                EPA = _mapper.Map<EPAVM>(epa),

                FormId = request.AssessmentFormId,
                Form = form != null ? _mapper.Map<AssessmentFormVM>(form) : null,

                // default the assessment date to "now" for convenience
                AssessmentDate = DateTime.Now
            };

            return vm;
        }

        private string LoadTemplateAndInsertValues( LoggedAssessmentVM vm,
                                                    string url )
        {
            string templatePath = Path.Combine(_environment.WebRootPath, "Templates", "LoggedAssessment.html");
            var html = System.IO.File.ReadAllText(templatePath);
            return html.Replace("{{link}}", url);
        }

        public async Task<int> SubmitAssessmentAsync( LoggedAssessmentVM vm, 
                                                      string assessorId, 
                                                      HttpRequest httpRequest )
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            AssessmentRequest request = null;
            if (vm.AssessmentRequestId != null && vm.AssessmentRequestId != 0)
            {
                // Load request and guard
                request = await _assessmentRequestRepository.GetAsync(vm.AssessmentRequestId)
                          ?? throw new InvalidOperationException("Assessment request not found.");

                if (request.Status != AssessmentRequestStatus.Accepted)
                    throw new InvalidOperationException("Only accepted requests can be completed.");

                // Authorization: only assigned assessor (tweak if admins/coordinators allowed)
                if (!string.Equals(request.AssessorId, assessorId, StringComparison.Ordinal))
                    throw new UnauthorizedAccessException("Only the assigned assessor may submit this assessment.");
            }

            // Normalize responses (moved out of controller)
            if (vm.OptionCriterionResponses != null)
            {
                foreach (var r in vm.OptionCriterionResponses)
                {
                    if (r.OptionId == 0) r.OptionId = null;
                    r.Comment ??= string.Empty;
                }
            }

            if (vm.AssessmentRequestId == 0)
                vm.AssessmentRequestId = null;

            // Map and persist within a transaction to keep request+assessment consistent
            using var tx = await _context.Database.BeginTransactionAsync();

            var logged = _mapper.Map<LoggedAssessment>(vm);
            // Ensure relationships
            logged.AssessorId = assessorId;

            if (request != null)
            {
                logged.TraineeId = request.TraineeId;
                logged.EPAId = request.EPAId;
                logged.FormId = request.AssessmentFormId;
            }

            await _loggedAssessmentRepository.AddAsync(logged);

            if (request != null)
            {
                // Transition request -> Completed (this stamps CompletionDate in TransitionAsync)
                await TransitionAsync(request.Id, AssessmentRequestStatus.Completed, assessorId,
                                      message: "Assessment submitted.", httpRequest: httpRequest);
            }

            await tx.CommitAsync();

            // Notify trainee (optional: switch to your templated email)
            var trainee = await _userManager.FindByIdAsync(request.TraineeId);
            if (trainee != null)
            {
                var url = _linkGenerator.GetUriByAction(
                    httpContext: httpRequest.HttpContext,
                    action: "DetailsFromRequest",
                    controller: "LoggedAssessments",
                    values: new { id = logged.Id });

                var body = LoadTemplateAndInsertValues (vm, url);
                var subject = "Assessment Submitted";
                await _notificationService.NotifyAsync(trainee.Id, subject, body);
            }

            return logged.Id;
        }

        public async Task RescheduleRequestAsync( int requestId,
                                                  DateTime newAssessmentDateLocal,
                                                  string? comment,
                                                  string actorId,
                                                  HttpRequest httpRequest )
        {
            var request = await _assessmentRequestRepository.GetAsync(requestId)
                         ?? throw new InvalidOperationException("Assessment request not found.");

            var isTrainee = string.Equals(request.TraineeId, actorId, StringComparison.Ordinal);
            var isAssessor = string.Equals(request.AssessorId, actorId, StringComparison.Ordinal);
            var isAdmin = false; // plug in role check if needed

            if (request.Status is AssessmentRequestStatus.Cancelled or AssessmentRequestStatus.Completed)
                throw new InvalidOperationException("Cancelled or completed requests cannot be rescheduled.");

            if (!(isTrainee || isAssessor || isAdmin))
                throw new UnauthorizedAccessException("Only the trainee or the assigned assessor may reschedule.");

            var newDate = newAssessmentDateLocal; // convert to UTC if that’s your convention
            var oldDate = request.AssessmentDate;

            if (oldDate.HasValue && oldDate.Value == newDate) return; // no-op

            request.AssessmentDate = newDate;
            await _assessmentRequestRepository.UpdateAsync(request);

            string message = "";
            if(string.IsNullOrWhiteSpace(comment))
            {
                message = oldDate.HasValue
                    ? $"Assessment date changed from {oldDate.Value:g} to {newDate:g}."
                    : $"Assessment date set to {newDate:g}.";
            }
            else
            {
                message = comment.Trim();
            }

            // Always log the reschedule event
            _context.AssessmentEvents.Add(new AssessmentEvent
            {
                ActorId = actorId,
                AssessmentRequestId = request.Id,
                Type = AssessmentEventType.RequestRescheduled,
                Timestamp = DateTime.UtcNow,
                Message = message
            });
            await _context.SaveChangesAsync();

            // Follow-up state changes
            if (request.Status == AssessmentRequestStatus.Accepted && isTrainee)
            {
                await TransitionAsync(request.Id, AssessmentRequestStatus.Requested, actorId,
                    "Rescheduled by trainee; requires assessor re-acceptance.", httpRequest);
            }
            else if (request.Status == AssessmentRequestStatus.Declined)
            {
                if (isTrainee || isAdmin)
                {
                    await TransitionAsync(request.Id, AssessmentRequestStatus.Requested, actorId,
                        "Rescheduled after decline; requires assessor re-acceptance.", httpRequest);
                }
                else if (isAssessor)
                {
                    await TransitionAsync(request.Id, AssessmentRequestStatus.Accepted, actorId,
                        "Assessor rescheduled and accepted on the new date.", httpRequest);
                }
            }

            await NotifyRescheduleAsync(request, actorId, oldDate, newDate, httpRequest);
        }


        private string LoadRescheduleTemplateAndInsertValues( string recipientName,
                                                              string actorName,
                                                              string epaName,
                                                              string? oldDateStr,
                                                              string newDateStr,
                                                              string? statusNote,
                                                              string url)
        {
            var path = Path.Combine(_environment.WebRootPath, "Templates", "AssessmentRescheduled.html");
            var html = System.IO.File.ReadAllText(path);

            // very simple “templating”
            html = html.Replace("{{recipientName}}", recipientName ?? "there")
                       .Replace("{{actorName}}", actorName ?? "Someone")
                       .Replace("{{epaName}}", epaName ?? "the EPA")
                       .Replace("{{oldDate}}", string.IsNullOrWhiteSpace(oldDateStr) ? "—" : oldDateStr)
                       .Replace("{{newDate}}", newDateStr ?? "—")
                       .Replace("{{link}}", url ?? "#");

            // Optional status note handling (remove block if empty)
            if (string.IsNullOrWhiteSpace(statusNote))
            {
                // naive removal of the note block
                var startTag = "{{#statusNote}}";
                var endTag = "{{/statusNote}}";
                var startIdx = html.IndexOf(startTag, StringComparison.Ordinal);
                var endIdx = html.IndexOf(endTag, StringComparison.Ordinal);
                if (startIdx >= 0 && endIdx > startIdx)
                {
                    html = html.Remove(startIdx, (endIdx + endTag.Length) - startIdx);
                }
            }
            else
            {
                html = html.Replace("{{#statusNote}}", "")
                           .Replace("{{/statusNote}}", "")
                           .Replace("{{statusNote}}", statusNote);
            }

            return html;
        }

        private async Task NotifyRescheduleAsync( AssessmentRequest request,
                                                  string actorId,
                                                  DateTime? oldDate,
                                                  DateTime newDate,
                                                  HttpRequest httpRequest )
        {
            var actor = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(actorId));
            var recipientId = actorId == request.TraineeId ? request.AssessorId : request.TraineeId;
            if (string.IsNullOrEmpty(recipientId)) return;

            var recipient = _mapper.Map<WombatUserVM>(await _userManager.FindByIdAsync(recipientId));
            var epa = request.EPA ?? await _epaRepository.GetAsync(request.EPAId);

            var url = _linkGenerator.GetUriByAction(
                httpContext: httpRequest.HttpContext,
                action: "Details",
                controller: "AssessmentRequests",
                values: new { id = request.Id, anchor = "events" });

            string? statusNote = request.Status switch
            {
                AssessmentRequestStatus.Requested => "This request moved back to Requested and needs to be accepted again.",
                AssessmentRequestStatus.Accepted => "The assessor accepted the request on the new date.",
                _ => null
            };

            var html = LoadRescheduleTemplateAndInsertValues(
                recipientName: recipient?.DisplayName ?? recipient?.Email,
                actorName: actor?.DisplayName ?? actor?.Email,
                epaName: epa?.Name,
                oldDateStr: oldDate.HasValue ? oldDate.Value.ToString("g") : "",
                newDateStr: newDate.ToString("g"),
                statusNote: statusNote,
                url: url
            );

            await _notificationService.NotifyAsync(recipient.Id, "Assessment Request Rescheduled", html);
        }


    }

}
