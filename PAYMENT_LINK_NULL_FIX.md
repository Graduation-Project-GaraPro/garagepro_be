# Payment Link Null CheckoutUrl Fix

## Problem

**Scenario**:
1. Manager creates PayOS payment link
2. PayOS API fails or returns null
3. Payment record is saved with `CheckoutUrl = null`
4. Manager tries to create payment link again
5. System finds existing payment and returns it (with null CheckoutUrl)
6. **Result**: Manager gets null link and cannot create a new one

## Root Cause

The code checked for existing unpaid PayOS payments and returned them immediately, **without checking if CheckoutUrl is valid**:

```csharp
// ❌ Before - Returns even if CheckoutUrl is null
if (existingOpen != null)
{
    return new CreatePaymentLinkResult 
    { 
        PaymentId = existingOpen.PaymentId, 
        OrderCode = existingOpen.PaymentId, 
        CheckoutUrl = existingOpen.CheckoutUrl  // Could be null!
    };
}
```

## Solution

Now the code:
- ✅ Uses **transaction** for atomicity
- ✅ Checks if CheckoutUrl is valid
- ✅ If valid → Return existing link
- ✅ If null → Delete old payment and create new one
- ✅ Commits transaction on success
- ✅ Rolls back transaction on error

```csharp
// ✅ After - With transaction and CheckoutUrl validation
await using var transaction = await _db.Database.BeginTransactionAsync(ct);

try
{
    // Check if CheckoutUrl is valid
    if (existingOpen != null)
    {
        // If existing payment has valid CheckoutUrl, return it
        if (!string.IsNullOrEmpty(existingOpen.CheckoutUrl))
        {
            Console.WriteLine($"[PaymentService] Returning existing payment link for payment {existingOpen.PaymentId}");
            return new CreatePaymentLinkResult 
            { 
                PaymentId = existingOpen.PaymentId, 
                OrderCode = existingOpen.PaymentId, 
                CheckoutUrl = existingOpen.CheckoutUrl 
            };
        }
        
        // If CheckoutUrl is null (PayOS failed before), delete and create new one
        Console.WriteLine($"[PaymentService] Existing payment {existingOpen.PaymentId} has null CheckoutUrl, deleting and creating new one");
        _db.Payments.Remove(existingOpen);
        await _db.SaveChangesAsync(ct);
    }
    
    // Create new payment...
    // Call PayOS API...
    
    // Commit transaction
    await transaction.CommitAsync(ct);
    return result;
}
catch
{
    // Rollback on error
    await transaction.RollbackAsync(ct);
    throw;
}
```

## Flow Diagram

### Before Fix:
```
Manager: Create QR Payment
↓
Check existing payment → Found with CheckoutUrl = null
↓
Return null CheckoutUrl ❌
↓
Manager: Cannot create new link (stuck!)
```

### After Fix:
```
Manager: Create QR Payment
↓
Check existing payment → Found with CheckoutUrl = null
↓
Delete old payment ✅
↓
Create new payment
↓
Call PayOS API
↓
Return valid CheckoutUrl ✅
```

## Benefits

✅ **Automatic Recovery** - If PayOS fails, next attempt will retry  
✅ **No Manual Cleanup** - Old failed payments are automatically deleted  
✅ **Better UX** - Manager can always get a valid link  
✅ **Logging** - Console logs show what's happening  

## Testing

### Test 1: Normal Flow (No Existing Payment)
```http
POST /api/payments/manager-qr-payment/{repairOrderId}
Body: { "method": 0 }
```

Expected:
- Creates new payment
- Calls PayOS API
- Returns valid CheckoutUrl ✅

### Test 2: Existing Valid Link
```http
POST /api/payments/manager-qr-payment/{repairOrderId}
Body: { "method": 0 }
```

Expected:
- Finds existing payment with valid CheckoutUrl
- Returns existing link (no new API call) ✅
- Console: "Returning existing payment link for payment {id}"

### Test 3: Existing Null Link (The Fix!)
```http
POST /api/payments/manager-qr-payment/{repairOrderId}
Body: { "method": 0 }
```

Expected:
- Finds existing payment with null CheckoutUrl
- Deletes old payment ✅
- Creates new payment
- Calls PayOS API
- Returns valid CheckoutUrl ✅
- Console: "Existing payment {id} has null CheckoutUrl, deleting and creating new one"

## Edge Cases Handled

### Case 1: PayOS API Fails
```
Create payment → PayOS fails → CheckoutUrl = null
↓
Next attempt → Detects null → Deletes old → Creates new → Retry PayOS
```

### Case 2: Multiple Retries
```
Attempt 1: PayOS fails → CheckoutUrl = null
Attempt 2: Detects null → Deletes → Creates new → PayOS fails again → CheckoutUrl = null
Attempt 3: Detects null → Deletes → Creates new → PayOS succeeds → Valid link ✅
```

### Case 3: Valid Link Exists
```
Attempt 1: Creates payment → Valid CheckoutUrl
Attempt 2: Finds existing → Returns same link (efficient, no duplicate API calls)
```

## Summary

✅ **Fixed**: Existing payments with null CheckoutUrl are now deleted and recreated  
✅ **Benefit**: Manager can always retry if PayOS fails  
✅ **Logging**: Console logs show the recovery process  
✅ **No Breaking Changes**: Existing valid links still work  

**Now if PayOS fails and returns null, the next attempt will automatically clean up and retry!**
