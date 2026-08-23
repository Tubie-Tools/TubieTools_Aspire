# TUBIE TOOLS: ACCESSIBILITY & SOCIAL IMPACT ENFORCEMENT STANDARD

**Mission**: Every line of code serves the disabled community, caregivers, parents, and families  
**Effective**: All code generation going forward  
**Authority**: TubieTools Core Values  
**Verified By**: Accessibility audit, code review, user testing  

---

## 🎯 OUR PURPOSE

**TubieTools exists to solve real problems for:**
- 👨‍👩‍👧‍👦 Families managing complex medical care at home
- 🏥 Caregivers supporting dependent relatives
- ♿ Individuals with disabilities requiring assistive technology
- 📱 Parents coordinating care across multiple caregivers
- 🌍 Communities underserved by existing healthcare software

**Every feature, every line of code, every UI element must serve this mission.**

---

## 📋 RULE DEFINITION

```
RULE: All TubieTools Code Must Serve the Disabled Community
APPLIES TO: All features, APIs, UI, database design, documentation
EFFECTIVE DATE: Immediate (retroactive review recommended)
OWNER: Accessibility Lead + Product Manager
BINDING: Cannot be violated; violations require remediation
ESCALATION: Accessibility violations block production deploy
```

---

## ✅ REQUIREMENTS (MUST)

### Accessibility (Technical)

- [ ] MUST meet **WCAG 2.1 Level AA** accessibility standards at minimum
  - Contrast ratios: 4.5:1 for normal text, 3:1 for large text
  - Alt text on ALL images
  - Keyboard navigation on ALL interactive elements
  - Screen reader compatibility (tested with NVDA/JAWS)

- [ ] MUST support **assistive technologies**:
  - Screen readers (NVDA, JAWS, VoiceOver)
  - Keyboard-only navigation (no mouse required)
  - Voice control (Voice Over, Narrator)
  - High contrast modes
  - Text scaling (up to 200%)
  - Zoom without horizontal scrolling

- [ ] MUST implement **semantic HTML** if web-based:
  - Proper heading hierarchy (`<h1>` → `<h2>`)
  - Form labels properly associated with inputs
  - ARIA attributes where semantic HTML insufficient
  - Landmarks: `<nav>`, `<main>`, `<aside>`, `<footer>`

- [ ] MUST provide **error messages** that are:
  - Clear and specific (not "Error")
  - Visible near the error source
  - Accessible to screen readers
  - Actionable with instructions to fix

- [ ] MUST ensure **status messages** are announced:
  - Use ARIA `role="status"` or `role="alert"`
  - "Loading...", "Saving...", "Error" must be announced
  - NOT announced only visually

### Usability for Users with Disabilities

- [ ] MUST provide **multiple ways** to complete critical tasks:
  - Not click-only (must support keyboard)
  - Not time-dependent (if timer, must pause/extend)
  - Not color-dependent for meaning
  - Not text-heavy for important info (use icons + labels)

- [ ] MUST **reduce cognitive load**:
  - Simple, predictable navigation
  - Consistent layout and terminology
  - Clear purpose of each field/button
  - Avoid technical jargon where possible
  - Break complex tasks into steps

- [ ] MUST support **medication/medical tracking**:
  - Large, easy-to-read fonts for dosages
  - Clear time/date display (not relative "2 days ago")
  - Medication name in full (not abbreviations)
  - Warnings/allergies highly visible
  - History viewable over time

- [ ] MUST make **caregiver workflows** simple:
  - Quick data entry (forms pre-filled where possible)
  - Mobile-friendly (thumb-sized buttons for one-handed use)
  - Offline capability (syncs when online)
  - Handoff-friendly (easy to share status with other caregivers)

### Code-Level Impact

- [ ] MUST include **comments explaining impact**:
  ```csharp
  // ACCESSIBILITY: This button allows non-mouse users to submit the form via [Enter]
  // SOCIAL IMPACT: Parents with motor disabilities can navigate without keyboard shortcuts

  // ACCESSIBILITY: Tab order is explicit to support screen readers
  // SOCIAL IMPACT: Blind caregivers can navigate medication list quickly
  ```

- [ ] MUST avoid **exclusionary patterns**:
  - ❌ No CAPTCHA without alternative (blocks users with cognitive disabilities)
  - ❌ No auto-playing media without pause control (seizure trigger risk)
  - ❌ No flashing content (can trigger seizures)
  - ❌ No font sizes below 12px minimum
  - ❌ No critical functions requiring hover-only (cannot be accessed via keyboard)
  - ❌ No forced login without offline alternative (for users with connectivity issues)

- [ ] MUST log **who benefits from each feature**:
  ```
  /// <summary>
  /// Enables caregivers with limited typing speed to input medication names quickly.
  /// WHO BENEFITS: Parent with cerebral palsy managing child's tube feeding schedule.
  /// </summary>
  ```

