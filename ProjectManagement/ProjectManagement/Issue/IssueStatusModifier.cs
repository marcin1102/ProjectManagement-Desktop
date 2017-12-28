using ProjectManagement.Infrastructure.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Issue
{
    public class IssueStatusModifier
    {
        private CommandQueryDispatcher commandQueryDispatcher;

        public IssueStatusModifier(CommandQueryDispatcher commandQueryDispatcher)
        {
            this.commandQueryDispatcher = commandQueryDispatcher;
        }

        //public async Task MoveToInProgress(Guid projectId, Guid issueId, )
    }
}
