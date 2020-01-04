using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CreateAzureVM.Core
{
    public sealed class DemoVirtualMachine
    {
        private readonly ILogger<DemoVirtualMachine> logger;

        private readonly Region REGION = Region.USCentral;

        const string RESOURCE_GROUP_NAME = "demo001";

        const string AVAILABILITY_SET_NAME = "demo001_as";

        const string IP_ADDRESS_NAME = "demo001_ip";

        const string VIRTUAL_MACHINE_NAME = "devUScWeb003";

        const string NETWORK_NAME = "demo001_network";

        const string SUBNET_NAME = "demo001_subnet";

        const string NIC_NAME = "demo001_nic";

        const string VM_NAME = "demo001_vm";


        public IAzure Subscription { get; private set; }

        public IResourceGroup ResourceGroup { get; private set; }

        public IAvailabilitySet AvailabilitySet { get; private set; }

        public IPublicIPAddress PublicIp { get; private set; }

        public INetwork Network { get; private set; }

        public INetworkInterface NIC { get; private set; }

        public IVirtualMachine VirtualMachine { get; private set; }

        public DemoVirtualMachine(ILogger<DemoVirtualMachine> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SetupCredentials()
        {
            logger.LogInformation("subscription setup init");

            var credentials = SdkContext.AzureCredentialsFactory
               .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            Subscription = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            logger.LogInformation("Done");
        }

        public async Task CreateResourceGroupAsync(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Creating resource group... with name {RESOURCE_GROUP_NAME}");

            bool exists = await Subscription.ResourceGroups.ContainAsync(RESOURCE_GROUP_NAME, cancellationToken);
            if (exists)
            {
                ResourceGroup = await Subscription.ResourceGroups.GetByNameAsync(RESOURCE_GROUP_NAME, cancellationToken);
            }
            else
            {
                ResourceGroup = await Subscription.ResourceGroups.Define(RESOURCE_GROUP_NAME)
                    .WithRegion(RESOURCE_GROUP_NAME)
                    .CreateAsync(cancellationToken);
            }

            logger.LogInformation("Done");
        }

        public async Task CreateAvailabilitySetAsync(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Creating availability set... with name {AVAILABILITY_SET_NAME}");

            AvailabilitySet = await Subscription.AvailabilitySets.Define(AVAILABILITY_SET_NAME)
                .WithRegion(ResourceGroup.Region)
                .WithExistingResourceGroup(ResourceGroup)
                .WithSku(AvailabilitySetSkuTypes.Aligned)
                .CreateAsync(cancellationToken);

            logger.LogInformation("Done");
        }

        public async Task CreatePublicIPAddress(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Creating public IP address... with name {IP_ADDRESS_NAME}");

            PublicIp = await Subscription.PublicIPAddresses.Define(IP_ADDRESS_NAME)
                .WithRegion(ResourceGroup.Region)
                .WithExistingResourceGroup(ResourceGroup)
                .WithDynamicIP()
                .CreateAsync(cancellationToken);

            logger.LogInformation("Done");
        }

        public async Task CreateVirtualNetwork(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Creating virtual network... with name {NETWORK_NAME}");

            Network = await Subscription.Networks.Define(NETWORK_NAME)
                .WithRegion(ResourceGroup.Region)
                .WithExistingResourceGroup(ResourceGroup)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet(SUBNET_NAME, "10.0.0.0/24")
                .CreateAsync(cancellationToken);

            logger.LogInformation("Done");
        }

        public async Task CreateNIC(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Creating network interface...");

            NIC = await Subscription.NetworkInterfaces.Define(NIC_NAME)
                .WithRegion(ResourceGroup.Region)
                .WithExistingResourceGroup(ResourceGroup)
                .WithExistingPrimaryNetwork(Network)
                .WithSubnet(SUBNET_NAME)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(PublicIp)
                .CreateAsync(cancellationToken);

            logger.LogInformation("Done");
        }

        public async Task CreateVirtualMachine(
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Creating virtual machine...");

            VirtualMachine = await Subscription.VirtualMachines.Define(VM_NAME)
                .WithRegion(ResourceGroup.Region)
                .WithExistingResourceGroup(ResourceGroup)
                .WithExistingPrimaryNetworkInterface(NIC)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("azureuser")
                .WithAdminPassword("Azure12345678")
                .WithComputerName(VM_NAME)
                .WithExistingAvailabilitySet(AvailabilitySet)
                .WithSize(VirtualMachineSizeTypes.StandardDS1)
                .CreateAsync();

            logger.LogInformation("Done");
        }

        public void Describe()
        {
            var vm = VirtualMachine;

            logger.LogInformation("Getting information about the virtual machine...");
            logger.LogInformation("hardwareProfile");
            logger.LogInformation("   vmSize: " + vm.Size);
            logger.LogInformation("storageProfile");
            logger.LogInformation("  imageReference");
            logger.LogInformation("    publisher: " + vm.StorageProfile.ImageReference.Publisher);
            logger.LogInformation("    offer: " + vm.StorageProfile.ImageReference.Offer);
            logger.LogInformation("    sku: " + vm.StorageProfile.ImageReference.Sku);
            logger.LogInformation("    version: " + vm.StorageProfile.ImageReference.Version);
            logger.LogInformation("  osDisk");
            logger.LogInformation("    osType: " + vm.StorageProfile.OsDisk.OsType);
            logger.LogInformation("    name: " + vm.StorageProfile.OsDisk.Name);
            logger.LogInformation("    createOption: " + vm.StorageProfile.OsDisk.CreateOption);
            logger.LogInformation("    caching: " + vm.StorageProfile.OsDisk.Caching);
            logger.LogInformation("osProfile");
            logger.LogInformation("  computerName: " + vm.OSProfile.ComputerName);
            logger.LogInformation("  adminUsername: " + vm.OSProfile.AdminUsername);
            logger.LogInformation("  provisionVMAgent: " + vm.OSProfile.WindowsConfiguration.ProvisionVMAgent.Value);
            logger.LogInformation("  enableAutomaticUpdates: " + vm.OSProfile.WindowsConfiguration.EnableAutomaticUpdates.Value);
            logger.LogInformation("networkProfile");
            foreach (string nicId in vm.NetworkInterfaceIds)
            {
                logger.LogInformation("  networkInterface id: " + nicId);
            }
            logger.LogInformation("vmAgent");
            logger.LogInformation("  vmAgentVersion" + vm.InstanceView.VmAgent.VmAgentVersion);
            logger.LogInformation("    statuses");
            foreach (InstanceViewStatus stat in vm.InstanceView.VmAgent.Statuses)
            {
                logger.LogInformation("    code: " + stat.Code);
                logger.LogInformation("    level: " + stat.Level);
                logger.LogInformation("    displayStatus: " + stat.DisplayStatus);
                logger.LogInformation("    message: " + stat.Message);
                logger.LogInformation("    time: " + stat.Time);
            }
            Console.WriteLine("disks");
            foreach (DiskInstanceView disk in vm.InstanceView.Disks)
            {
                logger.LogInformation("  name: " + disk.Name);
                logger.LogInformation("  statuses");
                foreach (InstanceViewStatus stat in disk.Statuses)
                {
                    logger.LogInformation("    code: " + stat.Code);
                    logger.LogInformation("    level: " + stat.Level);
                    logger.LogInformation("    displayStatus: " + stat.DisplayStatus);
                    logger.LogInformation("    time: " + stat.Time);
                }
            }
            logger.LogInformation("VM general status");
            logger.LogInformation("  provisioningStatus: " + vm.ProvisioningState);
            logger.LogInformation("  id: " + vm.Id);
            logger.LogInformation("  name: " + vm.Name);
            logger.LogInformation("  type: " + vm.Type);
            logger.LogInformation("  location: " + vm.Region);
            logger.LogInformation("VM instance status");
            foreach (InstanceViewStatus stat in vm.InstanceView.Statuses)
            {
                logger.LogInformation("  code: " + stat.Code);
                logger.LogInformation("  level: " + stat.Level);
                logger.LogInformation("  displayStatus: " + stat.DisplayStatus);
            }
            logger.LogInformation("Press enter to continue...");
        }
    }
}
