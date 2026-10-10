# Auto-scaling and performance optimization for TubieTools

variable "enable_cluster_autoscaling" {
  description = "Enable AKS cluster autoscaling"
  type        = bool
  default     = true
}

variable "min_node_count" {
  description = "Minimum number of nodes in auto-scaling node pool"
  type        = number
  default     = 2
}

variable "max_node_count" {
  description = "Maximum number of nodes in auto-scaling node pool"
  type        = number
  default     = 10
}

variable "pod_autoscaling_enabled" {
  description = "Enable Horizontal Pod Autoscaler (HPA)"
  type        = bool
  default     = true
}

variable "enable_vertical_pod_autoscaling" {
  description = "Enable Vertical Pod Autoscaler (VPA)"
  type        = bool
  default     = false
}

# Create additional auto-scaling node pool
resource "azurerm_kubernetes_cluster_node_pool" "autoscaling" {
  name                  = "autoscale"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.aks.id
  vm_size               = "Standard_DS3_v2"

  node_count            = var.aks_node_count

  # Auto-scaling configuration
  enable_auto_scaling = var.enable_cluster_autoscaling
  min_count          = var.enable_cluster_autoscaling ? var.min_node_count : null
  max_count          = var.enable_cluster_autoscaling ? var.max_node_count : null

  # Memory and CPU optimization
  os_disk_size_gb = 256

  # Node labels for workload isolation
  node_labels = {
	Environment = var.environment
	WorkloadType = "Services"
	AutoScaling = "Enabled"
  }

  # Taints for workload isolation (optional)
  node_taints = []

  priority            = "Regular"
  eviction_policy     = "Delete"
  spot_max_price      = null

  tags = local.common_tags
}

# Create GPU node pool for ML workloads (optional)
resource "azurerm_kubernetes_cluster_node_pool" "gpu" {
  name                  = "gpupool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.aks.id
  vm_size               = "Standard_NC6s_v3"

  node_count            = 0  # Start with 0 nodes, scale up as needed

  enable_auto_scaling = true
  min_count          = 0
  max_count          = 3

  node_labels = {
	Environment = var.environment
	WorkloadType = "ML"
	GPU = "Enabled"
  }

  node_taints = [
	"gpu=true:NoSchedule"
  ]

  priority = "Spot"
  spot_max_price = 0.30

  tags = merge(local.common_tags, {
	Purpose = "MachineLearning"
  })
}

# Log aggregation with Prometheus for monitoring
resource "helm_release" "prometheus" {
  name             = "prometheus"
  repository       = "https://prometheus-community.github.io/helm-charts"
  chart            = "kube-prometheus-stack"
  namespace        = "monitoring"
  create_namespace = true
  version          = "51.0.0"

  values = [
	yamlencode({
	  prometheus = {
		prometheusSpec = {
		  retention           = "15d"
		  storageSpec = {
			volumeClaimTemplate = {
			  spec = {
				accessModes = ["ReadWriteOnce"]
				resources = {
				  requests = {
					storage = "50Gi"
				  }
				}
			  }
			}
		  }
		  resources = {
			requests = {
			  cpu    = "500m"
			  memory = "2Gi"
			}
			limits = {
			  cpu    = "2000m"
			  memory = "4Gi"
			}
		  }
		}
	  }
	  grafana = {
		adminPassword = random_password.grafana_password.result
		persistence = {
		  enabled = true
		  size    = "10Gi"
		}
	  }
	})
  ]

  depends_on = [azurerm_kubernetes_cluster.aks]
}

# Generate Grafana password
resource "random_password" "grafana_password" {
  length  = 16
  special = true
}

# Install metrics-server for HPA
resource "helm_release" "metrics_server" {
  name             = "metrics-server"
  repository       = "https://kubernetes-sigs.github.io/metrics-server/"
  chart            = "metrics-server"
  namespace        = "kube-system"
  version          = "3.11.0"

  set {
	name  = "args[0]"
	value = "--kubelet-preferred-address-types=InternalIP"
  }

  depends_on = [azurerm_kubernetes_cluster.aks]
}

