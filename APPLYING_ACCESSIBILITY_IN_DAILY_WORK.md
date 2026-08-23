# APPLYING THE TubieTools ACCESSIBILITY STANDARD IN DAILY WORK

**How to integrate accessibility and social impact into every decision**

---

## 🎯 CORE PRINCIPLE

Before writing ANY code, before any planning meeting, before any feature:

### THE QUESTION
**"Who am I building this for, and can they actually use it?"**

If you cannot answer that with a real person in mind, the feature is not ready.

---

## 📋 FEATURE REQUEST → ACCESSIBILITY ROADMAP

### When You Get a Feature Request

**OLD (Bad) Process:**
```
Manager: "Add user profile page"
Developer: "OK, I'll build it"
→ Code ships with accessibility problems
```

**NEW (TubieTools) Process:**
```
Manager: "Add user profile page"

DEVELOPER ASKS:
1. "Who is using this?" → "Parents managing child's care"
2. "What disabilities?" → "Some use screen readers, some have low vision"
3. "What's the pain point?" → "Can't quickly update emergency contacts"
4. "How will they use it?" → "Keyboard + mobile, some one-handed"

DEVELOPER COMMITS:
- [ ] Include screen reader labels
- [ ] Support keyboard navigation
- [ ] Large buttons for mobile
- [ ] High contrast option
- [ ] Test with accessibility tools before code review

FEATURE READY FOR DEVELOPMENT
```

---

## 🛠️ DEVELOPER CHECKLIST: FEATURE CODE IN PROGRESS

Copy this to your task/PR description:

```markdown
## Accessibility Checklist

### User Context
- [ ] Target user identified: [Who uses this feature?]
- [ ] Disability/limitation considered: [What challenge do they have?]
- [ ] Real-world scenario documented: [Their workflow]

### Keyboard Support
- [ ] All interactive elements accessible via Tab
- [ ] [Enter] / [Space] / [Escape] work appropriately
- [ ] No keyboard trap (can exit any element)
- [ ] Focus indicator clearly visible
- [ ] Tab order is logical

### Screen Reader Support
- [ ] All text content announced
- [ ] Form labels associated with inputs
- [ ] Buttons have accessible names
- [ ] Alerts/status changes announced
- [ ] Headings properly structured

### Visual Accessibility
- [ ] Text contrast ratio ≥ 4.5:1
- [ ] Font size ≥ 12px minimum
- [ ] Works at 150% and 200% zoom
- [ ] No information conveyed by color alone
- [ ] Images have alt text

### Mobile & Touch
- [ ] Touch targets ≥ 44px minimum
- [ ] Works with one hand
- [ ] No phone-rotation required
- [ ] Touch gestures have keyboard alternatives

### Testing
- [ ] [ ] Axe DevTools scan: Zero high/medium violations
- [ ] [ ] Tested with keyboard only (no mouse)
- [ ] [ ] Tested with screen reader (NVDA or Narrator)
- [ ] [ ] Tested at 150% zoom
- [ ] [ ] Tested in high contrast mode

### Code Quality
- [ ] Impact comment included on all public methods
- [ ] Comments explain WHO benefits and HOW
- [ ] No accessibility-unfriendly patterns used
- [ ] Accessibility requirements in unit tests
```

---

## 💻 CODE EXAMPLES BY SCENARIO

### Scenario 1: Building a Button Users Need to Click

