using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Constants
{
    public enum AssessmentEventType
    {
        RequestCreated,
        RequestAccepted,
        RequestDeclined,
        RequestCancelled,
        AssessmentLogged,
        CommentAdded
    }
}
