terraform {
  backend "azurerm" {
    resource_group_name  = "rg-authorization-001-dev"
    storage_account_name = "stauthztfstate001dev"
    container_name       = "github"
    key                  = "terraform.tfstate"
    use_azuread_auth     = true
  }
}

provider "azurerm" {
  features {

  }
}

locals {
  location = "norwayeast"
  files = toset([
    "Altinn.AccessManagement_All.json"
  ])
}

resource "azurerm_resource_group" "rg" {
  name     = "rg-authorization-apim-poc-001-dev"
  location = local.location
}

data "azurerm_api_management" "apim_authorization" {
  name                = "apim-altinn-authorization-poc-001-dev"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  publisher_name      = "APIM deploy PoC"
  publisher_email     = "ext-anils@ai-dev.no"
  sku_name            = "Basic_1"
}

resource "azuread_application_federated_identity_credential" "repo_oidc" {
  application_id = ""
  subject        = ""
  audiences      = ""
  display_name   = ""
  issuer         = ""
}

resource "azurerm_api_management_api" "apim_api" {
  api_management_name = azurerm_api_management.apim_authorization.name
  resource_group_name = azurerm_resource_group.rg.name

  name         = each.key
  display_name = each.value.display_name
  path         = each.value.path
  revision     = each.value.revision
  protocols    = ["https"]

  import {
    content_format = "openapi+json"
    content_value  = file(each.value.file)
  }
  api_type = "http"

  for_each = { for api in var.apis : api.identifier => api }
}
