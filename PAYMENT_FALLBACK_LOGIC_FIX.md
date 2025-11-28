# Payment Fallback Logic - Inspection Service Pricing

## 🎯 Problem Statement

When a customer **rejects all quotations** (negotiation fails), the payment preview/summary should still charge for the **inspection service** that was performed.

Currently, the fallback logic in `GetPaymentPreviewAsync()` doesn't properly handle this scenario.

---

## 📋 Current Flow

### **Scenario 1: Approved Quotation Exists** ✅
```
Inspection → Quotation → Customer Approves → Jobs Created → Payment
```
**Payment includes:**
- Selected services from approved quotation
- Selected parts from approved quotation
- Discounts from promotions

### **Scenario 2: No Approved Quotation** ⚠️
```
Inspection → Quotation → Customer Rejects → No Jobs → Payment ???
```
**Current fallback:**
- Gets services from `RepairOrderServices`
- Gets parts from `JobParts`
- **MISSING: Inspection service price**

---

## 🔧 Recommended Solution

### **Option 1: Charge for Inspection Service (Recommended)**

When no approved quotation exists, the payment should include:
1. **Inspection service price** (the diagnostic work that was done)
2. Any other services from `RepairOrderServices` (if any)
3. No parts (since no repairs were done)

### **Implementation:**

```csharp
public async Task<PaymentPreviewDto> GetPaymentPreviewAsync(Guid repairOrderId, CancellationToken ct = default)
{
    var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
    if (repairOrder == null)
        throw new Exception($"Repair order with ID {repairOrderId} not found");

    if (repairOrder.StatusId != 3)
        throw new Exception($"Repair order must be in Completed status to process payment");

    var preview = new PaymentPreviewDto
    {
        RepairOrderId = repairOrder.RepairOrderId,
        RepairOrderCost = repairOrder.Cost,
        EstimatedAmount = repairOrder.EstimatedAmount,
        PaidAmount = repairOrder.PaidAmount,
        CustomerName = repairOrder.User != null ? $"{repairOrder.User.FirstName} {repairOrder.User.LastName}".Trim() : "Unknown Customer",
        VehicleInfo = repairOrder.Vehicle != null ? $"{repairOrder.Vehicle.Brand?.BrandName ?? "Unknown Brand"} {repairOrder.Vehicle.Model?.ModelName ?? "Unknown Model"} ({repairOrder.Vehicle.LicensePlate ?? "No Plate"})" : "Unknown Vehicle"
    };

    // Calculate discount from approved quotations only
    decimal totalDiscount = 0;
    if (repairOrder.Quotations != null && repairOrder.Quotations.Any())
    {
        var approvedQuotations = repairOrder.Quotations.Where(q => q.Status == QuotationStatus.Approved).ToList();
        foreach (var quotation in approvedQuotations)
        {
            totalDiscount += quotation.DiscountAmount;
            preview.Quotations.Add(new QuotationInfoDto
            {
                QuotationId = quotation.QuotationId,
                TotalAmount = quotation.TotalAmount,
                DiscountAmount = quotation.DiscountAmount,
                Status = quotation.Status.ToString()
            });
        }
    }
    preview.DiscountAmount = totalDiscount;
    preview.TotalAmount = preview.RepairOrderCost - preview.DiscountAmount;

    // Get services and parts
    if (repairOrder.Quotations != null && repairOrder.Quotations.Any())
    {
        var approvedQuotation = repairOrder.Quotations.FirstOrDefault(q => q.Status == QuotationStatus.Approved);
        
        if (approvedQuotation != null && approvedQuotation.QuotationServices != null)
        {
            // ✅ SCENARIO 1: Approved quotation exists - use customer's selections
            foreach (var quotationService in approvedQuotation.QuotationServices.Where(qs => qs.IsSelected))
            {
                if (quotationService.Service != null)
                {
                    preview.Services.Add(new ServicePreviewDto
                    {
                        ServiceId = quotationService.ServiceId,
                        ServiceName = quotationService.Service.ServiceName,
                        Price = quotationService.Price,
                        EstimatedDuration = quotationService.Service.EstimatedDuration
                    });
                }
                
                if (quotationService.QuotationServiceParts != null)
                {
                    foreach (var quotationPart in quotationService.QuotationServiceParts.Where(qsp => qsp.IsSelected))
                    {
                        if (quotationPart.Part != null)
                        {
                            preview.Parts.Add(new PartPreviewDto
                            {
                                PartId = quotationPart.PartId,
                                PartName = quotationPart.Part.Name,
                                Quantity = (int)quotationPart.Quantity,
                                UnitPrice = quotationPart.Price,
                                TotalPrice = quotationPart.Quantity * quotationPart.Price
                            });
                        }
                    }
                }
            }
        }
        else
        {
            // ⚠️ SCENARIO 2: No approved quotation - charge for inspection service
            // This happens when customer rejects all quotations
            
            // Get the inspection that was performed
            var completedInspection = repairOrder.Inspections?
                .FirstOrDefault(i => i.Status == InspectionStatus.Completed);
            
            if (completedInspection != null && completedInspection.Service != null)
            {
                // Add the inspection service to the payment
                preview.Services.Add(new ServicePreviewDto
                {
                    ServiceId = completedInspection.ServiceId,
                    ServiceName = completedInspection.Service.ServiceName,
                    Price = completedInspection.Service.Price,
                    EstimatedDuration = completedInspection.Service.EstimatedDuration
                });
                
                // Update RepairOrder.Cost to match inspection service price
                // This ensures consistency between preview and actual payment
                if (repairOrder.Cost == 0)
                {
                    repairOrder.Cost = completedInspection.Service.Price;
                    preview.RepairOrderCost = repairOrder.Cost;
                    preview.TotalAmount = repairOrder.Cost - preview.DiscountAmount;
                    
                    // Save the updated cost
                    await _repoRepairOrder.UpdateAsync(repairOrder);
                    await _repoRepairOrder.Context.SaveChangesAsync(ct);
                }
            }
            else
            {
                // Fallback: Use RepairOrderServices if no inspection found
                if (repairOrder.RepairOrderServices != null)
                {
                    var addedServiceIds = new HashSet<Guid>();
                    foreach (var roService in repairOrder.RepairOrderServices)
                    {
                        if (roService.Service != null && !addedServiceIds.Contains(roService.ServiceId))
                        {
                            preview.Services.Add(new ServicePreviewDto
                            {
                                ServiceId = roService.ServiceId,
                                ServiceName = roService.Service.ServiceName,
                                Price = roService.Service.Price,
                                EstimatedDuration = roService.Service.EstimatedDuration
                            });
                            addedServiceIds.Add(roService.ServiceId);
                        }
                    }
                }
            }
            
            // Note: No parts are added when there's no approved quotation
            // because no repair work was done (only inspection)
        }
    }

    return preview;
}
```

