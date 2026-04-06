variable "project_name" {
  description = "Project base name used for Azure resources"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "eastus"
}

variable "postgres_location" {
  description = "Azure region for PostgreSQL Flexible Server"
  type        = string
  default     = "westus"
}

variable "postgres_zone" {
  description = "Availability zone for PostgreSQL Flexible Server (set null to let Azure choose)"
  type        = string
  default     = null
}

variable "kubernetes_version" {
  description = "AKS Kubernetes version"
  type        = string
  default     = null
}

variable "node_count" {
  description = "AKS node count"
  type        = number
  default     = 1
}

variable "node_vm_size" {
  description = "AKS node size"
  type        = string
  default     = "Standard_B2s"
}

variable "acr_sku" {
  description = "Azure Container Registry SKU"
  type        = string
  default     = "Basic"
}

variable "tags" {
  description = "Common tags"
  type        = map(string)
  default     = {}
}

variable "postgres_admin_username" {
  description = "Admin username for PostgreSQL Flexible Server"
  type        = string
  default     = "sgadmin"
}

variable "postgres_admin_password" {
  description = "Admin password for PostgreSQL Flexible Server"
  type        = string
  sensitive   = true
}

variable "postgres_db_name" {
  description = "Application PostgreSQL database name"
  type        = string
  default     = "sharedgroceries"
}

variable "postgres_version" {
  description = "PostgreSQL major version for Flexible Server"
  type        = string
  default     = "15"
}

variable "postgres_sku_name" {
  description = "SKU for PostgreSQL Flexible Server"
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "Storage size in MB for PostgreSQL Flexible Server"
  type        = number
  default     = 32768
}
