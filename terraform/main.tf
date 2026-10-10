terraform {
  required_version = ">= 1.0"
  required_providers {
	azurerm = {
	  source  = "hashicorp/azurerm"
	  version = "~> 3.0"
	}
  }

  # Uncomment to use Azure blob storage for state management
  # backend "azurerm" {
  #   resource_group_name  = "tfstate-rg"
  #   storage_account_name = "tfstate<random>"
  #   container_name       = "tfstate"
  #   key                  = "tubitools.tfstate"
  # }
}

provider "azurerm" {
  features {}
}

# Variables
variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "East US"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "tubietools"
}

variable "sql_admin_username" {
  description = "SQL Server admin username"
  type        = string
  default     = "sqladmin"
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}

variable "aks_node_count" {
  description = "Number of AKS nodes"
  type        = number
  default     = 2
}

variable "aks_vm_size" {
  description = "VM size for AKS nodes"
  type        = string
  default     = "Standard_DS2_v2"
}

variable "container_image_tag" {
  description = "Container image tag (e.g., latest, v1.0.0)"
  type        = string
  default     = "latest"
}

variable "devops_pat" {
  description = "Azure DevOps Personal Access Token for pipeline setup"
  type        = string
  sensitive   = true
  default     = ""
}

variable "devops_org" {
  description = "Azure DevOps organization name"
  type        = string
  default     = ""
}

variable "devops_project" {
  description = "Azure DevOps project name"
  type        = string
  default     = ""
}

# Local variables
locals {
  resource_group_name = "${var.project_name}-${var.environment}-rg"
  acr_name            = "${replace(var.project_name, "-", "")}${var.environment}acr"
  aks_cluster_name    = "${var.project_name}-${var.environment}-aks"
  sql_server_name     = "${replace(var.project_name, "-", "")}-${var.environment}-sql"
  app_insights_name   = "${var.project_name}-${var.environment}-appinsights"

  common_tags = {
	Environment = var.environment
	Project     = var.project_name
	ManagedBy   = "Terraform"
	CreatedDate = timestamp()
  }
}

# Create Resource Group
resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.location
  tags     = local.common_tags
}

# Create Container Registry
resource "azurerm_container_registry" "acr" {
  name                = local.acr_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Standard"
  admin_enabled       = true

  tags = local.common_tags
}

# Create Application Insights
resource "azurerm_application_insights" "app_insights" {
  name                = local.app_insights_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"

  tags = local.common_tags
}

# Create Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "law" {
  name                = "${var.project_name}-${var.environment}-law"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"
  retention_in_days   = 30

  tags = local.common_tags
}

# Create SQL Server
resource "azurerm_mssql_server" "sql_server" {
  name                         = local.sql_server_name
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"

  tags = local.common_tags
}

# Create SQL Database
resource "azurerm_mssql_database" "sql_database" {
  name           = "${var.project_name}-${var.environment}-db"
  server_id      = azurerm_mssql_server.sql_server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 250
  sku_name       = "S0" # Basic: S0, Standard: S1-S12, Premium: P1-P15

  deletion_protection_enabled = var.environment == "prod" ? true : false

  tags = local.common_tags
}

# SQL Firewall Rule - Allow Azure Services
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Create Virtual Network for AKS
resource "azurerm_virtual_network" "vnet" {
  name                = "${var.project_name}-${var.environment}-vnet"
  address_space       = ["10.0.0.0/8"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = local.common_tags
}

# Create Subnet for AKS
resource "azurerm_subnet" "aks_subnet" {
  name                 = "aks-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.240.0.0/16"]
}

# Create User Assigned Identity for AKS
resource "azurerm_user_assigned_identity" "aks_identity" {
  location            = azurerm_resource_group.main.location
  name                = "${var.project_name}-${var.environment}-aks-identity"
  resource_group_name = azurerm_resource_group.main.name

  tags = local.common_tags
}

# Give AKS identity pull permissions on ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope              = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id       = azurerm_user_assigned_identity.aks_identity.principal_id
}

# Create AKS Cluster
resource "azurerm_kubernetes_cluster" "aks" {
  name                = local.aks_cluster_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "${var.project_name}-${var.environment}"

  kubernetes_version = "1.27"

  default_node_pool {
	name            = "default"
	node_count      = var.aks_node_count
	vm_size         = var.aks_vm_size
	vnet_subnet_id  = azurerm_subnet.aks_subnet.id
	os_disk_size_gb = 128

	node_labels = {
	  Environment = var.environment
	}
  }

  identity {
	type         = "UserAssigned"
	identity_ids = [azurerm_user_assigned_identity.aks_identity.id]
  }

  network_profile {
	network_plugin    = "azure"
	network_policy    = "azure"
	service_cidr      = "10.0.0.0/16"
	dns_service_ip    = "10.0.0.10"
	docker_bridge_cidr = "172.17.0.1/16"
	load_balancer_sku = "standard"
  }

  monitor_metrics {
	annotations_allowed = null
	labels_allowed      = null
	logs_allowed = [
	  "var.log",
	  "kube-system"
	]
  }

  oms_agent {
	log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
	msi_auth_for_monitoring_enabled = true
  }

  ingress_application_gateway {
	enabled   = true
	subnet_id = azurerm_subnet.aks_subnet.id
  }

  tags = local.common_tags

  depends_on = [
	azurerm_role_assignment.aks_acr_pull
  ]
}

# Storage Account (for shared data if needed)
resource "azurerm_storage_account" "storage" {
  name                     = "${replace(var.project_name, "-", "")}${var.environment}sa"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = local.common_tags
}

# Create container in storage account
resource "azurerm_storage_container" "data_container" {
  name                  = "application-data"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "private"
}