**ACCESSIBLE Pattern:**
```csharp
// ACCESSIBILITY & SOCIAL IMPACT
// WHO BENEFITS: Parent with motor disability using keyboard only
// PROBLEM: Clicking tiny buttons requires fine motor control
// SOLUTION: Keyboard support + large touch target

public class MedicationConfirmButton : Button
{
	public MedicationConfirmButton(string medicationName)
	{
		// ✅ LARGE ENOUGH FOR TOUCH (44px minimum for mobile)
		this.Height = 50;
		this.Width = 200;
		this.Font = new Font("Arial", 14, FontStyle.Bold);

		// ✅ KEYBOARD SUPPORT
		this.Click += (s, e) => ConfirmMedication();

		// ✅ SCREEN READER ANNOUNCES PURPOSE
		this.AccessibleName = $"Confirm {medicationName} administered";
		this.AccessibleDescription = "Click to log medication administration time";

		// ✅ HIGH CONTRAST (Black on White = 21:1)
		this.BackColor = Color.White;
		this.ForeColor = Color.Black;
		this.FlatStyle = FlatStyle.Flat;
		this.FlatAppearance.BorderSize = 2;
		this.FlatAppearance.BorderColor = Color.Black;

		// ✅ VISIBLE FOCUS INDICATOR
		this.FocusedColor = Color.Blue; // Developer must implement

		// ✅ NO HOVER-ONLY BEHAVIOR
		// (Any functionality by keyboard + click available)
	}
}
```

### Scenario 2: Displaying Medical Information

**ACCESSIBLE Pattern:**
```csharp
// ACCESSIBILITY & SOCIAL IMPACT
// WHO BENEFITS: Parent with low vision, dyslexia, or cognitive disability
// PROBLEM: Medical info overload = missed crucial details
// SOLUTION: Visual hierarchy, icons, simple language, large fonts

public class MedicationDisplayCard : UserControl
{
	public MedicationDisplayCard(Medication med)
	{
		// ✅ SEMANTIC STRUCTURE for screen readers
		var medicationSection = new GroupBox();
		medicationSection.Text = "Current Medication";
		medicationSection.AccessibleRole = AccessibleRole.Grouping;

		// ✅ MEDICATION NAME: Large, prominent
		var nameLabel = new Label();
		nameLabel.Text = med.FullName; // "Amoxicillin" not "AMX"
		nameLabel.Font = new Font("Arial", 18, FontStyle.Bold);
		nameLabel.ForeColor = Color.Black;
		nameLabel.BackColor = Color.White; // High contrast
		nameLabel.AutoSize = true;
		nameLabel.Margin = new Padding(10);

		// ✅ DOSAGE: Extra large for clarity
		var dosageLabel = new Label();
		dosageLabel.Text = $"Dosage: {med.Dosage}";
		dosageLabel.Font = new Font("Arial", 16); // Large
		dosageLabel.ForeColor = Color.DarkGreen; // + text, not color-only
		dosageLabel.AccessibleName = $"Dosage {med.Dosage}";

		// ✅ WARNINGS: VERY PROMINENT
		if (med.HasAllergies)
		{
			var warningPanel = new Panel();
			warningPanel.BackColor = Color.Red;
			warningPanel.Role = AccessibleRole.Alert; // Screen reader announces

			var warningIcon = new Label();
			warningIcon.Text = "⚠️"; // Icon + text (not icon-only)
			warningIcon.Font = new Font("Arial", 24);

			var warningText = new Label();
			warningText.Text = $"ALLERGY: {med.Allergies}";
			warningText.Font = new Font("Arial", 14, FontStyle.Bold);
			warningText.ForeColor = Color.White;
			warningText.AccessibleRole = AccessibleRole.Alert;

			warningPanel.Controls.Add(warningIcon);
			warningPanel.Controls.Add(warningText);
			medicationSection.Controls.Add(warningPanel);
		}

		// ✅ TIME: Explicit, not relative
		var timeLabel = new Label();
		var nextDoseTime = med.NextDoseTime;
		timeLabel.Text = $"Next Dose: {nextDoseTime:dddd, MMMM d, yyyy at h:mm tt}";
		// ❌ NOT "in 3 hours" (relative times confuse aging/cognitive disabilities)
		timeLabel.Font = new Font("Arial", 14);
		timeLabel.AccessibleName = $"Next dose at {nextDoseTime:h:mm tt}";

		medicationSection.Controls.Add(nameLabel);
		medicationSection.Controls.Add(dosageLabel);
		medicationSection.Controls.Add(timeLabel);

		this.Controls.Add(medicationSection);
	}
}
```

### Scenario 3: Building a Form

