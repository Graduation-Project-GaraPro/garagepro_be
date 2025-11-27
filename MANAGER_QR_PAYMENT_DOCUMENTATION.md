# Manager QR Payment Documentation

## Overview
This document describes the QR payment feature for managers, which reuses the PayOS implementation originally built for Android mobile app, now adapted for the manager web interface.

## Architecture

### Key Components

1. **PaymentService.CreateManagerPayOsPaymentAsync** - Core business logic for creating PayOS payment links for managers
2. **PaymentsController.ManagerCreateQrPayment** - API endpoint for manager QR payment creation
3. **PayOsClient** - Handles communication with PayOS API
4. **PayOsWebhookProcessor** - Background service that processes payment webhooks

## API Endpoints

### 1. Create Manager QR Payment

**Endpoint:** `POST /api/Payments/manager-qr-payment/{repairOrderId}`

**Authorization:** Manager role required

**Purpose:** Creates a PayOS QR payment link that can be displayed to customers for scanning and payment

**Request Body:**
```json
{
  "method": "PayOs",
  "description": "Payment for repair services"
}
```

**Response:**
```json
{
  "message": "PayOS QR payment link created successfully",
  "paymentId": 123456,
  "orderCode": 123456,
  "checkoutUrl": "https://pay.payos.vn/web/...",
  "qrCodeUrl": "https://pay.payos.vn/web/..."
}
```

**Business Rules:**
- ✅ User must be authenticated and have "Manager" role
- ✅ Repair order must exist
- ✅ Repair order status must be "Completed" (StatusId = 3)
- ✅ All jobs must be completed
- ✅ Cannot create payment if already paid
- ✅ If an unpaid PayOS payment already exists, returns existing payment link
- ✅ Amount is automatically calculated: `EstimatedAmount - PaidAmount`
- ✅ Payment status is set to "Unpaid" until customer completes payment
- ✅ Customer's user ID is used for payment tracking (not manager's ID)

### 2. Existing Endpoints (Reused)

#### Get Payment Preview
**Endpoint:** `GET /api/Payments/preview/{repairOrderId}`
- Shows payment breakdown before creating payment

#### Get Payment Status
**Endpoint:** `GET /api/Payments/status/{orderCode}`
- Check current status of a payment

#### Payment Webhook
**Endpoint:** `POST /api/Payments/webhook`
- Receives payment status updates from PayOS

## Payment Flow

### Manager Initiates QR Payment

```
1. Manager navigates to completed repair order
   ↓
2. Manager clicks "QR Payment" button
   ↓
3. Frontend calls GET /api/Payments/preview/{repairOrderId}
   - Shows customer info, services, parts, total amount
   ↓
4. Manager confirms and clicks "Generate QR Code"
   ↓
5. Frontend calls POST /api/Payments/manager-qr-payment/{repairOrderId}
   ↓
6. Backend creates payment record with status "Unpaid"
   ↓
7. Backend calls PayOS API to create payment link
   ↓
8. Backend returns checkout URL
   ↓
9. Frontend displays QR code (generated from checkout URL)
   ↓
10. Customer scans QR code with banking app
   ↓
11. Customer completes payment in banking app
   ↓
12. PayOS sends webhook to /api/Payments/webhook
   ↓
13. PayOsWebhookProcessor updates payment status to "Paid"
   ↓
14. RepairOrder.PaidStatus updated to "Paid"
   ↓
15. Manager sees updated status in real-time
```

## Service Implementation

### CreateManagerPayOsPaymentAsync

**Location:** `Services/PaymentServices/PaymentService.cs`

**Key Features:**
1. **Validation**
   - Validates repair order exists and is completed
   - Validates all jobs are completed
   - Checks for existing paid payments

2. **Idempotency**
   - Returns existing unpaid PayOS payment if one exists
   - Prevents duplicate payment creation

3. **Payment Creation**
   - Creates payment record with customer's user ID
   - Sets status to "Unpaid"
   - Amount calculated automatically

4. **PayOS Integration**
   - Generates payment link via PayOS API
   - Stores checkout URL in payment record
   - Returns link for QR code generation

5. **URL Configuration**
   - Return URL: `{baseUrl}/manager/payment/success?orderCode={paymentId}&repairOrderId={repairOrderId}`
   - Cancel URL: `{baseUrl}/api/payments/cancel?orderCode={paymentId}&reason=user_cancel`

**Code Example:**
```csharp
public async Task<CreatePaymentLinkResult> CreateManagerPayOsPaymentAsync(
    Guid repairOrderId, 
    string managerId, 
    string? description = null, 
    CancellationToken ct = default)
{
    // 1. Validate repair order
    var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
    
    // 2. Calculate amount
    var amountToPay = repairOrder.EstimatedAmount - repairOrder.PaidAmount;
    
    // 3. Check for existing payment
    var existingOpen = await _repo.GetByConditionAsync(
        p => p.RepairOrderId == repairOrderId && 
             p.Method == PaymentMethod.PayOs &&
             p.Status == PaymentStatus.Unpaid, ct);
    
    if (existingOpen != null)
        return new CreatePaymentLinkResult { ... };
    
    // 4. Create payment record
    var payment = new Payment { ... };
    await _repo.AddAsync(payment);
    
    // 5. Create PayOS link
    var payOsResponse = await _payos.CreatePaymentLinkAsync(payOsRequest, ct);
    
    // 6. Update and return
    payment.CheckoutUrl = payOsResponse.data.checkoutUrl;
    await _repo.UpdateAsync(payment, ct);
    
    return new CreatePaymentLinkResult { ... };
}
```

## Webhook Processing

### PayOsWebhookProcessor

