# Multiple Quotations Feature - Implementation Guide

## Overview
The system now supports **multiple quotations** for a single repair order, enabling flexible pricing options and better customer service.

---

## What Changed?

### Before (Single Quotation)
- ❌ Only one quotation allowed per repair order
- ❌ Error thrown if trying to create second quotation
- ❌ Had to update existing quotation instead of creating alternatives

### After (Multiple Quotations)
- ✅ Unlimited quotations per repair order
- ✅ Create alternative quotes with different service/part combinations
- ✅ Each quotation can be independently approved/rejected
- ✅ Multiple approved quotations can create jobs

---

## Code Changes

### Removed Constraint in `QuotationService.cs`

**Before:**
```csharp
if (createQuotationDto.RepairOrderId.HasValue)
{
    // Check if a quotation already exists for this repair order
    var existingQuotations = await _quotationRepository.GetByRepairOrderIdAsync(createQuotationDto.RepairOrderId.Value);
    if (existingQuotations != null && existingQuotations.Any())
    {
        throw new InvalidOperationException($"A quotation already exists for repair order {createQuotationDto.RepairOrderId.Value}. Please update the existing quotation instead.");
    }
    // ...
}
```

**After:**
```csharp
if (createQuotationDto.RepairOrderId.HasValue)
{
    // Multiple quotations are now allowed for the same repair order
    // This enables creating alternative quotes with different service/part combinations
    
    var repairOrder = await _repairOrderRepository.GetByIdAsync(createQuotationDto.RepairOrderId.Value);
    if (repairOrder != null)
    {
        userId = repairOrder.UserId;
        vehicleId = repairOrder.VehicleId;
    }
}
```

---

## Use Cases

### 1. Budget Options
**Scenario:** Customer has budget constraints

**Solution:**
- Create 3 quotations:
  - **Basic**: Essential repairs only ($500)
  - **Standard**: Recommended repairs ($800)
  - **Premium**: All repairs + preventive maintenance ($1,200)

**Customer Benefits:**
- Clear price comparison
- Flexibility to choose based on budget
- Understanding of what's included at each level

---

### 2. Phased Repairs
**Scenario:** Large repair job that can be split

**Solution:**
- Create multiple quotations:
  - **Phase 1**: Safety-critical repairs (immediate)
  - **Phase 2**: Performance repairs (next month)
  - **Phase 3**: Cosmetic repairs (future)

**Customer Benefits:**
- Spread cost over time
- Prioritize critical repairs
- Plan future maintenance

**Implementation:**
- Customer can approve Phase 1 immediately
- Approve Phase 2 later when ready
- Each approved quotation creates separate jobs

---

### 3. Parts Options
**Scenario:** Different part quality levels

**Solution:**
- Create quotations with different parts:
  - **OEM Parts**: Original manufacturer parts (highest quality)
  - **Aftermarket Premium**: High-quality alternatives
  - **Aftermarket Standard**: Budget-friendly options

**Customer Benefits:**
- Understand quality vs. price tradeoff
- Make informed decisions
- Choose based on vehicle age/value

---

### 4. Negotiation & Revision
**Scenario:** Customer wants modifications

**Solution:**
- **Quote v1**: Initial recommendation
- **Quote v2**: Revised based on customer feedback
- **Quote v3**: Final negotiated version

**Workflow:**
1. Manager creates initial quotation
2. Customer reviews and requests changes
3. Manager creates revised quotation
4. Customer approves final version

---

## API Usage Examples

### Creating Multiple Quotations

```javascript
// Create first quotation (Basic Package)
POST /api/Inspection/convert-to-quotation
{
  "inspectionId": "abc-123",
  "note": "Basic Package - Essential repairs only"
}

// Create second quotation (Standard Package)
POST /api/Inspection/convert-to-quotation
{
  "inspectionId": "abc-123",
  "note": "Standard Package - Recommended repairs"
}

// Create third quotation (Premium Package)
POST /api/Inspection/convert-to-quotation
{
  "inspectionId": "abc-123",
  "note": "Premium Package - All repairs + maintenance"
}
```

### Viewing All Quotations for Repair Order

```javascript
GET /api/Quotations/repair-order/{repairOrderId}

Response:
[
  {
    "quotationId": "quote-1",
    "status": "Rejected",
    "totalAmount": 500,
    "note": "Basic Package - Essential repairs only",
    "createdAt": "2024-01-01T10:00:00Z"
  },
  {
    "quotationId": "quote-2",
    "status": "Approved",
    "totalAmount": 800,
    "note": "Standard Package - Recommended repairs",
    "createdAt": "2024-01-01T11:00:00Z"
  },
  {
    "quotationId": "quote-3",
    "status": "Pending",
    "totalAmount": 1200,
    "note": "Premium Package - All repairs + maintenance",
    "createdAt": "2024-01-01T12:00:00Z"
  }
]
```

### Customer Approving Multiple Quotations (Phased Repairs)

```javascript
// Approve Phase 1
PUT /api/CustomerQuotations/customer-response
{
  "quotationId": "phase-1-quote",
  "isApproved": true,
  "selectedServices": [...]
}

// Later, approve Phase 2
PUT /api/CustomerQuotations/customer-response
{
  "quotationId": "phase-2-quote",
  "isApproved": true,
  "selectedServices": [...]
}
```

### Creating Jobs from Multiple Approved Quotations

```javascript
// Create jobs from Phase 1
POST /api/Quotations/phase-1-quote/copy-to-jobs
// Creates jobs: Job-1, Job-2, Job-3

// Create jobs from Phase 2
POST /api/Quotations/phase-2-quote/copy-to-jobs
// Creates jobs: Job-4, Job-5, Job-6

// All jobs are now in the system for the same repair order
```

