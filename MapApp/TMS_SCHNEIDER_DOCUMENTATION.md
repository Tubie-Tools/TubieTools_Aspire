# Schneider International TMS Enhancement
## Transport Management System Upgrade

### Executive Summary

A comprehensive upgrade to Schneider International's Transport Management System adding:
- **Real-Time Processing** - Accidents, weather, construction delays
- **Batch Processing** - End-of-day reconciliation and code-to-cash validation
- **Just-In-Time (JIT)** - Immediate urgent shipment assignment
- **Fuel & Cost Tracking** - Real-time fuel prices and efficiency metrics
- **Billing & Revenue** - Complete code-to-cash pipeline with regulatory compliance

---

## System Architecture

### Three Processing Layers

#### 1. Real-Time Processing (Sub-Second)
```
Event Reported → Immediate Processing → Route Recalculation → Customer Notification
	↓
  Accidents, Weather, Construction
  Fuel Stops, HOS Violations
  Delay < 60 seconds response target
```

#### 2. Just-In-Time (JIT) Processing (Minutes)
```
Urgent Shipment Received → Availability Check → Truck Assignment → ETA Calculation
	↓
  Field dispatch for immediate service
  Urgent/emergency shipments
  Response < 5 minutes
```

#### 3. Batch Processing (End-of-Day)
```
Daily Reconciliation → Validation → Billing → Compliance Audit → Reporting
	↓
  All shipments consolidated
  Bill generation (1000s per day)
  Response < 30 minutes (2AM-2:30AM window)
```

---

## Industry Standards & Practices Implemented

### 1. Hours of Service (HOS) Compliance - DOT Regulation

**What We Track:**
- 70-hour weekly limit
- 10-hour rest period required
- 11-hour driving limit per day
- Mandatory breaks every 11 hours (sleeper berth exception: 10 hours)

**Implementation:**
```csharp
// Driver HOS check
if (driver.HoursWorkedThisWeek >= 70)
	throw new Exception("Weekly HOS limit exceeded");

if (hoursWorkedToday > 11)
	throw new Exception("Daily limit exceeded - mandatory rest required");
```

### 2. Electronic Logging Device (ELD) Mandate

**What We Track:**
- All trips >100 miles require ELD recording
- Real-time position tracking via GPS
- Automatic compliance alerts

**Implementation:**
```csharp
if (!shipment.ELDRecorded && shipment.ActualDistanceMiles > 100)
	billingValidation.Issues.Add("Missing ELD for >100 mile trip");
```

### 3. Fuel Surcharge Indexing (FSI)

**Industry Standard:**
- Base fuel cost: $2.50/gallon
- FSI adjustment: +/- 6% per $0.01 variance
- Tuesday EIA reporting (national baseline)

**Implementation:**
```csharp
// National fuel index
var priceVariance = currentPrice - basePrice; // $2.50
var surchargePercentage = priceVariance * 0.06; 
var surcharge = baseSurcharge * (1 + surchargePercentage);
```

### 4. Revenue Recognition (ASC 606)

**Accounting Standard:**
- Revenue recognized upon service completion (delivery)
- Performance obligation: Safe transport to destination
- Not recognized on pickup or en route

**Implementation:**
```csharp
// Revenue only recognized after delivery
if (shipment.ActualDeliveryTime.HasValue)
{
	revenueRecognition.RevenueAmount = record.TotalInvoiceAmount;
	revenueRecognition.RecognitionDate = shipment.ActualDeliveryTime.Value;
}
```

### 5. Accident Response Protocol

**Framework:**
- Immediate escalation (HQ notification <5 min)
- Tow service coordination
- Driver welfare check (mandatory)
- Regulatory reporting (DOT if injury/hazmat)
- Cost allocation: 50% driver premium, 50% emergency handling fee

### 6. Vehicle Utilization Targets

**Industry Benchmarks:**
- Target MPG: 6.5 (varies ±1.0 by terrain)
- Fuel cost per mile: $0.52-$0.60
- Maximum 14-hour duty window per 24-hour period
- Minimum 34-hour weekly reset (resets 70-hour clock)

---

## Real-Time Processing Examples

### Example 1: Weather Delay Event

