# Manager Permissions Added

## Summary
Added comprehensive permissions for Manager role to access inspection management, quotation management, technician management, repair requests, and emergency requests features.

## New Permissions Added

### 1. Inspection Management (New Category)
```csharp
INSPECTION_VIEW          // Can view all inspections
INSPECTION_CREATE        // Can create new inspections
INSPECTION_UPDATE        // Can update inspection details
INSPECTION_DELETE        // Can delete inspections
INSPECTION_ASSIGN        // Can assign inspections to technicians
INSPECTION_CONVERT       // Can convert completed inspections to quotations
```

**Use Cases:**
- View all inspections in the system
- Create inspections for repair orders
- Update inspection details and findings
- Delete unnecessary inspections
- Assign inspections to available technicians
- Convert completed inspections to quotations for customer approval

### 2. Quotation Management (New Category)
```csharp
QUOTATION_VIEW           // Can view all quotations
QUOTATION_CREATE         // Can create new quotations
QUOTATION_UPDATE         // Can update quotation details
QUOTATION_DELETE         // Can delete quotations
QUOTATION_SEND           // Can send quotations to customers
QUOTATION_APPROVE        // Can approve/reject quotations
QUOTATION_COPY_TO_JOBS   // Can copy approved quotations to jobs
```

**Use Cases:**
- View all quotations in the system
- Create quotations manually or from inspections
- Edit quotation services, parts, and pricing
- Delete draft quotations
- Send quotations to customers for approval
- Approve/reject customer responses
- Copy approved quotations to create repair jobs

### 3. Technician Management (User Management Category)
```csharp
TECHNICIAN_VIEW          // Can view technician list by branch
TECHNICIAN_ASSIGN        // Can assign technicians to inspections/jobs
TECHNICIAN_SCHEDULE      // Can view technician schedules and availability
```

**Use Cases:**
- View all technicians in manager's branch
- Assign technicians to inspections
- Assign technicians to repair jobs
- View technician availability and schedules
- Monitor technician workload

### 4. Repair Request Management (Booking Management Category)
```csharp
REPAIR_REQUEST_VIEW      // Can view customer repair requests
REPAIR_REQUEST_MANAGE    // Can approve/reject repair requests
```

**Use Cases:**
- View all repair requests from customers
- Approve repair requests and create repair orders
- Reject repair requests with reasons
- Manage repair request queue

### 5. Emergency Request Management (Booking Management Category)
```csharp
EMERGENCY_REQUEST_VIEW      // Can view emergency repair requests
EMERGENCY_REQUEST_MANAGE    // Can approve/reject emergency requests
```

**Use Cases:**
- View emergency repair requests
- Approve urgent repair requests
- Reject emergency requests
- Prioritize emergency cases

## Manager Role Permissions (Complete List)

```csharp
"Manager" => [
    // User Management
    "USER_VIEW",
    
    // Technician Management ✅ NEW
    "TECHNICIAN_VIEW",
    "TECHNICIAN_ASSIGN",
    "TECHNICIAN_SCHEDULE",
    
    // Inspection Management ✅ NEW
    "INSPECTION_VIEW",
    "INSPECTION_CREATE",
    "INSPECTION_UPDATE",
    "INSPECTION_DELETE",
    "INSPECTION_ASSIGN",
    "INSPECTION_CONVERT",
    
    // Quotation Management ✅ NEW
    "QUOTATION_VIEW",
    "QUOTATION_CREATE",
    "QUOTATION_UPDATE",
    "QUOTATION_DELETE",
    "QUOTATION_SEND",
    "QUOTATION_APPROVE",
    "QUOTATION_COPY_TO_JOBS",
    
    // Branch Management
    "BRANCH_VIEW",
    
    // Service Management
    "SERVICE_VIEW",
    
    // Promotional Management
    "PROMO_VIEW",
    
    // Part Management
    "PART_VIEW",
    
    // Booking Management
    "BOOKING_VIEW",
    "BOOKING_MANAGE",
    
    // Repair Request Management ✅ NEW
    "REPAIR_REQUEST_VIEW",
    "REPAIR_REQUEST_MANAGE",
    
    // Emergency Request Management ✅ NEW
    "EMERGENCY_REQUEST_VIEW",
    "EMERGENCY_REQUEST_MANAGE",
    
    // Vehicle Management
    "VEHICLE_VIEW",
    "VEHICLE_CREATE",
    "VEHICLE_UPDATE",
    "VEHICLE_DELETE",
    "VEHICLE_SCHEDULE",
    
    // Repair Management
    "REPAIR_VIEW",
    "REPAIR_CREATE",
    "REPAIR_UPDATE",
    "REPAIR_HISTORY_VIEW"
]
```

## How to Use in Controllers

### Example 1: Inspection Management
```csharp
[Authorize("INSPECTION_VIEW")]
[HttpGet("inspections")]
public async Task<IActionResult> GetAllInspections()
{
    // Manager can view all inspections
}

[Authorize("INSPECTION_CREATE")]
[HttpPost("inspections")]
public async Task<IActionResult> CreateInspection(CreateInspectionDto dto)
{
    // Manager can create inspection
}

[Authorize("INSPECTION_ASSIGN")]
[HttpPut("inspections/{id}/assign/{technicianId}")]
public async Task<IActionResult> AssignInspection(Guid id, Guid technicianId)
{
    // Manager can assign inspection to technician
}

[Authorize("INSPECTION_CONVERT")]
[HttpPost("inspections/convert-to-quotation")]
public async Task<IActionResult> ConvertToQuotation(ConvertDto dto)
{
    // Manager can convert inspection to quotation
}
```

