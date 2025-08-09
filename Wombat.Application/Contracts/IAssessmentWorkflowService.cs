using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IAssessmentWorkflowService
    {
        Task<AssessmentRequest> CreateRequestAsync( AssessmentRequestVM model,
                                                    string actorId,
                                                    HttpRequest httpRequest);
        Task AcceptRequestAsync( AssessmentRequestVM model, 
                                 string actorId,
                                 HttpRequest httpRequest );
        Task DeclineRequestAsync( AssessmentRequestVM model,
                                  string actorId,
                                  HttpRequest httpRequest );
        Task CancelRequestAsync( AssessmentRequestVM model,
                                 string actorId,
                                 HttpRequest httpRequest );
        Task<AssessmentEvent> AddCommentToRequestAsync( int requestId,
                                                        string actorId,
                                                        string comment,
                                                        HttpRequest httpRequest );
        Task RescheduleRequestAsync( int requestId,
                                     DateTime newAssessmentDateLocal,   // incoming local time; convert to UTC if you store UTC
                                     string? comment,
                                     string actorId,
                                     HttpRequest httpRequest);

        Task<LoggedAssessmentVM> PrepareLogRequestedAssessmentAsync( int requestId, 
                                                                     string assessorId );
        Task<int> SubmitAssessmentAsync( LoggedAssessmentVM vm, 
                                         string assessorId, 
                                         HttpRequest httpRequest );
    }

}
