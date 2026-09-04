# Deployment Automation Pilot — Power Platform Solution Scaffold

This folder contains an **unmanaged solution scaffold** for the pilot described in
[../DECISION_POINT_1_DEPLOYMENT_AGENT.md](../DECISION_POINT_1_DEPLOYMENT_AGENT.md).
It is source you pack/import yourself with `pac` CLI — nothing here is deployed
automatically, and no credentials from this session are used or stored.

## Why you run this, not the assistant
Provisioning Power Platform environments/apps requires an authenticated `pac auth`
profile tied to your tenant. That authentication must happen in your own terminal
session — pasting session/tenant/object IDs into chat does not grant an agent the
ability to act on your tenant, and shouldn't be relied on as a credential.

## Contents

| Path | Purpose |
|------|---------|
| `schema/CopilotDeploymentConfig.table.json` | Dataverse table schema backing all 4 agents |
| `app/DeploymentAutomationApp.app.json` | Model-driven app definition (table + 4 embedded Copilot Studio agents) |
| `deploy.ps1` | `pac` CLI script: auth, solution init, table + app import |

## Steps (run these yourself)

```powershell
# 1. Authenticate to your tenant (interactive browser sign-in)
pac auth create --environment https://<your-env>.crm.dynamics.com

# 2. Verify the right environment is selected
pac org who

# 3. Run the scaffold script from this folder
cd PowerPlatformIntegration/solution
./deploy.ps1

# 4. Import the 4 Copilot Studio agents (Maker Portal → Copilot Studio → Import)
#    using the manifests in ../agents/*.yaml
```

## Creating the table in Maker Portal (make.powerapps.com)

1. Select environment "Nicholas Kinney's Environment" → Tables → **New table**
2. Display name: `Copilot Deployment Config` (plural: `Copilot Deployment Configs`), enable **Auditing**
3. Primary column `Deployment Name` is created automatically — set type **Single line of text**, length 200
4. Add these columns (Data type → Name → extra settings):

   | Display Name | Data type | Settings |
   |---|---|---|
   | Environment | Choice | Options: `Dev`, `Staging`, `Prod` |
   | Status | Choice | Options: `Pending`, `Succeeded`, `Failed`, `RolledBack` |
   | Timestamp | Date and time | Date and Time behavior: User Local |
   | Rollback Triggered | Yes/No | Default: No |
   | Rollback Reason | Multiple lines of text | Max length 2000 |
   | Sync Latency (ms) | Whole Number | Format: None |
   | Retry Queue Depth | Whole Number | Format: None |
   | Last Sync Error | Multiple lines of text | Max length 2000 |
   | Health Check Response Code | Single line of text | Length 20 |

5. Save the table, then add it to the `DeploymentAutomationPilot` solution (Solutions → open solution → Add existing → Table)

Reference: [CopilotDeploymentConfig.table.json](schema/CopilotDeploymentConfig.table.json) has the same schema in JSON form.

## Table → Agent mapping

`CopilotDeploymentConfig` is the single Dataverse table all four agent manifests
in [../agents](../agents) query or write to (see each YAML's `dataSource` /
`flow` fields). Create it first, then the Power Automate flows referenced by
each agent, then import the agent manifests.

## Flows referenced by the agents (create in Power Automate, table above as trigger/data source)

- `GetRecentDeployments`, `GetRollbackHistory`, `TriageDeploymentQueue` — DevOps Lead agent
- `CheckDualWriteHealth`, `GetRetryQueueItems`, `ReplayRetryQueue` — Integration Developer agent
- `PostDeployHealthCheck`, `TriggerRollback`, `UpdateRollbackThreshold`, `SendDeploymentNotification` — Flow Specialist agent
- `GetPilotMilestoneStatus`, `EvaluateSuccessCriteria`, `GetOpenBlockers` — Executive Sponsor agent
