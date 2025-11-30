# Inspection SignalR Real-Time Updates

## Overview
The inspection system now has comprehensive real-time updates using SignalR, allowing managers to see inspection status changes instantly.

---

## SignalR Hub
**Hub URL:** `/inspectionHub`

---

## Available Groups

### 1. Managers Group
**Group Name:** `"Managers"`
- All managers/admins should join this group
- Receives ALL inspection status updates across the system
- Gets special notifications for inspection start and completion

### 2. Technician Group
**Group Name:** `"Technician_{technicianId}"`
- Individual technicians join their own group
- Receives updates for inspections assigned to them

### 3. Inspection-Specific Group
**Group Name:** `"Inspection_{inspectionId}"`
- Anyone watching a specific inspection can join
- Receives updates only for that specific inspection

---

## Hub Methods (Client → Server)

### For Managers:
```javascript
// Join the managers group to receive all inspection updates
connection.invoke("JoinManagersGroup");

// Leave the managers group
connection.invoke("LeaveManagersGroup");
```

### For Technicians:
```javascript
// Join technician group (use technician's GUID)
connection.invoke("JoinTechnicianGroup", technicianId);

// Leave technician group
connection.invoke("LeaveTechnicianGroup", technicianId);
```

### For Specific Inspection Monitoring:
```javascript
// Join inspection-specific group
connection.invoke("JoinInspectionGroup", inspectionId);

// Leave inspection-specific group
connection.invoke("LeaveInspectionGroup", inspectionId);
```

---

## SignalR Events (Server → Client)

### 1. InspectionStatusUpdated
**Triggered:** Whenever inspection status changes (Pending → InProgress → Completed)

**Event Name:** `"InspectionStatusUpdated"`

**Payload:**
```json
{
  "inspectionId": "guid",
  "technicianId": "guid",
  "technicianName": "John Doe",
  "oldStatus": "Pending",
  "newStatus": "InProgress",
  "repairOrderId": "guid",
  "customerConcern": "Engine noise",
  "finding": "Oil leak detected",
  "issueRating": 2,
  "inspection": { /* Full InspectionDto */ },
  "message": "Inspection is now in progress",
  "updatedAt": "2025-11-30T10:30:00Z"
}
```

**Client Handler Example:**
```javascript
connection.on("InspectionStatusUpdated", (data) => {
    console.log(`Inspection ${data.inspectionId} status changed: ${data.oldStatus} → ${data.newStatus}`);
    
    // Update UI based on new status
    if (data.newStatus === "InProgress") {
        showInProgressBadge(data.inspectionId);
    } else if (data.newStatus === "Completed") {
        showCompletedBadge(data.inspectionId);
        // Optionally fetch full details
    }
    
    // Show notification to manager
    showNotification(data.message, data.technicianName);
});
```

---

### 2. InspectionStarted
**Triggered:** When technician changes status from Pending → InProgress

**Event Name:** `"InspectionStarted"`

**Payload:**
```json
{
  "inspectionId": "guid",
  "repairOrderId": "guid",
  "technicianId": "guid",
  "technicianName": "John Doe",
  "customerConcern": "Engine noise",
  "startedAt": "2025-11-30T10:30:00Z",
  "message": "Technician has started the inspection"
}
```

**Client Handler Example:**
```javascript
connection.on("InspectionStarted", (data) => {
    console.log(`Technician ${data.technicianName} started inspection ${data.inspectionId}`);
    
    // Update dashboard - show inspection as "In Progress"
    updateInspectionCard(data.inspectionId, {
        status: "InProgress",
        technicianName: data.technicianName,
        startedAt: data.startedAt
    });
    
    // Show real-time notification
    showToast(`${data.technicianName} started working on inspection`, "info");
});
```

---

### 3. InspectionCompleted
**Triggered:** When technician marks inspection as Completed

**Event Name:** `"InspectionCompleted"`

**Payload:**
```json
{
  "inspectionId": "guid",
  "repairOrderId": "guid",
  "technicianId": "guid",
  "technicianName": "John Doe",
  "customerConcern": "Engine noise",
  "finding": "Oil leak detected in engine block",
  "issueRating": 2,
  "serviceCount": 3,
  "partCount": 5,
  "completedAt": "2025-11-30T11:45:00Z",
  "inspectionDetails": { /* Full InspectionDto with services and parts */ },
  "message": "Inspection completed and ready for quotation"
}
```

**Client Handler Example:**
```javascript
connection.on("InspectionCompleted", (data) => {
    console.log(`Inspection ${data.inspectionId} completed by ${data.technicianName}`);
    
    // Update dashboard - show inspection as "Completed"
    updateInspectionCard(data.inspectionId, {
        status: "Completed",
        technicianName: data.technicianName,
        completedAt: data.completedAt,
        serviceCount: data.serviceCount,
        partCount: data.partCount,
        finding: data.finding
    });
    
    // Show notification with action button
    showNotification({
        title: "Inspection Completed",
        message: `${data.technicianName} completed inspection. ${data.serviceCount} services, ${data.partCount} parts identified.`,
        action: {
            label: "Create Quotation",
            onClick: () => navigateToCreateQuotation(data.inspectionId)
        }
    });
    
    // Optionally auto-refresh inspection details
    fetchInspectionDetails(data.inspectionId);
});
```

---

## Complete Frontend Implementation Example

### React/TypeScript Example:

