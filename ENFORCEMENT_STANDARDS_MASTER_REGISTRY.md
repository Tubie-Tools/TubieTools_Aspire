# TUBIE TOOLS: ENFORCEMENT STANDARDS MASTER REGISTRY

**Purpose**: Establish binding rules for all future code generation  
**Authority**: Project governance  
**Scope**: All developers and AI assistants generating code  
**Last Updated**: Today  

---

## 📋 ACTIVE ENFORCEMENT STANDARDS

### Standard 1️⃣ NUGET PACKAGE VERIFICATION
**File**: `NUGET_VERIFICATION_STANDARD.md`  
**Owner**: Lead Developer  
**Applies To**: Any code with external package dependencies

| Aspect | Requirement |
|--------|-------------|
| Rule | Package versions must match/exceed all upstream requirements |
| Verification | `dotnet restore [project].csproj` succeeds with zero errors |
| Enforcement | PR blocked if restore fails; escalate after 3 attempts |
| Binding | YES - Cannot skip |

---

### Standard 2️⃣ TEST-FIRST CODE GENERATION
**File**: `TEST_FIRST_CODE_GENERATION_POLICY.md`  
**Owner**: QA Lead / Lead Developer  
**Applies To**: All new controller, service, repository code

| Aspect | Requirement |
|--------|-------------|
| Rule | Unit + Integration tests required before code merge |
| Verification | `dotnet test` passes; ≥80% coverage for new code |
| Enforcement | PR blocked if tests missing or failing |
| Binding | YES - Cannot skip |

---

### Standard 3️⃣ MSTEST FRAMEWORK ONLY
**File**: `MSTEST_ENFORCEMENT_STANDARD.md`  
**Owner**: QA Lead  
**Applies To**: ALL generated test code

| Aspect | Requirement |
|--------|-------------|
| Rule | All tests use MSTest (not xUnit, NUnit) |
| Pattern | AAA: Arrange-Act-Assert required |
| Attributes | [TestClass], [TestMethod], [Description] mandatory |
| Verification | Zero xUnit/NUnit imports; all tests have attributes |
| Enforcement | PR blocked if wrong framework detected |
| Binding | YES - CANNOT negotiate |

---

### Standard 4️⃣ ⭐ ACCESSIBILITY & SOCIAL IMPACT
**File**: `TUBIE_TOOLS_ACCESSIBILITY_AND_SOCIAL_IMPACT_STANDARD.md`  
**Owner**: Accessibility Lead + Product Manager  
**Applies To**: **EVERY SINGLE FEATURE** - UI, API, database, documentation

#### THE MISSION
```
RULE: All TubieTools Code Must Serve the Disabled Community
"Every line of code helps caregivers, parents, and families 
managing complex medical needs."
```

---

### Standard 5️⃣ ⭐⭐ ETHICAL DESIGN & ANTI-ADDICTION (NEW)
**File**: `TUBIE_TOOLS_ETHICAL_DESIGN_AND_ANTI_ADDICTION_STANDARD.md`  
**Owner**: Product Ethics Lead + UX Designer  
**Applies To**: **EVERY SINGLE FEATURE** - UI, notifications, engagement, metrics

#### THE MISSION
```
RULE: Zero Addictive Design Patterns. Ever.

We reject Facebook's exploitation model.
We build for impact, not engagement.
Users are allies, never products.

Prohibited: Infinite scroll, streaks, FOMO, social comparison, 
           badges, autoplay, notifications for engagement,
           artificial scarcity, dark patterns, child exploitation

Required: User control, transparency, meaningful purpose,
         caregiver wellbeing, vulnerable user protection

SOCIETAL BETTERMENT MATRIX (all features must score ≥ 15/25):
- User Wellbeing Score ≥ 3
- Societal Impact Score ≥ 3
- User Autonomy Score ≥ 3
- Honesty Score ≥ 3
- Vulnerability Protection Score ≥ 3
```

