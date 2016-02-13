using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkDistributionApp.Models
{
    public enum PriorityType
    {
        Lowest, Low, Normal, High, Urgent
    }

    public enum StatusType
    {
        Open, Process, Delay, Processed, End 
    }

    public enum TeamQueueType
    {
        Claims, 
        [Display(Name="New Business")]
        New_Business,
        Servicing
    }
    public class WorkObject
    {
        // PK
        public int WorkObjectID { get; set; }
        
        // unique ID property for a specific business e.g. polisy no for insurance company or pps no for hospital clients
        [Required(ErrorMessage="{0} is required")]
        [Display(Name="Policy No")]
        public String PolicyNo { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name="Client Name ")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z''-'\s]*$")]
        public String ClientName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Client Surname ")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z''-'\s]*$")]
        public String ClientSurname { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        [Display(Name = "Client DOB ")]
        public DateTime ClientDOB { get; set; }

        // start date of the Work Object, needed e.g. for increasing priority level over time 
        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        // allocate work object to a specific team within an organization
        [Required]
        [Display(Name = "Team Queue")]
        public TeamQueueType TeamQueue { get; set; }

        [Required]
        public StatusType Status { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime? CloseDate { get; set; }

        [DataType(DataType.EmailAddress)]
        public String CloseBy { get; set; }

        [Required]
        public PriorityType Priority { get; set; }

        [Display(Name = "Created by")]
        [DataType(DataType.EmailAddress)]
        public String CreatedBy { get; set; }

        public bool isLocked { get; set; }

        [DataType(DataType.EmailAddress)]
        public String LockedBy { get; set; }

        public DateTime? LockedTime { get; set;}

        // navigation properties
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }

    }
}