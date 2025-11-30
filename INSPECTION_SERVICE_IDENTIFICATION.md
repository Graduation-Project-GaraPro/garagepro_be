# How to Identify Inspection Services

## 🎯 Answer: Use ServiceCategory

Inspection services are identified by their **ServiceCategory** with `CategoryName = "Inspection"`.

---

## 📊 Service Category Hierarchy

### **Parent Categories:**
1. **Maintenance** - General maintenance services
2. **Repair** - Repair services for damaged parts
3. **Inspection** - Vehicle inspection and diagnostics ⭐
4. **Upgrade** - Performance and aesthetic upgrades

### **Inspection Sub-Categories:**
1. **Safety Inspection** - Check safety systems (brakes, lights, tires)
2. **Emissions Inspection** - Check exhaust and emissions compliance
3. **Pre-Purchase Inspection** - Comprehensive vehicle check before buying
4. **Engine Diagnostic** - Computer-based engine and sensor diagnostics

---

## 🔍 Inspection Services (from seed data):

| Service Name | Category | Price | Duration |
|-------------|----------|-------|----------|
| Basic Safety Inspection | Safety Inspection | 350,000 VND | 1 hour |
| Emissions Test | Emissions Inspection | 400,000 VND | 1 hour |
| Pre-Purchase Inspection | Pre-Purchase Inspection | 600,000 VND | 2 hours |
| Full Engine Diagnostic | Engine Diagnostic | 700,000 VND | 2 hours |

---

## 💻 Code Implementation

### **Method 1: Check Parent Category Name**

```csharp
public bool IsInspectionService(Service service)
{
    // Load the service with its category and parent category
    return service.ServiceCategory?.CategoryName == "Inspection" ||
           service.ServiceCategory?.ParentServiceCategory?.CategoryName == "Inspection";
}
```

### **Method 2: Query Services by Category**

```csharp
public async Task<List<Service>> GetInspectionServicesAsync()
{
    // Get the Inspection category
    var inspectionCategory = await _context.ServiceCategories
        .FirstOrDefaultAsync(c => c.CategoryName == "Inspection");
    
    if (inspectionCategory == null)
        return new List<Service>();
    
    // Get all services in Inspection category or its child categories
    var inspectionServices = await _context.Services
        .Include(s => s.ServiceCategory)
            .ThenInclude(sc => sc.ParentServiceCategory)
        .Where(s => 
            s.ServiceCategoryId == inspectionCategory.ServiceCategoryId ||
            s.ServiceCategory.ParentServiceCategoryId == inspectionCategory.ServiceCategoryId)
        .ToListAsync();
    
    return inspectionServices;
}
```

### **Method 3: Check in UpdateCostFromInspectionAsync**

Your existing code already handles this correctly:

```csharp
// RepairOrderService.cs - Line 575
public async Task<RepairOrder> UpdateCostFromInspectionAsync(Guid repairOrderId)
{
    var repairOrder = await _repairOrderRepository.GetRepairOrderWithFullDetailsAsync(repairOrderId);
    
    var completedInspections = repairOrder.Inspections?
        .Where(i => i.Status == InspectionStatus.Completed).ToList();
    
    // Calculate cost from inspection services
    decimal totalCost = 0;
    foreach (var inspection in completedInspections)
    {
        // Skip inspections with approved quotations
        if (inspection.Quotations?.Any(q => q.Status == QuotationStatus.Approved) == true)
            continue;
        
        // Add cost of services in this inspection
        if (inspection.ServiceInspections != null)
        {
            foreach (var serviceInspection in inspection.ServiceInspections)
            {
                if (serviceInspection.Service != null)
                {
                    // ✅ This gets the inspection service price
                    // The service is already an inspection service because it's in ServiceInspections
                    totalCost += serviceInspection.Service.Price;
                }
            }
        }
    }
    
    repairOrder.Cost = totalCost;
    await _repairOrderRepository.UpdateAsync(repairOrder);
    return repairOrder;
}
```

---

## 🔄 Data Flow

### **How Inspection Services are Used:**

```
1. Customer creates RepairRequest
   ↓
2. Manager creates RepairOrder
   ↓
3. Manager creates Inspection
   ↓
4. Manager adds ServiceInspections (inspection services)
   - Example: "Full Engine Diagnostic" (700,000 VND)
   - Example: "Basic Safety Inspection" (350,000 VND)
   ↓
5. Technician performs inspection
   ↓
6. Technician completes inspection
   ↓
7. UpdateCostFromInspectionAsync() is called
   - Calculates: Cost = Sum of ServiceInspection prices
   - Updates: RepairOrder.Cost = 1,050,000 VND
   ↓
8. Manager creates Quotation (optional)
   ↓
9. Customer approves/rejects quotation
   ↓
10. Payment uses RepairOrder.Cost
```

---

## 🎯 Key Relationships

### **Inspection → ServiceInspection → Service**

```csharp
public class Inspection
{
    public Guid InspectionId { get; set; }
    public Guid RepairOrderId { get; set; }
    
    // Collection of services used in this inspection
    public virtual ICollection<ServiceInspection> ServiceInspections { get; set; }
}

public class ServiceInspection
{
    public Guid ServiceInspectionId { get; set; }
    public Guid InspectionId { get; set; }
    public Guid ServiceId { get; set; } // ⭐ Links to Service
    
    public virtual Service Service { get; set; } // ⭐ The inspection service
    public virtual Inspection Inspection { get; set; }
}

public class Service
{
    public Guid ServiceId { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string ServiceName { get; set; }
    public decimal Price { get; set; }
    
    public virtual ServiceCategory ServiceCategory { get; set; } // ⭐ Category
}

public class ServiceCategory
{
    public Guid ServiceCategoryId { get; set; }
    public string CategoryName { get; set; } // "Inspection" for inspection services
    public Guid? ParentServiceCategoryId { get; set; }
    
    public virtual ServiceCategory ParentServiceCategory { get; set; }
}
```

---

## ✅ Summary

### **To determine if a service is an inspection service:**

1. **Check ServiceCategory.CategoryName**:
   ```csharp
   service.ServiceCategory?.CategoryName == "Inspection"
   ```

2. **Or check parent category**:
   ```csharp
   service.ServiceCategory?.ParentServiceCategory?.CategoryName == "Inspection"
   ```

3. **Or check if service is in ServiceInspections collection**:
   ```csharp
   // If a service is in inspection.ServiceInspections, it's an inspection service
   var isInspectionService = inspection.ServiceInspections.Any(si => si.ServiceId == serviceId);
   ```

### **Your current implementation is correct:**
- ✅ `UpdateCostFromInspectionAsync()` gets services from `inspection.ServiceInspections`
- ✅ These are automatically inspection services (by relationship)
- ✅ Their prices are summed to calculate `RepairOrder.Cost`
- ✅ Payment uses this cost value

**No changes needed** - your logic already correctly identifies and prices inspection services!
