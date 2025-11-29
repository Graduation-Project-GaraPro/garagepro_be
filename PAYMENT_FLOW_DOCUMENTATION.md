# Payment Flow Documentation - Backend & Frontend

## Overview
Complete documentation of the cash payment flow for repair orders, including backend business logic and frontend integration.

## Backend Implementation

### Key Business Logic (PaymentService.cs)

#### 1. Payment Preview (`GetPaymentPreviewAsync`)
**Purpose:** Provides detailed breakdown of repair order costs before payment

**Business Rules:**
- ✅ Repair order must exist
- ✅ Repair order status must be "Completed" (StatusId = 3)
- ✅ Returns decided services from `RepairOrderServices` (not from jobs to avoid duplication)
- ✅ Returns parts from `JobParts`
- ✅ Calculates discount from approved quotations only
- ✅ TotalAmount = RepairOrderCost - DiscountAmount
- ✅ Shows customer name and vehicle info

**Response Structure:**
```csharp
{
  RepairOrderId: Guid,
  RepairOrderCost: decimal,
  EstimatedAmount: decimal,
  PaidAmount: decimal,
  DiscountAmount: decimal,
  TotalAmount: decimal,
  CustomerName: string,
  VehicleInfo: string,
  Services: List<ServicePreviewDto>,
  Parts: List<PartPreviewDto>,
  Quotations: List<QuotationInfoDto>
}
```

#### 2. Manual Payment Creation (`CreateManualPaymentAsync`)
**Purpose:** Manager creates payment record for completed repair order

**Business Rules:**
- ✅ Amount must be > 0
- ✅ Method must be Cash or PayOs
- ✅ Repair order must exist
- ✅ Repair order status must be "Completed" (StatusId = 3)
- ✅ All jobs must be completed
- ✅ Cannot create payment if already paid
- ✅ **For Cash payments:**
  - Status is immediately set to "Paid"
  - PaymentDate is set to current time
  - RepairOrder.PaidAmount is updated
  - **RepairOrder.PaidStatus is updated to "Paid" if fully paid**
- ✅ **For PayOs payments:**
  - Status is set to "Unpaid" (until customer pays)
  - PaymentDate is not set yet

**Key Code:**
```csharp
// Create payment record
var payment = new Payment
{
    RepairOrderId = repairOrderId,
    UserId = managerId,
    Amount = amount,
    Method = method,
    Status = method == PaymentMethod.Cash ? PaymentStatus.Paid : PaymentStatus.Unpaid,
    PaymentDate = method == PaymentMethod.Cash ? DateTime.UtcNow : DateTime.MinValue,
    UpdatedAt = DateTime.UtcNow
};

// Update repair order paid status (only for cash payments)
if (method == PaymentMethod.Cash)
{
    repairOrder.PaidAmount += amount;
    if (repairOrder.PaidAmount >= repairOrder.EstimatedAmount)
    {
        repairOrder.PaidStatus = PaidStatus.Paid; // ✅ STATUS CHANGES HERE
    }
    else
    {
        repairOrder.PaidStatus = PaidStatus.Unpaid; // Partial payment
    }
    
    await _repoRepairOrder.UpdateAsync(repairOrder);
    await _repoRepairOrder.Context.SaveChangesAsync(ct);
}
```

#### 3. Payment Summary (`GetPaymentSummaryAsync`)
**Purpose:** Get complete payment information including history

**Returns:**
- Customer and vehicle information
- Repair order cost
- Total discount from quotations
- Amount to pay
- Paid status
- Payment history with all transactions

### API Endpoints (PaymentsController.cs)

#### 1. GET `/api/Payments/preview/{repairOrderId}`
**Authorization:** Manager only
**Purpose:** Get payment preview before processing payment

**Response:**
```json
{
  "repairOrderId": "guid",
  "repairOrderCost": 0,
  "estimatedAmount": 400000,
  "paidAmount": 0,
  "discountAmount": 0,
  "totalAmount": 400000,
  "customerName": "John Doe",
  "vehicleInfo": "Honda Civic (ABC123)",
  "services": [
    {
      "serviceId": "guid",
      "serviceName": "Emissions Test",
      "price": 400000,
      "estimatedDuration": 1
    }
  ],
  "parts": [],
  "quotations": []
}
```

#### 2. POST `/api/Payments/manager-create/{repairOrderId}`
**Authorization:** Manager only
**Purpose:** Create payment record for repair order

**Request Body:**
```json
{
  "method": "Cash",  // or "PayOs"
  "description": "Cash payment for repair services"
}
```

**Business Logic:**
1. Validates user is a manager
2. Validates payment method (Cash or PayOs only)
3. Gets repair order and validates:
   - Repair order exists
   - Status is "Completed" (StatusId = 3)
   - All jobs are completed
   - No existing paid payment
4. Calculates amount to pay: `EstimatedAmount - PaidAmount`
5. Creates payment record via `CreateManualPaymentAsync`
6. **For Cash: Backend automatically updates RepairOrder.PaidStatus to "Paid"**
7. For PayOs: Generates QR code
8. Returns payment information

**Response:**
```json
{
  "message": "Payment record created successfully",
  "paymentId": 123,
  "method": "Cash",
  "amount": 400000,
  "status": "Paid",
  "qrCodeData": null
}
```

#### 3. GET `/api/Payments/summary/{repairOrderId}`
**Authorization:** Authenticated users
**Purpose:** Get payment summary with history

**Response:**
```json
{
  "repairOrderId": "guid",
  "customerName": "John Doe",
  "vehicleInfo": "Honda Civic (ABC123)",
  "repairOrderCost": 400000,
  "totalDiscount": 0,
  "amountToPay": 400000,
  "paidStatus": "Paid",
  "paymentHistory": [
    {
      "paymentId": 123,
      "method": "Cash",
      "status": "Paid",
      "amount": 400000,
      "paymentDate": "2024-01-15T10:30:00Z",
      "processedBy": "manager@example.com"
    }
  ]
}
```

