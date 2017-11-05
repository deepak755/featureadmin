﻿using FeatureAdmin.Core.Models;
using FeatureAdmin.Core.Models.Enums;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;

namespace FeatureAdmin.Backends.Sp2013.Services
{
    public static class SpConverter
    {
        public static ActivatedFeature ToActivatedFeature(this SPFeature spFeature, Guid parentId)
        {
            FeatureDefinition definition = null;
            bool faulty = false;

            try
            {
                if (spFeature.Definition != null)
                {
                    var fDef = spFeature.Definition;
                    definition = fDef.ToFeatureDefinition();
                }
                else
                {
                    faulty = true;
                }
            }
            catch (Exception)
            {
                faulty = true;
            }


            var feature = ActivatedFeature.GetActivatedFeature(
                spFeature.DefinitionId,
                parentId,
                definition,
                faulty,
                spFeature.Properties == null ? null :
                spFeature.Properties.ToProperties(),
                spFeature.TimeActivated,
                spFeature.Version
                );

            return feature;
        }



        public static IEnumerable<ActivatedFeature> ToActivatedFeatures(this SPFeatureCollection spFeatures, Guid parentId)
        {
            var features = new List<ActivatedFeature>();

            if (spFeatures != null && spFeatures.Count > 0)
            {
                foreach (var f in spFeatures)
                {
                    features.Add(f.ToActivatedFeature(parentId));
                }
            }

            return features;
        }

        public static Location ToLocation(this SPWebService farm)
        {
            var id = farm.Id;
            var activatedFeatures = farm.Features.ToActivatedFeatures(id);

            var location = Location.GetFarm(id, activatedFeatures);

            return location;
        }

        public static Location ToLocation(this SPWebApplication webApp, Guid parentId)
        {
            var id = webApp.Id;

            var activatedFeatures = webApp.Features.ToActivatedFeatures(id);

            string url;
            var uri = webApp.GetResponseUri(SPUrlZone.Default);

            if (uri != null)
            {
                url = uri.ToString();
            }
            else
            {
                url = "No ResponseUri in default zone found.";
            }

            var location = Location.GetLocation(
                id,
                webApp.DisplayName,
                parentId,
                Scope.WebApplication,
                url,
                activatedFeatures);

            return location;
        }

        public static Location ToLocation(this SPSite site, Guid parentId)
        {
            var id = site.ID;
            var activatedFeatures = site.Features.ToActivatedFeatures(id);

            string displayName;

            if (site.RootWeb != null)
            {
                displayName = site.RootWeb.Title;
            }
            else
            {
                displayName = "Site has no root web!";
            }

            var location = Location.GetLocation(
                id,
                displayName,
                parentId,
                Scope.Site,
                site.Url,
                activatedFeatures
                );

            return location;
        }

        public static Location ToLocation(this SPWeb web, Guid parentId)
        {
            var id = web.ID;
            var activatedFeatures = web.Features.ToActivatedFeatures(id);

            var location = Location.GetLocation(
                id,
                web.Title,
                parentId,
                Scope.Site,
                web.Url,
                activatedFeatures
                );

            return location;
        }

        public static FeatureDefinition ToFeatureDefinition(this SPFeatureDefinition spFeatureDefinition)
        {
            bool faulty = false;

            var cultureInfo = new System.Globalization.CultureInfo(1033);

            if (spFeatureDefinition == null)
            {
                return null;
            }

            var fd = FeatureDefinition.GetFeatureDefinition(
                spFeatureDefinition.Id,
                spFeatureDefinition.CompatibilityLevel,
                spFeatureDefinition.GetDescription(cultureInfo),
                spFeatureDefinition.DisplayName,
                faulty,
                spFeatureDefinition.Hidden,
                spFeatureDefinition.Name,
                spFeatureDefinition.Properties == null ? null :
                spFeatureDefinition.Properties.ToProperties(),
                spFeatureDefinition.Scope.ToScope(),
                spFeatureDefinition.GetTitle(cultureInfo),
                spFeatureDefinition.SolutionId,
                spFeatureDefinition.UIVersion,
                spFeatureDefinition.Version);

            return fd;
        }

        public static Dictionary<string, string> ToProperties(this SPFeaturePropertyCollection featureProperties)
        {
            var properties = new Dictionary<string, string>();

            foreach (SPFeatureProperty p in featureProperties)
            {
                properties.Add(p.Name, p.Value);
            }

            return properties;
        }


        public static Scope ToScope(this SPFeatureScope spFeatureScope)
        {
            switch (spFeatureScope)
            {
                //case SPFeatureScope.ScopeInvalid:
                //    return Scope.ScopeInvalid;
                case SPFeatureScope.Farm:
                    return Scope.Farm;
                case SPFeatureScope.WebApplication:
                    return Scope.WebApplication;
                case SPFeatureScope.Site:
                    return Scope.Site;
                case SPFeatureScope.Web:
                    return Scope.Web;
                default:
                    return Scope.ScopeInvalid;
            }
        }

        public static IEnumerable<SPLocation> ToSPLocations(object spCollection, Guid parentId)
        {
            var spLocations = new List<SPLocation>();

            if (spCollection != null)
            {
                // Web
                if (spCollection is SPWebCollection)
                {
                    foreach (SPWeb item in spCollection as SPWebCollection)
                    {
                        var location = item.ToLocation(parentId);
                        spLocations.Add(SPLocation.GetSPLocation(location, item, false));
                    }
                }
                else
                {
                    // Site
                    if (spCollection is SPSiteCollection)
                    {
                        foreach (SPSite item in spCollection as SPSiteCollection)
                        {
                            var location = item.ToLocation(parentId);
                            spLocations.Add(SPLocation.GetSPLocation(location, item, false));
                        }
                    }
                    else
                    {
                        // WebApp
                        if (spCollection is SPWebApplicationCollection)
                        {
                            foreach (SPWebApplication item in spCollection as SPWebApplicationCollection)
                            {
                                var location = item.ToLocation(parentId);
                                spLocations.Add(SPLocation.GetSPLocation(location, item, false));
                            }
                        }
                    }
                }
            }
            return spLocations;
        }
    }
}