```
INPUT: Weather alert - Rain in Arizona
RESPONSE TIME: <2 seconds

1. System receives: WeatherDelay at lat/lon
2. Find impacted shipments within 50-mile radius
3. Calculate delay impact (60 min estimated)
4. Check HOS compliance:
   - If driver HOS  remaining < 7 hours → EXCEPTION
   - If delay + new ETA > delivery window → Customer notification
5. Evaluate re-route options:
   - Compare delay cost ($2/min = $120) vs routing cost
   - Reroute if savings > $150
6. Update customer with:
   - New estimated delivery (8:30 PM → 9:30 PM)
   - Delay reason
   - Tracking changes URL
7. Broadcast to mobile app in real-time via WebSocket
```

**Cost Calculation:**
```
Unmitigated delay cost = 60 min × $2/min = $120
Re-routing overhead = $45
Net savings if re-routed: $120 - $45 = $75
Auto-reroute if savings > threshold
```

### Example 2: Accident Event (CRITICAL)

```
INPUT: Accident reported - I-40 near Flagstaff
RESPONSE TIME: <10 seconds

1. IMMEDIATE: Alert compliance team + dispatcher + customer
2. Verify driver welfare (must confirm <5 min)
3. Contact DOT for hazmat/injury reporting if applicable
4. Assign tow service from preferred network
5. For shipment:
   - Change status to EXCEPTION
   - 50% emergency surcharge applied
   - 120-minute estimated delay
   - HOS violation risk assessed
6. Reroute remaining load to nearest available truck
7. Put original driver on break (HOS-compliant)
8. Customer notifications sent via:
   - SMS (immediate)
   - Email (detailed)
   - Phone call (C-level customers)
9. Insurance incident number assigned
```

**Billing Impact:**
```
Original shipment: $1,200 revenue
Emergency surcharge: +$600 (50%)
Revised reroute: $1,400 (smaller load)
Tow/recovery cost: -$500
Net insurance claim: $500
Customer extra charge: $150

Net revenue impact: $1,200 + $150 = $1,350 (vs $1,200)
```

### Example 3: Construction Zone Delay

```
INPUT: Construction alert - I-95 North Carolina
RESPONSE TIME: <5 seconds

1. Identify affected shipments:
   - Within 75 miles of construction
   - Scheduled next 24 hours
   - Estimated impact: 30-45 minutes

2. For each affected shipment:
   - Calculate re-route cost
   - Check driver HOS impact
   - Assess customer penalties:
	 * Late delivery penalty
	 * Customer committed time vs. actual
	 * Service level vs. paid (Ground vs Express)

3. Decision logic:
   - If delay < 30 min: Accept delay, eat time
   - If delay 30-60 min: Re-route if cost < $75
   - If delay > 60 min: Mandatory re-route + customer credit

4. Proactive customer communication:
   - Notify 24 hours before if possible
   - New ETA within 5-minute window
   - Offer upgrade to next-day if available
```

---

## Batch Processing (End-of-Day)

### Daily Reconciliation Flow (2:00 AM - 2:30 AM)

