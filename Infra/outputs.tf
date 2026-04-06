output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  value = azurerm_kubernetes_cluster.main.name
}

output "acr_name" {
  value = azurerm_container_registry.main.name
}

output "acr_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "postgres_server_fqdn" {
  value = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgres_database_name" {
  value = azurerm_postgresql_flexible_server_database.main.name
}

output "db_connection_string" {
  value     = format("Host=%s;Database=%s;Username=%s;Password=%s;SSL Mode=Require;Trust Server Certificate=true", azurerm_postgresql_flexible_server.main.fqdn, azurerm_postgresql_flexible_server_database.main.name, azurerm_postgresql_flexible_server.main.administrator_login, var.postgres_admin_password)
  sensitive = true
}
