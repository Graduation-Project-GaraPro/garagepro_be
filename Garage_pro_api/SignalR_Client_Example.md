# Hướng dẫn sử dụng SignalR Real-time cho EmergencyRequest

## 1. Kết nối SignalR Hub

```javascript
import * as signalR from "@microsoft/signalr";

// Tạo connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/emergencyrequesthub")
    .withAutomaticReconnect()
    .build();

// Bắt đầu kết nối
connection.start()
    .then(() => console.log("Connected to EmergencyRequest Hub"))
    .catch(err => console.error("Connection error:", err));
```

## 2. Lắng nghe sự kiện khi Emergency được tạo mới

```javascript
// Lắng nghe khi emergency mới được tạo
connection.on("EmergencyRequestCreated", (data) => {
    console.log("Emergency Request Created:", data);
    // data chứa:
    // - EmergencyRequestId
    // - Status: "Pending"
    // - CustomerId
    // - BranchId
    // - VehicleId
    // - IssueDescription
    // - Latitude, Longitude
    // - RequestTime
    // - CustomerName, CustomerPhone
    // - BranchName
    // - Message: "Có yêu cầu cứu hộ mới"
    // - Timestamp
    
    // Hiển thị thông báo cho user
    showNotification(data.Message, "info");
    
    // Thêm vào danh sách emergency requests (nếu là admin/branch)
    if (isAdmin || isBranch) {
        addEmergencyRequestToList(data);
    }
    
    // Cập nhật UI cho customer
    if (isCustomer) {
        addToMyEmergencyRequests(data);
    }
});
```

## 3. Lắng nghe sự kiện khi Emergency được Approve

```javascript
// Lắng nghe khi emergency được approve
connection.on("EmergencyRequestApproved", (data) => {
    console.log("Emergency Request Approved:", data);
    // data chứa:
    // - EmergencyRequestId
    // - Status: "Accepted"
    // - CustomerId
    // - BranchId
    // - EstimatedCost
    // - DistanceToGarageKm
    // - Message: "Yêu cầu cứu hộ đã được duyệt"
    // - Timestamp
    
    // Hiển thị thông báo cho user
    showNotification(data.Message, "success");
    
    // Cập nhật UI
    updateEmergencyStatus(data.EmergencyRequestId, "Accepted");
});
```

## 4. Lắng nghe sự kiện khi Emergency bị Reject

```javascript
// Lắng nghe khi emergency bị reject
connection.on("EmergencyRequestRejected", (data) => {
    console.log("Emergency Request Rejected:", data);
    // data chứa:
    // - EmergencyRequestId
    // - Status: "Canceled"
    // - CustomerId
    // - BranchId
    // - RejectReason
    // - IssueDescription
    // - CustomerName, CustomerPhone
    // - BranchName
    // - Message: "Yêu cầu cứu hộ đã bị từ chối"
    // - Timestamp
    
    // Hiển thị thông báo cho user
    showNotification(data.Message, "error");
    
    // Hiển thị lý do từ chối nếu có
    if (data.RejectReason) {
        showNotification(`Lý do: ${data.RejectReason}`, "warning");
    }
    
    // Cập nhật UI
    updateEmergencyStatus(data.EmergencyRequestId, "Canceled");
});
```

## 5. Join vào Group để nhận thông báo theo Customer hoặc Branch

```javascript
// Join vào group của customer để chỉ nhận thông báo của customer đó
connection.invoke("JoinCustomerGroup", customerId)
    .then(() => console.log(`Joined customer group: ${customerId}`))
    .catch(err => console.error("Error joining customer group:", err));

// Join vào group của branch để chỉ nhận thông báo của branch đó
connection.invoke("JoinBranchGroup", branchId)
    .then(() => console.log(`Joined branch group: ${branchId}`))
    .catch(err => console.error("Error joining branch group:", err));
```

## 6. Ví dụ đầy đủ với React/TypeScript

