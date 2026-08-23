# 🎯 TUBIE TOOLS CODE GENERATION: QUICK START GUIDE

**Use this before generating ANY code**

---

## ✅ THE THREE QUESTIONS (Ask BEFORE I start coding)

### Question 1: WHO ARE WE BUILDING FOR?
```
"I'm building THIS for: [Specific person/family]"

Examples:
✓ "A parent with one functional arm managing 3 kids' medications"
✓ "A blind caregiver coordinating care for an adult with CP"
✓ "A grandmother with dyslexia tracking grandchild's medical history"

❌ "General users" = NOT SPECIFIC ENOUGH
```

### Question 2: WHAT'S THEIR REAL LIMITATION?
```
"This user has: [Disability/limitation/challenge]"

Examples:
✓ "Low vision - can't see small fonts, needs high contrast"
✓ "Motor disability - can't use mouse, needs keyboard-only navigation"
✓ "Deafness - can't hear audio alerts, needs visual notifications"
✓ "Dyslexia - can't process text quickly, needs icons + labels"
✓ "Limited English - needs simple language, not medical jargon"

❌ "No limitations" = ACCESSIBILITY WILL BE SKIPPED
```

### Question 3: WHAT WILL THEY DO WITH THIS FEATURE?
```
"They will use it by: [Specific action/workflow]"

Examples:
✓ "Quickly confirm meds with one hand while holding child"
✓ "Navigate entire form without plugin mouse with screen reader"
✓ "Review 30-day medical history with vision at 20/60"
✓ "Dictate medication notes using voice-to-text"

❌ "Just use it normally" = YOU'RE NOT THINKING ABOUT ACCESSIBILITY
```

---

## 📋 FOUR BINDING STANDARDS

Before ANY code is written:

### 1. ♿ ACCESSIBILITY (MANDATORY)
```
RULE: All code must be accessible to users with disabilities

MUST:
☐ Work with keyboard only (no mouse required)
☐ Announce properly for screen readers
☐ Support zoom to 200%
☐ Have text contrast ratio ≥ 4.5:1
☐ Work on mobile with large touch targets (44px+)

MUST NOT:
☐ Require mouse to access any feature
☐ Use color alone to convey information
☐ Have time limits without ability to extend
☐ Have text smaller than 12px
☐ Use hover-only interactions

VERIFY:
- Tab through with no mouse - Does it work? ✓ YES / ✗ NO
- Test with screen reader - Does it announce correctly? ✓ YES / ✗ NO
- Zoom to 200% - Does it stay readable? ✓ YES / ✗ NO

FILE: TUBIE_TOOLS_ACCESSIBILITY_AND_SOCIAL_IMPACT_STANDARD.md
```

### 2. 🧪 TESTS ONLY (MANDATORY)
```
RULE: All code must have tests

MUST:
☐ Unit tests for business logic
☐ Integration tests for data/services
☐ External connectivity tests if calls external systems
☐ Use MSTest framework (not xUnit, not NUnit)
☐ Use [TestClass], [TestMethod], [Description]
☐ Use AAA pattern: Arrange-Act-Assert
☐ Tests must verify accessibility requirements

VERIFY:
- dotnet test passes ✓ YES / ✗ NO
- Test coverage ≥ 80% for new code ✓ YES / ✗ NO
- All tests use MSTest attributes ✓ YES / ✗ NO

FILE: MSTEST_ENFORCEMENT_STANDARD.md
FILE: TEST_FIRST_CODE_GENERATION_POLICY.md
```

### 3. 📦 NUGET PACKAGES (MANDATORY)
```
RULE: Package versions must work

MUST:
☐ Read all .csproj files for upstream requirements
☐ Choose versions matching or exceeding those requirements
☐ Document why each version was chosen
☐ User runs: dotnet restore [project].csproj
☐ Restore succeeds with ZERO errors
☐ No NU#### error codes

MUST NOT:
☐ Claim "it should work" without testing
☐ Guess at versions
☐ Skip restore verification

VERIFY:
- dotnet restore succeeds ✓ YES / ✗ NO
- Output: "Restore completed successfully" ✓ YES / ✗ NO

FILE: NUGET_VERIFICATION_STANDARD.md
```

### 4. 📍 IMPACT COMMENTS (MANDATORY)
```
RULE: Code must explain who it helps

MUST:
Every public method must have comment:
"WHO BENEFITS: [Specific user type]
PROBLEM: [Their real challenge]
SOLUTION: [How this code helps]"

Example:
/// WHO BENEFITS: Parent with cerebral palsy
/// PROBLEM: Cannot click tiny buttons with mouse
/// SOLUTION: Large buttons, keyboard navigation, screen reader support

VERIFY:
- Every public class has impact comment ✓ YES / ✗ NO
- Every public method has impact comment ✓ YES / ✗ NO

FILE: APPLYING_ACCESSIBILITY_IN_DAILY_WORK.md
```

