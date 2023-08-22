// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Grpc.Core;
using AccelByte.Platform.Revocation.V1;

namespace AccelByte.PluginArch.Revocation.Demo.Server.Services
{
    public class RevocationFunctionService : AccelByte.Platform.Revocation.V1.Revocation.RevocationBase
    {
        private readonly ILogger<RevocationFunctionService> _Logger;

        public RevocationFunctionService(ILogger<RevocationFunctionService> logger)
        {
            _Logger = logger;
        }

        public override Task<RevokeResponse> Revoke(RevokeRequest request, ServerCallContext context)
        {
            _Logger.LogInformation("Received Revoke request.");
            var response = new RevokeResponse();
            response.Status = "SUCCESS";

            string revokeType = request.RevokeEntryType.Trim().ToUpper();
            if (revokeType == "ITEM")
            {
                response.CustomRevocation.Add(new Dictionary<string, string>()
                {
                    { "namespace", request.Namespace },
                    { "userId", request.UserId },
                    { "quantity", request.Quantity.ToString() },
                    { "itemId", request.Item.ItemId },
                    { "sku", request.Item.ItemSku },
                    { "itemType", request.Item.ItemType },
                    { "useCount", request.Item.UseCount.ToString() },
                    { "entitlementType", request.Item.EntitlementType }
                });
            }
            else if (revokeType == "CURRENCY")
            {
                response.CustomRevocation.Add(new Dictionary<string, string>()
                {
                    { "namespace", request.Namespace },
                    { "userId", request.UserId },
                    { "quantity", request.Quantity.ToString() },
                    { "currencyNamespace", request.Currency.Namespace },
                    { "currencyCode", request.Currency.CurrencyCode },
                    { "balanceOrigin", request.Currency.BalanceOrigin }
                });
            }
            else if (revokeType == "ENTITLEMENT")
            {
                response.CustomRevocation.Add(new Dictionary<string, string>()
                {
                    { "namespace", request.Namespace },
                    { "userId", request.UserId },
                    { "quantity", request.Quantity.ToString() },
                    { "entitlementId", request.Entitlement.EntitlementId },
                    { "itemId", request.Entitlement.ItemId },
                    { "sku", request.Entitlement.Sku },
                });
            }
            else
            {
                response.Status = "FAIL";
                response.Reason = $"Revocation type [{revokeType}] is not supported.";
            }

            return Task.FromResult(response);
        }
    }
}
