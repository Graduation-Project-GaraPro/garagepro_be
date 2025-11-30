# Quotation Tree Selection API Documentation

## Overview
This API provides a hierarchical tree-based navigation system for managers to create quotations. The flow allows managers to browse service categories, drill down to services, and select part categories to include in quotations.

## Flow Diagram
```
Root Categories
    ├─ Category A
    │   ├─ Sub-category A1
    │   │   ├─ Service 1
    │   │   │   ├─ Part Category: Brake Pads
    │   │   │   │   ├─ Premium Brake Pad ($100)
    │   │   │   │   ├─ Standard Brake Pad ($60)
    │   │   │   │   └─ Economy Brake Pad ($40)
    │   │   │   └─ Part Category: Brake Fluid
    │   │   │       └─ DOT 4 Brake Fluid ($20)
    │   │   └─ Service 2
    │   └─ Sub-category A2
    └─ Category B
```

## API Endpoints

### 1. Get Root Categories
**Endpoint:** `GET /api/QuotationTreeSelection/root`

**Authorization:** Manager role required

**Description:** Returns the top-level service categories (starting point of the tree)

**Response:**
```json
{
  "currentCategoryId": null,
  "currentCategoryName": "Root",
  "childCategories": [
    {
      "serviceCategoryId": "guid",
      "categoryName": "Engine Services",
      "description": "All engine-related services",
      "parentServiceCategoryId": null,
      "hasChildren": true,
      "serviceCount": 0,
      "childCategoryCount": 3
    },
    {
      "serviceCategoryId": "guid",
      "categoryName": "Brake Services",
      "description": "Brake system services",
      "parentServiceCategoryId": null,
      "hasChildren": true,
      "serviceCount": 2,
      "childCategoryCount": 1
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

### 2. Get Category Children
**Endpoint:** `GET /api/QuotationTreeSelection/category/{categoryId}`

**Authorization:** Manager role required

**Description:** Drills down one level - returns child categories and services under a specific category

**Parameters:**
- `categoryId` (path, required): GUID of the parent category

**Response:**
```json
{
  "currentCategoryId": "guid",
  "currentCategoryName": "Brake Services",
  "childCategories": [
    {
      "serviceCategoryId": "guid",
      "categoryName": "Disc Brakes",
      "description": "Disc brake services",
      "parentServiceCategoryId": "parent-guid",
      "hasChildren": false,
      "serviceCount": 3,
      "childCategoryCount": 0
    }
  ],
  "services": [
    {
      "serviceId": "guid",
      "serviceName": "Brake Pad Replacement",
      "description": "Replace worn brake pads",
      "price": 150.00,
      "estimatedDuration": 1.5,
      "isAdvanced": false,
      "partCategories": [
        {
          "partCategoryId": "guid",
          "categoryName": "Brake Pads",
          "description": "Various brake pad options",
          "partCount": 3,
          "parts": [
            {
              "partId": "guid",
              "partName": "Premium Ceramic Brake Pads",
              "price": 100.00,
              "description": "High-performance ceramic pads"
            },
            {
              "partId": "guid",
              "partName": "Standard Brake Pads",
              "price": 60.00,
              "description": "OEM quality brake pads"
            }
          ]
        }
      ]
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
        "categoryId": "guid",
        "categoryName": "Brake Services",
        "level": 1
      }
    ]
  }
}
```

### 3. Get Service with Part Categories
**Endpoint:** `GET /api/QuotationTreeSelection/service/{serviceId}`

**Authorization:** Manager role required

**Description:** Returns a specific service with all its associated part categories and parts

**Parameters:**
- `serviceId` (path, required): GUID of the service

**Response:**
```json
{
  "serviceId": "guid",
  "serviceName": "Brake Pad Replacement",
  "description": "Replace worn brake pads",
  "price": 150.00,
  "estimatedDuration": 1.5,
  "isAdvanced": false,
  "partCategories": [
    {
      "partCategoryId": "guid",
      "categoryName": "Brake Pads",
      "description": "Various brake pad options",
      "partCount": 3,
      "parts": [
        {
          "partId": "guid",
          "partName": "Premium Ceramic Brake Pads",
          "price": 100.00,
          "description": "High-performance ceramic pads"
        },
        {
          "partId": "guid",
          "partName": "Standard Brake Pads",
          "price": 60.00,
          "description": "OEM quality brake pads"
        },
        {
          "partId": "guid",
          "partName": "Economy Brake Pads",
          "price": 40.00,
          "description": "Budget-friendly option"
        }
      ]
    },
    {
      "partCategoryId": "guid",
      "categoryName": "Brake Fluid",
      "description": "Brake fluid options",
      "partCount": 1,
      "parts": [
        {
          "partId": "guid",
          "partName": "DOT 4 Brake Fluid",
          "price": 20.00,
          "description": "High-performance brake fluid"
        }
      ]
    }
  ]
}
```

### 4. Get Part Category Details
**Endpoint:** `GET /api/QuotationTreeSelection/part-category/{partCategoryId}`

**Authorization:** Manager role required

**Description:** Returns detailed information about a specific part category including all parts (for preview/reference)

**Parameters:**
- `partCategoryId` (path, required): GUID of the part category

**Response:**
```json
{
  "partCategoryId": "guid",
  "categoryName": "Brake Pads",
  "description": "Various brake pad options",
  "partCount": 3,
  "parts": [
    {
      "partId": "guid",
      "partName": "Premium Ceramic Brake Pads",
      "price": 100.00,
      "description": "High-performance ceramic pads"
    },
    {
      "partId": "guid",
      "partName": "Standard Brake Pads",
      "price": 60.00,
      "description": "OEM quality brake pads"
    },
    {
      "partId": "guid",
      "partName": "Economy Brake Pads",
      "price": 40.00,
      "description": "Budget-friendly option"
    }
  ]
}
```

## Manager Workflow

### Step 1: Load Root Categories
```
GET /api/QuotationTreeSelection/root
```
Manager sees top-level categories like "Engine Services", "Brake Services", etc.

### Step 2: Drill Down Through Categories
```
GET /api/QuotationTreeSelection/category/{categoryId}
```
Manager clicks on a category to see its children and services.
Repeat until reaching leaf categories with services.

### Step 3: Select a Service
```
GET /api/QuotationTreeSelection/service/{serviceId}
```
Manager clicks on a service to see all associated part categories.

### Step 4: Review Part Categories
Manager sees all part categories linked to the service.
Each part category shows all available parts with prices.

### Step 5: Add to Quotation
Manager selects which part categories to include in the quotation using the existing quotation creation API:

```
POST /api/Quotation
{
  "userId": "customer-id",
  "vehicleId": "vehicle-guid",
  "note": "Brake service quotation",
  "quotationServices": [
    {
      "serviceId": "service-guid",
      "isRequired": true,
      "isSelected": false,
      "quotationServiceParts": [
        {
          "partId": "premium-pad-guid",
          "isSelected": false,  // Not pre-selected, customer will choose
          "quantity": 1
        },
        {
          "partId": "standard-pad-guid",
          "isSelected": false,
          "quantity": 1
        },
        {
          "partId": "economy-pad-guid",
          "isSelected": false,
          "quantity": 1
        }
      ]
    }
  ]
}
```

### Step 6: Customer Receives Quotation
Customer sees:
- Service: Brake Pad Replacement ($150)
- Part Category: Brake Pads (choose one)
  - Premium Ceramic Brake Pads ($100)
  - Standard Brake Pads ($60)
  - Economy Brake Pads ($40)

### Step 7: Customer Selects Parts
```
PUT /api/CustomerQuotations/customer-response
{
  "quotationId": "guid",
  "status": "Approved",
  "selectedServices": [
    {
      "quotationServiceId": "guid",
      "selectedPartIds": ["standard-pad-guid"]  // Customer chose standard
    }
  ]
}
```

## Key Features

1. **Hierarchical Navigation**: Browse through multiple levels of service categories
2. **Breadcrumb Trail**: Always know where you are in the tree
3. **Part Category Preview**: See all parts before adding to quotation
4. **Flexible Selection**: Manager includes part categories, customer chooses specific parts
5. **Price Transparency**: All prices visible at every level

## Error Responses

### 404 Not Found
```json
{
  "message": "Service category with ID {guid} not found."
}
```

### 500 Internal Server Error
```json
{
  "message": "Error retrieving category children",
  "detail": "Detailed error message"
}
```

## Notes

- All endpoints require Manager role authorization
- Parts within each category are ordered by price (descending) - most expensive first
- The `hasChildren` flag indicates if a category has sub-categories
- The `serviceCount` shows how many services are directly under a category
- Breadcrumb helps with navigation and shows the current path