```
START: 2:00 AM Daily Batch Process
│
├─ STAGE 1: Complete In-Transit Shipments
│  ├─ Query: status = InTransit, actualDeliveryTime IS NOT NULL
│  ├─ Action: Set status = Delivered, timestamp completion
│  ├─ Result: Typically 100-500 shipments depending on business
│  └─ Time: ~15 seconds
│
├─ STAGE 2: Validate Code-to-Cash Accuracy (CRITICAL)
│  ├─ For each delivered shipment (past 24h):
│  │  ├─ Check distance variance: actual vs planned < 20%
│  │  ├─ Verify ELD recording for all >100 mile trips
│  │  ├─ Validate billing amounts > 0
│  │  ├─ Check fuel surcharge alignment (±10% tolerance)
│  │  ├─ Verify delivery time window < 3 hours late
│  │  └─ Flag any anomalies
│  │
│  ├─ Results:
│  │  ├─ 1,247 shipments validated
│  │  ├─ 1,208 passed ✓
│  │  ├─ 39 issues flagged:
│  │  │  ├─ 15 distance variance  
│  │  │  ├─ 12 missing ELD
│  │  │  ├─ 8 fuel surcharge variance
│  │  │  └─ 4 late delivery
│  │  └─ Time: ~2 minutes
│  │
│  └─ Action: Generate detailed report for finance review
│
├─ STAGE 3: Generate Billing Records
│  ├─ For all delivered shipments:
│  │  ├─ Calculate linehaul: distance × rate_per_mile
│  │  ├─ Apply fuel surcharge based on fuel index
│  │  ├─ Add accessorials: tolls, detention, etc.
│  │  ├─ Calculate tax (varies by state)
│  │  └─ Create invoice
│  │
│  ├─ Results:
│  │  ├─ 1,208 valid billing records created
│  │  ├─ Total revenue: $3.2M
│  │  ├─ Range: $120 - $18,500 per shipment
│  │  └─ Time: ~90 seconds
│  │
│  ├─ Validation checks:
│  │  ├─ All linehaul > 0
│  │  ├─ Tax calculated correctly
│  │  ├─ Invoice totals accurate
│  │  └─ Customer info complete
│  │
│  └─ Status: Ready for Oracle EBS invoice generation
│
├─ STAGE 4: Calculate Performance Metrics
│  ├─ For entire day:
│  │  ├─ On-time delivery % (within 15 min scheduled time)
│  │  ├─ Fleet fuel economy vs benchmark
│  │  ├─ Driver utilization rates
│  │  ├─ Equipment utilization rates
│  │  ├─ Revenue per mile
│  │  ├─ Cost per mile
│  │  └─ Profit margin by lane/region
│  │
│  ├─ Results:
│  │  ├─ On-time: 92.3% (target: >95%)
│  │  ├─ Avg MPG: 6.48 (benchmark: 6.50)
│  │  ├─ Avg revenue/mile: $2.65 (plan: $2.60)
│  │  ├─ Avg cost/mile: $2.12 (plan: $2.15)
│  │  └─ Profit margin: 15.8% (plan: 15.5%) ✓
│  │
│  └─ Time: ~60 seconds
│
├─ STAGE 5: Compliance Audit
│  ├─ For each shipment:
│  │  ├─ Check HOS compliance: hours used ≤ hours available
│  │  ├─ Verify ELD requirement for miles > 100
│  │  ├─ Check delivery window compliance
│  │  ├─ Hazmat documentation if applicable
│  │  └─ Driver qualification status
│  │
│  ├─ Results:
│  │  ├─ Total shipments audited: 1,247
│  │  ├─ Compliant: 1,218 ✓
│  │  ├─ Issues found: 29
│  │  │  ├─ 8 HOS violations (drivers will be rested next run)
│  │  │  ├─ 12 missing ELD records (escalated for correction)
│  │  │  ├─ 6 late deliveries >3 hours
│  │  │  └─ 3 documentation missing
│  │  │
│  │  ├─ Potential regulatory fines if not corrected:
│  │  │  ├─ HOS violation: $100-$500 per incident
│  │  │  ├─ Missing ELD: $100-$300 per incident
│  │  │  └─ Total potential exposure: $8,500
│  │  │
│  │  └─ Action: Notify drivers + management for corrective training
│  │
│  └─ Time: ~45 seconds
│
└─ END: 2:28 AM (28 minute total execution)
   └─ Status: SUCCESS
	   ├─ 1,208 shipments processed
	   ├─ $3.2M revenue recognized
	   ├─ 39 issues flagged for review
	   ├─ 29 compliance issues for corrective action
	   └─ Ready for next business day

REPORTING OUTPUT:
├─ Daily Performance Dashboard (automated share to executives)
├─ Finance: Revenue recognition file (to Oracle EBS)
├─ Legal/Compliance: Audit trail (DOT ready)
├─ Operations: Performance report (to dispatchers)
└─ Exceptions: Issues requiring manual review (to managers)
```

---

## Just-In-Time (JIT) Processing

### Urgent Shipment Assignment (Target: <5 minutes)

