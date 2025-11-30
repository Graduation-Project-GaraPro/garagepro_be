# Quotation Flow - UI Implementation Summary

## Overview
**You are correct** - there is NO manual "Create Quotation" functionality. Quotations are **automatically generated** from completed inspections.

## ✨ Multiple Quotations Support
**NEW:** The system now supports **multiple quotations** for the same repair order. This allows:
- Creating alternative quotes with different service/part combinations
- Offering customers multiple repair options (e.g., basic vs. premium)
- Revising quotes based on customer feedback
- Each quotation can be independently approved/rejected by the customer

## Quotation Lifecycle Flow

### 1. Inspection Completion (Technician)
- Technician completes an inspection
- Inspection status becomes `Completed`
- Inspection contains:
  - Selected services (ServiceInspections)
  - Recommended parts (PartInspections)

### 2. Convert Inspection to Quotation (Manager)
**Endpoint:** `POST /api/Inspection/convert-to-quotation`

**Request Body:**
```json
{
  "inspectionId": "guid",
  "note": "string (optional)"
}
```

**What Happens:**
- System automatically creates a quotation from the completed inspection
- All services from inspection are included
- All parts recommended by technician are pre-selected
- Quotation status is set to `Pending`
- Returns the created `QuotationDto`
- **Multiple quotations can be created from the same inspection**

**Business Rules:**
- Only `Completed` inspections can be converted
- Inspection must have a valid RepairOrder
- Services and parts are automatically mapped
- **No limit on number of quotations per repair order**

---

## Quotation Status Flow

```
Pending → Sent → Approved/Rejected
                    ↓
                 Expired (if not responded)
```

### Status Definitions:
- **Pending**: Quotation created, not yet sent to customer
- **Sent**: Quotation sent to customer, awaiting response
- **Approved**: Customer accepted the quotation
- **Rejected**: Customer declined the quotation
- **Expired**: Quotation expired without customer response

---

## API Endpoints for UI

### Manager Operations

#### 1. Get All Quotations
```
GET /api/Quotations
Response: QuotationDto[]
```

#### 2. Get Quotation by ID
```
GET /api/Quotations/{id}
Response: QuotationDto
```

#### 3. Get Quotation Details (Full Info)
```
GET /api/Quotations/{id}/details
Response: QuotationDetailDto (includes services, parts, pricing)
```

#### 4. Get Quotations by Repair Order
```
GET /api/Quotations/repair-order/{repairOrderId}
Response: QuotationDto[]
```

#### 5. Get Quotations by Inspection
```
GET /api/Quotations/inspection/{inspectionId}
Response: QuotationDto[]
```

#### 6. Update Quotation
```
PUT /api/Quotations/{id}
Body: UpdateQuotationDto
Response: QuotationDto
```

**UpdateQuotationDto:**
```json
{
  "note": "string",
  "quotationServices": [
    {
      "quotationServiceId": "guid",
      "isSelected": true,
      "quotationServiceParts": [
        {
          "quotationServicePartId": "guid",
          "isSelected": true,
          "quantity": 1
        }
      ]
    }
  ]
}
```

#### 7. Update Quotation Status
```
PUT /api/Quotations/{id}/status
Body: UpdateQuotationStatusDto
Response: QuotationDto
```

**UpdateQuotationStatusDto:**
```json
{
  "status": "Sent" | "Approved" | "Rejected" | "Expired"
}
```

#### 8. Copy Approved Quotation to Jobs (Manager Only)
```
POST /api/Quotations/{id}/copy-to-jobs
Response: boolean
```

**What Happens:**
- Creates Job entries from approved quotation services
- Creates JobPart entries from approved quotation parts
- Jobs are created with status `Pending`
- Manager can then assign these jobs to technicians
- **Multiple approved quotations can create jobs** (useful for phased repairs)

**Business Rules:**
- Only `Approved` quotations can be copied
- **Multiple quotations can be copied to jobs** (no duplication check)
- Each quotation creates separate jobs

#### 9. Create Revision Jobs (Manager Only)
```
POST /api/Quotations/{id}/create-revision-jobs
Body: { "revisionReason": "string" }
Response: boolean
```

**Use Case:** When quotation is updated after initial approval

#### 10. Delete Quotation
```
DELETE /api/Quotations/{id}
Response: 204 No Content
```

---

### Customer Operations

#### 1. Get Customer's Quotations (Paginated)
```
GET /api/Quotations/user?pageNumber=1&pageSize=10&status=Pending
Response: Paginated QuotationDto[]
```

