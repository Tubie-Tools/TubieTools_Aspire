#!/bin/bash
# TubieTools Azure Deployment - Quick Reference Cheat Sheet
# Commands and configurations for rapid deployment and management

# ============================================================================
# SECTION 1: INITIAL SETUP
# ============================================================================

# Install all prerequisites (macOS with Homebrew)
brew install terraform azure-cli docker kubernetes-cli helm

# Install prerequisites (Linux - Ubuntu/Debian)
# terraform: https://www.terraform.io/downloads.html
# azure-cli: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
# docker: curl -fsSL https://get.docker.com -o get-docker.sh && sudo sh get-docker.sh
# kubectl: curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
# helm: curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Verify installations
terraform version
az --version
docker --version
kubectl version --client
helm version

# ============================================================================
# SECTION 2: AZURE SETUP
# ============================================================================

# Login to Azure
az login

# List subscriptions
az account list --output table

# Set default subscription
az account set --subscription "Subscription Name"

# Create resource group (alternative to Terraform)
az group create --name tubietools-dev-rg --location "East US"

# ============================================================================
# SECTION 3: TERRAFORM COMMANDS
# ============================================================================

# Initialize Terraform
cd terraform
terraform init

# Format Terraform files
terraform fmt -recursive

# Validate configuration
terraform validate

# Plan deployment (shows what will be created)
terraform plan -out=tfplan

# Review the plan
# (Read through output carefully)

# Apply the plan
terraform apply tfplan

# Get all outputs
terraform output

# Get specific output
terraform output -raw resource_group_name
terraform output -raw acr_login_server
terraform output -raw aks_cluster_name
terraform output -raw sql_server_fqdn

# Save credentials to environment variables
export RESOURCE_GROUP=$(terraform output -raw resource_group_name)
export ACR_LOGIN_SERVER=$(terraform output -raw acr_login_server)
export ACR_USERNAME=$(terraform output -raw acr_admin_username)
export ACR_PASSWORD=$(terraform output -raw acr_admin_password)
export AKS_CLUSTER_NAME=$(terraform output -raw aks_cluster_name)
export SQL_SERVER=$(terraform output -raw sql_server_fqdn)

# Destroy all resources (WARNING: irreversible!)
terraform destroy

# ============================================================================
# SECTION 4: DOCKER COMMANDS
# ============================================================================

# Login to Azure Container Registry
az acr login --name $ACR_NAME
# OR with credentials
docker login -u $ACR_USERNAME -p $ACR_PASSWORD $ACR_LOGIN_SERVER

# Build Docker images
docker build -f Dockerfiles/PublicAPI.Dockerfile -t $ACR_LOGIN_SERVER/publicapi:latest .
docker build -f Dockerfiles/WebFrontend.Dockerfile -t $ACR_LOGIN_SERVER/webfrontend:latest .
docker build -f Dockerfiles/SentimentAPI.Dockerfile -t $ACR_LOGIN_SERVER/sentimentapi:latest .
docker build -f Dockerfiles/ForecastingAPI.Dockerfile -t $ACR_LOGIN_SERVER/forecastingapi:latest .

# Push images to ACR
docker push $ACR_LOGIN_SERVER/publicapi:latest
docker push $ACR_LOGIN_SERVER/webfrontend:latest
docker push $ACR_LOGIN_SERVER/sentimentapi:latest
docker push $ACR_LOGIN_SERVER/forecastingapi:latest

# View images in ACR
az acr repository list --name $ACR_NAME --output table

# Build and tag with version
docker build -f Dockerfiles/PublicAPI.Dockerfile -t $ACR_LOGIN_SERVER/publicapi:v1.0.0 .
docker push $ACR_LOGIN_SERVER/publicapi:v1.0.0

# ============================================================================
# SECTION 5: KUBERNETES - CLUSTER & CREDENTIALS
# ============================================================================

# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# Verify cluster connection
kubectl cluster-info
kubectl get nodes
kubectl get nodes -o wide

# View cluster resources
kubectl top nodes
kubectl describe node <node-name>

# Get cluster credentials in raw format
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME --file kubeconfig.txt

# ============================================================================
# SECTION 6: KUBERNETES - NAMESPACES & SECRETS
# ============================================================================

# Create namespace
kubectl create namespace tubietools

# Create image pull secret
kubectl create secret docker-registry acr-credentials \
  --docker-server=$ACR_LOGIN_SERVER \
  --docker-username=$ACR_USERNAME \
  --docker-password=$ACR_PASSWORD \
  --docker-email="admin@tubietools.com" \
  -n tubietools

# View secrets
kubectl get secrets -n tubietools
kubectl describe secret acr-credentials -n tubietools

# Create/update generic secret
kubectl create secret generic db-credentials \
  --from-literal=username=sqladmin \
  --from-literal=password='YourPassword123!' \
  -n tubietools

