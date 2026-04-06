locals {
  base_name = lower(replace("${var.project_name}-${var.environment}", "_", "-"))

  tags = merge(
    {
      "project"     = var.project_name
      "environment" = var.environment
      "managed-by"  = "terraform"
    },
    var.tags
  )
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.base_name}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_container_registry" "main" {
  name                = substr(replace("acr${var.project_name}${var.environment}", "-", ""), 0, 50)
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = var.acr_sku
  admin_enabled       = false
  tags                = local.tags
}

resource "random_string" "postgres_suffix" {
  length  = 5
  special = false
  upper   = false
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                   = substr("psql-${local.base_name}-${random_string.postgres_suffix.result}", 0, 63)
  resource_group_name    = azurerm_resource_group.main.name
  location               = var.postgres_location
  version                = var.postgres_version
  delegated_subnet_id    = null
  private_dns_zone_id    = null
  administrator_login    = var.postgres_admin_username
  administrator_password = var.postgres_admin_password
  sku_name               = var.postgres_sku_name
  storage_mb             = var.postgres_storage_mb
  zone                   = var.postgres_zone
  backup_retention_days  = 7

  public_network_access_enabled = true

  tags = local.tags
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "allow-azure-services"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.postgres_db_name
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}

resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-${local.base_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "aks-${local.base_name}"
  kubernetes_version  = var.kubernetes_version
  tags                = local.tags

  sku_tier = "Free"

  default_node_pool {
    name                        = "system"
    node_count                  = var.node_count
    vm_size                     = var.node_vm_size
    os_sku                      = "Ubuntu"
    temporary_name_for_rotation = "systemtmp"
    upgrade_settings {
      max_surge = "33%"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "kubenet"
    load_balancer_sku = "standard"
  }
}

resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  depends_on           = [azurerm_kubernetes_cluster.main, azurerm_container_registry.main]
}
