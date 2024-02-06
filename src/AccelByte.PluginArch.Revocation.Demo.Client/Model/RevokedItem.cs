using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccelByte.Sdk.Api.Platform.Model;

namespace AccelByte.PluginArch.Revocation.Demo.Client.Model
{
    public class RevokedItem : SimpleItemInfo
    {
        public long Quantity { get; set; } = 0;

        public string Reason { get; set; } = String.Empty;

        public bool IsSkipped { get; set; } = false;

        public string Status { get; set; } = String.Empty;

        public string Strategy { get; set; } = String.Empty;

        public RevokedItem() { }

        public RevokedItem(ItemRevocation source)
        {
            Id = source.ItemId!;
            Sku = source.ItemSku!;

            Quantity = source.Quantity!.Value;
            Reason = source.Reason!;

            if (source.Skipped != null)
                IsSkipped = source.Skipped.Value;

            Status = source.Status!.Value;
            Strategy = source.Strategy!;
        }

        public void WriteToConsole(string prefix)
        {
            Console.WriteLine(prefix + "Id: {0}", Id);
            Console.WriteLine(prefix + "Sku: {0}", Sku);
            Console.WriteLine(prefix + "Qty: {0}", Quantity);
            Console.WriteLine(prefix + "Reason: {0}", Reason);
            Console.WriteLine(prefix + "Skipped: {0}", IsSkipped);
            Console.WriteLine(prefix + "Status: {0}", Status);
            Console.WriteLine(prefix + "Strategy: {0}", Strategy);
        }
    }
}
