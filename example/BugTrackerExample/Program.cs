using System;

namespace BugTrackerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioWorkflow = new AgencyApprovalIOWorkflow();

            string graph = ioWorkflow.ToDotGraph();
            try
            {
                ioWorkflow.MakeReadyForApproval(new AgencyApprovalIOWorkflow.MakeReadyForAgencyApprovalParameters());
                ioWorkflow.Reject();
                ioWorkflow.MakeReadyForApproval(new AgencyApprovalIOWorkflow.MakeReadyForAgencyApprovalParameters());
                ioWorkflow.Approve();
                ioWorkflow.Reject();
            }
            catch (Exception e)
            {
            }
        }
    }
}