```
SCENARIO: "Need to pickup urgent medical supplies now - 
must reach Denver by 6 PM tonight"

1. CREATE SHIPMENT
   ├─ Origin: Phoenix  
   ├─ Destination: Denver
   ├─ Current time: 10:00 AM
   ├─ Deadline: 6:00 PM (8 hours)
   ├─ Distance: 600 miles
   └─ Required avg speed: 600 / 8 = 75 MPH (legal limit!)

2. TRIGGER JIT ASSIGNMENT
   ├─ Request: Assign urgent shipment
   ├─ Urgency level: CRITICAL (< 2 hours is critical)
   └─ Response time: <30 seconds

3. TRUCK AVAILABILITY CHECK
   ├─ Search available trucks in Phoenix area
   ├─ Results found: 7 trucks
   │  ├─ Truck #1: At Phoenix terminal, full fuel, no cargo → BEST
   │  ├─ Truck #2: 45 miles north, 50% fuel
   │  ├─ Truck #3: At maintenance (unavailable)
   │  ├─ Truck #4: En route to Flagstaff (bad direction)
   │  ├─ Truck #5: At fuel stop (available in 15 minutes)
   │  ├─ Truck #6: Driver taking break (can depart in 30 min)
   │  └─ Truck #7: Load committed, won't empty until 2 PM
   │
   ├─ Scoring criteria:
   │  ├─ Distance from pickup location
   │  ├─ Fuel level (can reach without fuel stop?)
   │  ├─ Current load (can unload quickly?)
   │  ├─ Driver HOS availability
   │  └─ Equipment condition
   │
   └─ SELECTED: Truck #1 (Score: 98/100)

4. DRIVER AVAILABILITY CHECK
   ├─ Driver: John Smith
   ├─ Current status: Off duty at terminal
   ├─ HOS analysis:
   │  ├─ Hours worked this week: 45/70 ✓
   │  ├─ Hours worked today: 0/11 ✓
   │  ├─ Last break: Yesterday 6 PM (16 hours ago) ✓
   │  ├─ Can drive: Up to 11 hours today ✓
   │  ├─ Trip requires: 8.5 hours (with traffic) ✓
   │  └─ HOS status: COMPLIANT ✓
   │
   ├─ Pay status:
   │  ├─ Base rate: $1.88/mile = $1,128 (base)
   │  ├─ Urgency premium: +25% = +$282
   │  ├─ Early pickup bonus: +$50
   │  └─ Total driver pay: $1,460
   │
   └─ Driver confirmed ready

5. ASSIGNMENT CONFIRMATION (T+3 min from request)
   ├─ Truck: #1 (Phoenix - Denver)
   ├─ Driver: John Smith
   ├─ Estimated pickup: 10:15 AM
   ├─ Estimated delivery: 5:45 PM ✓ (WITHIN deadline!)
   ├─ Revenue:
   │  ├─ Base linehaul: $1,500
   │  ├─ Urgency premium: +$375 (25%)
   │  └─ Total customer charge: $1,875
   │
   └─ CONFIRMATION SENT to customer

6. REAL-TIME MONITORING (Continuous)
   ├─ GPS tracking every 2 minutes
   ├─ ETA recalculation at each major segment
   ├─ Customer gets SMS updates at:
   │  ├─ Load confirmation (10:15 AM)
   │  ├─ Departure from Phoenix (10:20 AM)
   │  ├─ Midpoint update Flagstaff (1:00 PM)
   │  └─ Pre-delivery notice 30 min before
   │
   ├─ If issues arise:
   │  ├─ Traffic adds 15 min → ETA 6:00 PM (no longer compliant!)
   │  ├─ System alerts dispatcher
   │  ├─ Evaluate options:
   │  │  ├─ Option A: Continue, explain delay to customer
   │  │  ├─ Option B: Transfer cargo to faster truck (if available)
   │  │  ├─ Option C: Offer air freight (expensive but guaranteed)
   │  │  └─ Selected: Option A + $200 late delivery credit
   │  │
   │  └─ Customer notified with resolution within 5 minutes
   │
   └─ Successful delivery: 5:58 PM ✓

7. POST-DELIVERY PROCESSING
   ├─ Status: Delivered on time (2 minutes early!)
   ├─ ELD: Auto-recorded, compliant ✓
   ├─ Revenue recognition: Immediate (upon delivery)
   ├─ Billing generated automatically
   ├─ Customer satisfaction: 5 stars (on-time + communication)
   └─ Entire transaction: Code → Cash ready next day

RESULTS:
├─ Assignment time: 3 minutes (target met!)
├─ Service level: Premium (urgent)
├─ Customer satisfaction: Exceeded expectations
├─ System decision: Fully automated, human-in-loop for escalations
└─ Revenue captured: $1,875 + $200 credit = $2,075
	(vs. standard $1,500 = 38% premium)
```

