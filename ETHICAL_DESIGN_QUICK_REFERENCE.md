# ETHICAL DESIGN QUICK REFERENCE

**Print this. Keep it visible. Use before every feature.**

---

## ⚡ THE TWO QUESTIONS

Before ANY feature is designed or coded:

### Question 1: "Who does this help?"
```
✅ GOOD ANSWERS:
- "Parent forgets when to give medication"
- "Caregiver needs to coordinate with 3 other people"
- "Child's seizure patterns confuse doctors"
- "Elderly parent can't remember dosages"
- "Blind caregiver needs to know when to give meds"

❌ BAD ANSWERS:
- "Increases daily active users"
- "Gets people to spend more time in app"
- "Creates habit loop"
- "Goes viral"
- "Maximizes engagement"

If you can't answer Question 1 with a real user problem:
→ DON'T BUILD IT
```

### Question 2: "Who does this hurt?"
```
✅ GOOD ANSWERS:
- "No one. This just helps them complete their task."
- "Actually protects vulnerable users (children, elderly)"

❌ BAD ANSWERS:
- "Their mental health (but not much, right?)"
- "They might feel obligated, but that's okay"
- "It exploits habit loops, but that's industry standard"
- "Children might get addicted, but we limit notifications"
- "We harvest data, but it's anonymous"

If any answer is "but that's okay" or "industry standard":
→ DON'T BUILD IT
```

---

## 🚫 DARK PATTERNS CHEAT SHEET

**These are prohibited. Full stop. No exceptions.**

| Pattern | What It Does | Why It's Evil | TubieTools Alternative |
|---------|--------------|--------------|------------------------|
| **Infinite Scroll** | Never-ending feed | Makes you keep scrolling | Fixed time frame (last 30 days) |
| **Streaks** | "142 days! Don't break it!" | Exploits guilt if you miss | Just show history, no pressure |
| **Auto-Play** | Next video starts automatically | Traps you for hours | User clicks to play each video |
| **Notifications** | Constant "come back!" alerts | Interrupts life for profit | Only send: Medical + requested |
| **Badge Numbers** | Red "23" unread items | Creates anxiety | Remove badges, let user control |
| **FOMO** | "Everyone's doing this" | Exploits fear | Never compare users |
| **Streaks** | "Don't lose your streak!" | Converts care to obligation | No streaks, ever |
| **Social Compare** | "Better than 73% of users" | Shameful | Never rank users |
| **Gamification** | Badges, points, levels | Addiction mechanics | No game elements in medical app |
| **Artificial Scarcity** | "Only 3 spots left!" | Fake urgency | Never use on digital goods |
| **Time Pressure** | "Sale ends in 2 hours!" | Exploitation | No countdowns except real deadlines |
| **Share to Unlock** | "Invite friends to get feature" | Viral manipulation | No sharing incentives |
| **Sticky Defaults** | Hard to turn off | Requires work to protect self | Easy opt-in, easy opt-out |
| **Dark Patterns** | Hard to delete account | Trap users | Delete in 1 click, instant |

**If you recognize ANY pattern in your code:**
→ REMOVE IT before review
→ REGENERATE without it
→ NO EXCEPTIONS

---

## ✅ ETHICAL PATTERNS CHEAT SHEET

**Build these instead.**

| Pattern | What It Does | Why It Helps | Example |
|---------|--------------|-------------|---------|
| **Clear Purpose** | Feature solves 1 problem | User understands why it exists | "Log medication doses" |
| **Fast Completion** | Task takes 2-5 minutes | Caregiver saves time | Confirm med in 45 seconds |
| **Natural Exit** | Feature ends, user leaves | Doesn't trap you | Task done → Close app |
| **User Control** | You decide everything | Feels empowering | User sets own notification times |
| **Transparency** | You know ALL data collected | Builds trust | "We collect: medication + time only" |
| **Easy Delete** | Kill account in 2 clicks | You own your data | "Delete account" → Confirm → Done |
| **Privacy Default** | OFF by default, user opts IN | Protects you | "Share with doctor? [Ask each time]" |
| **Honest Metrics** | Measure impact, not engagement | Shows real success | "You saved 2 hours this week" |
| **Respects Time** | No manipulation | Doesn't interrupt unnecessarily | Notification: Only when med due |
| **Protects Vulnerable** | Extra care for kids/elderly | Actively safe | No gamification for children |

---

## 🎯 THE SOCIETAL BETTERMENT MATRIX (Quick Version)

**Rate every feature 1-5 on each dimension:**

```
1. WELLBEING
   5 = Reduces stress/burden/anxiety
   3 = Neutral/helps a bit
   1 = Creates burden

2. SOCIETY
   5 = Enables people to help each other
   3 = No negative impact
   1 = Harms society

3. AUTONOMY
   5 = User fully in control
   3 = Some control, easy to change
   1 = User trapped

4. HONESTY
   5 = Completely transparent
   3 = Generally clear
   1 = Deceptive/hidden

5. VULNERABILITY
   5 = Protects vulnerable users
   3 = No extra risk
   1 = Targets vulnerable for exploitation

TOTAL SCORE: ____ / 25

✅ BUILD IF: ≥ 15
⚠️  REDESIGN IF: 12-14
❌ REJECT IF: < 12
```

**Score BEFORE design. If < 15, don't proceed.**

---

## 🚨 RED FLAGS (Will Be Rejected)

