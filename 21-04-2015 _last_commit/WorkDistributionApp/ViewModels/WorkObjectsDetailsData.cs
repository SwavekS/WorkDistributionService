using System.Collections.Generic;
using WorkDistributionApp.Models;

namespace WorkDistributionApp.ViewModels
{
    public class WorkObjectsDetailsData
    {
        public IEnumerable<WorkObject> WorkObjects { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<Attachment> Attachments{ get; set; }
    }
}