# Payment Status Update Logic

## Business Rule

**Cash Payment** → Update PaidStatus immediately (customer already paid)  
**PayOS Payment** → Update PaidStatus later when webhook confirms (customer hasn't paid yet)

## Implementation

### CreateManualPaymentAsync (Manager Creates Payment)

```csharp
// Only update PaidStatus for Cash payments
if (method == PaymentMethod.Cash)
{
    repairOrder.PaidStatus = PaidStatus.Paid;
    repairOrder.PaidAmount = repairOrder.Cost;
    await _repoRepairOrder.UpdateAsync(repairOrder);
}
// For PayOS, PaidStatus stays Unpaid until webhook processed
```

### MarkPaidAsync (Webhook Processing)

```csharp
// When payment is marked as paid (webhook confirms), update RO
repairOrder.PaidStatus = PaidStatus.Paid;
repairOrder.PaidAmount = repairOrder.Cost;
await _repoRepairOrder.UpdateAsync(repairOrder);
```

## Flow Comparison

### Cash Payment Flow

```
Manager: "Confirm Cash Payment"
↓
CreateManualPaymentAsync()
├─ Create Payment (Status = Paid)
├─ ✅ Update RepairOrder.PaidStatus = Paid
├─ ✅ Update RepairOrder.PaidAmount = Cost
└─ Send SignalR notifications
↓
Done! Customer can take vehicle
```

### PayOS Payment Flow

```
Manager: "Generate QR Code"
↓
CreateManagerPayOsPaymentAsync()
├─ Create Payment (Status = Unpaid)
├─ Call PayOS API
├─ Get CheckoutUrl
└─ ❌ Do NOT update RepairOrder.PaidStatus (customer hasn't paid yet)
↓
Customer scans QR and pays
↓
PayOS sends webhook
↓
Manager processes webhook
↓
MarkPaidAsync()
├─ Update Payment.Status = Paid
├─ ✅ Update RepairOrder.PaidStatus = Paid
├─ ✅ Update RepairOrder.PaidAmount = Cost
└─ Send SignalR notifications
↓
Done! Customer can take vehicle
```

## Why This Makes Sense

### Cash Payment
- ✅ Customer **already paid** cash at counter
- ✅ Manager has the money in hand
- ✅ Safe to update PaidStatus immediately
- ✅ Customer can take vehicle right away

### PayOS Payment
- ❌ Customer **hasn't paid yet** (just got QR code)
- ❌ Money not received yet
- ❌ Cannot update PaidStatus yet
- ✅ Wait for webhook confirmation
- ✅ Only update when PayOS confirms payment

## Database State

### After Cash Payment Created
```
Payment:
  PaymentId: 123456789
  Method: Cash
  Status: Paid ✅
  Amount: 1500.00

RepairOrder:
  PaidStatus: Paid ✅
  PaidAmount: 1500.00 ✅
```

### After PayOS Link Created
```
Payment:
  PaymentId: 123456790
  Method: PayOs
  Status: Unpaid ⏳
  Amount: 1500.00
  CheckoutUrl: "https://pay.payos.vn/..."

RepairOrder:
  PaidStatus: Unpaid ⏳ (waiting for customer to pay)
  PaidAmount: 0
```

### After PayOS Webhook Processed
```
Payment:
  PaymentId: 123456790
  Method: PayOs
  Status: Paid ✅
  Amount: 1500.00

RepairOrder:
  PaidStatus: Paid ✅
  PaidAmount: 1500.00 ✅
```

## SignalR Notifications

### Cash Payment
```javascript
// Event: PaymentReceived
{
  "method": "Cash",
  "status": "Paid",
  "paidStatus": "Paid",  // ✅ Immediately Paid
  "message": "Cash payment created successfully"
}
```

### PayOS Link Created
```javascript
// Event: PaymentReceived
{
  "method": "PayOs",
  "status": "Unpaid",
  "paidStatus": "Unpaid",  // ⏳ Still Unpaid
  "message": "PayOs payment created successfully"
}
```

### PayOS Payment Confirmed
```javascript
// Event: PaymentConfirmed
{
  "method": "PayOs",
  "oldStatus": "Unpaid",
  "newStatus": "Paid",
  "paidStatus": "Paid",  // ✅ Now Paid
  "message": "Payment confirmed and processed"
}
```

## Manager Dashboard View

### Payment History

```
┌─────────────────────────────────────────────────────┐
│  Payment History - RO #123                          │
├─────────────────────────────────────────────────────┤
│  💵 Cash Payment                                    │
│     Amount: $1,500.00                               │
│     Status: ✅ Paid                                 │
│     Date: 2024-12-03 10:00                          │
│     RO Status: ✅ Paid                              │
├─────────────────────────────────────────────────────┤
│  📱 PayOS Payment                                   │
│     Amount: $2,000.00                               │
│     Status: ⏳ Unpaid (Waiting for customer)        │
│     QR Code: [Show QR]                              │
│     RO Status: ⏳ Unpaid                            │
│                                                     │
│     After customer pays:                            │
│     Status: ✅ Paid                                 │
│     RO Status: ✅ Paid                              │
└─────────────────────────────────────────────────────┘
```

## API Behavior

### POST /api/payments/manager-create/{repairOrderId}
```json
Request: { "method": 1 }  // Cash

Response:
{
  "message": "Payment record created successfully",
  "paymentId": 123456789,
  "method": "Cash",
  "status": "Paid"
}

Database:
- Payment.Status = Paid ✅
- RepairOrder.PaidStatus = Paid ✅
```

### POST /api/payments/manager-qr-payment/{repairOrderId}
```json
Request: { "method": 0 }  // PayOs

Response:
{
  "message": "PayOS QR payment link created successfully",
  "paymentId": 123456790,
  "checkoutUrl": "https://pay.payos.vn/..."
}

Database:
- Payment.Status = Unpaid ⏳
- RepairOrder.PaidStatus = Unpaid ⏳ (no change)
```

### Webhook Processing (Later)
```
Webhook received → MarkPaidAsync() called

Database:
- Payment.Status = Paid ✅
- RepairOrder.PaidStatus = Paid ✅
```

## Summary

✅ **Cash Payment** → PaidStatus updated immediately in `CreateManualPaymentAsync`  
✅ **PayOS Payment** → PaidStatus updated later in `MarkPaidAsync` (webhook)  
✅ **Correct Logic** → Only update when money is actually received  
✅ **SignalR Notifications** → Sent at appropriate times  

**Now the payment status updates correctly based on payment method!**