# Install KEDA for advanced scaling
resource "helm_release" "keda" {
  name             = "keda"
  repository       = "https://kedacore.github.io/charts"
  chart            = "keda"
  namespace        = "keda"
  create_namespace = true
  version          = "2.13.0"

  depends_on = [azurerm_kubernetes_cluster.aks]
}

# Storage class for persistent volumes
resource "kubernetes_storage_class" "premium" {
  depends_on = [azurerm_kubernetes_cluster.aks]

  metadata {
	name = "premium-storage"
  }

  storage_provisioner = "disk.csi.azure.com"
  reclaim_policy      = "Retain"

  parameters = {
	skuname = "Premium_LRS"
	cachingmode = "ReadOnly"
  }

  allow_volume_expansion = true
}

resource "kubernetes_storage_class" "standard" {
  depends_on = [azurerm_kubernetes_cluster.aks]

  metadata {
	name = "standard-storage"
  }

  storage_provisioner = "disk.csi.azure.com"
  reclaim_policy      = "Retain"

  parameters = {
	skuname = "Standard_LRS"
	cachingmode = "ReadOnly"
  }

  allow_volume_expansion = true
}

# Resource quota for namespace
resource "kubernetes_resource_quota" "tubietools" {
  depends_on = [azurerm_kubernetes_cluster.aks]

  metadata {
	name      = "tubietools-quota"
	namespace = "tubietools"
  }

  spec {
	hard = {
	  "requests.cpu"    = "10"
	  "requests.memory" = "20Gi"
	  "limits.cpu"      = "20"
	  "limits.memory"   = "40Gi"
	  "pods"            = "50"
	  "services"        = "10"
	  "persistentvolumeclaims" = "5"
	}
  }
}

# Network Policy for security
resource "kubernetes_network_policy" "tubietools" {
  depends_on = [azurerm_kubernetes_cluster.aks]

  metadata {
	name      = "tubietools-network-policy"
	namespace = "tubietools"
  }

  spec {
	pod_selector {}

	policy_types = ["Ingress", "Egress"]

	# Allow ingress from ingress controller
	ingress {
	  from {
		namespace_selector {
		  match_labels = {
			name = "ingress-nginx"
		  }
		}
	  }
	  ports {
		port     = "80"
		protocol = "TCP"
	  }
	}

	# Allow inter-pod communication within namespace
	ingress {
	  from {
		pod_selector {}
	  }
	}

	# Allow DNS egress
	egress {
	  to {
		namespace_selector {
		  match_labels = {
			name = "kube-system"
		  }
		}
	  }
	  ports {
		port     = "53"
		protocol = "UDP"
	  }
	}

	# Allow outbound to internet
	egress {
	  to {
		ip_block {
		  cidr = "0.0.0.0/0"
		  except = [
			"169.254.169.254/32"  # Azure metadata service
		  ]
		}
	  }
	}
  }
}

# Pod disruption budget for high availability
resource "kubernetes_pod_disruption_budget" "publicapi" {
  depends_on = [azurerm_kubernetes_cluster.aks]

  metadata {
	name      = "publicapi-pdb"
	namespace = "tubietools"
  }

  spec {
	min_available = 1
	selector {
	  match_labels = {
		app = "publicapi"
	  }
	}
  }
}

# Output auto-scaling information
output "cluster_autoscaling_enabled" {
  description = "Cluster auto-scaling status"
  value       = var.enable_cluster_autoscaling
}

output "node_pool_autoscaling_min" {
  description = "Minimum nodes for autoscaling"
  value       = var.enable_cluster_autoscaling ? var.min_node_count : null
}

output "node_pool_autoscaling_max" {
  description = "Maximum nodes for autoscaling"
  value       = var.enable_cluster_autoscaling ? var.max_node_count : null
}

output "gpu_node_pool_name" {
  description = "GPU node pool name"
  value       = azurerm_kubernetes_cluster_node_pool.gpu.name
}

output "prometheus_password" {
  description = "Grafana admin password"
  value       = random_password.grafana_password.result
  sensitive   = true
}
