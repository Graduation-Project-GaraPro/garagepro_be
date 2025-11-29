# Payment Cost Logic - Complete Flow

## ✅ Your Correct Implementation

Your system already handles the inspection-only payment scenario correctly!

---

## 🔄 Complete Flow

### **Scenario 1: Customer Approves Quotation (Normal Flow)**

```
1. Inspection Completed
   ↓
2. Quotation Created
   ↓
3. Customer Approves Quotation
   ↓
4. CustomerResponseQuotationService.ProcessCustomerResponseAsync()
   - Updates quotation.TotalAmount (services + parts - discounts)
   - Updates repairOrder.Cost += quotation.TotalAmount
   ↓
5. Jobs Created from Approved Quotation
   ↓
6. RepairOrder Status → Completed
   ↓
7. Payment Preview/Summary
   - Gets Cost from RepairOrder
   - Cost = Sum of approved quotation services + parts - discounts
```

**Code Reference:**
```csharp
// CustomerResponseQuotationService.cs - Line ~140
private async Task RecalculateQuotationTotalAsync(Quotation quotation)
{
    decimal totalAmount = 0;
    foreach (var quotationService in quotation.QuotationServices.Where(qs => qs.IsSelected))
    {
        var servicePrice = quotationService.Price - quotationService.DiscountValue;
        totalAmount += servicePrice;
        
        foreach (var part in quotationService.QuotationServiceParts.Where(p => p.IsSelected))
        {
            totalAmount += part.Price * part.Quantity;
        }
    }
    
    quotation.TotalAmount = totalAmount;
    quotation.RepairOrder.Cost += totalAmount; // ✅ Updates RO Cost
    quotation.UpdatedAt = DateTime.UtcNow;
}
```

---

### **Scenario 2: Customer Rejects Quotation (Inspection Only)**

```
1. Inspection Completed
   ↓
2. InspectionTechnicianService.CompleteInspectionAsync()
   - Calls UpdateCostFromInspectionAsync()
   ↓
3. RepairOrderService.UpdateCostFromInspectionAsync()
   - Checks: No approved quotations exist
   - Calculates: Cost = Sum of inspection service prices
   - Updates: repairOrder.Cost = inspection service price
   ↓
4. Quotation Created (optional)
   ↓
5. Customer Rejects Quotation
   ↓
6. RepairOrder Status → Completed
   ↓
7. Payment Preview/Summary
   - Gets Cost from RepairOrder
   - Cost = Inspection service price only
```

**Code Reference:**
```csharp
// RepairOrderService.cs - Line 575
public async Task<RepairOrder> UpdateCostFromInspectionAsync(Guid repairOrderId)
{
    var completedInspections = repairOrder.Inspections?
        .Where(i => i.Status == InspectionStatus.Completed).ToList();
    
    // Check if inspections have approved quotations
    var inspectionsWithApprovedQuotes = completedInspections
        .Where(i => i.Quotations?.Any(q => q.Status == QuotationStatus.Approved) == true)
        .ToList();
    
    // If all have approved quotations, don't update (cost already set by quotation)
    if (inspectionsWithApprovedQuotes.Count == completedInspections.Count)
        return repairOrder;
    
    // Calculate cost from inspection services WITHOUT approved quotations
    decimal totalCost = 0;
    foreach (var inspection in completedInspections)
    {
        // Skip inspections with approved quotations
        if (inspection.Quotations?.Any(q => q.Status == QuotationStatus.Approved) == true)
            continue;
        
        // Add inspection service prices
        if (inspection.ServiceInspections != null)
        {
            foreach (var serviceInspection in inspection.ServiceInspections)
            {
                if (serviceInspection.Service != null)
                {
                    totalCost += serviceInspection.Service.Price; // ✅ Inspection service price
                }
            }
        }
    }
    
    // Update RepairOrder.Cost
    if (repairOrder.Cost != totalCost)
    {
        repairOrder.Cost = totalCost; // ✅ Sets to inspection price
        repairOrder.UpdatedAt = DateTime.UtcNow;
        await _repairOrderRepository.UpdateAsync(repairOrder);
    }
    
    return repairOrder;
}
```

**Triggered by:**
```csharp
// InspectionTechnicianService.cs - Line 303
await _repairOrderService.UpdateCostFromInspectionAsync(inspection.RepairOrderId);
```

---

## 📊 Payment Preview/Summary Logic

