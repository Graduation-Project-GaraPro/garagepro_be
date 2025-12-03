# RepairOrder PaidStatus Update Fix

## Issues Found and Fixed

### Issue 1: Wrong Amount Comparison

**Problem**: The code was comparing `PaidAmount` with `EstimatedAmount` instead of `Cost`.

- `EstimatedAmount` = Initial estimate before work starts
- `Cost` = Actual final cost after work is completed (includes approved quotations)

**Impact**: Even when customer paid the full amount, the PaidStatus might not update to "Paid" because it was comparing against the wrong value.

### Issue 2: Missing Partial Status

**Problem**: The code had a comment saying "Keep as Unpaid for partial payments since there's no Partial status in the enum", but `PaidStatus.Partial` actually EXISTS in the enum!

**Impact**: Partial payments were marked as "Unpaid" instead of "Partial", making it impossible to distinguish between no payment and partial payment.

### Issue 3: Double-Counting Risk

**Problem**: In `MarkPaidAsync`, the payment amount was always added to `PaidAmount` without checking if the payment was already marked as paid.

**Impact**: If `MarkPaidAsync` is called multiple times for the same payment (e.g., webhook retry), the amount could be added multiple times.

## What Was Fixed

### 1. CreateManualPaymentAsync (Cash Payments)

**File**: `Services/PaymentServices/PaymentService.cs`  
**Line**: ~356-370

#### Before:
```csharp
repairOrder.PaidAmount += amount;
if (repairOrder.PaidAmount >= repairOrder.EstimatedAmount)  // ❌ Wrong comparison
{
    repairOrder.PaidStatus = PaidStatus.Paid;
}
else
{
    // ❌ Wrong - Partial status exists!
    repairOrder.PaidStatus = PaidStatus.Unpaid;
}
```

#### After:
```csharp
repairOrder.PaidAmount += amount;

// Compare with Cost (actual final cost) - only Paid or Unpaid
if (repairOrder.PaidAmount >= repairOrder.Cost)  // ✅ Correct comparison
{
    repairOrder.PaidStatus = PaidStatus.Paid;
}
else
{
    repairOrder.PaidStatus = PaidStatus.Unpaid;  // ✅ Simple: Paid or Unpaid only
}
```

### 2. MarkPaidAsync (PayOS/Webhook Processing)

**File**: `Services/PaymentServices/PaymentService.cs`  
**Line**: ~758-775

#### Before:
```csharp
repairOrder.PaidAmount += payment.Amount;  // ❌ Always adds, even if already paid
if (repairOrder.PaidAmount >= repairOrder.EstimatedAmount)  // ❌ Wrong comparison
{
    repairOrder.PaidStatus = PaidStatus.Paid;
}
else
{
    repairOrder.PaidStatus = PaidStatus.Partial;
}
```

#### After:
```csharp
// Only add to PaidAmount if payment status was not already Paid (prevent double-counting)
if (oldStatus != PaymentStatus.Paid)  // ✅ Check before adding
{
    repairOrder.PaidAmount += payment.Amount;
}

// Compare with Cost (actual final cost) - only Paid or Unpaid
if (repairOrder.PaidAmount >= repairOrder.Cost)  // ✅ Correct comparison
{
    repairOrder.PaidStatus = PaidStatus.Paid;
}
else
{
    repairOrder.PaidStatus = PaidStatus.Unpaid;  // ✅ Simple: Paid or Unpaid only
}
```

## PaidStatus - Only Paid or Unpaid

**Business Rule**: The system only supports **Paid** or **Unpaid** status. No partial payments are tracked in the status.

```csharp
public enum PaidStatus
{
    Unpaid,   // Not fully paid (includes no payment and partial payment)
    Paid,     // Fully paid (PaidAmount >= Cost)
    // Partial and Pending exist in enum but are NOT used
}
```

**Logic**:
- If `PaidAmount >= Cost` → **Paid**
- If `PaidAmount < Cost` → **Unpaid** (even if some amount is paid)

## Payment Flow Examples

### Example 1: Full Cash Payment

```
RepairOrder:
  Cost: $1,500
  PaidAmount: $0
  PaidStatus: Unpaid

Manager records cash payment: $1,500

After:
  Cost: $1,500
  PaidAmount: $1,500
  PaidStatus: Paid ✅
```

### Example 2: Partial Cash Payment (Still Unpaid)

