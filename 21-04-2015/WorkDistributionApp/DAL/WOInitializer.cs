using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WorkDistributionApp.Models;

namespace WorkDistributionApp.DAL
{
    public class WOInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<WorkObjectContext>
    {
        protected override void Seed(WorkObjectContext context)
        {
            var workobjects = new List<WorkObject>
            {
                // workObject 1
                new WorkObject { PolicyNo="F123456",ClientName="John",ClientSurname="Fitzgerald", ClientDOB=DateTime.Parse("1975-09-01"), 
                                StartDate=DateTime.Parse("2015-04-21 07:15:00"), TeamQueue=TeamQueueType.Servicing, Status=StatusType.Open, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Lowest, CreatedBy="admin@example.com", isLocked=false, LockedBy="", LockedTime=null},
                // workObject 2
                new WorkObject { PolicyNo="A001234",ClientName="Sarah",ClientSurname="Murphy", ClientDOB=DateTime.Parse("1976-12-21"), 
                                StartDate=DateTime.Parse("2015-04-20 14:01:00"), TeamQueue=TeamQueueType.Servicing, Status=StatusType.Process, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Lowest, CreatedBy="slav@example.com", isLocked=false, LockedBy="", LockedTime=null},
               // workObject 3
                new WorkObject { PolicyNo="1028",ClientName="Anthony",ClientSurname="O'Sullivan", ClientDOB=DateTime.Parse("1962-01-14"), 
                                StartDate=DateTime.Parse("2015-04-19 07:45:00"), TeamQueue=TeamQueueType.New_Business, Status=StatusType.Open, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Lowest, CreatedBy="alan@example.com",isLocked=false, LockedBy="", LockedTime=null },
               // workObject 4
                new WorkObject { PolicyNo="1029",ClientName="Caroline",ClientSurname="Griffin", ClientDOB=DateTime.Parse("1982-05-25"), 
                                StartDate=DateTime.Parse("2015-04-18 08:01:00"), TeamQueue=TeamQueueType.New_Business, Status=StatusType.Open, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Lowest, CreatedBy="alan@example.com", isLocked=false, LockedBy="", LockedTime=null },
               // workObject 5
                new WorkObject { PolicyNo="BB1234",ClientName="Nigel",ClientSurname="Kelly", ClientDOB=DateTime.Parse("1965-03-12"), 
                                StartDate=DateTime.Parse("2015-04-17 10:21:00"), TeamQueue=TeamQueueType.Claims, Status=StatusType.Open, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Lowest, CreatedBy="tom@example.com",isLocked=false, LockedBy="", LockedTime=null },
               // workObject 6
                new WorkObject { PolicyNo="A001221",ClientName="Richard",ClientSurname="O'Reilly", ClientDOB=DateTime.Parse("1952-03-12"), 
                                StartDate=DateTime.Parse("2014-10-11 13:21:00"), TeamQueue=TeamQueueType.Claims, Status=StatusType.Process, CloseDate=null, CloseBy = "",
                                Priority=PriorityType.Normal, CreatedBy="admin@example.com", isLocked=false, LockedBy="", LockedTime=null },
            };

            workobjects.ForEach(s => context.WorkObjects.Add(s));
            context.SaveChanges();

            var comments = new List<Comment>
            {
                // workObject 1
                new Comment{ WorkObjectID=1, Content="Client requested current value of the policy", Timestamp=DateTime.Parse("2015-04-21 07:15:00"), User="admin@example.com" },
                // workObject 2
                new Comment{ WorkObjectID=2, Content="Client requested change of correspondence address", Timestamp=DateTime.Parse("2015-04-20 14:01:12"), User="slav@example.com" },
                new Comment{ WorkObjectID=2, Content="Client asked to provide proof of address", Timestamp=DateTime.Parse("2015-04-21 08:00:01"), User="alan@example.com" },
                new Comment{ WorkObjectID=2, Content="*** Status changed: Open to Process ***", Timestamp=DateTime.Parse("2015-04-21 08:00:01"), User="alan@example.com" },
                // workObject 3
                new Comment{ WorkObjectID=3, Content="Client would like to set up a savings account", Timestamp=DateTime.Parse("2015-04-19 07:45:57"), User="alan@example.com" },
                // workObject 4
                new Comment{ WorkObjectID=4, Content="Client wants to set up a personal pension plan", Timestamp=DateTime.Parse("2015-04-18 08:01:30"), User="alan@example.com" },
                // workObject 5
                new Comment{ WorkObjectID=5, Content="Client wants to withdraw €2000", Timestamp=DateTime.Parse("2015-04-17 10:21:00"), User="tom@example.com" },
                new Comment{ WorkObjectID=5, Content="Client called making a complaint that haven't recieved his fund yet. Claim needs to be processed urgently", Timestamp=DateTime.Parse("2015-04-17 15:02:00"), User="rita@example.com" },
                new Comment{ WorkObjectID=5, Content="*** Priority changed: Normal to Urgent ***", Timestamp=DateTime.Parse("2015-04-17 15:02:00"), User="rita@example.com" },
                // workObject 6
                new Comment{ WorkObjectID=6, Content="Client wants to transfer out to SunLife Assurance", Timestamp=DateTime.Parse("2014-10-11 13:21:00"), User="admin@example.com" },
                new Comment{ WorkObjectID=6, Content="*** Priority changed: Normal to High***", Timestamp=DateTime.Parse("2014-10-18 13:21:00"), User="WDS Service" },
                new Comment{ WorkObjectID=6, Content="Confirmation from SunLife Assurance required, letter sent", Timestamp=DateTime.Parse("2014-10-19 09:10:00"), User="slav@example.com" },
                new Comment{ WorkObjectID=6, Content="*** Status changed: Open to Process ***", Timestamp=DateTime.Parse("2014-10-19 09:10:00"), User="slav@example.com" }
            };

            comments.ForEach(s => context.Comments.Add(s));
            context.SaveChanges();
        }
    }
}