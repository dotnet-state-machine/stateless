using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BugTrackerExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var ioWorkflow = new AgencyApprovalIOWorkflow();
            ioWorkflow.MakeReadyForApproval();
            ioWorkflow.Approve();
            ioWorkflow.Reject();
        }
    }
}
