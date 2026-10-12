# Example Terraform variables file
# Copy this to terraform.tfvars and update with your values

environment = "dev"
location    = "East US"
project_name = "tubietools"

# SQL Server credentials
sql_admin_username = "sqladmin"
sql_admin_password = "Your$ecurePassword123!"  # Change this!

# AKS Configuration
aks_node_count = 2
aks_vm_size    = "Standard_DS2_v2"

# Container image
container_image_tag = "latest"

# Azure DevOps Configuration (optional)
devops_pat     = ""  # Your PAT token
devops_org     = ""  # Your organization
devops_project = ""  # Your project name
