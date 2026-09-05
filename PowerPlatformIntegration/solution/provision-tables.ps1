<#
.SYNOPSIS
  Creates the 7 Dataverse tables from ./schema/*.table.json via the Dataverse Web API.

.DESCRIPTION
  Reads every table schema JSON in ./schema and provisions it in the environment
  you are connected to. Idempotent: existing tables/columns are detected and skipped.

  Tables are processed in dependency order so lookups resolve:
    1. CopilotModelConfiguration   (no lookups)
    2. CopilotGovernancePolicy     (no lookups)
    3. CopilotDeploymentConfig     (no lookups)
    4. CopilotApplication          (lookups -> modelconfig, governancepolicy)
    5. KnowledgeTool               (lookup -> application)
    6. CopilotPerformanceMetrics   (lookup -> application)
    7. CopilotVersion              (lookups -> application, self)

.PREREQUISITES
  pac auth create (already done -- an active profile must exist; connection uses -FromPac)
  Rnwood.Dataverse.Data.PowerShell module saved under C:\PSModules
    (Save-Module Rnwood.Dataverse.Data.PowerShell -Path C:\PSModules)

.PARAMETER EnvironmentUrl
  Dataverse org URL. Defaults to https://orgfca80698.crm.dynamics.com

.PARAMETER SchemaDir
  Folder containing *.table.json files. Defaults to ./schema

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File ./provision-tables.ps1
  powershell -ExecutionPolicy Bypass -File ./provision-tables.ps1 -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
  [string]$EnvironmentUrl = "https://orgfca80698.crm.dynamics.com",
  [string]$SchemaDir = "./schema"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# 1. Connect (reuses the existing pac auth profile -- no sign-in prompt)
# ---------------------------------------------------------------------------
$moduleManifest = 'C:\PSModules\Rnwood.Dataverse.Data.PowerShell\3.0.3\Rnwood.Dataverse.Data.PowerShell.psd1'
if (-not (Test-Path $moduleManifest)) {
  throw "Rnwood.Dataverse.Data.PowerShell not found at $moduleManifest. Save it first: Save-Module Rnwood.Dataverse.Data.PowerShell -Path C:\PSModules"
}
Import-Module $moduleManifest -Force

Write-Host "Connecting to $EnvironmentUrl via device code flow..."
Write-Host "(Watch the terminal for a code, then enter it at https://microsoft.com/devicelogin)"
# -DeviceCode is its own parameter set. It avoids the MSAL interactive browser
# window, which fails/hangs in this environment (same reason pac auth create
# needed --deviceCode). -FromPac was tried first but still pops a browser.
$conn = Connect-DataverseConnection -DeviceCode -Url $EnvironmentUrl
if (-not $conn) { throw "Dataverse connection failed." }
Write-Host "Connected.`n"

# ---------------------------------------------------------------------------
# 2. Helpers
# ---------------------------------------------------------------------------

# Default solution publisher prefix for new custom tables.
$PublisherPrefix = "copilot"

function Get-OptionSetOptions($choiceOptions) {
  $meta = New-Object Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata
  $meta.IsGlobal = $false
  $i = 1
  foreach ($opt in $choiceOptions) {
    $o = New-Object Microsoft.Xrm.Sdk.Metadata.OptionMetadata(
      (New-Object Microsoft.Xrm.Sdk.Label($opt, 1033)), ([int](100000 + $i)))
    $meta.Options.Add($o) | Out-Null
    $i++
  }
  return $meta
}

function New-AttributeMetadata($col, $tableLogical, $publisherPrefix) {
  # JSON files already carry the fully-prefixed logical names (e.g. copilot_temperature)
  $schemaName = $col.name

  $label = New-Object Microsoft.Xrm.Sdk.Label($col.displayName, 1033)
  $common = @{
    SchemaName = $schemaName
    DisplayName = $label
    RequiredLevel = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
      [Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel]::None)
  }
  if ($col.required) {
    $common.RequiredLevel = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
      [Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel]::ApplicationRequired)
  }

  switch ($col.type) {
    "Text" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata
      $attr.MaxLength = [int]($(if ($null -ne $col.maxLength) { $col.maxLength } else { 100 }))
      $attr.Format = [Microsoft.Xrm.Sdk.Metadata.StringFormat]::Text
      if ($col.format -eq "Url") { $attr.Format = [Microsoft.Xrm.Sdk.Metadata.StringFormat]::Url }
    }
    "Memo" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.MemoAttributeMetadata
      $attr.MaxLength = [int]($(if ($null -ne $col.maxLength) { $col.maxLength } else { 2000 }))
      $attr.Format = [Microsoft.Xrm.Sdk.Metadata.StringFormat]::TextArea
    }
    "Choice" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata
      $attr.OptionSet = Get-OptionSetOptions $col.options
      $attr.DefaultFormValue = -1
    }
    "Boolean" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata
      $attr.OptionSet = New-Object Microsoft.Xrm.Sdk.Metadata.BooleanOptionSetMetadata(
        (New-Object Microsoft.Xrm.Sdk.Metadata.OptionMetadata(
          (New-Object Microsoft.Xrm.Sdk.Label("Yes", 1033)), 1)),
        (New-Object Microsoft.Xrm.Sdk.Metadata.OptionMetadata(
          (New-Object Microsoft.Xrm.Sdk.Label("No", 1033)), 0)))
      if ($null -ne $col.defaultValue) { $attr.DefaultValue = [bool]$col.defaultValue }
    }
    "WholeNumber" {
      if ($col.format -eq "BigInt") {
        $attr = New-Object Microsoft.Xrm.Sdk.Metadata.BigIntAttributeMetadata
      } else {
        $attr = New-Object Microsoft.Xrm.Sdk.Metadata.IntegerAttributeMetadata
        $attr.Format = [Microsoft.Xrm.Sdk.Metadata.IntegerFormat]::None
        $attr.MinValue = [int]::MinValue; $attr.MaxValue = [int]::MaxValue
      }
    }
    "Decimal" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.DecimalAttributeMetadata
      $attr.Precision = [int]($(if ($null -ne $col.precision) { $col.precision } else { 2 }))
      if ($null -ne $col.minValue) { $attr.MinValue = [decimal]$col.minValue }
      if ($null -ne $col.maxValue) { $attr.MaxValue = [decimal]$col.maxValue }
    }
    "Currency" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata
      $attr.PrecisionSource = 2
    }
    "DateTime" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.DateTimeAttributeMetadata
      $attr.Format = [Microsoft.Xrm.Sdk.Metadata.DateTimeFormat]::DateAndTime
      if ($col.format -eq "DateOnly") { $attr.Format = [Microsoft.Xrm.Sdk.Metadata.DateTimeFormat]::DateOnly }
    }
    "Lookup" {
      $attr = New-Object Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata
      $attr.Targets = @($col.targets)
    }
    default { throw "Unsupported column type '$($col.type)' for $($col.name)" }
  }

  foreach ($k in $common.Keys) { $attr.$k = $common[$k] }
  return $attr
}

function Test-EntityExists($logicalName) {
  $req = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest
  $req.LogicalName = $logicalName
  $req.EntityFilters = [Microsoft.Xrm.Sdk.Metadata.EntityFilters]::Entity
  try { $conn.Execute($req) | Out-Null; return $true } catch { return $false }
}

function Test-AttributeExists($entityLogical, $attrLogical) {
  $req = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest
  $req.EntityLogicalName = $entityLogical
  $req.LogicalName = $attrLogical
  try { $conn.Execute($req) | Out-Null; return $true } catch { return $false }
}

# ---------------------------------------------------------------------------
# 3. Provision each table in dependency order
# ---------------------------------------------------------------------------
$orderedFiles = @(
  "CopilotModelConfiguration.table.json",
  "CopilotGovernancePolicy.table.json",
  "CopilotDeploymentConfig.table.json",
  "CopilotApplication.table.json",
  "KnowledgeTool.table.json",
  "CopilotPerformanceMetrics.table.json",
  "CopilotVersion.table.json"
)

foreach ($file in $orderedFiles) {
  $path = Join-Path $SchemaDir $file
  if (-not (Test-Path $path)) { Write-Warning "Schema file not found: $path -- skipping."; continue }
  $t = Get-Content $path -Raw | ConvertFrom-Json
  $logical = $t.logicalName

  Write-Host "=========================================="
  Write-Host "Table: $($t.displayName)  ($logical)"

  # --- create the entity if missing ---------------------------------------
  if (Test-EntityExists $logical) {
    Write-Host "  exists -- skipping creation."
  } else {
    if ($PSCmdlet.ShouldProcess($logical, "Create table")) {
      $entity = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
      $entity.SchemaName = $logical
      $entity.DisplayName = New-Object Microsoft.Xrm.Sdk.Label($t.displayName, 1033)
      $entity.DisplayCollectionName = New-Object Microsoft.Xrm.Sdk.Label($t.pluralName, 1033)
      $entity.Description = New-Object Microsoft.Xrm.Sdk.Label($t.description, 1033)
      $entity.OwnershipType = [Microsoft.Xrm.Sdk.Metadata.OwnershipTypes]::UserOwned
      $entity.IsActivity = $false
      $entity.HasActivities = $false
      $entity.HasNotes = $false
      $entity.IsAuditEnabled = New-Object Microsoft.Xrm.Sdk.BooleanManagedProperty([bool]$t.auditing)

      # primary name column
      $primaryCol = $t.columns | Where-Object { $_.name -eq $t.primaryColumn }
      $primaryAttr = New-Object Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata
      $primaryAttr.SchemaName = $t.primaryColumn
      $primaryAttr.DisplayName = New-Object Microsoft.Xrm.Sdk.Label($primaryCol.displayName, 1033)
      $primaryAttr.RequiredLevel = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
        [Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel]::ApplicationRequired)
      $primaryAttr.MaxLength = [int]($(if ($null -ne $primaryCol.maxLength) { $primaryCol.maxLength } else { 200 }))
      $primaryAttr.Format = [Microsoft.Xrm.Sdk.Metadata.StringFormat]::Text
      $entity.PrimaryNameAttribute = $t.primaryColumn

      $req = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityRequest
      $req.Entity = $entity
      $req.PrimaryAttribute = $primaryAttr
      $resp = $conn.Execute($req)
      Write-Host "  CREATED  (MetadataId: $($resp.EntityId))"
    }
  }

  # --- create columns ------------------------------------------------------
  foreach ($col in $t.columns) {
    if ($col.name -eq $t.primaryColumn) { continue }  # created with the table
    $attrLogical = $col.name

    if (Test-AttributeExists $logical $attrLogical) {
      Write-Host "  column $attrLogical -- exists, skipped."
      continue
    }

    if ($PSCmdlet.ShouldProcess("$logical.$attrLogical", "Create column ($($col.type))")) {
      $attr = New-AttributeMetadata $col $logical $PublisherPrefix

      if ($col.type -eq "Lookup") {
        $req = New-Object Microsoft.Xrm.Sdk.Messages.CreateOneToManyRelationshipRequest
        $rel = New-Object Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata
        $rel.ReferencedEntity = $col.targets[0]
        $rel.ReferencingEntity = $logical
        $rel.SchemaName = "$($col.targets[0])_$($logical)_$($col.name)"
        $req.OneToManyRelationship = $rel
        $req.Lookup = $attr
        $conn.Execute($req) | Out-Null
      } else {
        $req = New-Object Microsoft.Xrm.Sdk.Messages.CreateAttributeRequest
        $req.EntityName = $logical
        $req.Attribute = $attr
        $conn.Execute($req) | Out-Null
      }
      Write-Host "  + $($col.name)  [$($col.type)]"
    }
  }
}

Write-Host "`n=========================================="
Write-Host "Provisioning complete."
Write-Host "Next: add tables to your solution in Maker Portal, then build the flows in README.md."
