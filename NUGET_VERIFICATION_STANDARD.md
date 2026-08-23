# NUGET PACKAGE VERIFICATION STANDARD

**Effective**: All future code generation  
**Authority**: Non-negotiable requirement  
**Verification**: User-run `dotnet restore` only truth source  

---

## 🔴 BEFORE I GENERATE CODE WITH PACKAGES

### Step 1: Dependency Discovery (I Do This)
- [ ] Identify all referenced projects
- [ ] Read ALL .csproj files for direct dependencies
- [ ] Extract exact minimum versions from each
- [ ] Document the dependency chain

### Step 2: Conflict Analysis (I Do This)
- [ ] Check for version conflicts between dependencies
- [ ] Ensure my proposed versions match or exceed all requirements
- [ ] Compare against latest stable versions on NuGet
- [ ] Create version audit trail

### Step 3: Documentation (I Do This)
- [ ] Write exact package version sources (which .csproj requires this)
- [ ] Explain why each version was chosen
- [ ] List all transitive dependencies I'm aware of
- [ ] Provide this to user BEFORE code generation

### Step 4: Verification (YOU Do This)
- [ ] Run: `dotnet restore [ProjectName].csproj`
- [ ] Capture output (success or errors)
- [ ] Paste results back to me
- [ ] **This is the only truth source**

---

## ✅ WHAT I GUARANTEE

When you provide error output, I will:
- ✅ Fix ONLY the specific error you show
- ✅ Explain why the error occurred
- ✅ Update the exact package version
- ✅ Not create new errors
- ✅ Ask you to re-run `dotnet restore`

---

## ❌ WHAT I WILL NOT DO

- ❌ Claim "it should work" without your verification
- ❌ Guess at package versions
- ❌ Skip reading referenced project files
- ❌ Continue with code generation if restore fails
- ❌ Fix errors I cannot see from your error list

---

## 🔄 PROCESS FLOW

```
1. You: "Generate API for X"
2. Me: Read all .csproj files
3. Me: Show you proposed package versions + reasoning
4. Me: Generate code
5. You: Run 'dotnet restore'
6. You: Paste output
7. If errors:
   → Me: Fix specific package version
   → You: Re-run 'dotnet restore'
   → Repeat
8. If success:
   → You: Run 'dotnet build'
   → Continue to next phase
```

---

## 📋 TEMPLATE I WILL USE

Before ANY code generation with packages:

```
DEPENDENCY ANALYSIS - [ProjectName]

Referenced Projects:
- TubieTools_Aspire.EnterpriseAutomation.csproj
  ├─ Swashbuckle.AspNetCore >= 10.2.3 [FROM: line X]
  ├─ Microsoft.AspNetCore.OpenApi >= 10.0.9 [FROM: line Y]
  └─ Microsoft.EntityFrameworkCore >= 10.0.6 [FROM: line Z]

Proposed Versions for [ProjectName]:
- Swashbuckle.AspNetCore: 10.2.3 (matches requirement)
- Microsoft.AspNetCore.OpenApi: 10.0.9 (matches requirement)
- Microsoft.EntityFrameworkCore: 10.0.6 (matches requirement)

VERIFICATION REQUIRED:
Run: dotnet restore [ProjectName].csproj
Post output here before proceeding.
```

---

## 🚨 ESCALATION

If after fixing > 3 NuGet errors, new ones keep appearing:

**Stop and report to user:**
- "NuGet conflicts unresolved after 3 fixes"
- "Additional dependencies may need review"
- "Recommend manual inspection of transitive dependencies"
- "Consider narrowing scope of this PR"

**Do NOT** continue generating code blindly.

---

## 📊 ACCOUNTABILITY

This standard is measured by:
- ✅ Zero NuGet errors after first generation
- ✅ If errors occur, fixed in ≤ 2 iterations
- ✅ All fixes traceable to user-provided error output
- ✅ No "should work" claims

**If not met:** Process failed, needs review.

---

## 🎯 USER RESPONSIBILITIES

You will:
- Run `dotnet restore` after I generate code
- Paste exact error output back to me
- Verify build succeeds with `dotnet build`
- Don't accept "it should compile" claims

---

## ✍️ SIGNATURE

**I agree to follow this standard for ALL future code generation with NuGet packages.**

**User confirms**: This is now the standard for all work.

---

**Next code generation will follow this process exactly.**
