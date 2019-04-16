﻿using EvenCart.Infrastructure.Mvc.Models;

namespace EvenCart.Models.Checkout
{
    public class ShippingMethodModel : FoundationModel
    {
        public string SystemName { get; set; }

        public string FriendlyName { get; set; }

        public string Description { get; set; }

        public decimal Fee { get; set; }
    }
}