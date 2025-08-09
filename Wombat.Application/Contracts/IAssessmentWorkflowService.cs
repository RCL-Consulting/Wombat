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
        Task<AssessmentRequest> AcceptRequestAsync( AssessmentRequestVM model, 
                                                    string actorId,
                                                    HttpRequest httpRequest );
        Task<AssessmentRequest> DeclineRequestAsync( AssessmentRequestVM model,
                                                     string actorId,
                                                     HttpRequest httpRequest );

        Task<AssessmentEvent> AddCommentToRequestAsync( int requestId,
                                                        string actorId,
                                                        string comment,
                                                        HttpRequest httpRequest );

        Task<AssessmentRequest> CancelRequestAsync(AssessmentRequestVM model,
                                                    string actorId,
                                                    HttpRequest httpRequest);
        //Task<LoggedAssessment> LogAssessmentAsync(LoggedAssessmentVM model, string actorId);
        //Task<IEnumerable<AssessmentEvent>> GetTimelineForUserAsync(string userId);
    }

}