## Frontend Implementation

### Payment Service (payment-service.ts)

```typescript
async getPaymentPreview(repairOrderId: string): Promise<PaymentPreviewResponse> {
  const response = await apiClient.get<PaymentPreviewResponse>(
    `/Payments/preview/${repairOrderId}`
  );
  return response.data;
}

async createPayment(
  repairOrderId: string,
  request: CreatePaymentRequest
): Promise<CreatePaymentResponse> {
  const response = await apiClient.post<CreatePaymentResponse>(
    `/payments/manager-create/${repairOrderId}`,
    request
  );
  return response.data;
}
```

### Payment Tab Component (payment-tab.tsx)

**User Flow:**
1. Manager navigates to completed repair order's Payment tab
2. Manager clicks "Cash Payment" button
3. Frontend calls `GET /api/Payments/preview/{repairOrderId}`
4. Dialog displays:
   - Customer & vehicle information
   - List of decided services from approved quotation
   - List of parts from jobs
   - Cost breakdown (estimated, repair order cost, discount, total, paid, balance)
5. Manager reviews and optionally adds description
6. Manager clicks "Confirm Payment"
7. Frontend calls `POST /api/payments/manager-create/{repairOrderId}`
8. **Backend processes payment and updates RepairOrder.PaidStatus to "Paid"**
9. Frontend shows success toast
10. Frontend refreshes payment summary (transaction history)
11. **Frontend refreshes repair order data (status updates to "Paid")**
12. Dialog closes

**Key Features:**
- ✅ Loading states during API calls
- ✅ Error handling with toast notifications
- ✅ Automatic refresh of payment history after payment
- ✅ Automatic refresh of repair order status via callback
- ✅ Disabled state when fully paid
- ✅ Optional description field

## Status Update Flow

### Backend (Automatic)
When cash payment is created:
```csharp
// In CreateManualPaymentAsync
if (method == PaymentMethod.Cash)
{
    repairOrder.PaidAmount += amount;
    if (repairOrder.PaidAmount >= repairOrder.EstimatedAmount)
    {
        repairOrder.PaidStatus = PaidStatus.Paid; // ✅ Automatic status update
    }
    await _repoRepairOrder.UpdateAsync(repairOrder);
    await _repoRepairOrder.Context.SaveChangesAsync(ct);
}
```

### Frontend (Refresh)
After successful payment:
```typescript
// In handleConfirmPayment
await paymentService.createPayment(orderId, { method: "Cash", description });

// Reload payment summary
await loadPaymentSummary();

// Notify parent to refresh repair order (including status)
if (onPaymentSuccess) {
  onPaymentSuccess(); // ✅ Triggers repair order refresh
}
```

## Validation Rules

### Backend Validations
1. ✅ User must be authenticated
2. ✅ User must have "Manager" role
3. ✅ Repair order must exist
4. ✅ Repair order status must be "Completed" (StatusId = 3)
5. ✅ All jobs must be completed
6. ✅ No existing paid payment
7. ✅ Amount must be > 0
8. ✅ Payment method must be Cash or PayOs

### Frontend Validations
1. ✅ Repair order must be completed (repairOrderStatus === 3)
2. ✅ Shows informative message for incomplete orders
3. ✅ Disables payment when balance is already paid
4. ✅ Shows loading states during API calls

## Error Handling

### Backend Errors
- `Unauthorized`: User not authenticated or not a manager
- `NotFound`: Repair order not found
- `BadRequest`: 
  - Repair order not completed
  - Jobs not completed
  - Already paid
  - Invalid payment method
  - Invalid amount

### Frontend Error Handling
- Network errors: Shows toast with error message
- API errors: Shows toast with backend error message
- Validation errors: Shows toast with validation message
- Loading states: Shows spinner during operations

## Testing Checklist

### Backend Tests
- [ ] Preview returns correct data for completed repair order
- [ ] Preview fails for non-completed repair order
- [ ] Cash payment creates record with "Paid" status
- [ ] Cash payment updates RepairOrder.PaidStatus to "Paid"
- [ ] Cash payment updates RepairOrder.PaidAmount
- [ ] PayOs payment creates record with "Unpaid" status
- [ ] Cannot create payment if already paid
- [ ] Cannot create payment if jobs not completed
- [ ] Manager authorization works correctly
- [ ] Non-manager users are rejected

### Frontend Tests
- [ ] Preview dialog opens when clicking Cash Payment
- [ ] Preview shows correct customer and vehicle info
- [ ] Services list displays correctly
- [ ] Parts list displays correctly
- [ ] Payment summary calculations are correct
- [ ] Confirm button processes payment
- [ ] Success toast appears after payment
- [ ] Transaction history updates automatically
- [ ] Repair order status refreshes to "Paid"
- [ ] Dialog closes after successful payment
- [ ] Error handling works for API failures
- [ ] Loading states display correctly

## Summary

The payment flow is **fully handled by the backend**:
- ✅ Business logic in `PaymentService.cs`
- ✅ Validation rules enforced by backend
- ✅ Status updates automatic on backend
- ✅ Frontend only displays data and triggers actions
- ✅ Frontend refreshes to show updated status

**Key Point:** When a cash payment is successfully created, the backend automatically:
1. Creates payment record with "Paid" status
2. Updates RepairOrder.PaidAmount
3. Updates RepairOrder.PaidStatus to "Paid" (if fully paid)
4. Saves all changes to database

The frontend simply needs to refresh the data to display the updated status.
