<#
.SYNOPSIS
  Scaffolds and imports the Deployment Automation Pilot solution into the
  currently authenticated Power Platform environment (pac auth create first).

.NOTES
  You must run `pac auth create --environment <url>` yourself before this script.
  No credentials are read from or written to this script.
#>
param(
  [string]$PublisherName = "TubieTools",
  [string]$PublisherPrefix = "copilot",
  [string]$SolutionName = "DeploymentAutomationPilot"
)

$ErrorActionPreference = "Stop"

Write-Host "Verifying pac CLI auth context..."
pac org who

# pac solution init creates the publisher inline; no separate publisher command exists in this CLI version.
$solutionDir = "./_generated/$SolutionName"
if (Test-Path $solutionDir) {
  Write-Host "Solution '$SolutionName' already scaffolded at $solutionDir, skipping init."
} else {
  Write-Host "Initializing solution '$SolutionName'..."
  pac solution init --publisher-name $PublisherName --publisher-prefix $PublisherPrefix --outputDirectory $solutionDir
}

Write-Host "Copying table schema and app definition into solution scaffold..."
Copy-Item ./schema/CopilotDeploymentConfig.table.json ./_generated/$SolutionName/ -Force
Copy-Item ./app/DeploymentAutomationApp.app.json ./_generated/$SolutionName/ -Force

Write-Host "NEXT STEPS (manual, Maker Portal):"
Write-Host "  1. Open the Maker Portal for your environment"
Write-Host "  2. Create table 'Copilot Deployment Config' using ./schema/CopilotDeploymentConfig.table.json as reference"
Write-Host "  3. Build the Power Automate flows listed in README.md"
Write-Host "  4. Import each Copilot Studio agent from ../agents/*.yaml"
Write-Host "  5. Build the model-driven app per ./app/DeploymentAutomationApp.app.json"
Write-Host "  6. pac solution pack / pac solution import when ready to move across environments"