```typescript
import * as signalR from "@microsoft/signalr";

// Initialize SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://your-api.com/inspectionHub", {
        accessTokenFactory: () => getAuthToken()
    })
    .withAutomaticReconnect()
    .build();

// Start connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Manager joins the managers group
        if (userRole === "Manager" || userRole === "Admin") {
            await connection.invoke("JoinManagersGroup");
            console.log("Joined Managers group");
        }
        
        // Technician joins their group
        if (userRole === "Technician" && technicianId) {
            await connection.invoke("JoinTechnicianGroup", technicianId);
            console.log("Joined Technician group");
        }
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(startConnection, 5000); // Retry after 5 seconds
    }
}

// Register event handlers
connection.on("InspectionStatusUpdated", (data) => {
    // Update state/UI
    dispatch(updateInspectionStatus(data));
    
    // Show toast notification
    toast.info(`Inspection ${data.newStatus}: ${data.message}`);
});

connection.on("InspectionStarted", (data) => {
    // Update dashboard
    dispatch(markInspectionAsStarted(data));
    
    // Show notification
    toast.info(`${data.technicianName} started inspection`);
});

connection.on("InspectionCompleted", (data) => {
    // Update dashboard
    dispatch(markInspectionAsCompleted(data));
    
    // Show notification with action
    toast.success(
        `Inspection completed by ${data.technicianName}`,
        {
            action: {
                label: "View Details",
                onClick: () => navigate(`/inspections/${data.inspectionId}`)
            }
        }
    );
    
    // Refresh inspection list
    refetchInspections();
});

// Handle reconnection
connection.onreconnected(() => {
    console.log("SignalR Reconnected");
    // Rejoin groups after reconnection
    if (userRole === "Manager") {
        connection.invoke("JoinManagersGroup");
    }
});

// Start the connection
startConnection();
```

---

### Vue.js Example:

```javascript
import * as signalR from "@microsoft/signalr";

export default {
    data() {
        return {
            connection: null,
            inspections: []
        };
    },
    
    async mounted() {
        // Initialize SignalR
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("https://your-api.com/inspectionHub", {
                accessTokenFactory: () => this.getAuthToken()
            })
            .withAutomaticReconnect()
            .build();
        
        // Register handlers
        this.connection.on("InspectionStatusUpdated", this.handleStatusUpdate);
        this.connection.on("InspectionStarted", this.handleInspectionStarted);
        this.connection.on("InspectionCompleted", this.handleInspectionCompleted);
        
        // Start connection
        await this.connection.start();
        
        // Join managers group
        if (this.isManager) {
            await this.connection.invoke("JoinManagersGroup");
        }
    },
    
    methods: {
        handleStatusUpdate(data) {
            // Find and update inspection in list
            const index = this.inspections.findIndex(i => i.inspectionId === data.inspectionId);
            if (index !== -1) {
                this.inspections[index].status = data.newStatus;
                this.inspections[index].updatedAt = data.updatedAt;
            }
            
            // Show notification
            this.$notify({
                title: "Inspection Updated",
                message: data.message,
                type: "info"
            });
        },
        
        handleInspectionStarted(data) {
            this.$notify({
                title: "Inspection Started",
                message: `${data.technicianName} started working`,
                type: "info"
            });
        },
        
        handleInspectionCompleted(data) {
            this.$notify({
                title: "Inspection Completed",
                message: `${data.technicianName} completed inspection`,
                type: "success",
                duration: 0, // Don't auto-close
                onClick: () => {
                    this.$router.push(`/inspections/${data.inspectionId}`);
                }
            });
        }
    },
    
    beforeUnmount() {
        if (this.connection) {
            this.connection.stop();
        }
    }
};
```

---

## Testing the Real-Time Updates

### Test Scenario 1: Technician Starts Inspection
1. Manager opens dashboard and connects to SignalR
2. Manager joins "Managers" group
3. Technician updates inspection status to "InProgress"
4. Manager receives `InspectionStarted` event immediately
5. Dashboard updates to show inspection as "In Progress"

### Test Scenario 2: Technician Completes Inspection
1. Manager is monitoring inspections dashboard
2. Technician completes inspection (status → "Completed")
3. Manager receives `InspectionCompleted` event with full details
4. Dashboard shows "Completed" badge
5. Notification appears with "Create Quotation" button

### Test Scenario 3: Multiple Managers
1. Multiple managers connect and join "Managers" group
2. Any inspection status change is broadcast to ALL managers
3. All managers see updates simultaneously

---

## Status Flow

```
Pending → InProgress → Completed
   ↓          ↓            ↓
   ✓    InspectionStarted  InspectionCompleted
        event sent         event sent
```

---

## Important Notes

1. **Authentication:** SignalR connection requires valid JWT token
2. **Reconnection:** Connection automatically reconnects if dropped
3. **Group Rejoin:** After reconnection, clients must rejoin groups
4. **Multiple Tabs:** Each browser tab creates separate connection
5. **Performance:** Managers group broadcasts to all connected managers efficiently

---

## Troubleshooting

### Connection Issues:
```javascript
connection.onclose((error) => {
    console.error("SignalR Disconnected:", error);
    // Attempt to reconnect
    setTimeout(() => startConnection(), 5000);
});
```

### Not Receiving Events:
- Verify you've joined the correct group
- Check authentication token is valid
- Ensure SignalR hub is configured in Startup.cs
- Check browser console for errors

### Events Not Firing:
- Verify inspection status is actually changing
- Check server logs for SignalR send operations
- Ensure UpdateInspectionAsync is being called with status change
