# Quotation Tree Selection - Usage Example

## Complete Flow Example

### Scenario
Manager wants to create a quotation for brake pad replacement service.

---

## Step 1: Load Root Categories

**Request:**
```http
GET /api/QuotationTreeSelection/root
Authorization: Bearer {manager-token}
```

**Response:**
```json
{
  "currentCategoryId": null,
  "currentCategoryName": "Root",
  "childCategories": [
    {
      "serviceCategoryId": "cat-1-guid",
      "categoryName": "Brake Services",
      "description": "All brake-related services",
      "parentServiceCategoryId": null,
      "hasChildren": true,
      "serviceCount": 0,
      "childCategoryCount": 2
    },
    {
      "serviceCategoryId": "cat-2-guid",
      "categoryName": "Engine Services",
      "description": "Engine maintenance and repair",
      "parentServiceCategoryId": null,
      "hasChildren": true,
      "serviceCount": 0,
      "childCategoryCount": 3
    }
  ],
  "services": [],
  "breadcrumb": {
    "items": [
      {
        "categoryId": null,
        "categoryName": "All Categories",
        "level": 0
      }
    ]
  }
}
```

**Manager Action:** Clicks on "Brake Services"

---

## Step 2: Drill Down to Brake Services

**Request:**
```http
GET /api/QuotationTreeSelection/category/cat-1-guid
Authorization: Bearer {manager-token}
```

**Response:**
```json
{
  "currentCategoryId": "cat-1-guid",
  "currentCategoryName": "Brake Services",
  "childCategories": [
    {
      "serviceCategoryId": "cat-1-1-guid",
      "categoryName": "Disc Brakes",
      "description": "Disc brake services",
      "parentServiceCategoryId": "cat-1-guid",
      "hasChildren": false,
      "serviceCount": 2,
      "childCategoryCount": 0
    },
    {
      "serviceCategoryId": "cat-1-2-guid",
      "categoryName": "Drum Brakes",
      "description": "Drum brake services",
      "parentServiceCategoryId": "cat-1-guid",
      "hasChildren": false,
      "serviceCount": 1,
      "childCategoryCount": 0
    }
  ],
  "services": [],
  "breadcrumb": {
    "items": [
      {
        "categoryId": null,
        "categoryName": "All Categories",
        "level": 0
      },
      {
        "categoryId": "cat-1-guid",
        "categoryName": "Brake Services",
        "level": 1
      }
    ]
  }
}
```

**Manager Action:** Clicks on "Disc Brakes"

---

## Step 3: View Services in Disc Brakes Category

**Request:**
```http
GET /api/QuotationTreeSelection/category/cat-1-1-guid
Authorization: Bearer {manager-token}
```

**Response:**
```json
{
  "currentCategoryId": "cat-1-1-guid",
  "currentCategoryName": "Disc Brakes",
  "childCategories": [],
  "services": [
    {
      "serviceId": "service-1-guid",
      "serviceName": "Brake Pad Replacement",
      "description": "Replace worn brake pads",
      "price": 150.00,
      "estimatedDuration": 1.5,
      "isAdvanced": false,
      "partCategories": [
        {
          "partCategoryId": "part-cat-1-guid",
          "categoryName": "Brake Pads",
          "description": "Various brake pad options",
          "partCount": 3,
          "parts": [
            {
              "partId": "part-1-guid",
              "partName": "Premium Ceramic Brake Pads",
              "price": 100.00,
              "description": "High-performance ceramic pads"
            },
            {
              "partId": "part-2-guid",
              "partName": "Standard Brake Pads",
              "price": 60.00,
              "description": "OEM quality brake pads"
            },
            {
              "partId": "part-3-guid",
              "partName": "Economy Brake Pads",
              "price": 40.00,
              "description": "Budget-friendly option"
            }
          ]
        },
        {
          "partCategoryId": "part-cat-2-guid",
          "categoryName": "Brake Fluid",
          "description": "Brake fluid options",
          "partCount": 1,
          "parts": [
            {
              "partId": "part-4-guid",
              "partName": "DOT 4 Brake Fluid",
              "price": 20.00,
              "description": "High-performance brake fluid"
            }
          ]
        }
      ]
    },
    {
      "serviceId": "service-2-guid",
      "serviceName": "Brake Rotor Resurfacing",
      "description": "Resurface brake rotors",
      "price": 80.00,
      "estimatedDuration": 1.0,
      "isAdvanced": false,
      "partCategories": []
    }
  ],
  "breadcrumb": {
    "items": [
      {
        "categoryId": null,
        "categoryName": "All Categories",
        "level": 0
      },
      {
        "categoryId": "cat-1-guid",
        "categoryName": "Brake Services",
        "level": 1
      },
      {
        "categoryId": "cat-1-1-guid",
        "categoryName": "Disc Brakes",
        "level": 2
      }
    ]
  }
}
```

**Manager Action:** Selects "Brake Pad Replacement" service

---

## Step 4: View Service Details (Optional)

**Request:**
```http
GET /api/QuotationTreeSelection/service/service-1-guid
Authorization: Bearer {manager-token}
```