---

## Code-to-Cash Process (Financial Controls)

### The Complete Revenue Cycle

```
SHIPMENT CREATED → PICKED UP → IN-TRANSIT → DELIVERED → BILLED → PAID

Step 1: ORDER/SHIPMENT CREATED
   ├─ Entry point: Sales/Customer Service
   ├─ Data required:
   │  ├─ Customer account verified
   │  ├─ Origin/destination confirmed
   │  ├─ Weight/dimensions validated
   │  ├─ Hazmat flag if applicable
   │  ├─ Delivery instructions documented
   │  └─ Base rate approved
   │
   ├─ System creates rate calculation:
   │  ├─ Distance: 523 miles
   │  ├─ Base rate: $2.50/mile
   │  ├─ Linehaul: $1,307.50
   │  ├─ Estimated fuel: $135 (based on fuel index)
   │  └─ Preliminary revenue: $1,442.50
   │
   └─ Status: PENDING ASSIGNMENT

Step 2: TRUCK ASSIGNED & PICKED UP
   ├─ Truck selected based on capacity/route
   ├─ Driver confirms under DOT medical certification
   ├─ ELD started (logs available)
   ├─ Actual pickup time recorded with GPS
   └─ Status: IN-TRANSIT

Step 3: MONITORING & EVENT TRACKING
   ├─ Real-time events recorded:
   │  ├─ Departure from origin
   │  ├─ Fuel stops with cost tracking
   │  ├─ Rest breaks (HOS compliance)
   │  ├─ Any delays/issues with impact analysis
   │  └─ Actual distance traveled (odometer + GPS)
   │
   └─ Adjustments made if:
	   ├─ Route changed (weather/traffic)
	   ├─ Service level altered
	   └─ Additional services rendered

Step 4: DELIVERY
   ├─ Proof of delivery captured:
   │  ├─ Electronic signature from receiver
   │  ├─ Timestamp of delivery
   │  ├─ Condition verification
   │  └─ Photos (if hazmat/value shipment)
   │
   ├─ Actual details captured:
   │  ├─ Delivery address confirmed
   │  ├─ Recipient name/title recorded
   │  ├─ Actual distance: 521 miles (variance: -0.4% ✓)
   │  ├─ Actual duration: 8h 23m (vs 8h planned)
   │  └─ Fuel consumed: 80 gallons (real-time from truck telemetry)
   │
   └─ Status: DELIVERED (revenue recognition point for ASC 606)

Step 5: BILLING CALCULATION
   ├─ Review actual vs. estimate:
   │  ├─ Distance: 521 mi actual vs 523 planned → adjust
   │  ├─ Fuel: 80 gal @ $3.45 = $276 actual cost
   │  ├─ Fuel surcharge: 521 × fuel index surcharge
   │  └─ Additional: Any toll receipts, detention time
   │
   ├─ Final invoice calculation:
   │  ├─ Linehaul: 521 mi × $2.50 = $1,302.50
   │  ├─ Fuel surcharge: (based on index) = $134.00
   │  ├─ Tolls: $17.50 (actual)
   │  ├─ Subtotal: $1,454.00
   │  ├─ Tax (state): $116.32 (8%)
   │  └─ TOTAL INVOICE: $1,570.32
   │
   ├─ Validation matrix (pre-billing):
   │  ├─ ✓ Distance variance acceptable
   │  ├─ ✓ ELD properly recorded
   │  ├─ ✓ Bill amount reasonable
   │  ├─ ✓ Fuel surcharge accurate
   │  ├─ ✓ Delivery window met
   │  ├─ ✓ Driver compliant
   │  └─ ✓ All documentation complete
   │
   ├─ Invoice generation:
   │  ├─ Invoice #: INV-20240115-A7F3
   │  ├─ Terms: Net 30
   │  ├─ Due date: Feb 14, 2024
   │  └─ Auto-sent to: customer@company.com, AP@company.com
   │
   └─ Status: INVOICED

Step 6: REVENUE RECOGNITION (Accounting)
   ├─ Journal entry recorded:
   │  ├─ DR: Accounts Receivable $1,570.32
   │  ├─ CR: Revenue (transportation services) $1,570.32
   │  └─ Date: Delivery date (ASC 606 compliance)
   │
   ├─ Impact on financials:
   │  ├─ Revenue: +$1,570.32
   │  ├─ Cost of service: -$ 856.00 (fuel $276 + labor $580)
   │  ├─ Gross profit: $714.32 (45.5% margin)
   │  └─ Entry reflects in next day's books
   │
   └─ Status: REVENUE RECOGNIZED

Step 7: COLLECTION (Accounts Receivable)
   ├─ Customer payment options:
   │  ├─ ACH transfer (2-3 days)
   │  ├─ Credit card (next day, 2.5% fee)
   │  ├─ Check (5-7 days)
   │  └─ EDI/Lockbox (automated)
   │
   ├─ Payment received: Feb 2, 2024 (12 days early!)
   │  ├─ Improvement in DSO (Days Sales Outstanding)
   │  ├─ Cash flow impact: +$1,570.32 on balance sheet
   │  └─ Payment applied automatically to invoice
   │
   ├─ Reconciliation check:
   │  ├─ ✓ Full payment received
   │  ├─ ✓ Matches invoice amount
   │  ├─ ✓ No disputes
   │  └─ ✓ Customer satisfaction verified
   │
   └─ Status: PAID

Step 8: CASH COMPLETION
   ├─ Outstanding receivable: $0
   ├─ Payment confirmation sent
   ├─ Financial statement impact:
   │  ├─ Accounts Receivable: -$1,570.32
   │  ├─ Cash: +$1,570.32
   │  ├─ Revenue: Unchanged (recognized at delivery)
   │  └─ DSO metric: 12 days (excellent vs. 30 day terms)
   │
   ├─ Accounting closure:
   │  ├─ Invoice marked PAID
   │  ├─ No further follow-up needed
   │  ├─ Customer relationship: Excellent
   │  └─ Eligible for repeat business
   │
   └─ Status: COMPLETE (Code-to-Cash cycle closed)

ENTIRE CYCLE SUMMARY:
├─ Business days: 17 (Jan 15 order → Feb 2 payment)
├─ Service delivery: 1 day
├─ Billing cycle: 1 day
├─ Collection: 12 days (vs 30-day terms)
│
├─ Financial impact:
│  ├─ Revenue generated: $1,570.32
│  ├─ Gross profit: $714.32 (45.5%)
│  ├─ Cash realized: $1,570.32
│  └─ ROI on assets deployed: Excellent
│
└─ Control points verified:
	├─ 7 different system validations
	├─ Zero errors/disputes
	├─ Full regulatory compliance
	├─ Customer satisfaction confirmed
	└─ Ready to replicate 1000s of times daily
```