**Query Parameters:**
- `pageNumber`: int (default: 1)
- `pageSize`: int (default: 10)
- `status`: QuotationStatus enum (optional filter)

#### 2. Customer Response to Quotation
```
PUT /api/CustomerQuotations/customer-response
Body: CustomerQuotationResponseDto
Response: QuotationDto
```

**CustomerQuotationResponseDto:**
```json
{
  "quotationId": "guid",
  "isApproved": true,
  "rejectionReason": "string (required if isApproved = false)",
  "selectedServices": [
    {
      "quotationServiceId": "guid",
      "isSelected": true,
      "selectedParts": [
        {
          "quotationServicePartId": "guid",
          "isSelected": true,
          "quantity": 1
        }
      ]
    }
  ]
}
```

**What Happens:**
- If `isApproved = true`: Status → `Approved`
- If `isApproved = false`: Status → `Rejected`
- Customer can select/deselect services and parts
- System recalculates total amount based on selections

---

## Key DTOs Structure

### QuotationDto (Summary)
```typescript
{
  quotationId: string;
  inspectionId: string;
  repairOrderId: string;
  userId: string;
  vehicleId: string;
  status: "Pending" | "Sent" | "Approved" | "Rejected" | "Expired";
  totalAmount: number;
  note: string;
  createdAt: Date;
  updatedAt: Date;
  expiryDate: Date;
}
```

### QuotationDetailDto (Full Details)
```typescript
{
  ...QuotationDto,
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  vehicleLicensePlate: string;
  vehicleBrand: string;
  vehicleModel: string;
  quotationServices: [
    {
      quotationServiceId: string;
      serviceId: string;
      serviceName: string;
      servicePrice: number;
      isSelected: boolean;
      isRequired: boolean;
      quotationServiceParts: [
        {
          quotationServicePartId: string;
          partId: string;
          partName: string;
          unitPrice: number;
          quantity: number;
          isSelected: boolean;
          subtotal: number;
        }
      ];
      serviceSubtotal: number;
    }
  ];
  selectedServicesTotal: number;
  selectedPartsTotal: number;
  grandTotal: number;
}
```

---

## Managing Multiple Quotations

### Use Cases for Multiple Quotations:

1. **Alternative Repair Options**
   - Basic repair (essential services only)
   - Standard repair (recommended services)
   - Premium repair (all services + upgrades)

2. **Phased Repairs**
   - Immediate repairs (safety-critical)
   - Phase 2 repairs (maintenance)
   - Phase 3 repairs (cosmetic)

3. **Budget Options**
   - OEM parts quotation
   - Aftermarket parts quotation
   - Mixed parts quotation

4. **Revision After Customer Feedback**
   - Original quotation
   - Revised quotation based on customer budget
   - Final negotiated quotation

### Best Practices:

1. **Naming Convention**
   - Use descriptive notes: "Basic Package", "Premium Package", "Budget Option"
   - Include version numbers: "Quote v1", "Quote v2 (Revised)"

2. **Status Management**
   - Only send one quotation at a time to avoid customer confusion
   - Mark superseded quotations as `Rejected` or `Expired`
   - Clearly indicate which quotation is the "active" one

3. **UI Display**
   - Show all quotations in a list with status badges
   - Highlight the most recent or active quotation
   - Allow filtering by status
   - Show comparison view for multiple quotations

4. **Customer Communication**
   - Send quotations sequentially, not simultaneously
   - Explain differences between quotations
   - Set clear expiry dates

---

## UI Implementation Workflow

### Manager Workflow

1. **View Completed Inspections**
   - GET `/api/Inspection/completed-with-details`
   - Show list of completed inspections

2. **Convert Inspection to Quotation**
   - Button: "Create Quotation from Inspection"
   - POST `/api/Inspection/convert-to-quotation`
   - **Can create multiple quotations from same inspection**
   - Show success message

3. **View All Quotations for Repair Order**
   - GET `/api/Quotations/repair-order/{repairOrderId}`
   - Display list with status badges
   - Show comparison table if multiple quotations exist
   - Highlight differences between quotations

4. **View/Edit Quotation**
   - GET `/api/Quotations/{id}/details`
   - Allow manager to:
     - Add/remove services
     - Add/remove parts
     - Adjust quantities
     - Add descriptive notes (e.g., "Budget Option", "Premium Package")
   - PUT `/api/Quotations/{id}` to save changes