# View logs of secret creation
kubectl get events -n tubietools --sort-by='.lastTimestamp'

# ============================================================================
# SECTION 7: KUBERNETES - DEPLOYMENTS & SERVICES
# ============================================================================

# Apply all manifests
kubectl apply -f kubernetes/advanced-deployments.yaml

# View deployments
kubectl get deployments -n tubietools
kubectl describe deployment publicapi -n tubietools

# View pods
kubectl get pods -n tubietools
kubectl get pods -n tubietools -o wide

# View services
kubectl get svc -n tubietools
kubectl describe svc webfrontend-service -n tubietools

# View ingress
kubectl get ingress -n tubietools
kubectl describe ingress tubietools-ingress -n tubietools

# Rollout status
kubectl rollout status deployment/publicapi -n tubietools

# Get logs
kubectl logs deployment/publicapi -n tubietools
kubectl logs -f deployment/publicapi -n tubietools  # Follow logs
kubectl logs deployment/publicapi -n tubietools --previous  # Previous version
kubectl logs -l app=publicapi -n tubietools  # All replicas

# Execute commands in pod
kubectl exec -it <pod-name> -n tubietools -- bash
kubectl exec <pod-name> -n tubietools -- curl http://localhost/health

# Port forward to local machine
kubectl port-forward svc/publicapi-service 8080:80 -n tubietools
kubectl port-forward pod/<pod-name> 8080:80 -n tubietools

# ============================================================================
# SECTION 8: KUBERNETES - SCALING & UPDATES
# ============================================================================

# View HPA (Horizontal Pod Autoscaler)
kubectl get hpa -n tubietools
kubectl describe hpa publicapi-hpa -n tubietools
kubectl get hpa -n tubietools -w  # Watch mode

# Manual pod scaling
kubectl scale deployment publicapi --replicas=5 -n tubietools

# Update deployment image
kubectl set image deployment/publicapi \
  publicapi=$ACR_LOGIN_SERVER/publicapi:v1.0.1 -n tubietools

# Restart deployment (rolling restart)
kubectl rollout restart deployment/publicapi -n tubietools

# Undo last deployment
kubectl rollout undo deployment/publicapi -n tubietools

# View rollout history
kubectl rollout history deployment/publicapi -n tubietools

# Update environment variables
kubectl set env deployment/publicapi \
  ASPNETCORE_ENVIRONMENT=Production \
  LOG_LEVEL=Information \
  -n tubietools

# ============================================================================
# SECTION 9: KUBERNETES - MONITORING & DEBUGGING
# ============================================================================

# Resource usage
kubectl top pods -n tubietools
kubectl top nodes
kubectl top pod <pod-name> -n tubietools --containers

# Events
kubectl get events -n tubietools --sort-by='.lastTimestamp'
kubectl get events -A --sort-by='.lastTimestamp'  # All namespaces

# Describe resources
kubectl describe pod <pod-name> -n tubietools
kubectl describe node <node-name>

# Debug pod
kubectl debug pod/<pod-name> -it -n tubietools

# Check pod status
kubectl get pod <pod-name> -n tubietools -o yaml

# CPU/Memory limits
kubectl get pod <pod-name> -n tubietools -o jsonpath='{.spec.containers[*].resources}'

# ============================================================================
# SECTION 10: MONITORING & GRAFANA
# ============================================================================

# Port forward Prometheus
kubectl port-forward -n monitoring svc/prometheus 9090:9090
# Visit: http://localhost:9090

# Port forward Grafana
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80
# Visit: http://localhost:3000
# Default: admin / prom-operator

# Get Grafana password
kubectl get secret -n monitoring prometheus-grafana \
  -o jsonpath="{.data.admin-password}" | base64 -d

# Port forward AlertManager
kubectl port-forward -n monitoring svc/prometheus-kube-prom-alertmanager 9093:9093
# Visit: http://localhost:9093

# ============================================================================
# SECTION 11: DATABASE CONNECTIONS
# ============================================================================

# Get SQL Server FQDN
echo $SQL_SERVER

# SQL Connection String
"Server=tcp:$SQL_SERVER,1433;Initial Catalog=<DB_NAME>;User ID=sqladmin;Password=<PASSWORD>;"

# Test SQL connection with kubectl
kubectl run -it --rm mssql-test \
  --image=mcr.microsoft.com/mssql/server:latest \
  --restart=Never \
  -n tubietools \
  -- /bin/bash

# Inside the pod:
# /opt/mssql-tools/bin/sqlcmd -S $SQL_SERVER -U sqladmin -P $PASSWORD -Q "SELECT @@VERSION"

# Forward SQL port for local connections
kubectl port-forward <sql-pod> 1433:1433 -n tubietools

# ============================================================================
# SECTION 12: CLEANUP & REMOVAL
# ============================================================================

# Delete namespace (deletes all resources in it)
kubectl delete namespace tubietools