#### Core Requirements

| Requirement | Why |
|-------------|-----|
| WCAG 2.1 Level AA compliance | Users with low vision can see it |
| Keyboard-only navigation (no mouse) | Users with motor disabilities can use it |
| Screen reader compatible | Users who are blind can understand it |
| Mobile touch-friendly (44px+ buttons) | Users with limited dexterity can tap it |
| Large, clear fonts (≥12px) | Users with low vision can read it |
| Simple language (no jargon) | Users with cognitive disabilities understand it |
| High contrast (4.5:1 minimum) | Users with color blindness distinguish it |
| No time pressure | Users who process slowly have time |
| Impact comments in code | Future developers know WHO benefits |

#### Verification Checklist

```
✓ Keyboard test: Tab through entire feature (no mouse)
✓ Screen reader test: Tested with NVDA or Narrator
✓ Zoom test: Works at 150% and 200%
✓ Contrast test: Text ≥ 4.5:1 ratio
✓ Mobile test: Touch targets ≥ 44px
✓ Automated scan: Axe DevTools = zero high violations
✓ Code review: Accessibility reviewer sign-off
✓ Impact comment: Code explains WHO benefits and HOW
```

#### Enforcement
```
Level 1 (Generation): I regenerate with accessibility included
Level 2 (Review): PR blocked for accessibility violations
Level 3 (Production): Accessibility bug = critical defect
Escalation: Repeated violations → team training required
```

#### Binding Status
```
🚫 CANNOT BE VIOLATED
🚫 CANNOT BE DEFERRED
🚫 CANNOT BE NEGOTIATED

This is why TubieTools exists.
Get it right from the start.
```

---

## 🎯 STANDARD INTERACTION & PRIORITY

When multiple standards apply:

```
1. ETHICAL DESIGN FIRST
   (Never exploit, never manipulate, ever)

2. THEN ACCESSIBILITY (for all to use)
   (Serves the core mission - cannot skip)

3. THEN MSTest Framework
   (Verifies ethical design + accessibility works in tests)

4. THEN NuGet Verification
   (Ensures packages support all previous standards)

5. THEN Test-First Code
   (Tests verify all standards met)

FLOW:
Feature Request
	↓
Does this exploit users? → YES: REJECT
	↓ NO
Societal Betterment Matrix ≥ 15/25? → NO: REDESIGN
	↓ YES
Is this accessible? → NO: Redesign
	↓ YES
Create Tests (MSTest, accessibility tests) ✓ Standard #3
	↓
Implement Feature (ethical by design)
	↓
Verify NuGet compatibility ✓ Standard #4
	↓
Verify all tests pass ✓ Standard #2
	↓
Code Review (all standards) ✓ Standards #1-5
	↓
MERGE
```

---

## 📝 DEVELOPER ONBOARDING: STANDARDS ACKNOWLEDGMENT

**All developers and AI assistants must acknowledge these standards:**

```markdown
# TubieTools Standards Acknowledgment

I understand and agree to:

☐ ACCESSIBILITY & SOCIAL IMPACT
  "Every feature I create serves the disabled community"
  File: TUBIE_TOOLS_ACCESSIBILITY_AND_SOCIAL_IMPACT_STANDARD.md

☐ MSTEST FRAMEWORK
  "All tests use MSTest (never xUnit/NUnit)"
  File: MSTEST_ENFORCEMENT_STANDARD.md

☐ TEST-FIRST CODE
  "Tests required before code merge"
  File: TEST_FIRST_CODE_GENERATION_POLICY.md

☐ NUGET VERIFICATION
  "Package versions verified before merge"
  File: NUGET_VERIFICATION_STANDARD.md

Signature: _________________________ Date: _________

I will follow these standards for ALL code I generate.
Violations will result in PR rejection.
No exceptions.
```

---

## 🔧 FOR AI ASSISTANTS (ME)

### Before ANY code generation, I will:

