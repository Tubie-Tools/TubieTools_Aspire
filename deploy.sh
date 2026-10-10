#!/bin/bash
# Comprehensive deployment script for TubieTools to Azure with Terraform and Kubernetes
# This script handles infrastructure setup, Docker builds, and Kubernetes deployments

set -e  # Exit on error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
	echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
	echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
	echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
	echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
	command -v "$1" >/dev/null 2>&1
}

# Verify prerequisites
verify_prerequisites() {
	log_info "Verifying prerequisites..."

	local missing_tools=()

	if ! command_exists terraform; then
		missing_tools+=("terraform")
	fi

	if ! command_exists az; then
		missing_tools+=("az (Azure CLI)")
	fi

	if ! command_exists docker; then
		missing_tools+=("docker")
	fi

	if ! command_exists kubectl; then
		missing_tools+=("kubectl")
	fi

	if ! command_exists helm; then
		missing_tools+=("helm")
	fi

	if [ ${#missing_tools[@]} -gt 0 ]; then
		log_error "Missing required tools: ${missing_tools[*]}"
		log_info "Please install the missing tools and try again."
		exit 1
	fi

	log_success "All prerequisites verified!"
}

# Azure login
azure_login() {
	log_info "Logging into Azure..."
	az login

	# Get subscription ID
	SUBSCRIPTION_ID=$(az account show --query id -o tsv)
	log_success "Logged in with subscription: $SUBSCRIPTION_ID"
}

# Initialize Terraform
terraform_init() {
	log_info "Initializing Terraform..."
	cd terraform
	terraform init
	cd ..
	log_success "Terraform initialized!"
}

# Validate Terraform
terraform_validate() {
	log_info "Validating Terraform configuration..."
	cd terraform
	terraform validate
	cd ..
	log_success "Terraform configuration is valid!"
}

# Plan Terraform deployment
terraform_plan() {
	log_info "Planning Terraform deployment..."
	cd terraform
	terraform plan -out=tfplan
	cd ..
	log_info "Review the plan above and press Enter to continue..."
	read
}

# Apply Terraform
terraform_apply() {
	log_info "Applying Terraform configuration..."
	cd terraform
	terraform apply tfplan

	# Export outputs
	log_info "Extracting Terraform outputs..."

	RESOURCE_GROUP=$(terraform output -raw resource_group_name)
	ACR_LOGIN_SERVER=$(terraform output -raw acr_login_server)
	ACR_USERNAME=$(terraform output -raw acr_admin_username)
	ACR_PASSWORD=$(terraform output -raw acr_admin_password)
	AKS_CLUSTER_NAME=$(terraform output -raw aks_cluster_name)
	SQL_SERVER_FQDN=$(terraform output -raw sql_server_fqdn)
	SQL_DB_NAME=$(terraform output -raw sql_database_name)

	cd ..

	log_success "Terraform deployment completed!"
}

# Docker login to ACR
docker_login_acr() {
	log_info "Logging into Azure Container Registry..."
	docker login -u "$ACR_USERNAME" -p "$ACR_PASSWORD" "$ACR_LOGIN_SERVER"
	log_success "Docker logged into ACR!"
}

# Build and push Docker images
build_and_push_images() {
	log_info "Building and pushing Docker images..."

	local services=("PublicAPI" "WebFrontend" "SentimentAPI" "ForecastingAPI")
	local dockerfile_mapping=(
		"PublicAPI:Dockerfiles/PublicAPI.Dockerfile:TubieTools_PublicAPI"
		"WebFrontend:Dockerfiles/WebFrontend.Dockerfile:TubieTools_Aspire.Web"
		"SentimentAPI:Dockerfiles/SentimentAPI.Dockerfile:TubieTools_SentimentModel_WebApi"
		"ForecastingAPI:Dockerfiles/ForecastingAPI.Dockerfile:TubieTools_Forecasting_API"
	)

	for mapping in "${dockerfile_mapping[@]}"; do
		IFS=':' read -r service_name dockerfile_path service_folder <<< "$mapping"

		log_info "Building $service_name..."

		# Convert to lowercase for image name
		local image_name=$(echo "$service_name" | tr '[:upper:]' '[:lower:]')
		local full_image_name="$ACR_LOGIN_SERVER/$image_name:latest"

		docker build -f "$dockerfile_path" -t "$full_image_name" .

		log_info "Pushing $service_name to ACR..."
		docker push "$full_image_name"

		log_success "$service_name pushed successfully!"
	done

	log_success "All Docker images built and pushed!"
}

# Get AKS credentials
get_aks_credentials() {
	log_info "Getting AKS credentials..."
	az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$AKS_CLUSTER_NAME" --overwrite-existing
	log_success "AKS credentials configured!"
}

# Create namespace and secrets
setup_k8s_namespace() {
	log_info "Setting up Kubernetes namespace and secrets..."

	kubectl create namespace tubietools --dry-run=client -o yaml | kubectl apply -f -

	# Create ACR secret
	kubectl create secret docker-registry acr-credentials \
		--docker-server="$ACR_LOGIN_SERVER" \
		--docker-username="$ACR_USERNAME" \
		--docker-password="$ACR_PASSWORD" \
		--docker-email="admin@tubietools.com" \
		-n tubietools \
		--dry-run=client -o yaml | kubectl apply -f -

	log_success "Kubernetes namespace and secrets created!"
}

# Deploy applications
deploy_applications() {
	log_info "Deploying applications to AKS..."

	# Update image placeholders in manifests
	local temp_manifest=$(mktemp)
	sed "s|<ACR_LOGIN_SERVER>|$ACR_LOGIN_SERVER|g" kubernetes/advanced-deployments.yaml > "$temp_manifest"
	sed -i "s|<IMAGE_TAG>|latest|g" "$temp_manifest"

	kubectl apply -f "$temp_manifest"
	rm "$temp_manifest"

	log_success "Applications deployed!"
}

# Wait for deployments
wait_for_deployments() {
	log_info "Waiting for deployments to be ready..."

	kubectl rollout status deployment/publicapi -n tubietools --timeout=5m
	kubectl rollout status deployment/webfrontend -n tubietools --timeout=5m

	log_success "All deployments are ready!"
}

# Display deployment info
display_deployment_info() {
	log_info "Deployment Information:"
	echo ""
	echo "=========================="
	echo "Azure Resource Group: $RESOURCE_GROUP"
	echo "ACR Login Server: $ACR_LOGIN_SERVER"
	echo "AKS Cluster: $AKS_CLUSTER_NAME"
	echo "SQL Server: $SQL_SERVER_FQDN"
	echo "SQL Database: $SQL_DB_NAME"
	echo "=========================="
	echo ""

	log_info "Kubernetes Services:"
	kubectl get svc -n tubietools

	echo ""
	log_info "Deployed Pods:"
	kubectl get pods -n tubietools

	echo ""
	log_info "Pod Resources:"
	kubectl top pods -n tubietools || log_warning "Metrics not yet available"
}

# Cleanup function
cleanup() {
	log_warning "Initiating cleanup..."

	read -p "Are you sure you want to destroy all resources? (yes/no): " confirm
	if [ "$confirm" = "yes" ]; then
		cd terraform
		terraform destroy
		cd ..
		log_success "Resources destroyed!"
	else
		log_info "Cleanup cancelled."
	fi
}

# Main deployment flow
main() {
	log_info "=========================================="
	log_info "TubieTools Azure Deployment Script"
	log_info "=========================================="

	# Check command line arguments
	if [ "$1" = "cleanup" ]; then
		cleanup
		exit 0
	fi

	verify_prerequisites

	# Check if terraform.tfvars exists
	if [ ! -f terraform/terraform.tfvars ]; then
		log_error "terraform/terraform.tfvars not found!"
		log_info "Please copy terraform/terraform.tfvars.example to terraform/terraform.tfvars and configure it."
		exit 1
	fi

	azure_login
	terraform_init
	terraform_validate
	terraform_plan
	terraform_apply

	docker_login_acr
	build_and_push_images

	get_aks_credentials
	setup_k8s_namespace
	deploy_applications
	wait_for_deployments

	display_deployment_info

	log_success "=========================================="
	log_success "Deployment completed successfully!"
	log_success "=========================================="
}

# Run main function
if [ "$1" = "" ]; then
	main
else
	main "$@"
fi
