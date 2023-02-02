terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0.2"
    }
  }

  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = "${var.location}"
}

resource "azurerm_virtual_network" "vnet" {
  name                = "vnet-${var.app_name}"
  address_space       = ["10.4.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_virtual_network_peering" "vnet_peering" {
  name                      = "${azurerm_virtual_network.vnet.name}-to-${var.peered_network_name}"
  resource_group_name       = azurerm_resource_group.rg.name
  virtual_network_name      = azurerm_virtual_network.vnet.name
  remote_virtual_network_id = var.peered_network_id
}


resource "azurerm_virtual_network_peering" "vnet_peering_from_peered" {
  name                      = "${var.peered_network_name}-to-${azurerm_virtual_network.vnet.name}"
  resource_group_name       = var.peered_network_rg_name
  virtual_network_name      = var.peered_network_name
  remote_virtual_network_id = azurerm_virtual_network.vnet.id
}

resource "azurerm_subnet" "sn1" {
  name                 = "vnet-${var.app_name}-sn1"
  resource_group_name  = azurerm_resource_group.rg.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.4.0.0/24"]
  service_endpoints = [ "Microsoft.Storage" ]
}

resource "azurerm_network_interface" "vm_nic" {
  name                = "vm-backend-dev-eastus-nic"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.sn1.id
    private_ip_address_allocation = "Static"
    private_ip_address = "10.4.0.4"
  }
}

resource "azurerm_network_interface" "vm_proxy_nic" {
  name                = "vm-proxy-dev-eastus-nic"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.sn1.id
    private_ip_address_allocation = "Static"
    private_ip_address = "10.4.0.5"
    public_ip_address_id = var.auctionhouse_ip_id
  }
}

resource "azurerm_storage_account" "vm_sa" {
  name                     = "auctionhousevmsa"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  network_rules {
    default_action = "Deny"
    ip_rules = ["20.98.146.84", "20.98.194.64", "20.69.5.162", "20.83.222.102"]
    virtual_network_subnet_ids = [ azurerm_subnet.sn1.id ]
  }
}


resource "azurerm_virtual_machine" "vm" {
  name                  = "vm-backend-dev-eastus"
  location              = azurerm_resource_group.rg.location
  resource_group_name   = azurerm_resource_group.rg.name
  network_interface_ids = [azurerm_network_interface.vm_nic.id]
  vm_size               = "Standard_D2as_v4"

  delete_os_disk_on_termination = true
  delete_data_disks_on_termination = true

  identity {
    type = "SystemAssigned"
  }

  storage_image_reference {
    publisher = "canonical"
    offer     = "0001-com-ubuntu-server-focal"
    sku       = "20_04-lts-gen2"
    version   = "latest"
  }
  storage_os_disk {
    name              = "vm-backend-dev-eastus-osdisk"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Premium_LRS"
    disk_size_gb = 30
    os_type = "Linux"
  }
  os_profile {
    computer_name  = "vm-backend-dev-eastus"
    admin_username = "${var.vm_username}"
    custom_data = <<EOT
#cloud-config

package_update: true
package_upgrade: true

packages:
  - net-tools
  - ca-certificates
  - curl
  - gnupg
  - lsb-release

runcmd:
  - mkdir -p /etc/apt/keyrings
  - curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
  - echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
  - apt-get update
  - apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
  - curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
  - docker run hello-world
  - docker volume create portainer_data && docker run -d -p 8000:8000 -p 9443:9443 --name portainer --restart=always -v /var/run/docker.sock:/var/run/docker.sock -v portainer_data:/data portainer/portainer-ce:latest
  - git clone -b azure-pipelines https://github.com/pekalam/auctionhouse
  - sleep 120s
  - prev=`pwd` && cd auctionhouse/azure/azure-vm/docker-registry && docker swarm init && bash setup_registry_secrets.bash && docker stack deploy -c ./docker-compose.yml registry && cd "$prev"
  - while true; do docker swarm join-token -q worker | nc -q 0 -l 0.0.0.0 8888; done &

final_message: "The system is finally up, after $UPTIME seconds"
EOT
  }
  os_profile_linux_config {
    disable_password_authentication = true
    ssh_keys {
      key_data = file("./ssh/${var.sshkey1_name}.pub")
      path = "/home/${var.vm_username}/.ssh/authorized_keys"
    }
  }

  boot_diagnostics {
    enabled = true
    storage_uri = azurerm_storage_account.vm_sa.primary_blob_endpoint
  }
}

