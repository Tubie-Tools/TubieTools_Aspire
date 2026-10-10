# TubieTools Azure Deployment Guide

Complete guide for deploying the TubieTools Aspire application to Azure using Terraform and Kubernetes.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Configuration](#configuration)
4. [Deployment Steps](#deployment-steps)
5. [Post-Deployment](#post-deployment)
6. [Troubleshooting](#troubleshooting)
7. [Monitoring and Scaling](#monitoring-and-scaling)
8. [Cleanup](#cleanup)

## Prerequisites

### Required Tools

- **Azure CLI** (v2.50+): [Install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **Terraform** (v1.0+): [Install](https://www.terraform.io/downloads.html)
- **Docker Desktop** (latest): [Install](https://www.docker.com/products/docker-desktop)
- **kubectl** (v1.27+): [Install](https://kubernetes.io/docs/tasks/tools/)
- **Helm** (v3.10+): [Install](https://helm.sh/docs/intro/install/)
- **PowerShell 7+** (Windows): [Install](https://github.com/PowerShell/PowerShell/releases)
- **Bash** (macOS/Linux)

### Azure Requirements

- Active Azure subscription with sufficient quota
- Contributor or Owner role on subscription
- Default region set to your preferred location

### Local Requirements

- Minimum 8GB RAM for Docker builds
- 50GB free disk space
- Git access to the repository

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│           Azure Resource Group                       │
├─────────────────────────────────────────────────────┤
│                                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │   Azure Kubernetes Service (AKS)             │  │
│  │  ┌────────────────────────────────────────┐  │  │
│  │  │ PublicAPI        (3 replicas + HPA)    │  │  │
│  │  │ WebFrontend      (2 replicas + HPA)    │  │  │
│  │  │ SentimentAPI     (1-3 replicas + HPA)  │  │  │
│  │  │ ForecastingAPI   (1-3 replicas + HPA)  │  │  │
│  │  └────────────────────────────────────────┘  │  │
│  │                                                │  │
│  │  ┌─────────────────────────────────────────┐ │  │
│  │  │ Monitoring Stack                        │ │  │
│  │  │ - Prometheus                            │ │  │
│  │  │ - Grafana                               │ │  │
│  │  │ - Log Analytics                         │ │  │
│  │  └─────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────┘  │
│                                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │ Azure Container Registry (ACR)               │  │
│  │ - publicapi:latest                           │  │
│  │ - webfrontend:latest                         │  │
│  │ - sentimentapi:latest                        │  │
│  │ - forecastingapi:latest                      │  │
│  └──────────────────────────────────────────────┘  │
│                                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │ Azure SQL Database                           │  │
│  │ - Primary database instance                  │  │
│  │ - Automated backups                          │  │
│  │ - Geo-redundant storage (optional)           │  │
│  └──────────────────────────────────────────────┘  │
│                                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │ Additional Services                          │  │
│  │ - Storage Account (data persistence)         │  │
│  │ - Key Vault (secrets management)             │  │
│  │ - Application Insights (monitoring)          │  │
│  │ - Log Analytics Workspace                    │  │
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

## Configuration

### Step 1: Prepare Terraform Variables

```bash
# Copy the example file
cp terraform/terraform.tfvars.example terraform/terraform.tfvars

# Edit with your values
nano terraform/terraform.tfvars
```

### Step 2: Required Variables

```hcl
# terraform/terraform.tfvars

environment = "dev"                    # dev, staging, prod
location    = "East US"                # Azure region
project_name = "tubietools"

# SQL Server credentials (CHANGE THESE!)
sql_admin_username = "sqladmin"
sql_admin_password = "YourSecureP@ssw0rd123!"  # Min 8 chars, mix of upper/lower/numbers/symbols

# AKS Configuration
aks_node_count = 2                     # Initial node count
aks_vm_size    = "Standard_DS2_v2"    # VM size for nodes

# Container image
container_image_tag = "latest"

# Optional: DNS/SSL Configuration
enable_custom_domain = true
custom_domain_name   = "tubietools.com"
certificate_email    = "admin@tubietools.com"

# Optional: Azure DevOps Integration
devops_pat     = "your-pat-token"
devops_org     = "your-org"
devops_project = "your-project"
```

### Step 3: Azure Login

```bash
# Login to Azure
az login

# Verify subscription (optional)
az account show

# Set default subscription if multiple exist
az account set --subscription <SUBSCRIPTION_ID>
```

## Deployment Steps

### Option 1: Automated Deployment (Recommended)

```bash
# Make script executable
chmod +x deploy.sh

# Run deployment
./deploy.sh

# The script will:
# 1. Verify prerequisites
# 2. Initialize and validate Terraform
# 3. Plan and apply infrastructure
# 4. Build and push Docker images
# 5. Configure Kubernetes cluster
# 6. Deploy applications
# 7. Display deployment information
```

### Option 2: Manual Deployment

#### Step 1: Initialize Terraform

```bash
cd terraform
terraform init
terraform validate
```

#### Step 2: Plan and Apply Infrastructure

```bash
# Review the plan
terraform plan -out=tfplan

# Apply the plan
terraform apply tfplan

# Export outputs
export RESOURCE_GROUP=$(terraform output -raw resource_group_name)
export ACR_LOGIN_SERVER=$(terraform output -raw acr_login_server)
export ACR_PASSWORD=$(terraform output -raw acr_admin_password)
export AKS_CLUSTER_NAME=$(terraform output -raw aks_cluster_name)
```

#### Step 3: Build Docker Images

```bash
# Login to Docker
docker login -u <ACR_USERNAME> -p "$ACR_PASSWORD" "$ACR_LOGIN_SERVER"

# Build and push each image
docker build -f Dockerfiles/PublicAPI.Dockerfile -t "$ACR_LOGIN_SERVER/publicapi:latest" .
docker push "$ACR_LOGIN_SERVER/publicapi:latest"

docker build -f Dockerfiles/WebFrontend.Dockerfile -t "$ACR_LOGIN_SERVER/webfrontend:latest" .
docker push "$ACR_LOGIN_SERVER/webfrontend:latest"

docker build -f Dockerfiles/SentimentAPI.Dockerfile -t "$ACR_LOGIN_SERVER/sentimentapi:latest" .
docker push "$ACR_LOGIN_SERVER/sentimentapi:latest"

docker build -f Dockerfiles/ForecastingAPI.Dockerfile -t "$ACR_LOGIN_SERVER/forecastingapi:latest" .
docker push "$ACR_LOGIN_SERVER/forecastingapi:latest"
```

#### Step 4: Configure Kubernetes

```bash
# Get AKS credentials
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$AKS_CLUSTER_NAME"

# Create namespace and secrets
kubectl create namespace tubietools
kubectl create secret docker-registry acr-credentials \
  --docker-server="$ACR_LOGIN_SERVER" \
  --docker-username=<ACR_USERNAME> \
  --docker-password="$ACR_PASSWORD" \
  --docker-email="admin@tubietools.com" \
  -n tubietools

# Deploy applications
kubectl apply -f kubernetes/advanced-deployments.yaml
```

#### Step 5: Verify Deployment

```bash
# Check deployments
kubectl get deployments -n tubietools

# Check pods
kubectl get pods -n tubietools

# Check services
kubectl get svc -n tubietools

# Check events
kubectl get events -n tubietools --sort-by='.lastTimestamp'
```

## Post-Deployment

### 1. Update Application Configuration

```bash
# Configure database connection string
kubectl set env deployment/publicapi \
  ConnectionStrings__DefaultConnection="<YOUR_CONNECTION_STRING>" \
  -n tubietools

# Configure Application Insights
kubectl set env deployment/publicapi \
  ApplicationInsights__ConnectionString="<YOUR_CONNECTION_STRING>" \
  -n tubietools
```

### 2. Configure DNS (if using custom domain)

Get the ingress public IP:

```bash
kubectl get ingress -n tubietools
# or
terraform output ingress_public_ip
```

In your domain registrar, create A records:
- `api.tubietools.com` → `<INGRESS_IP>`
- `app.tubietools.com` → `<INGRESS_IP>`
- `tubietools.com` → `<INGRESS_IP>`

### 3. SSL/TLS Setup (Optional)

Install cert-manager for automatic SSL:

```bash
# Add Helm repo
helm repo add jetstack https://charts.jetstack.io
helm repo update

# Install cert-manager
helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --create-namespace \
  --version v1.13.0 \
  --set installCRDs=true
```

Create ClusterIssuer for Let's Encrypt:

```bash
kubectl apply -f - <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
	server: https://acme-v02.api.letsencrypt.org/directory
	email: admin@tubietools.com
	privateKeySecretRef:
	  name: letsencrypt-prod
	solvers:
	- http01:
		ingress:
		  class: nginx
EOF
```

### 4. Database Migration

```bash
# Forward port to SQL Server
kubectl port-forward -n tubietools <sql-pod> 1433:1433 &

# Run migrations or seed data
# Use your preferred migration tool (EF Core, FluentMigrator, etc.)
```

## Post-Deployment Configuration

### 1. Get Access Credentials

```bash
# Get kubeconfig for kubectl
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# Get ACR credentials
az acr login --name $ACR_NAME

# Get SQL Server connection
terraform output sql_server_fqdn
terraform output sql_database_name
```

### 2. Access Services

```bash
# Get LoadBalancer IP for web frontend
kubectl get svc webfrontend-service -n tubietools -o jsonpath='{.status.loadBalancer.ingress[0].ip}'

# Port forward for local testing
kubectl port-forward svc/webfrontend-service 5000:80 -n tubietools
# Access at http://localhost:5000
```

### 3. View Logs

```bash
# Real-time logs for a specific pod
kubectl logs -f deployment/publicapi -n tubietools

# Logs from all pods in a deployment
kubectl logs -l app=publicapi -n tubietools --all-containers=true

# Previous logs (if pod crashed)
kubectl logs deployment/publicapi -n tubietools --previous
```

## Monitoring and Scaling

### 1. View Metrics

```bash
# Get pod resource usage
kubectl top pods -n tubietools

# Get node resource usage
kubectl top nodes

# Get HPA status
kubectl get hpa -n tubietools
kubectl describe hpa publicapi-hpa -n tubietools
```

### 2. Access Grafana Dashboard

```bash
# Forward Grafana port
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80

# Access at http://localhost:3000
# Default: admin / prom-operator
```

### 3. View Application Insights

```bash
# In Azure Portal, navigate to:
# Resource Groups > $RESOURCE_GROUP > Application Insights resource
```

### 4. Manual Scaling

```bash
# Scale deployment manually
kubectl scale deployment publicapi --replicas=5 -n tubietools

# Check HPA is not overriding
kubectl get hpa publicapi-hpa -n tubietools
```

## Troubleshooting

### Issue: Pods stuck in ImagePullBackOff

```bash
# Check image pull secret
kubectl get secrets -n tubietools
kubectl describe secret acr-credentials -n tubietools

# Verify ACR credentials
az acr login --name $ACR_NAME

# Recreate secret if needed
kubectl delete secret acr-credentials -n tubietools
kubectl create secret docker-registry acr-credentials \
  --docker-server="$ACR_LOGIN_SERVER" \
  --docker-username=<USER> \
  --docker-password="<PASSWORD>" \
  -n tubietools
```

### Issue: Database connection failures

```bash
# Check connection string in secrets
kubectl get secret tubietools-secrets -n tubietools -o yaml

# Verify SQL Server is accessible
kubectl run -it --rm debug --image=mcr.microsoft.com/azure-cli --restart=Never -- \
  bash -c "apt-get update && apt-get install -y sqlcmd && sqlcmd -S <SERVER_FQDN> -U <USER> -P '<PASSWORD>' -Q 'SELECT 1'"
```

### Issue: Insufficient resources

```bash
# Check node capacity
kubectl describe nodes

# Check resource quotas
kubectl get resourcequota -n tubietools

# Scale AKS cluster
az aks scale --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME --node-count 5
```

### Issue: High memory/CPU usage

```bash
# Identify resource-heavy pods
kubectl top pods -n tubietools --sort-by=memory
kubectl top pods -n tubietools --sort-by=cpu

# Adjust resource limits in manifests
# Edit kubernetes/advanced-deployments.yaml and reapply
```

## Cleanup

To destroy all resources and avoid unnecessary charges:

```bash
# Using the deployment script
./deploy.sh cleanup

# Or manually
cd terraform
terraform destroy
cd ..

# Clean up local resources
docker logout $ACR_LOGIN_SERVER
rm -rf ~/.kube/config  # Optional
```

## Environment-Specific Configurations

### Development Environment

```hcl
environment = "dev"
aks_node_count = 2
aks_vm_size = "Standard_B2s"  # Cost-effective
enable_cluster_autoscaling = true
min_node_count = 1
max_node_count = 3
sql_admin_password = "DevP@ssw0rd123"  # Change before production
```

### Production Environment

```hcl
environment = "prod"
aks_node_count = 3
aks_vm_size = "Standard_D4s_v3"  # Higher performance
enable_cluster_autoscaling = true
min_node_count = 3
max_node_count = 20
enable_custom_domain = true
custom_domain_name = "tubietools.com"
sql_admin_password = "<STRONG_SECURE_PASSWORD>"
```

## Support and Resources

- [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Kubernetes Service Documentation](https://docs.microsoft.com/en-us/azure/aks/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/reference-index)

## Security Best Practices

1. **Never commit secrets to version control**
   - Use `.gitignore` for `terraform.tfvars`
   - Rotate credentials regularly

2. **Enable Azure Policy** for compliance

3. **Configure Network Policies** for pod-to-pod communication

4. **Enable RBAC** on AKS cluster

5. **Use Azure Key Vault** for certificate management

6. **Enable Azure Security Center** for vulnerability scanning

7. **Implement Pod Security Policies** for pod-level security

8. **Use private ACR** with firewall rules

9. **Enable audit logging** on SQL Database

10. **Implement network security groups** for cluster access
