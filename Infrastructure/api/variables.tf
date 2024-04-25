variable "apis" {
  type = set(object({
    identifier   = string
    path         = string
    display_name = string
    file         = string
    revision     = string
  }))
  description = "Different APIs"

  default = [
    {
      identifier   = "accessmanagement_internal"
      path         = "v1/accessmanagement/internal"
      display_name = "Access Management: Internal"
      file         = "Altinn.AccessManagement_Internal.json"
      revision     = "v1"
    },
    {
      identifier   = "accessmanagement_enduser"
      path         = "v1/accessmanagement/enduser"
      display_name = "Access Management: Enduser"
      file         = "Altinn.AccessManagement_EnduserSystem.json"
      revision     = "v1"
  }]
}
