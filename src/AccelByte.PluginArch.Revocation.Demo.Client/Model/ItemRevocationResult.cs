using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccelByte.Sdk.Api.Platform.Model;

namespace AccelByte.PluginArch.Revocation.Demo.Client.Model
{
    public class ItemRevocationResult
    {
        public string Id { get; set; } = String.Empty;

        public string Status { get; set; } = String.Empty;

        public List<RevokedItem> Items { get; set; } = new();

        public ItemRevocationResult() { }

        public ItemRevocationResult(RevocationResult result)
        {
            if (result.ItemRevocations == null)
                throw new Exception("Could not use revocation result data for ItemRevocationResult.");

            Id = result.Id!;
            Status = result.Status!.Value;

            foreach (var item in result.ItemRevocations)
            {
                if (item != null)
                    Items.Add(new RevokedItem(item));
            }   
        }

        public void WriteToConsole()
        {
            Console.WriteLine("Revocation result:");
            Console.WriteLine("\tId: {0}", Id);
            Console.WriteLine("\tStatus: {0}", Status);
            Console.WriteLine("\tItem(s):");
            foreach (var item in Items)
                item.WriteToConsole("\t\t");
        }
    }
}