---

## Regulatory & Compliance Features

### DOT Compliance Tracking

1. **Hours of Service (HOS)**
   - Automatic calculation
   - Mandatory break enforcement
   - Sleeper berth provisions
   - Weekly reset tracking

2. **Electronic Logging Device (ELD)**
   - Telematics integration
   - Automatic recordings >100 miles
   - Tamper detection
   - Audit trail

3. **Driver Qualification File (DQF)**
   - Medical certification tracking
   - License expiration alerts
   - Background check status
   - Training compliance

4. **Vehicle Inspection Reports**
   - DVIR tracking
   - Maintenance schedules
   - Safety defect documentation
   - Corrective action follow-up

---

## Key Metrics & KPIs

### Operational
- On-time delivery %
- Average fleet fuel economy
- Load factor %
- Driver utilization %

### Financial (Code-to-Cash)
- Days Sales Outstanding (DSO)
- Collection rate %
- Invoice accuracy %
- Revenue per mile

### Compliance
- HOS violations
- ELD exceptions
- Safety incidents
- Regulatory fines

---

## Next Steps for Schneider Implementation

1. **Data Migration** - Import current TMS data
2. **API Integration** - Connect to existing systems
3. **Testing** - Validation with sample shipments
4. **Rollout** - Phased deployment by region
5. **Training** - Operations, billing, compliance teams
6. **Monitoring** - KPI dashboards and reporting

---

*This document provides the technical blueprint for upgrading Schneider International's TMS with real-time, batch, and JIT processing capabilities while maintaining full regulatory compliance and financial control.*