---

## UI Implementation Guidelines

### Manager Interface

#### 1. Quotation List View
```
Repair Order #RO-12345
├── Quote 1: Basic Package [$500] - Rejected
├── Quote 2: Standard Package [$800] - Approved ✓
└── Quote 3: Premium Package [$1,200] - Pending

Actions:
[Create Alternative Quote] [View Details] [Send to Customer]
```

#### 2. Create Alternative Quote Flow
```
Step 1: Select base quotation to duplicate
Step 2: Modify services/parts
Step 3: Add descriptive note (e.g., "Budget Option")
Step 4: Save as new quotation
```

#### 3. Quotation Comparison Table
```
| Feature              | Basic | Standard | Premium |
|---------------------|-------|----------|---------|
| Oil Change          | ✓     | ✓        | ✓       |
| Brake Inspection    | ✓     | ✓        | ✓       |
| Brake Pad Replace   | -     | ✓        | ✓       |
| Tire Rotation       | -     | ✓        | ✓       |
| Full Detail         | -     | -        | ✓       |
|---------------------|-------|----------|---------|
| Total               | $500  | $800     | $1,200  |
| Status              | ❌    | ✓        | ⏳      |
```

---

### Customer Interface

#### 1. Quotation Selection View
```
Your Repair Options for Vehicle: ABC-1234

┌─────────────────────────────────────┐
│ Basic Package - $500                │
│ Essential repairs only              │
│ Status: Available                   │
│ [View Details] [Select This]        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Standard Package - $800 ⭐ RECOMMENDED│
│ All recommended repairs             │
│ Status: Available                   │
│ [View Details] [Select This]        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Premium Package - $1,200            │
│ Complete service + maintenance      │
│ Status: Available                   │
│ [View Details] [Select This]        │
└─────────────────────────────────────┘
```

#### 2. Comparison View
```
Compare Options

[Basic] [Standard] [Premium]

Drag slider to compare →

Services Included:
✓ Oil Change (all packages)
✓ Brake Inspection (all packages)
⚠ Brake Pad Replace (Standard & Premium only)
⚠ Tire Rotation (Standard & Premium only)
⚠ Full Detail (Premium only)

Price Difference: $300 more for Standard
Estimated Time: +2 hours
```

---

## Best Practices

### For Managers

1. **Use Clear Naming**
   - ✅ "Basic Package - Essential repairs"
   - ✅ "Phase 1 - Safety Critical"
   - ✅ "OEM Parts Option"
   - ❌ "Quote 1", "Quote 2", "Quote 3"

2. **Limit Active Quotations**
   - Don't send 10 quotations at once
   - Send 2-3 well-defined options
   - Mark superseded quotations as Expired

3. **Document Differences**
   - Use the note field to explain what's included
   - Highlight key differences
   - Explain value proposition

4. **Version Control**
   - Keep track of revisions
   - Note: "Revised based on customer feedback"
   - Reference previous quotation if applicable

### For Customers

1. **Review All Options**
   - Compare prices and services
   - Understand what's included
   - Ask questions if unclear

2. **Consider Phased Approach**
   - Approve critical repairs first
   - Plan future repairs
   - Spread costs over time

3. **Communicate Preferences**
   - Provide feedback on quotations
   - Request modifications
   - Discuss budget constraints

---

## Database Considerations

### No Schema Changes Required
The existing schema already supports multiple quotations:
- `Quotation` table has `RepairOrderId` (not unique)
- No unique constraint on `(RepairOrderId, InspectionId)`
- Each quotation has independent status

### Data Integrity
- Each quotation is independent
- Approving one doesn't affect others
- Jobs are linked to specific quotation
- Audit trail maintained for all quotations

---

## Testing Scenarios

### Test Case 1: Create Multiple Quotations
```
1. Complete an inspection
2. Convert to quotation (Quote 1)
3. Convert same inspection to quotation again (Quote 2)
4. Verify both quotations exist
5. Verify both have status "Pending"
```

### Test Case 2: Approve Multiple Quotations
```
1. Create 2 quotations for same repair order
2. Customer approves Quote 1
3. Customer approves Quote 2
4. Verify both have status "Approved"
5. Create jobs from Quote 1
6. Create jobs from Quote 2
7. Verify all jobs exist in system
```

### Test Case 3: Mixed Status Quotations
```
1. Create 3 quotations
2. Customer approves Quote 1
3. Customer rejects Quote 2
4. Quote 3 remains pending
5. Verify correct status for each
6. Verify only Quote 1 can create jobs
```

---

## Migration Notes

### For Existing Systems
If you have existing data with single quotations:
- ✅ No migration required
- ✅ Existing quotations continue to work
- ✅ New quotations can be added
- ✅ No breaking changes

### Backward Compatibility
- ✅ All existing API endpoints work unchanged
- ✅ Existing quotations remain valid
- ✅ No changes to DTOs or models
- ✅ Only business logic constraint removed

---

## Summary

### Key Benefits
1. **Flexibility**: Offer multiple repair options
2. **Customer Choice**: Empower customers to decide
3. **Phased Repairs**: Split large jobs over time
4. **Better Negotiation**: Revise quotes easily
5. **Increased Sales**: Upsell opportunities

### Implementation Effort
- **Backend**: ✅ Complete (constraint removed)
- **API**: ✅ No changes needed
- **Database**: ✅ No changes needed
- **Frontend**: 🔨 UI updates recommended

### Next Steps
1. Update UI to show multiple quotations
2. Add comparison view for customers
3. Implement quotation duplication feature
4. Add filtering/sorting for quotation lists
5. Update notifications to handle multiple quotes
