using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkDistributionApp.Models
{
    public enum SubscriptionType
    {
        [Display(Name="Standard")]
        standard,
        [Display(Name = "Ultimate")]
        ultimate
    }
    public class StripeModel
    {
        [Required]
        public string Token { get; set; }
        
        [Display(Name="Card Holder Name")]
        public string CardHolderName { get; set; }

        [Required]
        [Display(Name = "Subscription Type")]
        public SubscriptionType SubscriptionType { get; set; }

        // These fields are optional and are not required
        [Display(Name="Address Line 1")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }

        [Display(Name = "City")]
        public string AddressCity { get; set; }

        [Display(Name = "Postcode")]
        public string AddressPostcode { get; set; }

        [Display(Name = "Country")]
        public string AddressCountry { get; set; }

    }
}