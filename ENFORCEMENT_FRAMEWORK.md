# ENFORCEMENT FRAMEWORK FOR CODE GENERATION STANDARDS

**Purpose**: Establish binding rules for all future AI-generated code  
**Authority**: Project governance document  
**Scope**: All code generation requests by any team member  

---

## 📋 HOW TO CREATE AN ENFORCEMENT STANDARD

Every standard has this structure:

### 1. Rule Name & Scope
```
RULE: [Name]
APPLIES TO: [What code this affects]
EFFECTIVE DATE: [When this starts]
OWNER: [Who validates this]
```

### 2. Requirements (MUST, NOT MUST)
```
MUST:
- Specific, testable requirement
- Specific, testable requirement

MUST NOT:
- Prohibited behavior
- Prohibited behavior
```

### 3. Verification Method
```
HOW VERIFIED:
- Concrete command to run
- Expected output format
- Success criteria (binary: pass/fail)
```

### 4. Enforcement Action
```
IF VIOLATED:
- What happens
- Who is notified
- What gets blocked
```

### 5. Documentation
```
WHERE STORED:
- Link to rule document
- Required before code generation
- Developer acknowledgment required
```

---

## ✅ EXAMPLE: NUGET VERIFICATION STANDARD (Already Created)

```
RULE: NuGet Package Version Matching
APPLIES TO: Any code generation with external package dependencies
EFFECTIVE DATE: [Today]
OWNER: Lead developer (final dotnet restore approval)

MUST:
- Read all referenced .csproj files for version requirements
- Propose versions matching or exceeding all transitive dependencies
- Document why each version was chosen
- Require user to run 'dotnet restore' before accepting

MUST NOT:
- Guess at package versions
- Claim "it should work" without verification
- Continue if restore fails > 3 times
- Generate code before restore succeeds

HOW VERIFIED:
Command: dotnet restore [project].csproj
Success: "Restore completed successfully" with zero errors
Failure: Any NU#### error code = rule violated

IF VIOLATED:
- Fix specific package version (user provides error)
- Re-run restore
- If > 3 iterations: escalate to human review

WHERE STORED:
- /NUGET_VERIFICATION_STANDARD.md
- Referenced at start of every code generation with packages
```

---

## 📝 NOW: MSTest ENFORCEMENT STANDARD

