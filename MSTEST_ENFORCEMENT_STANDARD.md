# MSTEST ENFORCEMENT STANDARD

**Effective**: All test code generation going forward  
**Authority**: Project testing policy  
**Verified By**: Code compilation + test execution  

---

## 📋 RULE DEFINITION

```
RULE: All Generated Tests Use MSTest Framework
APPLIES TO: Unit tests, Integration tests, any automated test code
EFFECTIVE DATE: Immediate
OWNER: QA Lead / Developer verification
BINDING: Cannot be overridden without written approval
```

---

## ✅ REQUIREMENTS (MUST)

### Test Framework
- [ ] MUST use `Microsoft.VisualStudio.TestTools.TestFramework` package
- [ ] MUST use `[TestClass]` attribute on test classes
- [ ] MUST use `[TestMethod]` attribute on test methods
- [ ] MUST use `[TestInitialize]` for setup
- [ ] MUST use `[TestCleanup]` for teardown
- [ ] MUST NOT use xUnit or NUnit attributes
- [ ] MUST NOT use other test frameworks

### Test Pattern (AAA - Arrange-Act-Assert)
- [ ] MUST have `// ARRANGE` section
- [ ] MUST have `// ACT` section
- [ ] MUST have `// ASSERT` section
- [ ] MUST have descriptive method names: `[Scenario]_[Given]_[Expected]()`

### Assertions
- [ ] MUST use `Assert.IsNotNull()`, `Assert.AreEqual()`, etc.
- [ ] MUST use `Assert.IsTrue()` / `Assert.IsFalse()` for booleans
- [ ] MUST NOT use FluentAssertions or other assertion libraries
- [ ] MUST have explanatory messages in assertions
- [ ] Example: `Assert.AreEqual(expected, actual, "Description of what failed");`

### Test Attributes
- [ ] MUST have `[Description("...")]` on every test method
- [ ] MUST have `[ExpectedException(typeof(...))]` for error cases
- [ ] MUST use `[Ignore("reason")]` only if approved by lead

### Mocking
- [ ] MUST use `Moq` package for mocking dependencies
- [ ] MUST use `Mock<IInterface>` for interfaces
- [ ] MUST use `.Setup()` for mocking behavior
- [ ] MUST verify calls with `.Verify()`
- [ ] MUST NOT create fake implementations (use mocks instead)

### Test Organization
- [ ] MUST create separate projects for Unit/Integration tests
- [ ] MUST organize by folder: `Unit/`, `Integration/`, `External/`
- [ ] MUST organize by concern: `Controllers/`, `Services/`, `Repositories/`
- [ ] MUST have unique, non-conflicting test class names

---

## ❌ PROHIBITED (MUST NOT)

- ❌ xUnit framework
- ❌ NUnit framework
- ❌ Other testing frameworks
- ❌ Mixed frameworks in same solution
- ❌ Tests without AAA pattern
- ❌ Tests without [Description] attributes
- ❌ Hardcoded test data (use Arrange section)
- ❌ Tests that depend on other tests running first
- ❌ Tests without isolation (use mocks)
- ❌ Console.WriteLine() instead of assertions
- ❌ Skipped tests without [Ignore] attribute and reason

---

## 🔍 VERIFICATION METHOD

### Pre-Compilation Check
```bash
# Search for prohibited patterns
grep -r "using Xunit" *.csproj
grep -r "using NUnit" *.csproj
grep -r "[Fact]" *.cs
grep -r "[Test]" *.cs
```

**Expected**: Zero results (no other frameworks found)

### Compilation Check
```bash
dotnet build [TestProject].csproj -c Release
```

**Expected**: `Build succeeded. 0 Warning(s) 0 Error(s)`

### Test Execution
```bash
dotnet test [TestProject].csproj --logger "console;verbosity=detailed"
```

**Expected Output Format**:
```
Passed:  [N] | Failed: 0 | Skipped: 0
Test run successful.
```

### Pattern Verification
```bash
# Verify AAA pattern
grep -c "// ARRANGE" [TestFile].cs  # Should be > 0
grep -c "// ACT" [TestFile].cs      # Should be > 0
grep -c "// ASSERT" [TestFile].cs   # Should be > 0
```

**Expected**: Each test has all 3 sections

### Attribute Verification
```bash
# Verify required attributes
grep "[TestClass]" [TestFile].cs      # Per class
grep "[TestMethod]" [TestFile].cs     # Per method
grep "[Description(" [TestFile].cs    # Per method
```

**Expected**: 
- Every class has `[TestClass]`
- Every method has `[TestMethod]`
- Every method has `[Description(...)]`

---

## ✅ SUCCESS CRITERIA

Test code passes this standard if ALL of:

1. ✅ Compiles with zero errors
2. ✅ All tests use MSTest attributes
3. ✅ All tests follow AAA pattern
4. ✅ All tests have [Description]
5. ✅ No xUnit or NUnit imports
6. ✅ Tests execute and pass
7. ✅ Assertions use Assert.* methods
8. ✅ Mocking uses Moq package

---

## 🚨 IF STANDARD VIOLATED

### Level 1: Generation Phase
```
If I generate code NOT following this standard:
1. User identifies violation
2. User provides specific example
3. I regenerate test file with correct framework
4. No code merged until standard met
5. Documented as learning correction
```

### Level 2: Code Review Phase
```
If code review finds violation:
1. PR blocked until fixed
2. Developer notified
3. Link to this standard provided
4. Must be fixed before approval
```

### Level 3: Merge Prevention
```
If violation reaches main branch:
1. PR reverted immediately
2. Issue created with link to this standard
3. Developer retraining required
4. Review process updated
```

---

## 📋 CODE GENERATION CHECKLIST

**I will include this in EVERY test generation:**

```
# TEST CODE VERIFICATION CHECKLIST

Before I generate test code, verify:
- [ ] Framework: MSTest only
- [ ] Pattern: AAA (Arrange-Act-Assert)
- [ ] Attributes: [TestClass], [TestMethod], [Description]
- [ ] Assertions: Assert.* methods only
- [ ] Mocking: Moq for dependencies
- [ ] Organization: Proper folder structure
- [ ] Isolation: No shared state between tests
- [ ] Names: Descriptive, not generic

After I generate, YOU verify:
- [ ] dotnet build succeeds
- [ ] dotnet test shows all passing
- [ ] No xUnit or NUnit references
- [ ] Every test has [Description]
- [ ] Every test has AAA pattern
- [ ] grep finds no prohibited keywords

Do not accept test code that fails ANY of above.
```

---

## 📞 ENFORCEMENT AUTHORITY

**Who enforces this:**
- Code reviewers (block merge if violated)
- Build pipeline (run test verification script)
- Lead developer (final approval)

**What triggers re-review:**
- Any xUnit/NUnit found in PR
- Test class without [TestClass]
- Test method without [TestMethod]
- Test without [Description]
- Failed test execution

---

## 🔗 RELATED DOCUMENTS

- `NUGET_VERIFICATION_STANDARD.md` - How packages are verified
- `TEST_FIRST_CODE_GENERATION_POLICY.md` - Why tests are required
- `ENFORCEMENT_FRAMEWORK.md` - How rules are created and enforced

---

## ✍️ ACKNOWLEDGMENT

**Project Manager**: Acknowledge this standard is binding
**Lead Developer**: Acknowledge this will be enforced in code review
**AI Assistant**: Acknowledge this will be followed for all test generation

---

**This standard is NON-NEGOTIABLE for all test code going forward.**