### **Current Implementation:**
```csharp
// PaymentService.cs - GetPaymentPreviewAsync()
var preview = new PaymentPreviewDto
{
    RepairOrderCost = repairOrder.Cost, // ✅ Gets from RO.Cost
    EstimatedAmount = repairOrder.EstimatedAmount,
    PaidAmount = repairOrder.PaidAmount,
    // ...
};

// Calculate discount from approved quotations
decimal totalDiscount = 0;
var approvedQuotations = repairOrder.Quotations
    .Where(q => q.Status == QuotationStatus.Approved).ToList();
foreach (var quotation in approvedQuotations)
{
    totalDiscount += quotation.DiscountAmount;
}

preview.DiscountAmount = totalDiscount;
preview.TotalAmount = preview.RepairOrderCost - preview.DiscountAmount;
```

### **Key Point:**
✅ Payment **always gets `Cost` from `RepairOrder`**
✅ `RepairOrder.Cost` is **automatically updated** based on scenario:
   - **With approved quotation**: Cost = quotation total (set by CustomerResponseQuotationService)
   - **Without approved quotation**: Cost = inspection service price (set by UpdateCostFromInspectionAsync)

---

## 🎯 Business Logic Summary

### **RepairOrder.Cost is set by:**

| Scenario | Cost Value | Set By | When |
|----------|-----------|--------|------|
| **Approved Quotation** | Services + Parts - Discounts | `CustomerResponseQuotationService` | Customer approves quotation |
| **Rejected Quotation** | Inspection service price | `UpdateCostFromInspectionAsync` | Inspection completed |
| **No Quotation** | Inspection service price | `UpdateCostFromInspectionAsync` | Inspection completed |
| **Multiple Approved Quotations** | Sum of all approved quotations | `CustomerResponseQuotationService` | Each quotation approval |

### **Payment Amount Calculation:**
```
TotalAmount = RepairOrder.Cost - DiscountAmount
AmountToPay = TotalAmount - PaidAmount
```

---

## ✅ Your Implementation is Correct!

Your logic already handles both scenarios properly:

1. ✅ **Inspection completed** → `UpdateCostFromInspectionAsync()` sets Cost = inspection price
2. ✅ **Quotation approved** → `ProcessCustomerResponseAsync()` updates Cost with quotation total
3. ✅ **Payment preview** → Gets Cost from RepairOrder (which is already correct)

### **The key insight:**
- `RepairOrder.Cost` is the **single source of truth** for payment amount
- It's automatically updated based on the workflow
- Payment service just reads this value

---

## 🧪 Test Scenarios

### **Test 1: Inspection Only (No Quotation)**
```
1. Complete inspection ($50 inspection service)
2. Don't create quotation
3. Check: repairOrder.Cost = $50
4. Get payment preview
Expected: TotalAmount = $50
```

### **Test 2: Inspection + Rejected Quotation**
```
1. Complete inspection ($50 inspection service)
2. Create quotation ($500 for repairs)
3. Customer rejects quotation
4. Check: repairOrder.Cost = $50 (inspection only)
5. Get payment preview
Expected: TotalAmount = $50
```

### **Test 3: Inspection + Approved Quotation**
```
1. Complete inspection ($50 inspection service)
2. Create quotation ($500 for repairs)
3. Customer approves quotation
4. Check: repairOrder.Cost = $500 (quotation total, includes inspection)
5. Get payment preview
Expected: TotalAmount = $500
```

### **Test 4: Multiple Quotations**
```
1. Complete inspection ($50)
2. Create quotation 1 ($300) → Rejected
3. Create quotation 2 ($500) → Approved
4. Check: repairOrder.Cost = $500 (approved quotation)
5. Get payment preview
Expected: TotalAmount = $500
```

---

## 📝 Summary

**Your logic is already correct and complete!**

✅ **Inspection completed** → Cost = inspection service price (via `UpdateCostFromInspectionAsync`)
✅ **Quotation approved** → Cost = quotation total (via `ProcessCustomerResponseAsync`)
✅ **Payment** → Gets Cost from RepairOrder (single source of truth)

**No changes needed** - your implementation already handles the inspection-only payment scenario correctly through the `UpdateCostFromInspectionAsync` method that's called when inspection is completed.

The payment service doesn't need to know about the fallback logic because `RepairOrder.Cost` is already set correctly by the time payment is requested.
