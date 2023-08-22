// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using AccelByte.Platform.Revocation.V1;
using AccelByte.PluginArch.Revocation.Demo.Server.Services;

namespace AccelByte.PluginArch.Revocation.Demo.Tests
{
    [TestFixture]
    public class RevocationFunctionServiceTests
    {
        private ILogger<RevocationFunctionService> _ServiceLogger;

        public RevocationFunctionServiceTests()
        {
            ILoggerFactory loggerFactory = new NullLoggerFactory();
            _ServiceLogger = loggerFactory.CreateLogger<RevocationFunctionService>();
        }

        [Test]
        public async Task RevokeTests()
        {
            var service = new RevocationFunctionService(_ServiceLogger);

            string itemId = Guid.NewGuid().ToString().Replace("-", "");

            var response = await service.Revoke(new RevokeRequest()
            {
                Namespace = "test",
                UserId = "1",
                RevokeEntryType = "ITEM",
                Item = new RevokeItemObject()
                {
                    ItemId = itemId,
                    ItemSku = "SKU",
                    UseCount = 1,
                    ItemType = "DURABLE",
                    EntitlementType = "PURCHASE"
                }
            }, new UnitTestCallContext());
            Assert.IsNotNull(response);
            if (response != null)
            {
                Assert.IsTrue(response.CustomRevocation.ContainsKey("itemId"));
                Assert.AreEqual(itemId, response.CustomRevocation["itemId"]);
            }
        }
    }
}