**ACCESSIBLE Pattern:**
```csharp
// ACCESSIBILITY & SOCIAL IMPACT
// WHO BENEFITS: Parent with dyslexia or cognitive disability filling out medical forms
// PROBLEM: Complex forms = missed/incorrect entries = safety risk
// SOLUTION: Simple fields, clear labels, helpful error messages

public class MedicationLogForm : Form
{
	public MedicationLogForm()
	{
		// ✅ FORM TITLE (for screen readers)
		this.Text = "Log Medication Dose";
		this.AccessibleName = "Medication Dose Log";
		this.AccessibleDescription = 
			"Record when medication was given and any side effects";

		// FIELD 1: Medication Selection (Not Free Text = Accessibility)
		var medLabel = new Label();
		medLabel.Text = "Which medication?";
		medLabel.Font = new Font("Arial", 12, FontStyle.Bold);

		var medDropdown = new ComboBox();
		medDropdown.Items.Add("Amoxicillin (500mg)");
		medDropdown.Items.Add("Ibuprofen (200mg)");
		medDropdown.Items.Add("Metformin (1000mg)");

		// ✅ LABEL ASSOCIATED with control (screen reader knows what this field is)
		this.Controls.Add(medLabel);
		this.Controls.Add(medDropdown);
		medDropdown.AccessibleName = medLabel.Text;
		medDropdown.AccessibleDescription = 
			"Select from pre-filled medications. Type to search.";

		// FIELD 2: Dose Time
		var timeLabel = new Label();
		timeLabel.Text = "What time was it given?";
		timeLabel.Font = new Font("Arial", 12, FontStyle.Bold);

		var timePicker = new DateTimePicker();
		timePicker.Format = DateTimePickerFormat.Custom;
		timePicker.CustomFormat = "dddd, MMMM d, yyyy h:mm tt";
		timePicker.Size = new Size(300, 30); // Visible, not tiny

		this.Controls.Add(timeLabel);
		this.Controls.Add(timePicker);
		timePicker.AccessibleName = timeLabel.Text;

		// FIELD 3: Side Effects (Simple Checkbox, not Free Text)
		var effectsLabel = new Label();
		effectsLabel.Text = "Any side effects?";
		effectsLabel.Font = new Font("Arial", 12, FontStyle.Bold);

		var nauseaCheck = new CheckBox();
		nauseaCheck.Text = "Nausea";
		nauseaCheck.Size = new Size(100, 30); // Large touch target

		var rashCheck = new CheckBox();
		rashCheck.Text = "Rash";
		rashCheck.Size = new Size(100, 30);

		this.Controls.Add(effectsLabel);
		this.Controls.Add(nauseaCheck);
		this.Controls.Add(rashCheck);

		// SUBMIT BUTTON
		var submitBtn = new Button();
		submitBtn.Text = "Log This Dose";
		submitBtn.Size = new Size(200, 50); // Large button
		submitBtn.Font = new Font("Arial", 14, FontStyle.Bold);
		submitBtn.Click += (s, e) => SubmitForm();

		// ✅ KEYBOARD SUPPORT [Enter] to submit
		this.AcceptButton = submitBtn;

		this.Controls.Add(submitBtn);

		// ✅ ERROR HANDLING (if validation fails)
		this.FormClosing += (s, e) =>
		{
			if (!ValidateForm())
			{
				e.Cancel = true;
				ShowAccessibleError("Please select a medication before saving");
			}
		};
	}

	private void ShowAccessibleError(string message)
	{
		// ✅ ERROR MESSAGE: Announced by screen reader
		var errorPanel = new Panel();
		errorPanel.BackColor = Color.Red;
		errorPanel.Role = AccessibleRole.Alert; // IMPORTANT

		var errorText = new Label();
		errorText.Text = $"⚠️ ERROR: {message}";
		errorText.Font = new Font("Arial", 12, FontStyle.Bold);
		errorText.ForeColor = Color.White;
		errorText.AutoSize = true;

		errorPanel.Controls.Add(errorText);
		this.Controls.Add(errorPanel);

		// ✅ Focus on error (user knows what went wrong)
		errorPanel.Focus();
	}
}
```

