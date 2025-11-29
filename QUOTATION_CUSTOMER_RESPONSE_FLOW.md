# Quotation Customer Response Flow - Correct Implementation

## ✅ Current Implementation Status

The system now uses **`CustomerResponseQuotationService`** for handling customer responses to quotations. The duplicate implementation in `QuotationManagementService` has been removed.

---

## 🎯 Correct Service to Use

### **CustomerResponseQuotationService.ProcessCustomerResponseAsync()**
**Location:** `Services/QuotationServices/CustomerResponseQuotationService.cs`

**Controller:** `CustomerQuotationsController`
**Endpoint:** `PUT /api/CustomerQuotations/customer-response`

---

## 🔄 Complete Quotation Flow

### **1. Create Quotation (Manager)**
```
Inspection (Completed) 
    ↓
POST /api/Inspection/convert-to-quotation
    ↓
QuotationManagementService.CreateQuotationAsync()
    ↓
Quotation (Status: Pending)
```

**Features:**
- ✅ Automatically creates from completed inspection
- ✅ Includes all services and parts from inspection
- ✅ Calculates initial total amount
- ✅ Sends SignalR notification
- ✅ Sends FCM notification to customer
- ✅ Supports multiple quotations per repair order

---

### **2. Edit Quotation (Manager - Optional)**
```
PUT /api/Quotations/{id}
    ↓
QuotationManagementService.UpdateQuotationDetailsAsync()
    ↓
Quotation (Status: Still Pending)
```

**Manager Can:**
- ✅ Add/remove services
- ✅ Add/remove parts
- ✅ Adjust quantities
- ✅ Pre-select parts (recommendations)
- ✅ Add descriptive notes
- ❌ Cannot change `IsRequired` flag (set during inspection)

---

### **3. Send to Customer (Manager)**
```
PUT /api/Quotations/{id}/status
Body: { "status": "Sent" }
    ↓
QuotationManagementService.UpdateQuotationStatusAsync()
    ↓
Quotation (Status: Sent)
```

**What Happens:**
- ✅ Status changes: `Pending` → `Sent`
- ✅ Sets `SentToCustomerAt` timestamp
- ✅ Customer receives notification

---

### **4. Customer Response (Customer) ⭐ KEY STEP**
```
PUT /api/CustomerQuotations/customer-response
    ↓
CustomerResponseQuotationService.ProcessCustomerResponseAsync()
    ↓
Quotation (Status: Approved or Rejected)
```

**Request Body:**
```json
{
  "quotationId": "guid",
  "status": "Approved" | "Rejected",
  "customerNote": "string (optional)",
  "selectedServices": [
    {
      "quotationServiceId": "guid",
      "selectedPartIds": ["guid1", "guid2"],
      "appliedPromotionId": "guid (optional)"
    }
  ]
}
```

**What This Service Does:**

#### ✅ **Validation**
- Validates required services are selected
- Validates promotions are applicable
- Validates services belong to quotation
- Validates user owns the quotation

#### ✅ **Service Selection**
- Updates `IsSelected` for each service
- **Required services MUST stay selected**
- Optional services can be deselected

#### ✅ **Part Selection**
- Updates `IsSelected` for each part
- Validates part selection based on service type (advanced vs. simple)
- Non-advanced services: only 1 part can be selected
- Advanced services: multiple parts allowed

#### ✅ **Promotion Handling**
- Applies promotional discounts to services
- Calculates discount value based on promotion type
- Decrements promotion usage limit
- Increments promotion used count
- Stores `AppliedPromotionId` and `DiscountValue` on service

#### ✅ **Total Recalculation**
```
Total = Σ(Selected Services - Discounts) + Σ(Selected Parts × Quantity)
```
- Calculates service price after discount
- Adds selected parts cost
- Updates `quotation.TotalAmount`
- Updates `repairOrder.Cost`
- Sets `FinalPrice` for each service

#### ✅ **Transaction Management**
- Begins transaction
- Commits on success
- **Rolls back on any error** (data integrity)

#### ✅ **Notifications**
- SignalR to quotation group
- SignalR to promotion dashboard (if promotions applied)
- SignalR to specific promotion groups

---

### **5. Create Jobs (Manager)**
```
POST /api/Quotations/{id}/copy-to-jobs
    ↓
QuotationManagementService.CopyQuotationToJobsAsync()
    ↓
Jobs Created (Status: Pending)
```

**What Happens:**
- ✅ Creates Job for each selected service
- ✅ Creates JobPart for each selected part
- ✅ Uses final prices (after discounts)
- ✅ Multiple approved quotations can create jobs

---

## 🔍 Key Differences: CustomerResponseQuotationService vs QuotationManagementService

| Feature | CustomerResponseQuotationService | QuotationManagementService (REMOVED) |
|---------|----------------------------------|--------------------------------------|
| **Validation** | ✅ Validates required services | ❌ No validation |
| **Promotions** | ✅ Full support | ❌ Not supported |
| **Discounts** | ✅ Calculates & applies | ❌ Not calculated |
| **Transaction** | ✅ With rollback | ❌ No transaction |
| **RepairOrder Cost** | ✅ Updates | ❌ Not updated |
| **Part Validation** | ✅ Advanced logic | ❌ Simple logic |
| **Promotion Notifications** | ✅ Yes | ❌ No |
| **Data Integrity** | ✅ Guaranteed | ⚠️ Not guaranteed |

