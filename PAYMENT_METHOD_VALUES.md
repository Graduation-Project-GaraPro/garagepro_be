# Payment Method Enum Values

## PaymentMethod Enum

```csharp
public enum PaymentMethod
{
    PayOs = 0,  // QR Code / Online payment
    Cash = 1    // Cash payment
}
```

## API Request Examples

### Cash Payment
```http
POST /api/payments/manager-create/{repairOrderId}
Body: { "method": 1 }  // Cash = 1
```

### PayOS QR Payment
```http
POST /api/payments/manager-qr-payment/{repairOrderId}
Body: { "method": 0 }  // PayOs = 0
```

## Your Issue

You sent:
```json
{
  "method": 0  // ✅ Correct for PayOs
}
```

To endpoint:
```
POST /api/payments/manager-qr-payment/{repairOrderId}  // ✅ Correct endpoint
```

**The method value is correct!**

## Why You Got Null Values

The response shows:
```json
{
  "message": "PayOS QR payment link created successfully",
  "paymentId": 0,
  "orderCode": 0,
  "checkoutUrl": null,
  "qrCodeUrl": null
}
```

This means an **exception was thrown** but caught, and the default response was returned.

### Most Likely Cause:

The RepairOrder is **already marked as Paid** from your previous cash payment test.

The code checks:
```csharp
if (repairOrder.PaidStatus == PaidStatus.Paid)
{
    throw new Exception("Repair order is already fully paid. Cannot create another payment.");
}
```

## Solutions

### Option 1: Use a Different RepairOrder
Test with a RepairOrder that:
- ✅ Status = Completed (StatusId = 3)
- ✅ All jobs completed
- ✅ PaidStatus = Unpaid (not paid yet)

### Option 2: Check the Actual Error
Run the API again and check the console logs or response for the actual error message.

With the updated error handling, you should now see:
```json
{
  "message": "Repair order is already fully paid. Cannot create another payment.",
  "error": "..."
}
```

### Option 3: Reset the RepairOrder
If you want to test with the same RepairOrder, reset it in the database:
```sql
UPDATE RepairOrders 
SET PaidStatus = 0, PaidAmount = 0 
WHERE RepairOrderId = 'bf1e745f-5d9d-47d0-8f3f-7a1673f08bbd'
```

## Testing Flow

### Test 1: Cash Payment
```bash
# Use RepairOrder that is Completed but Unpaid
POST /api/payments/manager-create/{repairOrderId}
Body: { "method": 1 }  # Cash

Expected:
- Payment created
- PaidStatus = Paid ✅
- Can hand over vehicle
```

### Test 2: PayOS QR Payment
```bash
# Use DIFFERENT RepairOrder that is Completed but Unpaid
POST /api/payments/manager-qr-payment/{repairOrderId}
Body: { "method": 0 }  # PayOs

Expected:
- Payment link created
- checkoutUrl returned ✅
- Customer can scan QR and pay
- After payment, webhook updates PaidStatus = Paid
```

## Summary

✅ **Your method value is correct**: `"method": 0` for PayOs  
❌ **The RepairOrder is probably already paid** from previous test  
✅ **Solution**: Use a different RepairOrder that hasn't been paid yet  
✅ **Or**: Reset the current RepairOrder's PaidStatus to Unpaid  

Try the API call again with a different RepairOrder or check the error message in the response!
