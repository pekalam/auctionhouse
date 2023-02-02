variable "resource_group_name" {
  default = "auctionhouse-vm"
}

variable "app_name" {
  default = "auctionhouse-vm"
}

variable "location" {
  default = "eastus"
}

variable "vm_username" {
  default = "auctionhouse"
}

variable "sshkey1_name" {
  default = "sshk-vm-backend-dev"
}

variable "keyvault_id" {
  default = "/subscriptions/54a8b190-aefa-4989-849f-3931ec46cb62/resourceGroups/auctionhouse/providers/Microsoft.KeyVault/vaults/auctionhouse-kv-prod"
}

variable "auctionhouse_ip_id" {
  default = "/subscriptions/54a8b190-aefa-4989-849f-3931ec46cb62/resourceGroups/auctionhouse-ip/providers/Microsoft.Network/publicIPAddresses/pip-auctionhouse-vm"
}

variable "peered_network_id" {
  default = "/subscriptions/54a8b190-aefa-4989-849f-3931ec46cb62/resourceGroups/utils/providers/Microsoft.Network/virtualNetworks/utils-vnet"
}

variable "peered_network_name" {
  default = "utils-vnet"
}

variable "peered_network_rg_name" {
  default = "utils"
}