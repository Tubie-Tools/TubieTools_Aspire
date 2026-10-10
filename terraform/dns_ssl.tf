# DNS and SSL/TLS Configuration for TubieTools
# This file sets up Azure DNS zones, custom domains, and SSL certificates

variable "custom_domain_name" {
  description = "Custom domain name for the application"
  type        = string
  default     = ""  # e.g., "tubietools.com"
}

variable "enable_custom_domain" {
  description = "Enable custom domain configuration"
  type        = bool
  default     = false
}

variable "certificate_email" {
  description = "Email for Let's Encrypt certificate notifications"
  type        = string
  default     = ""
}

# Create Azure DNS Zone if custom domain is enabled
resource "azurerm_dns_zone" "main" {
  count               = var.enable_custom_domain ? 1 : 0
  name                = var.custom_domain_name
  resource_group_name = azurerm_resource_group.main.name

  tags = local.common_tags
}

# Create DNS A record for API
resource "azurerm_dns_a_record" "api" {
  count               = var.enable_custom_domain ? 1 : 0
  name                = "api"
  zone_name           = azurerm_dns_zone.main[0].name
  resource_group_name = azurerm_resource_group.main.name
  ttl                 = 300
  target_resource_id  = azurerm_public_ip.aks_ingress[0].id

  depends_on = [azurerm_public_ip.aks_ingress]
}

# Create DNS A record for web frontend
resource "azurerm_dns_a_record" "web" {
  count               = var.enable_custom_domain ? 1 : 0
  name                = "app"
  zone_name           = azurerm_dns_zone.main[0].name
  resource_group_name = azurerm_resource_group.main.name
  ttl                 = 300
  target_resource_id  = azurerm_public_ip.aks_ingress[0].id

  depends_on = [azurerm_public_ip.aks_ingress]
}

# Create DNS A record for root domain
resource "azurerm_dns_a_record" "root" {
  count               = var.enable_custom_domain ? 1 : 0
  name                = "@"
  zone_name           = azurerm_dns_zone.main[0].name
  resource_group_name = azurerm_resource_group.main.name
  ttl                 = 300
  target_resource_id  = azurerm_public_ip.aks_ingress[0].id

  depends_on = [azurerm_public_ip.aks_ingress]
}

# Public IP for AKS Ingress
resource "azurerm_public_ip" "aks_ingress" {
  count               = 1
  name                = "${var.project_name}-${var.environment}-ingress-pip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  ip_version          = "IPv4"
  sku                 = "Standard"

  tags = local.common_tags
}

# Key Vault for storing SSL certificates
resource "azurerm_key_vault" "main" {
  name                       = "${replace(var.project_name, "-", "")}${var.environment}kv"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  enabled_for_disk_encryption = true
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "premium"
  soft_delete_retention_days = 7
  purge_protection_enabled   = true

  tags = local.common_tags
}

# Key Vault Access Policy for current user
resource "azurerm_key_vault_access_policy" "current_user" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  certificate_permissions = [
	"Create",
	"Delete",
	"DeleteIssuers",
	"Get",
	"GetIssuers",
	"Import",
	"List",
	"ListIssuers",
	"ManageContacts",
	"ManageIssuers",
	"SetIssuers",
	"Update",
  ]

  key_permissions = [
	"Backup",
	"Create",
	"Decrypt",
	"Delete",
	"Encrypt",
	"Get",
	"Import",
	"List",
	"Purge",
	"Recover",
	"Restore",
	"Sign",
	"UnwrapKey",
	"Update",
	"Verify",
	"WrapKey",
  ]

  secret_permissions = [
	"Backup",
	"Delete",
	"Get",
	"List",
	"Purge",
	"Recover",
	"Restore",
	"Set",
  ]
}

# Key Vault Access Policy for AKS
resource "azurerm_key_vault_access_policy" "aks" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_kubernetes_cluster.aks.kubelet_identity[0].object_id

  certificate_permissions = [
	"Get",
	"List",
  ]

  secret_permissions = [
	"Get",
	"List",
  ]
}

# Application Gateway for SSL termination (optional but recommended)
resource "azurerm_application_gateway" "main" {
  count               = var.enable_custom_domain ? 1 : 0
  name                = "${var.project_name}-${var.environment}-appgw"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  sku {
	name     = "WAF_v2"
	tier     = "WAF_v2"
	capacity = 2
  }

  gateway_ip_configuration {
	name      = "gateway-ip-config"
	subnet_id = azurerm_subnet.aks_subnet.id
  }

  frontend_port {
	name = "http"
	port = 80
  }

  frontend_port {
	name = "https"
	port = 443
  }

  frontend_ip_configuration {
	name                 = "frontend-ip-config"
	public_ip_address_id = azurerm_public_ip.aks_ingress[0].id
  }

  backend_address_pool {
	name = "backend-pool"
  }

  backend_http_settings {
	name                  = "http-settings"
	cookie_based_affinity = "Disabled"
	port                  = 80
	protocol              = "Http"
	request_timeout       = 20
  }

  http_listener {
	name                           = "http-listener"
	frontend_ip_configuration_name = "frontend-ip-config"
	frontend_port_name             = "http"
	protocol                       = "Http"
  }

  request_routing_rule {
	name                       = "http-rule"
	priority                   = 1
	rule_type                  = "Basic"
	http_listener_name         = "http-listener"
	backend_address_pool_name  = "backend-pool"
	backend_http_settings_name = "http-settings"
  }

  waf_configuration {
	enabled           = true
	firewall_mode     = "Detection"
	rule_set_version  = "3.1"
  }

  tags = local.common_tags
}

# Get current Azure context for Key Vault access
data "azurerm_client_config" "current" {}

# Output DNS and SSL information
output "dns_zone_nameservers" {
  description = "Name servers for the DNS zone (configure in domain registrar)"
  value       = var.enable_custom_domain ? azurerm_dns_zone.main[0].name_servers : null
}

output "ingress_public_ip" {
  description = "Public IP for Ingress Controller"
  value       = azurerm_public_ip.aks_ingress[0].ip_address
}

output "key_vault_name" {
  description = "Key Vault name for storing SSL certificates"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

output "application_gateway_name" {
  description = "Application Gateway name (if enabled)"
  value       = var.enable_custom_domain ? azurerm_application_gateway.main[0].name : null
}