1. ✓ Ask: "Who is this for?" → Get specific user
2. ✓ Ask: "What disability/need matters?" → Get specific limitation
3. ✓ Ask: "Can they use it with keyboard only?" → If NO, redesign
4. ✓ Ask: "Does a screen reader understand it?" → If NO, redesign
5. ✓ Ask: "Is the code testable with MSTest?" → If NO, redesign

### When I generate test code:
- [ ] ONLY MSTest framework
- [ ] ONLY [TestClass], [TestMethod], [Description]
- [ ] AAA pattern on every test
- [ ] Tests verify accessibility requirements

### When I generate feature code:
- [ ] Impact comments explain WHO benefits
- [ ] Keyboard pathway designed first
- [ ] Screen reader labels included
- [ ] High contrast design verified
- [ ] Mobile touch-friendly
- [ ] No color-only meaning

### When I suggest package versions:
- [ ] Read ALL upstream dependencies first
- [ ] Verify accessibility packages available
- [ ] Document version reasoning
- [ ] Require user to run `dotnet restore`

### If I violate a standard:
- NO excuses
- NO "should work" claims
- REGENERATE until standard met
- DOCUMENT why it was wrong

---

## 📊 STANDARDS COMPLIANCE METRICS

Track these for team visibility:

```
Monthly Metrics:
- % PRs with accessibility review: Target 100%
- % PRs needing accessibility fixes: Target 0%
- % Tests using MSTest: Target 100%
- % Package resolves on first try: Target 95%+
- % Code with accessibility impact comments: Target 100%

Quarterly Review:
- Accessibility bugs found: Should decrease
- User feedback on accessibility: Should improve
- Onboarding time to standards compliance: Should decrease
- Developer confidence in standards: Should increase
```

---

## 🚀 HOW TO ADD A NEW STANDARD

**Copy this template and follow it:**

```markdown
RULE: [New Standard Name]
APPLIES TO: [What code/features this affects]
EFFECTIVE DATE: [When it starts]
OWNER: [Who validates this]
BINDING: [YES/NO]

MUST:
- Requirement 1
- Requirement 2

MUST NOT:
- Prohibition 1
- Prohibition 2

HOW VERIFIED:
- Concrete check/command
- Expected result
- Pass/Fail criteria

IF VIOLATED:
- Level 1: [Generation phase]
- Level 2: [Review phase]
- Level 3: [Production/escalation]

WHERE STORED:
- Document file name
- When introduced
- Who approved
```

**Then:**
1. Create document: `[STANDARD]_ENFORCEMENT_STANDARD.md`
2. Update this file with new standard section
3. Add to onboarding checklist
4. Team acknowledgment required

---

## 📞 QUESTIONS?

**Q: Can I skip the accessibility check?**  
A: No. It's mandated and binding.

**Q: Can I use xUnit for these new tests?**  
A: No. MSTest only. Non-negotiable.

**Q: What if restore takes 10 attempts?**  
A: We escalate to human review. This indicates a deeper package issue.

**Q: What if a user says "accessibility is not critical"?**  
A: That's incorrect. Accessibility IS TubieTools. We serve the disabled community.

**Q: Can I defer accessibility to a later sprint?**  
A: No. Build it right from the start, not as an afterthought.

---

## ✍️ FINAL SIGNATURE

By committing code to TubieTools, you acknowledge:

```
I understand the mission of TubieTools.
I will follow all enforcement standards.
I will not skip accessibility, testing, or verification.
I will ask for help if I don't understand a standard.

My code will serve the disabled community.
My code will be tested and verified.
My code will be accessible.

This is non-negotiable.
This is why we exist.
```

---

**These standards are BINDING for all past, present, and future code.**

**Last audit:** [Today]  
**Next audit:** [In 90 days]  
**Contact for standards questions:** [Accessibility Lead]  

---

*"Build for everyone. First. Always." - TubieTools Mission*