---

## 📊 Data Flow Comparison

### **With Approved Quotation:**
```
Inspection (Completed)
    ↓
Quotation (Approved)
    ↓
Jobs Created
    ↓
Payment Preview:
    - Services: From approved quotation (customer's selections)
    - Parts: From approved quotation (customer's selections)
    - Discounts: From promotions
    - Total: Sum of selected services + parts - discounts
```

### **Without Approved Quotation (Rejected):**
```
Inspection (Completed)
    ↓
Quotation (Rejected)
    ↓
No Jobs Created
    ↓
Payment Preview:
    - Services: Inspection service only
    - Parts: None (no repairs done)
    - Discounts: None
    - Total: Inspection service price
```

---

## 🎯 Business Logic

### **Why charge for inspection?**
1. **Work was performed**: Technician spent time diagnosing the vehicle
2. **Value was provided**: Customer received diagnostic information
3. **Industry standard**: Most garages charge for diagnostic/inspection services
4. **Fair compensation**: Garage should be paid for the work done

### **When to charge inspection price:**
- ✅ Customer rejects all quotations
- ✅ Customer doesn't respond to quotations (expired)
- ✅ Negotiation fails and no agreement is reached
- ✅ Customer decides not to proceed with repairs

### **When NOT to charge inspection price:**
- ❌ Customer approves a quotation (inspection cost is included in repair cost)
- ❌ Inspection was free (promotional offer)
- ❌ Inspection failed or was incomplete

---

## 🔍 Key Properties

### **RepairOrder Properties:**
- `Cost`: **Actual cost** of work performed (should be updated based on scenario)
- `EstimatedAmount`: **Estimated cost** before work begins
- `PaidAmount`: **Amount already paid** by customer

### **Calculation:**
```csharp
// With approved quotation:
Cost = Sum(Selected Services + Selected Parts - Discounts)

// Without approved quotation:
Cost = Inspection Service Price

// Amount to pay:
AmountToPay = Cost - PaidAmount
```

---

## ✅ Testing Scenarios

### **Test 1: Approved Quotation**
```
1. Complete inspection
2. Create quotation
3. Customer approves quotation
4. Get payment preview
Expected: Services and parts from approved quotation
```

### **Test 2: Rejected Quotation**
```
1. Complete inspection (e.g., $50 inspection service)
2. Create quotation
3. Customer rejects quotation
4. Get payment preview
Expected: Only inspection service ($50)
```

### **Test 3: Multiple Quotations, All Rejected**
```
1. Complete inspection ($50)
2. Create quotation 1 → Rejected
3. Create quotation 2 → Rejected
4. Get payment preview
Expected: Only inspection service ($50)
```

### **Test 4: No Quotation Created**
```
1. Complete inspection ($50)
2. No quotation created
3. Get payment preview
Expected: Only inspection service ($50)
```

---

## 🚨 Important Notes

### **RepairOrder.Cost Update:**
When falling back to inspection service, you should update `RepairOrder.Cost` to reflect the inspection price:

```csharp
if (repairOrder.Cost == 0 && completedInspection != null)
{
    repairOrder.Cost = completedInspection.Service.Price;
    await _repoRepairOrder.UpdateAsync(repairOrder);
    await _repoRepairOrder.Context.SaveChangesAsync(ct);
}
```

### **Status Requirements:**
- RepairOrder must be in `Completed` status (StatusId = 3)
- Inspection must be in `Completed` status
- This ensures work was actually performed before charging

### **Customer Communication:**
Make sure to communicate clearly to customers:
- "Inspection fee: $50"
- "This covers the diagnostic work performed"
- "If you proceed with repairs, this fee may be waived/included"

---

## 📝 Summary

**Your logic is mostly correct**, but needs enhancement for the **no approved quotation** scenario:

✅ **Current:** Gets services/parts from approved quotation
✅ **Current:** Falls back to RepairOrderServices and JobParts
❌ **Missing:** Charges for inspection service when no quotation is approved
✅ **Fix:** Add inspection service price to payment when no approved quotation exists

This ensures the garage is compensated for diagnostic work even when customers don't proceed with repairs.
