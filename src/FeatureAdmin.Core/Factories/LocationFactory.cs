﻿using FeatureAdmin.Core.Models;
using FeatureAdmin.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureAdmin.Core.Factories
{
    public class LocationFactory
    {       
        public static Location GetDummyFarmForLoadCommand()
        {
            var farmDummy = GetFarm(Guid.Empty, new List<ActivatedFeature>(),0);
            return farmDummy;
        }
        public static Location GetFarm(Guid farmId, IEnumerable<ActivatedFeature> activatedFeatures, int childCount)
        {
            var location = new Location(farmId,
               "Farm",
               Guid.Empty,
               Scope.Farm,
               "Farm",
               activatedFeatures,
               childCount);

            return location;
        }

        public static Location GetLocation(Guid id, string displayName, Guid parent, Scope scope, string url, IEnumerable<ActivatedFeature> activatedFeatures, int childCount)
        {
            var location = new Location(id, displayName, parent, scope, url, activatedFeatures, childCount);
            return location;
        }

        public static Location GetLocationUndefined(Guid id, Guid parent, string displayName = "undefined", Scope? scope = Scope.ScopeInvalid, string url = "undefined")
        {
            var location = new Location(id, displayName, parent, scope == null ? Scope.ScopeInvalid : scope.Value, url, 0);
            return location;
        }


    }
}