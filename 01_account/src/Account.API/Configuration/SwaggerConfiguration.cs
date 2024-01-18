// <copyright file="SwaggerConfiguration.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.API.Configuration
{
    public class SwaggerConfiguration
    {
        public string UiEndpoint { get; set; } = null!;

        public string JsonEndpoint { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Version { get; set; } = null!;

        public string ContactName { get; set; } = null!;

        public string ContactEmail { get; set; } = null!;

        public string LicenseName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public Uri TermsOfService { get; set; } = null!;
    }
}
