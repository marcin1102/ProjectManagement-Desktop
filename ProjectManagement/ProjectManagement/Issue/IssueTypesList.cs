using ProjectManagementView.Contracts.Issues.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Issue
{
    public class IssueTypesList
    {
        public static List<string> IssueTypes => Enum.GetNames(typeof(IssueType)).ToList();
    }
}