### Example 2: Quotation Management
```csharp
[Authorize("QUOTATION_VIEW")]
[HttpGet("quotations")]
public async Task<IActionResult> GetAllQuotations()
{
    // Manager can view all quotations
}

[Authorize("QUOTATION_UPDATE")]
[HttpPut("quotations/{id}")]
public async Task<IActionResult> UpdateQuotation(Guid id, UpdateQuotationDto dto)
{
    // Manager can update quotation
}

[Authorize("QUOTATION_SEND")]
[HttpPost("quotations/{id}/send")]
public async Task<IActionResult> SendQuotation(Guid id)
{
    // Manager can send quotation to customer
}

[Authorize("QUOTATION_COPY_TO_JOBS")]
[HttpPost("quotations/{id}/copy-to-jobs")]
public async Task<IActionResult> CopyToJobs(Guid id)
{
    // Manager can copy approved quotation to jobs
}
```

### Example 3: Technician Management
```csharp
[Authorize("TECHNICIAN_VIEW")]
[HttpGet("technicians/by-branch/{branchId}")]
public async Task<IActionResult> GetTechniciansByBranch(Guid branchId)
{
    // Manager can view technicians in their branch
}

[Authorize("TECHNICIAN_ASSIGN")]
[HttpPost("inspections/{id}/assign-technician")]
public async Task<IActionResult> AssignTechnician(Guid id, AssignTechnicianDto dto)
{
    // Manager can assign technician to inspection
}

[Authorize("TECHNICIAN_SCHEDULE")]
[HttpGet("technicians/{id}/schedule")]
public async Task<IActionResult> GetTechnicianSchedule(Guid id)
{
    // Manager can view technician schedule
}
```

### Example 4: Repair Request Management
```csharp
[Authorize("REPAIR_REQUEST_VIEW")]
[HttpGet("repair-requests")]
public async Task<IActionResult> GetRepairRequests()
{
    // Manager can view all repair requests
}

[Authorize("REPAIR_REQUEST_MANAGE")]
[HttpPost("repair-requests/{id}/approve")]
public async Task<IActionResult> ApproveRepairRequest(Guid id)
{
    // Manager can approve repair request
}

[Authorize("REPAIR_REQUEST_MANAGE")]
[HttpPost("repair-requests/{id}/reject")]
public async Task<IActionResult> RejectRepairRequest(Guid id, RejectDto dto)
{
    // Manager can reject repair request
}
```

### Example 5: Emergency Request Management
```csharp
[Authorize("EMERGENCY_REQUEST_VIEW")]
[HttpGet("emergency-requests")]
public async Task<IActionResult> GetEmergencyRequests()
{
    // Manager can view emergency requests
}

[Authorize("EMERGENCY_REQUEST_MANAGE")]
[HttpPost("emergency-requests/{id}/approve")]
public async Task<IActionResult> ApproveEmergencyRequest(Guid id)
{
    // Manager can approve emergency request
}
```

## Existing Controllers That Need Updates

### InspectionController
```csharp
// Currently uses generic BOOKING_VIEW and BOOKING_MANAGE
[Authorize(Policy = "BOOKING_VIEW")]
```
**Recommendation:** Update to use specific permissions:
```csharp
[Authorize("INSPECTION_VIEW")]
[Authorize("INSPECTION_CREATE")]
[Authorize("INSPECTION_UPDATE")]
[Authorize("INSPECTION_ASSIGN")]
[Authorize("INSPECTION_CONVERT")]
```

### QuotationController
```csharp
// Currently has NO authorization! (commented out)
//[Authorize]
```
**Recommendation:** Add specific permissions:
```csharp
[Authorize("QUOTATION_VIEW")]
[Authorize("QUOTATION_CREATE")]
[Authorize("QUOTATION_UPDATE")]
[Authorize("QUOTATION_SEND")]
[Authorize("QUOTATION_APPROVE")]
[Authorize("QUOTATION_COPY_TO_JOBS")]
```

### ManagerRepairRequestController
```csharp
[Authorize(Roles = "Manager")] // ✅ Already restricted to Manager
```
**Recommendation:** Add specific permissions:
```csharp
[Authorize("REPAIR_REQUEST_VIEW")]
[Authorize("REPAIR_REQUEST_MANAGE")]
```

### EmergencyRequestController
```csharp
[Authorize(Roles = "Manager")] // ✅ Already has some Manager endpoints
```
**Recommendation:** Add specific permissions:
```csharp
[Authorize("EMERGENCY_REQUEST_VIEW")]
[Authorize("EMERGENCY_REQUEST_MANAGE")]
```

### TechnicianController
**Recommendation:** Add Manager endpoints with permissions:
```csharp
[Authorize("TECHNICIAN_VIEW")]
[Authorize("TECHNICIAN_ASSIGN")]
[Authorize("TECHNICIAN_SCHEDULE")]
```

## Database Migration

After adding these permissions, you need to:

1. **Drop and recreate database** (if using DbInitializer):
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```

2. **Or run the application** - DbInitializer will seed the new permissions automatically

3. **Verify permissions** in database:
   ```sql
   SELECT * FROM Permissions WHERE Code LIKE 'TECHNICIAN_%'
   SELECT * FROM Permissions WHERE Code LIKE '%_REQUEST_%'
   SELECT * FROM RolePermissions WHERE RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Manager')
   ```

## Benefits

✅ **Granular Control** - Fine-grained permissions instead of just role-based
✅ **Flexibility** - Can customize manager permissions per branch/region
✅ **Security** - Better access control and audit trail
✅ **Scalability** - Easy to add/remove permissions without code changes
✅ **Consistency** - Follows existing permission pattern in the system

## Next Steps

1. Update existing controllers to use specific permissions instead of `[Authorize(Roles = "Manager")]`
2. Add permission checks in service layer for additional security
3. Create UI to manage permissions dynamically
4. Add permission-based menu rendering in frontend