```
RepairOrder:
  Cost: $1,500
  PaidAmount: $0
  PaidStatus: Unpaid

Manager records cash payment: $500

After:
  Cost: $1,500
  PaidAmount: $500
  PaidStatus: Unpaid ✅ (not fully paid yet)
```

### Example 3: PayOS Payment

```
RepairOrder:
  Cost: $2,000
  PaidAmount: $0
  PaidStatus: Unpaid

Customer pays via PayOS: $2,000
Webhook received and processed

After:
  Cost: $2,000
  PaidAmount: $2,000
  PaidStatus: Paid ✅
```

### Example 4: Multiple Payments Until Fully Paid

```
RepairOrder:
  Cost: $3,000
  PaidAmount: $0
  PaidStatus: Unpaid

Payment 1 (Cash): $1,000
After:
  PaidAmount: $1,000
  PaidStatus: Unpaid ✅ (not fully paid yet)

Payment 2 (PayOS): $2,000
After:
  PaidAmount: $3,000
  PaidStatus: Paid ✅ (fully paid)
```

### Example 5: Webhook Retry (Double-Counting Prevention)

```
RepairOrder:
  Cost: $1,500
  PaidAmount: $0
  PaidStatus: Unpaid

Payment created: $1,500 (Status: Unpaid)

Webhook 1: MarkPaidAsync called
  - oldStatus = Unpaid
  - Adds $1,500 to PaidAmount
  - PaidAmount = $1,500
  - PaidStatus = Paid ✅

Webhook 2 (retry): MarkPaidAsync called again
  - oldStatus = Paid ✅ (prevents double-counting)
  - Does NOT add $1,500 again
  - PaidAmount = $1,500 (unchanged)
  - PaidStatus = Paid ✅
```

## Cost vs EstimatedAmount

### EstimatedAmount
- Set when repair order is created
- Based on initial inspection
- May not include all services/parts
- Can change as work progresses

### Cost
- Final actual cost
- Includes all approved quotations
- Includes all services and parts used
- This is what customer actually pays

**Always compare with `Cost` for payment status!**

## SignalR Notifications

The PaidStatus is included in SignalR notifications:

```json
{
  "paymentId": 123456789,
  "repairOrderId": "guid",
  "amount": 1500.00,
  "paidStatus": "Paid",  // ✅ Now correctly reflects RO status
  "message": "Payment confirmed and processed"
}
```

## Testing Scenarios

### Test 1: Full Payment
```http
POST /api/payments/manager-create/{repairOrderId}
Body: { "method": "Cash" }
```

Expected:
- Payment.Status = Paid
- RepairOrder.PaidAmount = Cost
- RepairOrder.PaidStatus = Paid ✅

### Test 2: Partial Payment (Still Unpaid)
```http
POST /api/payments/manager-create/{repairOrderId}
Body: { "method": "Cash" }
Amount: 50% of Cost
```

Expected:
- Payment.Status = Paid (the payment itself is paid)
- RepairOrder.PaidAmount = 50% of Cost
- RepairOrder.PaidStatus = Unpaid ✅ (not fully paid yet)

### Test 3: PayOS Payment
```http
POST /api/payments/manager-qr-payment/{repairOrderId}
Customer pays via QR
Webhook processed
```

Expected:
- Payment.Status = Paid
- RepairOrder.PaidAmount = Cost
- RepairOrder.PaidStatus = Paid ✅

### Test 4: Webhook Retry
```
Process same webhook twice
```

Expected:
- PaidAmount only added once ✅
- No double-counting ✅

## Summary

✅ **Fixed**: Compare with `Cost` instead of `EstimatedAmount`  
✅ **Fixed**: Simple logic - only **Paid** or **Unpaid** (no partial status)  
✅ **Fixed**: Prevent double-counting in `MarkPaidAsync`  
✅ **Logic**: `PaidAmount >= Cost` → Paid, otherwise → Unpaid  
✅ **Impact**: RepairOrder.PaidStatus now correctly updates when payment is successful  
✅ **Compiles**: No errors  

**Now when payment is successful and PaidAmount >= Cost, the RepairOrder's PaidStatus is correctly updated to Paid!**

## Files Modified

- `Services/PaymentServices/PaymentService.cs`
  - `CreateManualPaymentAsync()` - Lines ~356-370
  - `MarkPaidAsync()` - Lines ~758-775
