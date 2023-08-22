// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Sdk.Api.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccelByte.PluginArch.Revocation.Demo.Client.Model
{
    public class SimpleItemInfo
    {
        public string Id { get; set; } = String.Empty;

        public string Sku { get; set; } = String.Empty;

        public string Title { get; set; } = String.Empty;

        public int Price { get; set; } = 1;

        public void WriteToConsole()
        {
            Console.WriteLine($"\t{Id} : {Sku} : {Title} , Price: {Price}");
        }
    }
}