5. **Create Alternative Quotation**
   - Button: "Create Alternative Quote"
   - Duplicate existing quotation
   - Modify services/parts for different option
   - Save as new quotation

6. **Send Quotation to Customer**
   - Button: "Send to Customer"
   - PUT `/api/Quotations/{id}/status` with `status: "Sent"`
   - Optionally mark other quotations as `Expired`
   - Trigger notification to customer

7. **After Customer Approval**
   - Button: "Create Jobs"
   - POST `/api/Quotations/{id}/copy-to-jobs`
   - **Multiple approved quotations can create jobs**
   - Navigate to Job Assignment screen

### Customer Workflow

1. **View Quotations**
   - GET `/api/Quotations/user`
   - Show list with filters (Pending, Sent, Approved, Rejected)
   - **May see multiple quotations for same repair order**
   - Group by repair order for clarity

2. **Compare Multiple Quotations**
   - If multiple quotations exist for same repair order
   - Show side-by-side comparison
   - Highlight differences in services, parts, and pricing
   - Help customer make informed decision

3. **View Quotation Details**
   - GET `/api/Quotations/{id}/details`
   - Show all services and parts with prices
   - Allow customer to select/deselect optional items
   - Show real-time total calculation
   - Display quotation note/description (e.g., "Budget Option")

4. **Approve/Reject Quotation**
   - Buttons: "Approve" / "Reject"
   - PUT `/api/CustomerQuotations/customer-response`
   - **Customer can approve multiple quotations** (for phased repairs)
   - Show confirmation message
   - Optionally ask if customer wants to reject other pending quotations

---

## Important Business Rules

### ✅ DO:
- Convert only `Completed` inspections to quotations
- Allow manager to edit quotations before sending
- Allow customer to select/deselect optional services/parts
- Create jobs only from `Approved` quotations
- Validate that quotation hasn't expired before customer response
- **Allow creating multiple quotations for the same repair order**
- **Allow multiple approved quotations to create jobs**
- Show all quotations for a repair order (with status indicators)

### ❌ DON'T:
- Don't allow manual quotation creation (no POST `/api/Quotations`)
- Don't allow editing quotations after they're `Approved`
- Don't allow customer to respond to `Expired` quotations
- Don't confuse customer with too many active quotations (UX consideration)

---

## Validation Rules

### Converting Inspection to Quotation:
- Inspection must be `Completed`
- Inspection must have a valid RepairOrder
- Inspection must have at least one service

### Updating Quotation:
- Can only update `Pending` or `Sent` quotations
- Cannot update `Approved`, `Rejected`, or `Expired` quotations

### Customer Response:
- Quotation must be in `Sent` status
- Must not be expired
- If rejecting, `rejectionReason` is required
- At least one service must be selected if approving

### Copy to Jobs:
- Quotation must be `Approved`
- At least one service must be selected
- **Multiple quotations can create jobs** (no duplication check)

---

## Error Handling

### Common Error Responses:

**404 Not Found:**
```json
{
  "message": "Quotation not found"
}
```

**400 Bad Request:**
```json
{
  "message": "Only completed inspections can be converted to quotations"
}
```

**400 Invalid Operation:**
```json
{
  "message": "Only approved quotations can be copied to jobs"
}
```

**401 Unauthorized:**
```json
{
  "message": "User ID not found in token"
}
```

---

## Summary for Frontend Team

### Key Points:
1. **No manual quotation creation** - always convert from inspection
2. **Quotation = Inspection + Customer Selection**
3. **Manager controls** when to send to customer
4. **Customer can customize** their selection (optional items)
5. **Jobs are created** only after customer approval
6. **Status flow is linear**: Pending → Sent → Approved/Rejected

### Recommended UI Components:
- **Inspection List** with "Create Quotation" action
- **Quotation List** grouped by repair order with status badges
- **Quotation Comparison View** for multiple quotations
- **Quotation Editor** for manager (service/part selection)
- **Quotation Viewer** for customer (with approve/reject)
- **Alternative Quote Creator** (duplicate & modify)
- **Job Creation Confirmation** after quotation approval
- **Status badges** for quotation states (color-coded)
- **Real-time total calculator** as items are selected/deselected
- **Version/Option indicator** (e.g., "Quote 1 of 3", "Budget Option")

### State Management Considerations:
- Cache quotation details to avoid repeated API calls
- Update quotation list when status changes
- Show loading states during conversion/approval
- Handle optimistic updates for better UX