### Testing & Validation

- [ ] MUST be tested with **real assistive technologies**:
  - Screen reader test (NVDA on Windows, Narrator alternative)
  - Keyboard-only navigation (no mouse)
  - At 150% and 200% zoom levels
  - High contrast mode enabled

- [ ] MUST pass **automated accessibility checks**:
  - Axe DevTools (if web): Zero high/medium violations
  - WAVE (if web): Zero errors, document warnings
  - Lighthouse accessibility score: ≥ 90

- [ ] MUST include **manual accessibility audit**:
  - Reviewed by accessibility specialist before code review
  - Tested by user with actual disability who matches target user
  - Document results and sign-off

- [ ] MUST have **accessibility acceptance criteria**:
  ```gherkin
  Given a parent with cerebral palsy using keyboard-only navigation
  When they access the medication summary page
  Then they can navigate all fields without using mouse
  And screen reader announces all medication warnings
  And font can be scaled to 150% without breaking layout
  ```

### Documentation

- [ ] MUST document **accessibility features**:
  - How to use keyboard shortcuts
  - How to enable screen reader mode
  - What assistive technologies are supported
  - Contact for accessibility issues

- [ ] MUST maintain **accessibility README**:
  - Supported browsers and assistive tech
  - Known limitations and workarounds
  - How to report accessibility bugs
  - Timeline for accessibility improvements

- [ ] MUST track **accessibility debt**:
  - Log any WCAG violations with remediation timeline
  - Prioritize based on impact to users with disabilities
  - Cannot defer critical accessibility issues

---

## ❌ PROHIBITED (MUST NOT)

- ❌ Mouse-only interactions
- ❌ Time-limited sessions without ability to extend
- ❌ Auto-playing audio or video
- ❌ Flashing/blinking content
- ❌ Color as sole means of communicating information
- ❌ Functionality requiring hover-only
- ❌ Text with contrast ratio below 4.5:1
- ❌ Images without alt text
- ❌ Form fields without labels
- ❌ Skipping accessibility testing before merge
- ❌ Dismissing accessibility issues as "edge cases"
- ❌ Hardcoding fonts (must be user-scalable)
- ❌ Assuming all users can read at grade level 12
- ❌ Features that work only on desktop (mobile-first approach required)

---

## 🔍 VERIFICATION METHOD

### Pre-Deployment Checklist

```bash
✓ Manual Keyboard Navigation Test
  - Start at page top
  - Tab through ALL interactive elements
  - Verify focus indicator visible
  - Test [Enter], [Space], [Escape] keys
  - Verify no keyboard trap (can escape from any element)
  Result: [Pass/Fail, describe failures]

✓ Screen Reader Test (NVDA Windows / Narrator)
  - Open page
  - Navigate with screen reader
  - Verify all text content announced
  - Verify all form labels announced with fields
  - Verify all buttons announced with purpose
  - Verify alerts/status changes announced
  Result: [Pass/Fail, describe failures]

✓ Visual Accessibility Check
  - Zoom to 150%: No horizontal scrolling, content readable
  - Zoom to 200%: No horizontal scrolling, all buttons clickable
  - Enable high contrast: All text readable, no content hidden
  - Disable colors: No information lost (colors are redundant)
  Result: [Pass/Fail, describe failures]

✓ Automated Tool Scan
  - Run: axe DevTools (Chrome) or WAVE (if web)
  - Result: Zero high/medium violations
  - Log any warnings with justification
  Result: [Pass/Fail, violations listed]

✓ Lighthouse Audit
  - Run: chrome://inspect → Lighthouse
  - Accessibility score: ≥ 90
  - Submit report
  Result: [Pass/Fail, score: ___]

✓ Code Review Accessibility Check
  - Reviewer: Accessibility-trained team member
  - Verified: Comments explain WHO benefits and HOW
  - Verified: No exclusionary patterns in code
  - Verified: Accessibility requirements in tests
  Result: [Approved/Changes Required]

✓ User Testing (if possible)
  - Test with user matching target user profile
  - Collect feedback: What worked? What was hard?
  - Remediate blocking issues
  Result: [Feedback documented, blocking issues logged]
```

### Success Criteria

**Code passes this standard if ALL of:**

1. ✅ WCAG 2.1 Level AA compliance verified
2. ✅ Keyboard-only navigation works completely
3. ✅ Screen reader announces all content correctly
4. ✅ Automated tool scan: Zero high/medium violations
5. ✅ Text contrast ratios meet 4.5:1 minimum
6. ✅ All images have descriptive alt text
7. ✅ Form labels properly associated
8. ✅ Code includes comments on impact to disabled users
9. ✅ No exclusionary patterns used
10. ✅ Accessibility reviewer sign-off obtained