Your feature WILL be rejected if ANY of these:

❌ Designed to maximize engagement/session time  
❌ Contains infinite scroll/autoplay/streaks  
❌ Has engagement-based notifications  
❌ Uses FOMO, scarcity, or artificial urgency  
❌ Gamifies care (badges for remembering meds)  
❌ Compares users publicly  
❌ Targets children for engagement  
❌ Matrix score < 15  
❌ Hard to delete account/data  
❌ Hides how data is used  
❌ "Dark pattern" but "industry standard"  
❌ "We'll fix accessibility in v2.0"  
❌ "Let's A/B test if exploitation works"  

---

## ✅ THE STANDARD WORKFLOW

### 1. FEATURE REQUEST COMES IN
```
Manager: "Build a medication reminder system"
```

### 2. YOU ASK THREE QUESTIONS
```
Q1: Who specifically will use this?
A: "Parents managing children's medications"

Q2: What problem does it solve?
A: "Parents forget dosages, forget timing, don't track side effects"

Q3: What's the danger of exploitation?
A: "Could add streaks/notifications/social comparison to boost engagement"
```

### 3. YOU FILL OUT MATRIX
```
Wellbeing: 5 (reduces stress)
Society: 5 (keeps child healthy)
Autonomy: 5 (parent controls)
Honesty: 5 (transparent)
Vulnerability: 5 (protects child)

TOTAL: 25 ✅ BUILD
```

### 4. YOU DESIGN (ETHICALLY)
```
What it does:
- Parent enters med name + time
- Click confirm when given
- History available by date
- Can send to doctor

What it DOESN'T do:
- No streak counter
- No notifications unless parent sets
- No social sharing
- No gamification
- No comparisons
```

### 5. YOU CODE (ETHICALLY)
```csharp
// ETHICAL DESIGN: Timer notification ONLY when med is actually due
// (Not for engagement)
// 
// ANTI-PATTERN AVOIDED: 
// Could add daily "open app" notification = engagement tactic
// NOT DOING THIS because: Exploits parent, violates autonomy
//
// RESULT: User opens app when they need medical help, naturally
```

### 6. YOU TEST (ETHICALLY)
```
Test 1: User completes task
- Parent logs medication
- Feels accomplished
- Isn't tempted to stay
- Exits satisfied
Result: ✅ PASS

Test 2: No dark patterns
- grep -r "streak\|reward\|badge\|fomo\|urgency"
- Zero results
Result: ✅ PASS

Test 3: Accessibility
- Works with keyboard only
- Screen reader friendly
- Mobile-friendly
Result: ✅ PASS
```

### 7. CODE REVIEW
```
Reviewer checks:
☐ Purpose is beneficial (solved a problem)
☐ Matrix score ≥ 15
☐ No dark patterns in code
☐ Ethical comments in code
☐ Tests verify ethical behavior
☐ Accessible

All pass? → MERGE ✅
Any fail? → Request changes
```

---

## 💬 WHAT TO SAY (WHEN TEMPTED)

**When someone suggests an engagement tactic:**

"Can we add streaks? It would boost engagement."

✅ What to say:
```
"No. Streaks exploit guilt/obligation. Parents managing medical care 
shouldn't feel pressured by gamification. A parent who forgets a dose 
doesn't need shame; they need support.

Instead: Show their history/progress without pressure.
That solves the problem ethically."
```

"Let's add notifications to bring users back..."

✅ What to say:
```
"No. Notifications should only be for medical necessity or what 
the user explicitly requested. Using notifications for engagement 
interrupts caregivers' actual lives and erodes trust.

Instead: Let parents set their own reminder times.
They'll open the app when they need medical help (organic return)."
```

"Just one tiny engagement metric... it's industry standard..."

✅ What to say:
```
"We're not industry standard. We're TubieTools. Our standard is 
ethical. 'Everyone else exploits users' isn't a reason to exploit them.

We measure impact, not engagement.
We build for wellbeing, not addiction.
That's who we are."
```

---

## 📞 WHEN IN DOUBT

**Ask these questions:**

"Would our users be proud if they knew this feature existed?"  
→ If NO: Don't build it

"Is this optimized for user wellbeing or for our metrics?"  
→ If "our metrics": Redesign

"Would we use this exploitation pattern if it was our child?"  
→ If NO: Don't do it to other people's children

"Does this feature profit from users' time/attention/vulnerability?"  
→ If YES: Remove it

"Would this feature exist if we only measured real impact?"  
→ If NO: It's engagement-engineered

---

## 📚 FULL STANDARDS

For complete details:

- **TUBIE_TOOLS_ETHICAL_DESIGN_AND_ANTI_ADDICTION_STANDARD.md** - Full standard
- **APPLYING_ETHICAL_DESIGN_IN_DAILY_WORK.md** - Implementation guide
- **ENFORCEMENT_STANDARDS_MASTER_REGISTRY.md** - All standards together

---

## ✍️ ACKNOWLEDGMENT

I understand:
- [ ] Zero addictive design patterns, ever
- [ ] Ethical design is not negotiable
- [ ] Users are allies, not products
- [ ] Caregiver wellbeing > engagement metrics
- [ ] Every feature must pass societal betterment matrix
- [ ] I will refuse to build exploitative features
- [ ] I will call out dark patterns if I see them

Signature: _________________________ Date: _______

**This is TubieTools. We do better.**
