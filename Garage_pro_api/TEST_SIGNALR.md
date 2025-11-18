# Hướng dẫn Test SignalR Real-time

## Cách 1: Test bằng HTML Page (Dễ nhất) ⭐

### Bước 1: Chạy ứng dụng
```powershell
cd "D:\ĐỒ an final\garagepro_be\Garage_pro_api"
dotnet run
```

### Bước 2: Mở browser
Truy cập: **http://localhost:5117/test-signalr.html**

### Bước 3: Test
1. Click nút **"Connect"** để kết nối SignalR
2. Click các nút test:
   - **Test Created** - Test notification khi tạo emergency
   - **Test Approved** - Test notification khi approve
   - **Test Rejected** - Test notification khi reject
3. Xem logs real-time hiển thị trong trang

---

## Cách 2: Test bằng PowerShell Script

```powershell
cd "D:\ĐỒ an final\garagepro_be\Garage_pro_api"
.\test-signalr.ps1
```

Script sẽ test cả 3 endpoints và hiển thị kết quả.

---

## Cách 3: Test bằng Postman/Swagger

### Endpoints:
1. **POST** `/api/TestSignalR/test-created`
2. **POST** `/api/TestSignalR/test-approved`
3. **POST** `/api/TestSignalR/test-rejected`

Sau khi gọi API, nếu có client đang kết nối SignalR, sẽ nhận được notification real-time.

---

## Cách 4: Test bằng Browser Console

1. Mở browser console (F12)
2. Chạy code sau:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5117/api/emergencyrequesthub")
    .build();

connection.start().then(() => {
    console.log("✓ Connected!");
    
    connection.on("EmergencyRequestCreated", (data) => {
        console.log("📝 Created:", data);
    });
    
    connection.on("EmergencyRequestApproved", (data) => {
        console.log("✅ Approved:", data);
    });
    
    connection.on("EmergencyRequestRejected", (data) => {
        console.log("❌ Rejected:", data);
    });
});

// Sau đó gọi API test
fetch("http://localhost:5117/api/TestSignalR/test-created", { method: 'POST' })
    .then(r => r.json())
    .then(data => console.log("API Response:", data));
```

---

## Cách 5: Test với API thật

1. Tạo Emergency request thật:
   ```
   POST /api/EmergencyRequest/create
   ```

2. Approve Emergency:
   ```
   POST /api/EmergencyRequest/approve/{emergencyId}
   ```

3. Reject Emergency:
   ```
   PUT /api/EmergencyRequest/reject/{emergencyId}
   ```

Nếu có client đang kết nối SignalR, sẽ nhận được notification real-time.

---

## Kiểm tra Logs

Xem console logs của ứng dụng để thấy:
- `[TEST] Sent EmergencyRequestCreated: {id}`
- `[TEST] Sent EmergencyRequestApproved: {id}`
- `[TEST] Sent EmergencyRequestRejected: {id}`

---

## Troubleshooting

### Lỗi: "Connection refused"
- Đảm bảo ứng dụng đang chạy
- Kiểm tra port (5117 hoặc 7113)

### Lỗi: "404 Not Found"
- Kiểm tra route: `/api/emergencyrequesthub`
- Đảm bảo hub đã được map trong Program.cs

### Không nhận được notification
- Kiểm tra xem đã connect SignalR chưa
- Kiểm tra console logs của browser
- Kiểm tra CORS settings

