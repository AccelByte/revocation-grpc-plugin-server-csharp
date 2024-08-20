﻿// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

using AccelByte.Sdk.Core.Repository;
using AccelByte.Sdk.Core.Logging;
using AccelByte.Sdk.Core.Util;

namespace AccelByte.PluginArch.Revocation.Demo.Client.Model
{
    public class ApplicationConfig : IConfigRepository
    {
        [Option('b', "baseurl", Required = false, HelpText = "AGS base URL", Default = "")]
        public string BaseUrl { get; set; } = String.Empty;

        [Option('c', "client", Required = false, HelpText = "AGS client id", Default = "")]
        public string ClientId { get; set; } = String.Empty;

        [Option('s', "secret", Required = false, HelpText = "AGS client secret", Default = "")]
        public string ClientSecret { get; set; } = String.Empty;

        public string AppName { get; set; } = "CustomRevocationDemoClient";

        public string TraceIdVersion { get; set; } = String.Empty;

        [Option('n', "namespace", Required = false, HelpText = "AGS namespace", Default = "")]
        public string Namespace { get; set; } = String.Empty;

        public bool EnableTraceId { get; set; } = false;

        public bool EnableUserAgentInfo { get; set; } = false;

        public IHttpLogger? Logger { get; set; } = null;

        public string UserId { get; set; } = String.Empty;

        [Option('t', "category", Required = false, HelpText = "Store's category path for items", Default = "")]
        public string CategoryPath { get; set; } = String.Empty;

        [Option('g', "grpc-target", Required = false, HelpText = "Grpc plugin target server url.", Default = "")]
        public string GrpcServerUrl { get; set; } = String.Empty;

        [Option('e', "extend-app", Required = false, HelpText = "Extend app name for grpc plugin.", Default = "")]
        public string ExtendAppName { get; set; } = String.Empty;

        protected string ReplaceWithEnvironmentVariableIfExists(string pValue, string evKey)
        {
            string? temp = Environment.GetEnvironmentVariable(evKey);
            if ((pValue == "") && (temp != null))
                return temp.Trim();
            else
                return pValue;
        }

        public void FinalizeConfigurations()
        {
            BaseUrl = ReplaceWithEnvironmentVariableIfExists(BaseUrl, "AB_BASE_URL");
            ClientId = ReplaceWithEnvironmentVariableIfExists(ClientId, "AB_CLIENT_ID");
            ClientSecret = ReplaceWithEnvironmentVariableIfExists(ClientSecret, "AB_CLIENT_SECRET");
            Namespace = ReplaceWithEnvironmentVariableIfExists(Namespace, "AB_NAMESPACE");
            GrpcServerUrl = ReplaceWithEnvironmentVariableIfExists(GrpcServerUrl, "GRPC_SERVER_URL");
            ExtendAppName = ReplaceWithEnvironmentVariableIfExists(ExtendAppName, "EXTEND_APP_NAME");

            if (CategoryPath.Trim() == "")
                CategoryPath = $"/test{Helper.GenerateRandomId(8)}";
        }
    }
}