---

## 📝 Business Rules

### **Required Services**
- ✅ Set during inspection by technician
- ✅ Cannot be deselected by customer
- ✅ Must be included in customer response
- ✅ Validation enforced in `CustomerResponseQuotationService`

### **Optional Services**
- ✅ Customer can select or deselect
- ✅ Manager can pre-select (recommendations)
- ✅ Only selected services are included in total

### **Part Selection**
- **Non-Advanced Services:** Only 1 part can be selected
- **Advanced Services:** Multiple parts allowed
- ✅ Manager can pre-select parts (recommendations)
- ✅ Customer makes final selection

### **Promotions**
- ✅ Applied per service (not per quotation)
- ✅ Validated for applicability
- ✅ Usage limit decremented
- ✅ Discount calculated based on promotion type
- ✅ Stored on `QuotationService` entity

---

## 🚨 Important Notes

### **DO NOT USE:**
- ❌ `QuotationManagementService.ProcessCustomerResponseAsync()` - **REMOVED**
- ❌ Direct approval without customer selection (use only for emergencies)

### **ALWAYS USE:**
- ✅ `CustomerResponseQuotationService.ProcessCustomerResponseAsync()`
- ✅ Proper validation and transaction management
- ✅ Promotion handling when applicable

---

## 🧪 Testing Scenarios

### **Test 1: Basic Approval**
```json
{
  "quotationId": "abc-123",
  "status": "Approved",
  "selectedServices": [
    {
      "quotationServiceId": "service-1",
      "selectedPartIds": ["part-1"]
    }
  ]
}
```
**Expected:** Quotation approved, totals calculated, notifications sent

---

### **Test 2: Approval with Promotion**
```json
{
  "quotationId": "abc-123",
  "status": "Approved",
  "selectedServices": [
    {
      "quotationServiceId": "service-1",
      "selectedPartIds": ["part-1"],
      "appliedPromotionId": "promo-1"
    }
  ]
}
```
**Expected:** 
- Discount applied
- Promotion usage limit decremented
- Promotion notifications sent
- Total recalculated with discount

---

### **Test 3: Required Service Validation**
```json
{
  "quotationId": "abc-123",
  "status": "Approved",
  "selectedServices": [
    {
      "quotationServiceId": "optional-service-1",
      "selectedPartIds": ["part-1"]
    }
  ]
}
```
**Expected:** ❌ Error - "Required service 'X' must be selected"

---

### **Test 4: Multiple Quotations**
```
1. Create Quote 1 (Basic Package)
2. Create Quote 2 (Premium Package)
3. Customer approves Quote 2
4. Manager creates jobs from Quote 2
```
**Expected:** ✅ Jobs created only from approved quotation

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    QUOTATION LIFECYCLE                       │
└─────────────────────────────────────────────────────────────┘

Inspection (Completed)
        │
        ├─→ CreateQuotationAsync()
        │   ├─ Create Quotation (Pending)
        │   ├─ Include all services/parts
        │   ├─ Calculate initial total
        │   └─ Send notifications
        │
        ↓
Quotation (Pending)
        │
        ├─→ UpdateQuotationDetailsAsync() [Optional]
        │   ├─ Manager edits services/parts
        │   └─ Recalculate totals
        │
        ↓
Quotation (Pending)
        │
        ├─→ UpdateQuotationStatusAsync()
        │   ├─ Status: Pending → Sent
        │   └─ Set SentToCustomerAt
        │
        ↓
Quotation (Sent)
        │
        ├─→ ProcessCustomerResponseAsync() ⭐
        │   ├─ Validate required services
        │   ├─ Validate promotions
        │   ├─ Update service selection
        │   ├─ Update part selection
        │   ├─ Apply discounts
        │   ├─ Recalculate totals
        │   ├─ Update RepairOrder cost
        │   ├─ Begin transaction
        │   ├─ Commit/Rollback
        │   └─ Send notifications
        │
        ↓
Quotation (Approved/Rejected)
        │
        ├─→ CopyQuotationToJobsAsync() [If Approved]
        │   ├─ Create Jobs
        │   ├─ Create JobParts
        │   └─ Link to RepairOrder
        │
        ↓
Jobs (Pending)
```

---

## 🎓 Summary

**CustomerResponseQuotationService** is the **correct and complete** implementation for handling customer responses. It includes:

1. ✅ **Validation** - Ensures data integrity
2. ✅ **Promotions** - Full discount support
3. ✅ **Transactions** - Rollback on errors
4. ✅ **Calculations** - Accurate totals with discounts
5. ✅ **Notifications** - Complete notification system
6. ✅ **Business Rules** - Enforces required services, part selection rules

The duplicate method in `QuotationManagementService` has been removed to avoid confusion and ensure consistency.
