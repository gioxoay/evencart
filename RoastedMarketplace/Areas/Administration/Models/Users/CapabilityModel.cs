﻿using RoastedMarketplace.Infrastructure.Mvc.Models;

namespace RoastedMarketplace.Areas.Administration.Models.Users
{
    public class CapabilityModel : FoundationEntityModel
    {
        public string Name { get; set; }

        public bool Active { get; set; }
    }
}