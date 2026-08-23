# HOW TO CREATE ADDITIONAL ENFORCEMENT STANDARDS

**Use this template** for any new rule you want to establish across all future code generation.

---

## 📋 STANDARD TEMPLATE

Copy this structure for each new standard:

```markdown
# [STANDARD NAME] ENFORCEMENT STANDARD

**Effective**: [Date]
**Authority**: [Who decided this]
**Verified By**: [How it's tested]

---

## 📋 RULE DEFINITION

RULE: [What must happen]
APPLIES TO: [What code/projects/scenarios]
EFFECTIVE DATE: [When it starts]
OWNER: [Who validates]
BINDING: Yes / No

---

## ✅ REQUIREMENTS (MUST)

- [ ] MUST [specific requirement]
- [ ] MUST [specific requirement]
- [ ] MUST NOT [prohibited behavior]
- [ ] MUST NOT [prohibited behavior]

---

## 🔍 VERIFICATION METHOD

### Check 1: [Description]
Command: [exact command to run]
Expected: [exact expected output]
Success: [binary: pass/fail criteria]

### Check 2: [Description]
Command: [exact command to run]
Expected: [exact expected output]
Success: [binary: pass/fail criteria]

---

## ✅ SUCCESS CRITERIA

All of:
1. ✅ [Criterion 1]
2. ✅ [Criterion 2]
3. ✅ [Criterion 3]

---

## 🚨 IF VIOLATED

Level 1 (Generation): [What happens]
Level 2 (Review): [What happens]
Level 3 (Merge): [What happens]

---

## 📞 ENFORCEMENT

Who enforces: [Role/person]
Triggers re-review: [Conditions]

---

## ✍️ ACKNOWLEDGMENT

- [ ] Project Manager: Approves standard
- [ ] Lead Developer: Will enforce
- [ ] AI: Will follow this standard
```

---

## 🎯 EXAMPLES OF STANDARDS YOU COULD ENFORCE

### [Example 1] Nullable Reference Types
```
RULE: All code MUST use #nullable enable
APPLIES TO: All new C# files
VERIFICATION: grep "#nullable enable" *.cs
```

### [Example 2] Code Coverage Minimum
```
RULE: All code changes MUST have >= 80% test coverage
APPLIES TO: Any PR with code changes
VERIFICATION: dotnet test /p:CollectCoverage=true
```

### [Example 3] Async/Await Convention
```
RULE: All I/O operations MUST be async
APPLIES TO: Repositories, Services, Controllers
VERIFICATION: No Task.Result, .Wait(), or .GetAwaiter().GetResult()
```

### [Example 4] Logging Requirements
```
RULE: All services MUST log entry/exit of public methods
APPLIES TO: Service layer code
VERIFICATION: grep "LogInformation.*method name" *.cs
```

### [Example 5] Documentation Comments
```
RULE: All public methods MUST have XML doc comments
APPLIES TO: All public APIs
VERIFICATION: dotnet build with /p:TreatWarningsAsErrors=true
```

### [Example 6] Dependency Injection Only
```
RULE: Dependencies MUST be injected, never instantiated
APPLIES TO: Services, Controllers, Repositories
VERIFICATION: grep -r "new [ServiceClass]" (should be 0)
```

### [Example 7] Connection String Security
```
RULE: Connection strings MUST come from config, never hardcoded
APPLIES TO: Database configuration
VERIFICATION: grep "DefaultConnection" appsettings.json exists
```

---

## 📝 CURRENT STANDARDS IN THIS PROJECT

✅ **NUGET_VERIFICATION_STANDARD.md** - Package version matching  
✅ **MSTEST_ENFORCEMENT_STANDARD.md** - Test framework requirement  

---

## 🚀 TO ADD A NEW STANDARD

### Step 1: Define It
Create `[STANDARD_NAME]_ENFORCEMENT_STANDARD.md` using the template above

### Step 2: Document It
Add it to this file under "CURRENT STANDARDS"

### Step 3: Communicate It
Add to project onboarding documentation

### Step 4: Verify It
Add verification script / checklist to build pipeline

### Step 5: Enforce It
Add code review checklist that checks for violations

---

## 🔧 VERIFICATION SCRIPT TEMPLATE

Create `verify-standard.sh` for automated checking:

```bash
#!/bin/bash

STANDARD_NAME="[What you're checking]"
PASS=0
FAIL=0

echo "Verifying: $STANDARD_NAME"
echo "================================"

# Check 1
if [condition]; then
	echo "✓ Check 1 passed"
	((PASS++))
else
	echo "✗ Check 1 FAILED"
	((FAIL++))
fi

# Check 2
if [condition]; then
	echo "✓ Check 2 passed"
	((PASS++))
else
	echo "✗ Check 2 FAILED"
	((FAIL++))
fi

echo "================================"
echo "Results: $PASS passed, $FAIL failed"

if [ $FAIL -eq 0 ]; then
	echo "✓ Standard enforced!"
	exit 0
else
	echo "✗ Standard violations found"
	exit 1
fi
```

---

## ✍️ STANDARD ACKNOWLEDGMENT TEMPLATE

For each standard, collect signatures:

```
STANDARD: [Name]
EFFECTIVE: [Date]

Acknowledgments:

Project Manager: _________________ Date: _______
Lead Developer: _________________ Date: _______
AI Assistant: _________________ Date: _______

This standard is binding for all code generation in this project.
Violations will result in PR rejection and code review.
```

---

**Use this framework to enforce ANY standard you want in your project.**