---

## 🚀 YOUR CODE GENERATION WORKFLOW

### BEFORE code is written:
```
1. Answer the three questions above
2. Get user acknowledgment (they agree with who/why)
3. Check: Can this user actually use it?
   → NO: Redesign
   → YES: Continue
```

### WHILE code is generated:
```
1. Include impact comments
2. Design for keyboard first
3. Plan screen reader labels
4. Include high contrast option
5. Design mobile-first (large buttons)
```

### AFTER code is generated:
```
1. Run: dotnet build → Zero errors
2. Run: dotnet test → All pass (MSTest)
3. Test keyboard only (no mouse) → Works?
4. Test with screen reader → Announces correctly?
5. Test zoom 150%/200% → Still readable?
6. Run: dotnet restore → Succeeds?
7. Code review: Accessibility reviewer sign-off
```

### BEFORE commit:
```
Checklist:
☐ Specific user identified in comments
☐ Keyboard-only navigation works
☐ Screen reader compatible
☐ MSTest tests written and passing
☐ Packages verified and restore succeeds
☐ High contrast and readability verified
☐ No color-only meanings
☐ Large touch targets (44px+)
☐ No time limits or auto-dismissals
☐ Impact comments in code
```

---

## 📞 IF YOU'RE UNSURE

**Question**: Can a [USER TYPE] use this?

Examples:
- Can a BLIND user understand what this button does? → Screen reader labels
- Can a DEAF user know when an error happens? → Visual alerts, not sounds
- Can a MOTOR-DISABLED user submit this without a mouse? → Keyboard support
- Can a LOW-VISION user read these colors? → Check 4.5:1 contrast
- Can a DYSLEXIC user understand what to do? → Icons + labels, simple language
- Can a COGNITIVE user complete this quickly? → Clear steps, no complexity
- Can a VOICE-ONLY user navigate this? → Works with voice control

If the answer is NO → **DO NOT SUBMIT**
Fix it first → Then submit

---

## ⚠️ RED FLAGS (Will be rejected)

❌ Code without impact comments  
❌ Tests using xUnit or NUnit  
❌ No tests at all  
❌ Package restore fails  
❌ Code only works with mouse  
❌ Text contrast below 4.5:1  
❌ No screen reader labels  
❌ Tiny buttons (< 44px)  
❌ Color-only meaning  
❌ Auto-dismissing features  
❌ Time-limited without extension  
❌ "Accessibility is version 2.0"  

---

## ✅ WHAT SUCCESS LOOKS LIKE

Your code is APPROVED when:

```
✓ Specific user identified (not generic)
✓ Their disability/need clearly stated
✓ Keyboard-only navigation works perfectly
✓ Screen reader announces everything correctly
✓ Zoom to 200% = still readable, no horizontal scroll
✓ Contrast ratio ≥ 4.5:1 everywhere
✓ Mobile touch targets ≥ 44px
✓ MSTest tests exist and pass
✓ Package restore succeeds first try
✓ Impact comments in code
✓ Accessibility reviewer approves
✓ Code reviewer approves
✓ All CI/CD checks pass
```

---

## 📚 FULL STANDARDS (If you need details)

- **ACCESSIBILITY**: `TUBIE_TOOLS_ACCESSIBILITY_AND_SOCIAL_IMPACT_STANDARD.md`
- **APPLYING IT**: `APPLYING_ACCESSIBILITY_IN_DAILY_WORK.md`
- **TESTS**: `MSTEST_ENFORCEMENT_STANDARD.md`
- **PACKAGES**: `NUGET_VERIFICATION_STANDARD.md`
- **MASTER REGISTRY**: `ENFORCEMENT_STANDARDS_MASTER_REGISTRY.md`

---

## 🎯 REMEMBER

**We are not building software.**

**We are helping families manage complex medical care.**

**Every line of code is for:**
- 👨‍👩‍👧 Parents with disabilities parenting children
- 👩‍⚕️ Caregivers with exhaustion and burnout
- ♿ People with disabilities managing their own care
- 👵 Elderly family members coordinating help
- 👨‍👦 Families navigating hospital discharge

**Your code serves them.**

**Make it work for them. Not just for the average user.**

---

**No exceptions. No negotiation. No shortcuts.**

**This is TubieTools.**