---

## 🚨 IF STANDARD VIOLATED

### Level 1: Generation Phase
```
If I generate code NOT following accessibility standards:
1. User identifies violation
2. Specific accessibility failure described (e.g., "no alt text on logo")
3. I regenerate component with accessibility fix
4. No code merged until standard met
5. Root cause documented (why I failed)
```

### Level 2: Code Review Phase
```
If accessibility violation found in PR:
1. PR blocked with accessibility label
2. Developer provided link to remediation guidance
3. Accessibility reviewer added to approve fix
4. Cannot merge without accessibility sign-off
5. Failed accessibility checks prevent CI/CD pass
```

### Level 3: Post-Merge
```
If violation reaches production:
1. Incident logged with severity
2. Rollback deployed if critical
3. Remediation PR created with timeline
4. Root cause analysis: Process improvement needed
5. Team training scheduled on violation type
```

---

## 📋 IMPACT COMMENT TEMPLATE

**I will include this in EVERY generated feature:**

```csharp
/// <summary>
/// [SOCIAL IMPACT]
/// WHO BENEFITS: [Target user with disability]
/// PROBLEM SOLVED: [Real-world challenge they face]
/// HOW IMPLEMENTED: [Specific accessibility feature]
/// 
/// [ACCESSIBILITY FEATURES]
/// - Keyboard accessible: [Yes/No - how accessed without mouse]
/// - Screen reader support: [How announced]
/// - Visual: [Contrast, font size, color-independence]
/// - Mobile: [Touch targets, responsiveness]
/// </summary>
```

---

## 💡 EXAMPLES: WHAT THIS LOOKS LIKE

### ❌ BAD: Only considers able-bodied users
```csharp
// Show medication reminder popup (mouse only, auto-dismiss)
var dialog = new PopupDialog("Take medication now!");
// Close after 5 seconds automatically
Task.Delay(5000).ContinueWith(_ => dialog.Close());
```

**Problems:**
- Keyboard-only user cannot open/close
- Visually impaired: cannot see popup
- Timer expires before user processes info
- No screen reader announcement

### ✅ GOOD: Designed for disability access
```csharp
/// <summary>
/// Display persistent medication reminder that must be explicitly dismissed.
///
/// [SOCIAL IMPACT]
/// WHO BENEFITS: Parent with low vision and motor disability
/// PROBLEM: Auto-dismissing reminders prevent them from seeing/accessing
/// HOW: Persistent, keyboard-navigable, large fonts, high contrast
/// </summary>
public class MedicationReminderDialog
{
	public MedicationReminderDialog(string medicationName, string dosage)
	{
		// ACCESSIBILITY: Large, readable fonts (minimum 16px)
		LabelMedicationName.Text = medicationName;
		LabelMedicationName.Font = new Font("Arial", 16, FontStyle.Bold);

		// ACCESSIBILITY: High contrast background
		BackColor = Color.White;
		ForeColor = Color.Black; // 21:1 contrast ratio

		// ACCESSIBILITY: Button large enough for motor disabilities
		ButtonTake.Height = 50; // Touch target: ≥ 44px
		ButtonTake.Width = 200;

		// ACCESSIBILITY: Screen reader announces purpose
		ButtonTake.AccessibleName = $"Confirm {medicationName} {dosage} taken";
		ButtonTake.AccessibleDescription = 
			"Press to log medication and dismiss reminder";

		// ACCESSIBILITY: Keyboard support (no mouse required)
		ButtonTake.Click += (s, e) => ConfirmMedication();
		this.KeyDown += (s, e) => 
		{
			if (e.KeyCode == Keys.Enter) ConfirmMedication();
			if (e.KeyCode == Keys.Escape) CancelReminder();
		};

		// ACCESSIBILITY: No auto-dismiss (respects user pace)
		// Timer never triggered automatically
	}
}
```

**Why this works:**
- Parent can use keyboard OR mouse
- Screen reader announces reminder
- Parent can take their time
- Large buttons for motor disabilities
- High contrast = visible to low vision user
- Can be used one-handed

---

## 📱 REAL-WORLD USER STORIES

### Story 1: Parent with Cerebral Palsy
```
As a parent with cerebral palsy managing my child's feeding tube
I need the medication app to work with keyboard-only navigation
So that I can quickly confirm doses without fine mouse control
And my voice-to-text software can fill in the medical history

ACCEPTANCE CRITERIA:
- All fields accessible via Tab key
- No hover-only functionality
- Voice commands can navigate forms
- Text scaling to 150% works without truncation
```

### Story 2: Blind Caregiver
```
As a blind caregiver using a screen reader
I need all medication information announced clearly
So that I can safely administer care without sighted assistance

ACCEPTANCE CRITERIA:
- Dosage announced when form loads
- Warnings (allergies, interactions) announced as alerts
- Medication name in full, not abbreviation
- Time/date explicit (not "2 hours ago")
```

