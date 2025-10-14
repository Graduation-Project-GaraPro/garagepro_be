# Repair Order Frontend Integration

This document summarizes the backend changes made to support the frontend create repair order functionality.

## Changes Made

### 1. RepairOrder Entity Updates

Modified the `BusinessObject.RepairOrder` entity to include fields required by the frontend:
- `Odometer` (int?): Odometer reading
- `OdometerNotWorking` (bool): Flag indicating if odometer is not working
- `LabelId` (int?): Optional label ID

Removed the `VehicleConcern` property since we'll use the existing `Note` field instead.

### 2. DTO Creation

Created new DTOs in `Dtos.RepairOrder` namespace:
- `CreateRepairOrderRequestDto`: Matches the frontend request structure
  - CustomerId (string)
  - VehicleId (Guid)
  - RepairOrderType (RoType enum)
  - VehicleConcern (string) - maps to Note field
  - Odometer (int?)
  - OdometerNotWorking (bool)
  - LabelId (int?)
  - Status (string)
  - Progress (int)

### 3. Customer DTOs

Created new DTOs in `Dtos.Customer` namespace:
- `CustomerDto`: For returning customer information
- `CreateCustomerRequestDto`: For creating new customers

### 4. Vehicle DTOs

Updated `Dtos.Vehicle.VehicleDto` to include:
- `CreateVehicleDto`: For creating new vehicles
- `UpdateVehicleDto`: For updating existing vehicles

### 5. New Services

Created new services:
- `ICustomerService` and `CustomerService`: For customer search and creation
- Extended `IUserRepository` with search methods

### 6. New Controllers

Created new controllers:
- `CustomerController`: Handles customer search and creation endpoints
- `VehicleController`: Handles vehicle search and creation endpoints
- Updated `RepairOrderController` with new endpoint for create requests

## API Endpoints

### Customer Endpoints
- `GET /api/Customer/search?searchTerm={term}`: Search customers by name, phone, or email
- `POST /api/Customer`: Create a new customer
- `GET /api/Customer/{id}`: Get customer by ID

### Vehicle Endpoints
- `GET /api/Vehicle/customer/{customerId}`: Get vehicles for a specific customer
- `POST /api/Vehicle`: Create a new vehicle
- `GET /api/Vehicle/{id}`: Get vehicle by ID

### Repair Order Endpoints
- `POST /api/RepairOrder/create-request`: Create a new repair order from frontend request

## Database Considerations

The changes maintain backward compatibility with existing database structures. The new fields in the RepairOrder entity are nullable, so existing records will not be affected.

## Integration Points

1. Frontend should use the new `/api/Customer/search` endpoint to search for existing customers
2. When creating new customers, frontend should call `/api/Customer`
3. For vehicle management, frontend should use `/api/Vehicle/customer/{customerId}` to get existing vehicles
4. When creating new vehicles, frontend should call `/api/Vehicle`
5. Finally, to create a repair order, frontend should call `/api/RepairOrder/create-request` with the CreateRepairOrderRequestDto

## Validation

All new endpoints include proper validation:
- Required fields are marked with [Required] attributes
- String length constraints are applied where appropriate
- Range validation for numeric fields (e.g., Year for vehicles)