# Delete specific deployment
kubectl delete deployment publicapi -n tubietools

# Delete service
kubectl delete service publicapi-service -n tubietools

# Delete all resources in namespace
kubectl delete all --all -n tubietools

# Delete persistent volume claims
kubectl delete pvc --all -n tubietools

# Destroy Terraform resources
cd terraform
terraform destroy -auto-approve  # Skip confirmation

# ============================================================================
# SECTION 13: USEFUL ALIASES (Add to ~/.bashrc or ~/.zshrc)
# ============================================================================

# Add these to your shell profile
alias k='kubectl'
alias kns='kubectl config set-context --current --namespace'
alias kgp='kubectl get pods'
alias kgd='kubectl get deployments'
alias kgs='kubectl get services'
alias kdp='kubectl describe pod'
alias kdl='kubectl logs -f'
alias kex='kubectl exec -it'
alias kkill='kubectl delete pod --grace-period=0 --force'

# Usage:
# k get pods -n tubietools
# kns tubietools
# kgp
# kdl deployment/publicapi

# ============================================================================
# SECTION 14: AZURE CLI COMMANDS
# ============================================================================

# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# List all AKS clusters
az aks list --output table

# Get cluster details
az aks show --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME

# Scale AKS cluster
az aks scale --resource-group $RESOURCE_GROUP \
  --name $AKS_CLUSTER_NAME --node-count 5

# View ACR repositories
az acr repository list --name $ACR_NAME --output table

# View ACR images
az acr repository show-tags --name $ACR_NAME --repository publicapi

# Delete ACR image
az acr repository delete --name $ACR_NAME --image publicapi:old-tag

# View SQL Server
az sql server show --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME

# View SQL Database
az sql db list --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME --output table

# ============================================================================
# SECTION 15: ENVIRONMENT VARIABLES (.env file)
# ============================================================================

# Create .env file with:
export RESOURCE_GROUP=tubietools-dev-rg
export ACR_NAME=tubietoolsdevacr
export ACR_LOGIN_SERVER=tubietoolsdevacr.azurecr.io
export AKS_CLUSTER_NAME=tubietools-dev-aks
export SQL_SERVER_NAME=tubietools-dev-sql
export SUBSCRIPTION_ID=your-subscription-id

# Load environment:
# source .env

# ============================================================================
# SECTION 16: TROUBLESHOOTING COMMANDS
# ============================================================================

# Check pod events
kubectl describe pod <pod-name> -n tubietools

# Check pod logs
kubectl logs <pod-name> -n tubietools
kubectl logs <pod-name> -n tubietools --previous

# Check container runtime logs
kubectl logs <pod-name> -c <container-name> -n tubietools

# Inspect pod details
kubectl get pod <pod-name> -n tubietools -o yaml

# Test connectivity
kubectl run -it --rm curl --image=curlimages/curl --restart=Never -- \
  curl http://publicapi-service:80/health -n tubietools

# Check resource quotas
kubectl get resourcequota -n tubietools

# Check network policies
kubectl get networkpolicies -n tubietools

# Check persistent volumes
kubectl get pvc -n tubietools
kubectl describe pvc <pvc-name> -n tubietools

# ============================================================================
# SECTION 17: COMMON OPERATIONS WORKFLOW
# ============================================================================

# Deploy new version:
# 1. Build image
docker build -f Dockerfiles/PublicAPI.Dockerfile -t $ACR_LOGIN_SERVER/publicapi:v1.0.1 .

# 2. Push to ACR
docker push $ACR_LOGIN_SERVER/publicapi:v1.0.1

# 3. Update deployment
kubectl set image deployment/publicapi \
  publicapi=$ACR_LOGIN_SERVER/publicapi:v1.0.1 -n tubietools

# 4. Verify rollout
kubectl rollout status deployment/publicapi -n tubietools

# 5. Check logs
kubectl logs -f deployment/publicapi -n tubietools

# Rollback if needed:
kubectl rollout undo deployment/publicapi -n tubietools

# ============================================================================
# SECTION 18: COST MONITORING
# ============================================================================

# Get current resource costs (Azure CLI)
az cost management query --time-period P30D --granularity monthly

# View all resources in resource group
az resource list --resource-group $RESOURCE_GROUP --output table

# View resource sizes
kubectl get nodes -o custom-columns=NAME:.metadata.name,CPU:.status.capacity.cpu,MEMORY:.status.capacity.memory

# ============================================================================

# TIPS:
# - Always backup before running 'destroy' commands
# - Use 'kubectl dry-run=client -o yaml' to preview changes
# - Test deployments in dev before pushing to prod
# - Monitor costs regularly in Azure Portal
# - Keep Kubernetes and Azure CLI versions up-to-date
# - Document any manual changes made to the cluster

echo "✅ Quick reference loaded. Happy deploying!"