### Story 3: Deaf Parent
```
As a deaf parent who communicates in ASL
I need all video content in our app captioned
And important announcements in text form

ACCEPTANCE CRITERIA:
- All training videos have captions
- Alert sounds have visual equivalents
- Notification center has readable history
- No critical info only in audio
```

### Story 4: Parent with Dyslexia
```
As a parent with dyslexia managing multiple medications
I need clear visual separation between medications
And simple, predictable navigation

ACCEPTANCE CRITERIA:
- Each medication on separate card (visual chunking)
- Consistent button placement (predictable)
- Icons + labels (not text only)
- Search function with spell-tolerance
```

---

## ✅ CODE GENERATION CHECKLIST

**I will include this BEFORE generating ANY code:**

```
# ACCESSIBILITY & SOCIAL IMPACT CHECKLIST

Question: Who are we building this for?
Answer: [Specific disability, caregiver type, family scenario]

Question: What real-world problem does this solve?
Answer: [Concrete challenge, not abstract]

Verify EVERY feature for:
- [ ] Keyboard navigation (no mouse required)
- [ ] Screen reader compatibility
- [ ] Large text support (150%, 200% zoom)
- [ ] High contrast / color-independent
- [ ] Mobile touch-friendly
- [ ] Simple, clear error messages
- [ ] Respectful language (no "illness," no stigma)
- [ ] Code comments explaining WHO benefits and HOW

Before you accept ANY code I generate:
- [ ] I included impact comments
- [ ] I included accessibility requirements
- [ ] I can describe the disabled user this serves
- [ ] Running Axe/WAVE shows zero high violations
- [ ] You tested with keyboard only
- [ ] You tested with screen reader

Do not accept code that fails ANY of above.
```

---

## 🌍 ACCESSIBILITY FIRST MINDSET

When I generate code, I will ask myself:

1. **Question**: Can someone use this who cannot use a mouse?
   - If NO: Add keyboard support before generating

2. **Question**: Can someone use this who cannot see colors?
   - If NO: Ensure information independent of color

3. **Question**: Can someone use this with a screen reader?
   - If NO: Add proper semantics before generating

4. **Question**: Can someone use this with only the keyboard?
   - If NO: Refactor to support Tab/Enter/Escape

5. **Question**: Can someone understand this in 30 seconds?
   - If NO: Simplify before generating

6. **Question**: Does this work on mobile with thumbs only?
   - If NO: Redesign buttons/forms before generating

7. **Question**: Who specifically will this help?
   - If I cannot name them: This feature needs clarification

---

## 📞 ENFORCEMENT AUTHORITY

**Who enforces this:**
- Product Manager (approves new features for accessibility impact)
- Accessibility Reviewer (blocks merge if violations)
- Lead Developer (final code review authority)
- Users with disabilities (testing & feedback)

**What triggers escalation:**
- Code without impact comments
- No keyboard support
- No screen reader testing
- Accessibility violation patterns repeating
- Axe/WAVE shows high/medium violations

---

## 📚 RELATED STANDARDS

- `MSTEST_ENFORCEMENT_STANDARD.md` - Accessibility testing requirements
- `TEST_FIRST_CODE_GENERATION_POLICY.md` - Include accessibility tests
- `NUGET_VERIFICATION_STANDARD.md` - Verify accessibility packages

---

## 🎓 ACCESSIBILITY LEARNING RESOURCES

**For developers:**
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [WebAIM Screen Reader Testing](https://webaim.org/articles/screenreader_testing/)
- [Inclusive Components](https://inclusive-components.design/)
- [A11y Project](https://www.a11yproject.com/)

**For testing:**
- Free: Axe DevTools, WAVE, Lighthouse
- Screen Readers: NVDA (free, Windows), Narrator (free, Windows), VoiceOver (Mac)
- Keyboard testing: Disable mouse, use Tab/Arrows only

---

## ✍️ ENFORCEMENT ACKNOWLEDGMENT

```
STANDARD: TubieTools Accessibility & Social Impact
EFFECTIVE: [Today's date]
MISSION: Every line of code serves the disabled community

Acknowledgments:

Product Manager: _________________ Signature: ______ Date: _______
Accessibility Lead: _________________ Signature: ______ Date: _______
Lead Developer: _________________ Signature: ______ Date: _______
AI Code Assistant: _________________ Signature: ______ Date: _______

BINDING COMMITMENT:
This standard is non-negotiable. Our users depend on it.
Accessibility violations are defects, not nice-to-haves.
We will not compromise access for expedience.

"Build for everyone. First."
```

---

**This is why TubieTools exists. Every feature must serve this mission.**
