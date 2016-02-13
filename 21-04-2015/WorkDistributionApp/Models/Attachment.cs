using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkDistributionApp.Models
{
    public enum FileType
    {
        Email, Document, Audio, Letter, Request
    }
    public class Attachment
    {
        // PK
        public int AttachmentID { get; set; }
        // FK
        public int WorkObjectID { get; set; }

        [Display(Name="File Type")]
        public FileType FileType { get; set; }

        public byte[] File { get; set; }

        public String FileName { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime Timestamp { get; set; }

        [Display(Name="Added by:")]
        [DataType(DataType.EmailAddress)]
        public String User { get; set; }

        // navigation property
        public virtual WorkObject WorkObject { get; set; }
    }
}