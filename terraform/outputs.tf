output "resource_group_name" {
  description = "Name of the created resource group"
  value       = azurerm_resource_group.main.name
}

output "acr_login_server" {
  description = "Azure Container Registry login server"
  value       = azurerm_container_registry.acr.login_server
}

output "acr_admin_username" {
  description = "Azure Container Registry admin username"
  value       = azurerm_container_registry.acr.admin_username
}

output "acr_admin_password" {
  description = "Azure Container Registry admin password"
  value       = azurerm_container_registry.acr.admin_password
  sensitive   = true
}

output "aks_cluster_name" {
  description = "AKS cluster name"
  value       = azurerm_kubernetes_cluster.aks.name
}

output "aks_kube_config" {
  description = "Kubernetes config"
  value       = azurerm_kubernetes_cluster.aks.kube_config_raw
  sensitive   = true
}

output "sql_server_fqdn" {
  description = "SQL Server fully qualified domain name"
  value       = azurerm_mssql_server.sql_server.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "SQL Database name"
  value       = azurerm_mssql_database.sql_database.name
}

output "storage_account_name" {
  description = "Storage account name"
  value       = azurerm_storage_account.storage.name
}

output "app_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.app_insights.instrumentation_key
  sensitive   = true
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.app_insights.connection_string
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = azurerm_log_analytics_workspace.law.id
}

output "get_credentials_command" {
  description = "Command to get AKS cluster credentials"
  value       = "az aks get-credentials --resource-group ${azurerm_resource_group.main.name} --name ${azurerm_kubernetes_cluster.aks.name}"
}
