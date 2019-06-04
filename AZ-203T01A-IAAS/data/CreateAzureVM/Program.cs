using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace CreateAzureVM
{
    /// <summary>
    /// based on https://docs.microsoft.com/en-us/azure/virtual-machines/windows/csharp
    /// </summary>
    class Program
    {
        static IAzure Subscription;

        static readonly string ResourceGroupName = "demo001";

        static readonly string VmName = "devUScWeb003";

        static async Task Main(string[] args)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyPath, "azureauth.properties");

            Environment.SetEnvironmentVariable("AZURE_AUTH_LOCATION", path, EnvironmentVariableTarget.Process);

            SetupCredentials();

            var resourceGroup = await CreateResources(ResourceGroupName);

            var availabilitySet = await CreateAvailabilitySet(resourceGroup);

            var ip = await CreatePublicIPAddress(resourceGroup);

            var result = await CreateVirtualNetwork(resourceGroup);
            var network = result.Item1;
            var subnetName = result.Item2;

            var nic = await CreateNIC(resourceGroup, network, subnetName, ip);


            Console.WriteLine($"Creating virtual machine... with name {VmName}");
            var vm = await Subscription.VirtualMachines.Define(VmName)
                .WithRegion(resourceGroup.Region)
                .WithExistingResourceGroup(resourceGroup)
                .WithExistingPrimaryNetworkInterface(nic)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("azureuser")
                .WithAdminPassword("Azure12345678")
                .WithComputerName(VmName)
                .WithExistingAvailabilitySet(availabilitySet)
                .WithSize(VirtualMachineSizeTypes.StandardDS1)
                .CreateAsync();


            Console.WriteLine("Getting information about the virtual machine...");
            Console.WriteLine("hardwareProfile");
            Console.WriteLine("   vmSize: " + vm.Size);
            Console.WriteLine("storageProfile");
            Console.WriteLine("  imageReference");
            Console.WriteLine("    publisher: " + vm.StorageProfile.ImageReference.Publisher);
            Console.WriteLine("    offer: " + vm.StorageProfile.ImageReference.Offer);
            Console.WriteLine("    sku: " + vm.StorageProfile.ImageReference.Sku);
            Console.WriteLine("    version: " + vm.StorageProfile.ImageReference.Version);
            Console.WriteLine("  osDisk");
            Console.WriteLine("    osType: " + vm.StorageProfile.OsDisk.OsType);
            Console.WriteLine("    name: " + vm.StorageProfile.OsDisk.Name);
            Console.WriteLine("    createOption: " + vm.StorageProfile.OsDisk.CreateOption);
            Console.WriteLine("    caching: " + vm.StorageProfile.OsDisk.Caching);
            Console.WriteLine("osProfile");
            Console.WriteLine("  computerName: " + vm.OSProfile.ComputerName);
            Console.WriteLine("  adminUsername: " + vm.OSProfile.AdminUsername);
            Console.WriteLine("  provisionVMAgent: " + vm.OSProfile.WindowsConfiguration.ProvisionVMAgent.Value);
            Console.WriteLine("  enableAutomaticUpdates: " + vm.OSProfile.WindowsConfiguration.EnableAutomaticUpdates.Value);
            Console.WriteLine("networkProfile");
            foreach (string nicId in vm.NetworkInterfaceIds)
            {
                Console.WriteLine("  networkInterface id: " + nicId);
            }
            Console.WriteLine("vmAgent");
            Console.WriteLine("  vmAgentVersion" + vm.InstanceView.VmAgent.VmAgentVersion);
            Console.WriteLine("    statuses");
            foreach (InstanceViewStatus stat in vm.InstanceView.VmAgent.Statuses)
            {
                Console.WriteLine("    code: " + stat.Code);
                Console.WriteLine("    level: " + stat.Level);
                Console.WriteLine("    displayStatus: " + stat.DisplayStatus);
                Console.WriteLine("    message: " + stat.Message);
                Console.WriteLine("    time: " + stat.Time);
            }
            Console.WriteLine("disks");
            foreach (DiskInstanceView disk in vm.InstanceView.Disks)
            {
                Console.WriteLine("  name: " + disk.Name);
                Console.WriteLine("  statuses");
                foreach (InstanceViewStatus stat in disk.Statuses)
                {
                    Console.WriteLine("    code: " + stat.Code);
                    Console.WriteLine("    level: " + stat.Level);
                    Console.WriteLine("    displayStatus: " + stat.DisplayStatus);
                    Console.WriteLine("    time: " + stat.Time);
                }
            }
            Console.WriteLine("VM general status");
            Console.WriteLine("  provisioningStatus: " + vm.ProvisioningState);
            Console.WriteLine("  id: " + vm.Id);
            Console.WriteLine("  name: " + vm.Name);
            Console.WriteLine("  type: " + vm.Type);
            Console.WriteLine("  location: " + vm.Region);
            Console.WriteLine("VM instance status");
            foreach (InstanceViewStatus stat in vm.InstanceView.Statuses)
            {
                Console.WriteLine("  code: " + stat.Code);
                Console.WriteLine("  level: " + stat.Level);
                Console.WriteLine("  displayStatus: " + stat.DisplayStatus);
            }
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        static void SetupCredentials()
        {
            var credentials = SdkContext.AzureCredentialsFactory
               .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            Subscription = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();
        }

        static async Task<IResourceGroup> CreateResources(string name, Region region = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Creating resource group... with name");

            bool exists = await Subscription.ResourceGroups.ContainAsync(name, cancellationToken);
            if (exists)
            {
                return await Subscription.ResourceGroups.GetByNameAsync(name, cancellationToken);
            }

            return await Subscription.ResourceGroups.Define(ResourceGroupName)
                .WithRegion(region ?? Region.USCentral)
                .CreateAsync();
        }

        static async Task<IAvailabilitySet> CreateAvailabilitySet(IResourceGroup resourceGroup, string name = "CSharpAvailabilitySet")
        {
            Console.WriteLine($"Creating availability set... with name {name}");

            return await Subscription.AvailabilitySets.Define(name)
                .WithRegion(resourceGroup.Region)
                .WithExistingResourceGroup(resourceGroup)
                .WithSku(AvailabilitySetSkuTypes.Managed)
                .CreateAsync();
        }

        static async Task<IPublicIPAddress> CreatePublicIPAddress(IResourceGroup resourceGroup, string name = "CSIpAddress")
        {
            Console.WriteLine($"Creating public IP address... with name {name}");

            return await Subscription.PublicIPAddresses.Define(name)
                .WithRegion(resourceGroup.Region)
                .WithExistingResourceGroup(resourceGroup)
                .WithDynamicIP()
                .CreateAsync();
        }

        static async Task<(INetwork, string)> CreateVirtualNetwork(IResourceGroup resourceGroup, string name = "CSNetwork", string subnetName = "CSSubnet")
        {
            Console.WriteLine($"Creating virtual network... with name {name}");

            var network = await Subscription.Networks.Define("myVNet")
                .WithRegion(resourceGroup.Region)
                .WithExistingResourceGroup(resourceGroup)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet(subnetName, "10.0.0.0/24")
                .CreateAsync();

            return (network, subnetName);
        }

        static async Task<INetworkInterface> CreateNIC(IResourceGroup resourceGroup, INetwork network, string subnetName, IPublicIPAddress publicIPAddress, string name = "CSNIC")
        {
            Console.WriteLine("Creating network interface...");
            return await Subscription.NetworkInterfaces.Define("myNIC")
                .WithRegion(resourceGroup.Region)
                .WithExistingResourceGroup(resourceGroup)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(subnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                .CreateAsync();
        }
    }
}
