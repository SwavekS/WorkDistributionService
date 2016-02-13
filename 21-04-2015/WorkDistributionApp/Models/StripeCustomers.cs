using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkDistributionApp.Models
{
    public class StripeCustomers
    {
        [Key]
        public string StripeCustomerID { get; set; }

        public string StripeSubscriptionID { get; set; }

        public string SubscriptionType { get; set; }
        public bool HasSubscription { get; set; }

        [DataType(DataType.EmailAddress)]
        public string CustomerName { get; set; }

        public string Interval { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime StartDate { get; set; }

        public DateTime? TrialValidUntil { get; set; }

    }
}