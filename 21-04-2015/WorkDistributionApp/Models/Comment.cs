using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkDistributionApp.Models
{
    public class Comment
    {
        // PK
        public int CommentID { get; set; }
        //FK
        public int WorkObjectID { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name= "Content")]
        //[DataType(DataType.MultilineText)]
        public String Content { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Timestamp { get; set; }

        [Display(Name="Added by")]
        [DataType(DataType.EmailAddress)]
        public String User { get; set; }

        // navigation property
        public virtual WorkObject WorkObject { get; set; }
    }
}