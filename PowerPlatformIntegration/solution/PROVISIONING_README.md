# Dataverse Table Provisioning — Data Model, Schemas & Automation

Everything in this folder turns the 7-entity .NET data model (`ModelLayer/Models`) into real
Dataverse tables. Three artifacts work together:

| Artifact | What it is |
|---|---|
| `../IMPLEMENTATION_HANDBOOK.md` §1 | Human-readable data model for all 7 tables (columns, choices, relationships) |
| `schema/*.table.json` | Machine-readable JSON schema per table (consumed by `deploy.ps1` and `provision-tables.ps1`) |
| `provision-tables.ps1` | Automated Option 3: creates tables/columns/lookups via Dataverse Web API |

## The 7 tables (dependency order)

```
1. copilot_modelconfiguration   no lookups
2. copilot_governancepolicy     no lookups
3. copilot_deploymentconfig     no lookups (pilot audit table — ships with 10 columns)
4. copilot_copilotapplication   lookups → 1, 2 (+ systemuser Owner)
5. copilot_knowledgetool        lookup → 4 (required)
6. copilot_performancemetrics   lookup → 4 (required)
7. copilot_version              lookups → 4 (required), self (RollbackPath)
```

Relationship map:

```
CopilotGovernancePolicy   (1) ──→ (N) CopilotApplication
CopilotModelConfiguration (1) ──→ (N) CopilotApplication
CopilotApplication        (1) ──→ (N) KnowledgeTool / CopilotVersion /
                                    CopilotPerformanceMetrics / CopilotDeploymentConfig
CopilotVersion            (1) ──→ (N) CopilotVersion  (self, RollbackPath)
```

## Schema JSON conventions

Same convention as the original `CopilotDeploymentConfig.table.json`, extended:

- `logicalName` carries the `copilot_` publisher prefix
- Complex .NET objects (nested config classes) → `"type": "Memo"` JSON columns
- Extra per-column keys: `targets` (Lookup), `required`, `searchable`,
  `precision`/`minValue`/`maxValue` (Decimal), `format: "DateOnly" | "BigInt" | "Url"`,
  `defaultValue`
- Dataverse auto-adds `createdby/on`, `modifiedby/on`, `ownerid`, `statecode`,
  `statuscode`, `versionnumber` — never in the JSON

## Three ways to create the tables

### Option 1 — Manual (Maker Portal)

make.powerapps.com → environment → Tables → New table, following
`../IMPLEMENTATION_HANDBOOK.md` §1 column-by-column. ~2–3 hours total. Always works,
no tooling risk. Create in the dependency order above so lookups resolve.

### Option 2 — pac solution pack/import

`deploy.ps1` copies `schema/*.table.json` + `app/*.app.json` into
`_generated/DeploymentAutomationPilot`. Note `pac solution pack` only packs real
solution XML — raw table JSON is **reference material**, not importable source.
Use Option 2 to *promote* tables between environments after they exist, not for
first-time creation.

### Option 3 — Automated (provision-tables.ps1)

Creates all 7 tables via the Dataverse Web API, idempotently (existing
tables/columns are detected and skipped).

```powershell
# Prereq: pac auth profile must exist (pac auth list)
# Prereq: Rnwood.Dataverse.Data.PowerShell saved under C:\PSModules (see below)
powershell -ExecutionPolicy Bypass -File ./provision-tables.ps1          # live run
powershell -ExecutionPolicy Bypass -File ./provision-tables.ps1 -WhatIf  # dry run
```

The script connects with `Connect-DataverseConnection -FromPac`, reusing your
existing pac CLI auth profile — **no sign-in prompt, no credentials in the file**.

## Environment setup log (what the "malarkey" was, and why)

Provisioning on this box required working through four environmental issues —
documented here so the next run is boring:

1. **`pac auth create` hangs** — the interactive WAM broker window opens behind
   other windows (or fails silently in the VS Code integrated terminal). An active
   profile already existed (`pac auth list`), so re-auth was unnecessary. If you
   ever must re-auth: `pac auth create --deviceCode` avoids the broker window.
2. **Execution policy** — local scripts are unsigned; run with
   `powershell -ExecutionPolicy Bypass -File ...`. The persistent terminal reuses
   its cwd, so watch for doubled paths like `solution\PowerPlatformIntegration\solution`.
3. **`Microsoft.PowerPlatform.Dataverse.Client` is NOT a PowerShell module** — it's
   the .NET NuGet assembly. The PSGallery equivalent is
   `Rnwood.Dataverse.Data.PowerShell` (v3.0.3, wraps the same SDK, supports `-FromPac`).
4. **PSGallery installs were broken** by the stock `PowerShellGet 1.0.0.1` + a
   OneDrive-redirected Documents folder (`...Could not find a part of the path
   '...\OneDrive\Documents\WindowsPowerShell\Modules\PackageManagement\1.4.8.1'`).
   Fix: bootstrap NuGet provider + Save-Module to a non-redirected path:

   ```powershell
   [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
   Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force -Scope CurrentUser
   Save-Module PowerShellGet -Path C:\PSModules -Force
   Import-Module C:\PSModules\PackageManagement -Force
   Import-Module C:\PSModules\PowerShellGet -Force
   Save-Module Rnwood.Dataverse.Data.PowerShell -Path C:\PSModules -Force
   ```

   `provision-tables.ps1` imports the module by explicit manifest path
   (`C:\PSModules\Rnwood.Dataverse.Data.PowerShell\3.0.3\Rnwood.Dataverse.Data.PowerShell.psd1`)
   to dodge PS 5.1's folder-resolution quirks with the module's multi-TFM loader.

5. **PS 5.1 compatibility** — the script must not use `??` (null-coalescing, PS 7+);
   use `$null -eq` checks. Syntax-check with:

   ```powershell
   $e=$null; [System.Management.Automation.Language.Parser]::ParseFile('.\provision-tables.ps1',[ref]$null,[ref]$e)|Out-Null; $e
   ```

## Known remaining quirk (as of last session)

1. `Connect-DataverseConnection` loads as an **alias** whose target reports 0
   parameter sets to `Get-Command` under PS 5.1 — resolved: alias target is
   `Get-DataverseConnection`; `-FromPac` is its own parameter set (no `-Url`).
2. `-FromPac` still triggers an **MSAL interactive browser pop-up** (it reuses
   the pac profile, not the pac token cache) → `User canceled authentication`.
3. Switched to `-DeviceCode -Url <org>` flow. First code expired unused; second
   attempt hit `AADSTS900561` (endpoint only accepts POST, got GET) — a browser-
   side redirect issue; workaround is InPrivate window + `https://microsoft.com/devicelogin`.
4. **Repeated auth attempts locked the account (Entra smart lockout).**
   ⏸ STOPPED HERE. Before retrying: confirm the account is unlocked (portal →
   Users → account → Unblock sign-in, or wait out the lockout), then make ONE
   device-code attempt with the code entered promptly in InPrivate.

## Status checklist

- [x] 7 JSON schemas written and validated (`schema/*.table.json`)
- [x] `deploy.ps1` copies all schemas into solution scaffold
- [x] `provision-tables.ps1` written, PS 5.1 syntax-clean
- [x] Dataverse module toolchain repaired (C:\PSModules)
- [x] `Connect-DataverseConnection` parameter-set fix (alias → `Get-DataverseConnection`; `-FromPac` has no `-Url`)
- [ ] Auth: account locked by repeated attempts — UNLOCK FIRST, then single `-DeviceCode` run, code entered promptly in InPrivate window via https://microsoft.com/devicelogin
- [ ] First successful `-WhatIf` dry run
- [ ] Live provisioning run (7 tables created)
- [ ] Manual: flows (README), agent imports (`../agents/*.yaml`), model-driven app