---

## 🧪 TESTING YOUR WORK

### Test 1: Keyboard Only (Takes 5 minutes)
```
1. Close your mouse driver
2. Alt+Tab to your app
3. Use Only:
   - Tab (move forward)
   - Shift+Tab (move backward)
   - Enter (activate button)
   - Space (toggle checkbox)
   - Escape (close dialog)
4. Can you complete the task?
   YES → Continue to next test
   NO → Fix before next test
```

### Test 2: Screen Reader (Takes 10 minutes)
```
Windows: Start → Settings → Accessibility → Narrator → Turn On

1. Launch NVDA or Narrator
2. Read through your form
3. Can screen reader announce:
   ✓ Field label + purpose?
   ✓ Current value?
   ✓ Error messages?
   ✓ Status updates?
   YES → Continue to next test
   NO → Fix before next test
```

### Test 3: Automated Tool (Takes 2 minutes)
```
Chrome:
1. Open DevTools (F12)
2. Lighthouse → Accessibility → Analyze page load
3. Score ≥ 90?
   YES → You're done
   NO → Fix violations, retest
```

### Test 4: Zoom (Takes 5 minutes)
```
1. Press Ctrl+Plus to 150% zoom
   Can you read everything?
   Any horizontal scrolling?
   YES (readable, no H-scroll) → Continue
   NO → Fix, retest

2. Press Ctrl+Plus again to 200% zoom
   Still readable?
   Still usable?
   YES → No issues
   NO → Fix, retest
```

---

## 🚀 BEFORE YOU PUSH TO CODE REVIEW

**Print this and check off:**

```
ACCESSIBILITY PRE-REVIEW CHECKLIST

Code Quality:
☐ I can describe the specific user this feature serves
☐ I can describe their disability/limitation
☐ I included impact comments in code
☐ No accessibility anti-patterns used

Keyboard Testing:
☐ Tab through all controls
☐ All controls reachable
☐ No keyboard trap
☐ Focus indicator visible
☐ [Enter], [Space], [Escape] work

Screen Reader Testing:
☐ Tested with NVDA or Narrator
☐ All labels announced
☐ All button purposes announced
☐ Errors announced

Visual Testing:
☐ Tested at 150% and 200% zoom
☐ Tested in high contrast mode
☐ Tested with color blindness filter
☐ Font sizes ≥ 12px

Automated Testing:
☐ Axe scan: Zero high violations
☐ WAVE scan: Zero errors
☐ Lighthouse accessibility: ≥ 90

Unit Tests:
☐ Accessibility requirements in tests
☐ Tests verify keyboard paths
☐ Tests verify screen reader elements

Documentation:
☐ Commit message mentions accessibility feature
☐ PR description includes "WHO BENEFITS" section

I believe a user with a disability can actually use this.
Signature: _____________ Date: _______
```

---

## 📞 GETTING HELP

**If you're unsure:**
- Ask: "Is this accessible to someone using only keyboard?"
- Ask: "Can a screen reader announce this?"
- Ask: "Who will this help? Can you name them?"

**If a user reports an accessibility issue:**
- It is NOT "nice to have later"
- It is a DEFECT blocking that user's work
- Treat with same priority as security bug

---

## 📚 QUICK REFERENCE

| User Need | Your Solution |
|-----------|---------------|
| Cannot use mouse | Tab navigation, keyboard shortcuts |
| Cannot see | Screen reader labels, alt text, text descriptions |
| Cannot see small text | Large fonts (≥12px), zoom support, high contrast |
| Cannot read color | Text labels + icons, text descriptions |
| Cannot read quickly | Simple language, visual hierarchy, icons |
| Cannot stay focused | Clear purpose, simple tasks, progress indicators |
| Cannot use hands | Voice control, switch control, eye tracking |
| Cannot use phone/tablet | Desktop alternative, larger buttons |
| Cannot remember steps | Clear navigation, saved progress, help text |

---

**Remember: Accessibility is not a feature. It's a requirement.**

**Every line of TubieTools code should help someone who needs help.**