**Response:**
```json
{
  "serviceId": "service-1-guid",
  "serviceName": "Brake Pad Replacement",
  "description": "Replace worn brake pads",
  "price": 150.00,
  "estimatedDuration": 1.5,
  "isAdvanced": false,
  "partCategories": [
    {
      "partCategoryId": "part-cat-1-guid",
      "categoryName": "Brake Pads",
      "description": "Various brake pad options",
      "partCount": 3,
      "parts": [
        {
          "partId": "part-1-guid",
          "partName": "Premium Ceramic Brake Pads",
          "price": 100.00,
          "description": "High-performance ceramic pads"
        },
        {
          "partId": "part-2-guid",
          "partName": "Standard Brake Pads",
          "price": 60.00,
          "description": "OEM quality brake pads"
        },
        {
          "partId": "part-3-guid",
          "partName": "Economy Brake Pads",
          "price": 40.00,
          "description": "Budget-friendly option"
        }
      ]
    },
    {
      "partCategoryId": "part-cat-2-guid",
      "categoryName": "Brake Fluid",
      "description": "Brake fluid options",
      "partCount": 1,
      "parts": [
        {
          "partId": "part-4-guid",
          "partName": "DOT 4 Brake Fluid",
          "price": 20.00,
          "description": "High-performance brake fluid"
        }
      ]
    }
  ]
}
```

**Manager Decision:** 
- Include "Brake Pads" part category (let customer choose which one)
- Include "Brake Fluid" part category

---

## Step 5: Create Quotation with Selected Service and Part Categories

**Request:**
```http
POST /api/Quotation
Authorization: Bearer {manager-token}
Content-Type: application/json

{
  "userId": "customer-user-id",
  "vehicleId": "vehicle-guid",
  "repairOrderId": "repair-order-guid",
  "note": "Brake pad replacement quotation",
  "quotationServices": [
    {
      "serviceId": "service-1-guid",
      "isRequired": true,
      "isSelected": false,
      "quotationServiceParts": [
        {
          "partId": "part-1-guid",
          "isSelected": false,
          "quantity": 1
        },
        {
          "partId": "part-2-guid",
          "isSelected": false,
          "quantity": 1
        },
        {
          "partId": "part-3-guid",
          "isSelected": false,
          "quantity": 1
        },
        {
          "partId": "part-4-guid",
          "isSelected": false,
          "quantity": 1
        }
      ]
    }
  ]
}
```

**Response:**
```json
{
  "quotationId": "quotation-guid",
  "userId": "customer-user-id",
  "vehicleId": "vehicle-guid",
  "status": "Pending",
  "totalAmount": 330.00,
  "createdAt": "2024-01-15T10:00:00Z",
  "quotationServices": [
    {
      "quotationServiceId": "qs-guid",
      "serviceId": "service-1-guid",
      "serviceName": "Brake Pad Replacement",
      "price": 150.00,
      "isRequired": true,
      "isSelected": false,
      "parts": [
        {
          "quotationServicePartId": "qsp-1-guid",
          "partId": "part-1-guid",
          "partName": "Premium Ceramic Brake Pads",
          "price": 100.00,
          "quantity": 1,
          "isSelected": false
        },
        {
          "quotationServicePartId": "qsp-2-guid",
          "partId": "part-2-guid",
          "partName": "Standard Brake Pads",
          "price": 60.00,
          "quantity": 1,
          "isSelected": false
        },
        {
          "quotationServicePartId": "qsp-3-guid",
          "partId": "part-3-guid",
          "partName": "Economy Brake Pads",
          "price": 40.00,
          "quantity": 1,
          "isSelected": false
        },
        {
          "quotationServicePartId": "qsp-4-guid",
          "partId": "part-4-guid",
          "partName": "DOT 4 Brake Fluid",
          "price": 20.00,
          "quantity": 1,
          "isSelected": false
        }
      ]
    }
  ]
}
```

---

## Step 6: Customer Receives Quotation

Customer sees in their app:
```
Quotation #12345
Total: $330.00

Service: Brake Pad Replacement - $150.00 ✓ Required

Choose Brake Pads (select one):
○ Premium Ceramic Brake Pads - $100.00
○ Standard Brake Pads - $60.00
○ Economy Brake Pads - $40.00

Choose Brake Fluid:
○ DOT 4 Brake Fluid - $20.00
```

---

## Step 7: Customer Responds

**Request:**
```http
PUT /api/CustomerQuotations/customer-response
Authorization: Bearer {customer-token}
Content-Type: application/json

{
  "quotationId": "quotation-guid",
  "status": "Approved",
  "customerNote": "I'll go with the standard brake pads",
  "selectedServices": [
    {
      "quotationServiceId": "qs-guid",
      "selectedPartIds": [
        "qsp-2-guid",
        "qsp-4-guid"
      ]
    }
  ]
}
```

**Response:**
```json
{
  "quotationId": "quotation-guid",
  "status": "Approved",
  "totalAmount": 230.00,
  "customerResponseAt": "2024-01-15T14:30:00Z",
  "quotationServices": [
    {
      "quotationServiceId": "qs-guid",
      "isSelected": true,
      "parts": [
        {
          "quotationServicePartId": "qsp-2-guid",
          "partName": "Standard Brake Pads",
          "price": 60.00,
          "isSelected": true
        },
        {
          "quotationServicePartId": "qsp-4-guid",
          "partName": "DOT 4 Brake Fluid",
          "price": 20.00,
          "isSelected": true
        }
      ]
    }
  ]
}
```

**Final Total:** $150 (service) + $60 (standard pads) + $20 (fluid) = **$230.00**

---

## Summary

1. Manager browses service category tree
2. Manager drills down to find specific service
3. Manager sees all part categories for that service
4. Manager creates quotation with ALL parts from selected categories
5. Customer receives quotation and chooses specific parts
6. System calculates final price based on customer's selection

This flow provides maximum flexibility while maintaining transparency!