**Location:** `Garage_pro_api/BackgroundServices/PayOsWebhookProcessor.cs`

**How It Works:**
1. Webhook endpoint receives payment notification from PayOS
2. Validates signature to ensure authenticity
3. Stores webhook data in `WebhookInbox` table
4. Background processor picks up pending webhooks
5. Updates payment status based on webhook data
6. Updates repair order paid status if payment successful

**Status Updates:**
- Payment code "00" → Status: Paid
- Other codes → Status: Cancelled
- When Paid: RepairOrder.PaidStatus → Paid

## Differences from Android Flow

### Android (Mobile App)
- Customer initiates payment from mobile app
- Deep links return to mobile app: `myapp://payment/success`
- Customer's user ID used throughout

### Manager Web
- Manager initiates payment on behalf of customer
- Returns to web URL: `/manager/payment/success`
- Customer's user ID used for payment tracking
- Manager can monitor payment status in real-time

## Frontend Integration Guide

### 1. Display QR Code

```typescript
// Call API to create payment link
const response = await fetch(`/api/Payments/manager-qr-payment/${repairOrderId}`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    method: 'PayOs',
    description: 'Payment for repair services'
  })
});

const data = await response.json();

// Generate QR code from checkout URL
// Option 1: Use QR code library (recommended)
import QRCode from 'qrcode';
const qrCodeDataUrl = await QRCode.toDataURL(data.checkoutUrl);

// Option 2: Use PayOS checkout URL directly
// Display the URL as a clickable link or QR code
```

### 2. Poll for Payment Status

```typescript
// Poll every 5 seconds to check payment status
const pollPaymentStatus = async (orderCode: number) => {
  const response = await fetch(`/api/Payments/status/${orderCode}`);
  const status = await response.json();
  
  if (status.status === 'Paid') {
    // Payment successful - refresh UI
    showSuccessMessage();
    refreshRepairOrderData();
    return true;
  }
  
  return false;
};

// Start polling
const intervalId = setInterval(async () => {
  const isPaid = await pollPaymentStatus(orderCode);
  if (isPaid) {
    clearInterval(intervalId);
  }
}, 5000);
```

### 3. Handle Payment Success

```typescript
// When payment is detected as paid
const handlePaymentSuccess = async () => {
  // 1. Show success notification
  toast.success('Payment received successfully!');
  
  // 2. Refresh payment summary
  await loadPaymentSummary();
  
  // 3. Refresh repair order (status will be updated)
  await loadRepairOrder();
  
  // 4. Close QR code dialog
  closeQrDialog();
};
```

## Testing

### Test Scenarios

1. **Happy Path - QR Payment Success**
   - Manager creates QR payment
   - Customer scans and pays
   - Webhook received
   - Status updated to Paid
   - Repair order marked as Paid

2. **Idempotency - Duplicate Request**
   - Manager creates QR payment
   - Manager clicks again before payment
   - Same payment link returned
   - No duplicate payment created

3. **Validation - Incomplete Order**
   - Try to create payment for incomplete order
   - Should fail with appropriate error

4. **Validation - Already Paid**
   - Try to create payment for paid order
   - Should fail with appropriate error

5. **Webhook - Invalid Signature**
   - Webhook with invalid signature
   - Should be rejected

6. **Webhook - Payment Cancelled**
   - Customer cancels payment
   - Status updated to Cancelled

### Test with PayOS Sandbox

```bash
# PayOS Sandbox Configuration (in appsettings.json)
{
  "PayOs": {
    "ClientId": "your-sandbox-client-id",
    "ApiKey": "your-sandbox-api-key",
    "ChecksumKey": "your-sandbox-checksum-key",
    "BaseUrl": "https://api-merchant.payos.vn"
  }
}
```

## Error Handling

### Common Errors

1. **"Repair order must be in Completed status"**
   - Cause: Trying to create payment for non-completed order
   - Solution: Complete all jobs first

2. **"All jobs must be completed"**
   - Cause: Some jobs still in progress
   - Solution: Complete all jobs before payment

3. **"Payment already paid"**
   - Cause: Trying to create duplicate payment
   - Solution: Check payment history

4. **"PayOS error when creating link"**
   - Cause: PayOS API error
   - Solution: Check PayOS configuration and credentials

5. **"Repair order is already fully paid"**
   - Cause: PaidAmount >= EstimatedAmount
   - Solution: No action needed

## Security Considerations

1. **Authorization**
   - Only managers can create QR payments
   - JWT token required for all endpoints

2. **Webhook Validation**
   - Signature verification prevents fake webhooks
   - Only valid PayOS webhooks are processed

3. **Idempotency**
   - Prevents duplicate payments
   - Safe to retry failed requests

4. **Amount Validation**
   - Amount calculated by backend
   - Frontend cannot manipulate amount

## Configuration

### Required Settings (appsettings.json)

```json
{
  "App": {
    "BaseUrl": "https://your-domain.com"
  },
  "PayOs": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key",
    "BaseUrl": "https://api-merchant.payos.vn"
  }
}
```

## Summary

The manager QR payment feature successfully reuses the PayOS implementation from the Android app with these key adaptations:

✅ **Reused Components:**
- PayOsClient for API communication
- PayOsWebhookProcessor for status updates
- Payment entity and repository
- Webhook validation logic

✅ **New Components:**
- CreateManagerPayOsPaymentAsync service method
- ManagerCreateQrPayment controller endpoint
- Manager-specific return URLs

✅ **Key Benefits:**
- No code duplication
- Consistent payment processing
- Unified webhook handling
- Same security and validation rules
- Easy to maintain and extend

The implementation is production-ready and follows best practices for payment processing, security, and error handling.