```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface EmergencyNotification {
    EmergencyRequestId: string;
    Status: string;
    CustomerId: string;
    BranchId: string;
    EstimatedCost?: number;
    DistanceToGarageKm?: number;
    RejectReason?: string;
    Message: string;
    Timestamp: string;
}

export const useEmergencyRequestHub = (customerId?: string, branchId?: string) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [notifications, setNotifications] = useState<EmergencyNotification[]>([]);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/api/emergencyrequesthub")
            .withAutomaticReconnect()
            .build();

        // Lắng nghe sự kiện
        newConnection.on("EmergencyRequestCreated", (data: EmergencyNotification) => {
            console.log("Emergency Created:", data);
            setNotifications(prev => [...prev, data]);
            // Hiển thị toast notification
            toast.info(data.Message);
        });

        newConnection.on("EmergencyRequestApproved", (data: EmergencyNotification) => {
            console.log("Emergency Approved:", data);
            setNotifications(prev => [...prev, data]);
            // Hiển thị toast notification
            toast.success(data.Message);
        });

        newConnection.on("EmergencyRequestRejected", (data: EmergencyNotification) => {
            console.log("Emergency Rejected:", data);
            setNotifications(prev => [...prev, data]);
            // Hiển thị toast notification
            toast.error(data.Message);
        });

        // Kết nối
        newConnection.start()
            .then(() => {
                console.log("Connected to EmergencyRequest Hub");
                
                // Join groups nếu có
                if (customerId) {
                    newConnection.invoke("JoinCustomerGroup", customerId);
                }
                if (branchId) {
                    newConnection.invoke("JoinBranchGroup", branchId);
                }
            })
            .catch(err => console.error("Connection error:", err));

        setConnection(newConnection);

        // Cleanup
        return () => {
            if (newConnection) {
                newConnection.stop();
            }
        };
    }, [customerId, branchId]);

    return { connection, notifications };
};
```

## 7. Sử dụng trong Component

```typescript
const EmergencyRequestPage = () => {
    const { user } = useAuth();
    const { notifications } = useEmergencyRequestHub(user?.id, user?.branchId);

    return (
        <div>
            <h1>Emergency Requests</h1>
            {notifications.map((notif, index) => (
                <div key={index} className={`alert alert-${notif.Status === 'Accepted' ? 'success' : 'danger'}`}>
                    {notif.Message}
                </div>
            ))}
        </div>
    );
};
```

## 8. Test với Browser Console

```javascript
// Mở browser console và chạy:

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://your-api-url/api/emergencyrequesthub")
    .build();

connection.start().then(() => {
    console.log("Connected!");
    
    connection.on("EmergencyRequestCreated", (data) => {
        console.log("Created:", data);
    });
    
    connection.on("EmergencyRequestApproved", (data) => {
        console.log("Approved:", data);
    });
    
    connection.on("EmergencyRequestRejected", (data) => {
        console.log("Rejected:", data);
    });
});
```

Sau đó gọi API create/approve/reject để test real-time notification!

## 9. Tóm tắt Events được gửi

Có 3 events chính được gửi qua SignalR:

### 1. `EmergencyRequestCreated`
- **Khi nào**: Khi có emergency mới được tạo
- **Gửi đến**: Tất cả clients, customer group, branch group
- **Data chứa**:
  - EmergencyRequestId
  - Status: "Pending"
  - CustomerId, BranchId, VehicleId
  - IssueDescription
  - Latitude, Longitude
  - RequestTime
  - CustomerName, CustomerPhone
  - BranchName
  - Message: "Có yêu cầu cứu hộ mới"
  - Timestamp

### 2. `EmergencyRequestApproved`
- **Khi nào**: Khi emergency được approve
- **Gửi đến**: Tất cả clients, customer group, branch group
- **Data chứa**:
  - EmergencyRequestId
  - Status: "Accepted"
  - CustomerId, BranchId
  - EstimatedCost
  - DistanceToGarageKm
  - Message: "Yêu cầu cứu hộ đã được duyệt"
  - Timestamp

### 3. `EmergencyRequestRejected`
- **Khi nào**: Khi emergency bị reject
- **Gửi đến**: Tất cả clients, customer group, branch group
- **Data chứa**:
  - EmergencyRequestId
  - Status: "Canceled"
  - CustomerId, BranchId
  - RejectReason
  - IssueDescription
  - CustomerName, CustomerPhone
  - BranchName
  - Message: "Yêu cầu cứu hộ đã bị từ chối"
  - Timestamp