resource "azurerm_virtual_machine" "vm_proxy" {
  name                  = "vm-proxy-dev-eastus"
  location              = azurerm_resource_group.rg.location
  resource_group_name   = azurerm_resource_group.rg.name
  network_interface_ids = [azurerm_network_interface.vm_proxy_nic.id]
  vm_size               = "Standard_B1ls"

  delete_os_disk_on_termination = true
  delete_data_disks_on_termination = true

  identity {
    type = "SystemAssigned"
  }

  storage_image_reference {
    publisher = "canonical"
    offer     = "0001-com-ubuntu-server-focal"
    sku       = "20_04-lts-gen2"
    version   = "latest"
  }
  storage_os_disk {
    name              = "vm-proxy-dev-eastus-osdisk"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Premium_LRS"
    disk_size_gb = 30
    os_type = "Linux"
  }
  os_profile {
    computer_name  = "vm-proxy-dev-eastus"
    admin_username = "${var.vm_username}"
    custom_data = <<EOT
#cloud-config

package_update: true
package_upgrade: true

packages:
  - net-tools
  - ca-certificates
  - curl
  - gnupg
  - lsb-release

runcmd:
  - mkdir -p /etc/apt/keyrings
  - curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
  - echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
  - apt-get update
  - apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
  - curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
  - docker run hello-world
  - sleep 120s
  - docker swarm join --token $(cat /dev/null | sudo nc 10.4.0.4 8888) 10.4.0.4:2377

final_message: "The system is finally up, after $UPTIME seconds"
EOT
  }
  os_profile_linux_config {
    disable_password_authentication = true
    ssh_keys {
      key_data = file("./ssh/${var.sshkey1_name}.pub")
      path = "/home/${var.vm_username}/.ssh/authorized_keys"
    }
  }

  boot_diagnostics {
    enabled = true
    storage_uri = azurerm_storage_account.vm_sa.primary_blob_endpoint
  }
}

resource "azurerm_network_interface_security_group_association" "nsg-proxy-vm" {
  network_interface_id          = azurerm_network_interface.vm_proxy_nic.id
  network_security_group_id     = azurerm_network_security_group.nsg_proxy.id
}

resource "azurerm_network_interface_security_group_association" "nsg-vm" {
  network_interface_id          = azurerm_network_interface.vm_nic.id
  network_security_group_id     = azurerm_network_security_group.nsg.id
}

resource "azurerm_network_security_group" "nsg" {
  name                = "vm-backend-dev-eastus-nsg"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_network_security_group" "nsg_proxy" {
  name                = "vm-proxy-dev-eastus-nsg"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_network_security_rule" "nsr-ssh" {
  name = "SSH"
  access = "Allow"
  description = "SSH"
  destination_address_prefix = "*"
  destination_port_range = "22"
  direction = "Inbound"
  priority = 300
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg.name
}

resource "azurerm_network_security_rule" "nsr-ssh-proxy" {
  name = "SSH"
  access = "Allow"
  description = "SSH"
  destination_address_prefix = "*"
  destination_port_range = "22"
  direction = "Inbound"
  priority = 300
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg_proxy.name
}

resource "azurerm_network_security_rule" "nsr-docker-registry" {
  name = "DockerRegistry"
  access = "Allow"
  description = "DockerRegistry"
  destination_address_prefix = "*"
  destination_port_range = "5000"
  direction = "Inbound"
  priority = 301
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg.name
}

resource "azurerm_network_security_rule" "nsr-docker-registry-proxy" {
  name = "DockerRegistry"
  access = "Allow"
  description = "DockerRegistry"
  destination_address_prefix = "*"
  destination_port_range = "5000"
  direction = "Inbound"
  priority = 301
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg_proxy.name
}

resource "azurerm_network_security_rule" "nsr-http" {
  name = "HTTP"
  access = "Allow"
  description = "HTTP"
  destination_address_prefix = "*"
  destination_port_range = "80"
  direction = "Inbound"
  priority = 302
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg_proxy.name
}

resource "azurerm_network_security_rule" "nsr-https" {
  name = "HTTPS"
  access = "Allow"
  description = "HTTPS"
  destination_address_prefix = "*"
  destination_port_range = "443"
  direction = "Inbound"
  priority = 303
  protocol = "Tcp"
  source_address_prefix = "*"
  source_port_range = "*"
  resource_group_name         = azurerm_resource_group.rg.name
  network_security_group_name = azurerm_network_security_group.nsg_proxy.name
}

data "azurerm_subscription" "primary" {
}

data "azurerm_client_config" "current" {}

resource "azurerm_role_assignment" "name" {
  scope                = data.azurerm_subscription.primary.id
  role_definition_name = "Contributor"
  principal_id         = azurerm_virtual_machine.vm.identity[0].principal_id
}

resource "azurerm_key_vault_access_policy" "kv-access" {
  key_vault_id = var.keyvault_id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_virtual_machine.vm.identity[0].principal_id

  key_permissions = [
    "Get",
  ]

  secret_permissions = [
    "Get",
  ]

  certificate_permissions = [
    "Get",
  ]
}