# Decision Point 1: Deployment Automation Agent (Option A - Fast Track)

## Pilot Use Case Charter

---

## Decision

**Selected Pilot:** Deployment Automation Agent
**Rationale:** Fastest ROI, lowest complexity, no user-facing training curve, uses infrastructure your team already touches daily (deployments).

---

## Owner Assignment

| Role | Assignment | Responsibility | Agent Definition |
|------|-----------|-----------------|-------------------|
| Business Owner | @DevOps Lead | Approves scope, prioritizes deployment scenarios | [devops-lead-deployment-agent.yaml](agents/devops-lead-deployment-agent.yaml) |
| Technical Owner | @.NET Integration Developer | Builds dual-write + connector code | [integration-developer-sync-agent.yaml](agents/integration-developer-sync-agent.yaml) |
| Platform Owner | @Power Apps/Flow Specialist | Builds flows, agent logic in Power Automate/Copilot Studio | [flow-specialist-automation-agent.yaml](agents/flow-specialist-automation-agent.yaml) |
| Sponsor | @Executive Champion | Removes blockers, reviews Go/No-Go | [executive-sponsor-gonogo-agent.yaml](agents/executive-sponsor-gonogo-agent.yaml) |

Each agent is a Copilot Studio manifest scoped to its owner's responsibilities, backed by
Power Automate flows against the `CopilotDeploymentConfig` Dataverse table.

---

## Scope

### In Scope
- Automated deployment status notifications (Teams + Email)
- Health check polling after deployment completes
- Auto-rollback trigger on failed health check (configurable threshold)
- Deployment audit trail written to Dataverse (`CopilotDeploymentConfig` table)
- Dashboard tile showing last 10 deployments + status

### Out of Scope (deferred to later phases)
- Interactive copilot / chat-based deployment requests
- Cost optimization recommendations
- Multi-environment approval workflows (dev → staging → prod gating)
- Predictive failure analysis

---

## Architecture

```
CI/CD Pipeline (GitHub Actions / Azure DevOps)
			  ↓
	  DTOLayer Deployment Event
			  ↓
	  DualWriteService (SQL Server ↔ Dataverse)
			  ↓
	  CopilotDeploymentConfig (Dataverse table)
			  ↓
	  Power Automate Flow (trigger: row created/updated)
	  ├─ Teams notification
	  ├─ Email notification
	  ├─ Health check call (API)
	  └─ Conditional: Auto-rollback flow
			  ↓
	  Power BI tile (deployment history)
```

---

## Timeline (4 Weeks)

| Week | Milestone |
|------|-----------|
| 1 | Dataverse `CopilotDeploymentConfig` table live; deployment events wired to `DualWriteService` |
| 2 | Power Automate flow built (notification + health check) |
| 3 | Auto-rollback logic tested against a staged failure scenario |
| 4 | Dashboard tile live; demo to stakeholders; Go/No-Go review |

---

## Success Criteria / Go-No-Go Checklist

- [ ] Deployment events sync to Dataverse with < 5s latency
- [ ] Teams + Email notifications fire on every deployment (success and failure)
- [ ] Auto-rollback triggers correctly in at least 1 staged failure test
- [ ] Zero manual notification steps required from DevOps team during pilot period
- [ ] Stakeholder demo approved

---

## Estimated Value

- **Time saved:** ~2 FTE hours/day (manual status checks + notifications eliminated)
- **Annualized value:** ~$150K/year
- **Payback:** Within pilot timeline (4 weeks to first working flow)

---

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| False-positive rollback triggers | Configurable health-check threshold + manual override switch |
| Dataverse sync failure blocks deployment | Dual-write is fire-and-forget with retry queue; never blocks the pipeline |
| Notification fatigue | Route failures to a dedicated Teams channel, successes to a low-priority feed |

---

**Next Action:** Kickoff meeting with @DevOps Lead to confirm Week 1 scope and Dataverse schema fields for `CopilotDeploymentConfig`.
