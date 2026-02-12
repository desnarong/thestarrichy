# 📊 รายงานทั้งหมด (API Catalog)

## 🎯 วัตถุประสงค์
สร้างตารางแสดงรายงานทั้งหมดจาก API endpoints ที่มีอยู่ในระบบ TheStarRichy เพื่อให้ผู้ใช้สามารถดูฟิลด์ข้อมูลทั้งหมดที่สามารถเรียกใช้งานได้

## 📋 คุณสมบัติ
1. **แสดงรายการ API endpoints ทั้งหมด** - 19 endpoints จาก 7 controllers
2. **แสดงฟิลด์ข้อมูล** - แสดงฟิลด์ทั้งหมดที่ API แต่ละ endpoint คืนค่า
3. **ระบบกรองข้อมูล** - สามารถกรองตาม:
   - Controller (Member, Product, Static, Cart, KbankPayment, Login, Master)
   - Authentication (ต้องมี/ไม่ต้องมี)
   - ค้นหาด้วยคำค้นหา
4. **คัดลอก Endpoint** - คลิกที่ endpoint เพื่อคัดลอกไปใช้งาน
5. **ตัวอย่างการใช้งาน** - แสดงตัวอย่างการเรียกใช้งานด้วย cURL และ JavaScript

## 🚀 วิธีการใช้งาน

### 1. เข้าถึงรายงาน
```
http://localhost:5122/reports/reportcatalog
```

### 2. ข้อมูลในตาราง
ตารางแสดงข้อมูลดังนี้:
- **#**: ลำดับ
- **ชื่อรายงาน**: ชื่อรายงานภาษาไทย
- **Endpoint**: URL path ของ API
- **Method**: HTTP method (GET/POST)
- **Auth**: ต้องการ Authentication หรือไม่
- **ฟิลด์ข้อมูล**: ฟิลด์ทั้งหมดที่ API คืนค่า

### 3. ตัวอย่าง API Endpoints ที่สำคัญ

#### MemberController
- `/Member/display` - ข้อมูลสมาชิก
- `/Member/incomebyperiod` - รายได้ตามช่วง
- `/Member/reportmemberleftteam` - รายงานทีมซ้าย
- `/Member/reportmemberrightteam` - รายงานทีมขวา
- `/Member/reportbonusbydate` - รายงานโบนัสตามวันที่

#### ProductController
- `/api/Product/productgroup` - รายการกลุ่มสินค้า
- `/api/Product/groupofproducts` - สินค้าตามกลุ่ม
- `/api/Product/productlistfortopup` - สินค้า Topup

#### StaticController
- `/Static/banks` - รายการธนาคาร
- `/Static/countries` - รายการประเทศ
- `/Static/districts` - เขต/อำเภอ
- `/Static/titlenames` - คำนำหน้าชื่อ

## 🔧 การพัฒนา

### โครงสร้างไฟล์
```
TheStarRichyProject/
├── Controllers/
│   └── reportsController.cs          # เพิ่ม action reportcatalog()
├── Views/
│   └── reports/
│       └── reportcatalog.cshtml      # View หลักแสดงตารางรายงาน
└── REPORT_CATALOG_README.md          # ไฟล์นี้
```

### เทคโนโลยีที่ใช้
- **ASP.NET Core MVC** - สำหรับ backend
- **DataTables** - สำหรับตารางแบบ interactive
- **Bootstrap 5** - สำหรับ UI styling
- **Toastr** - สำหรับ notifications
- **jQuery** - สำหรับ JavaScript interactions

## 📝 ข้อมูล API

### Authentication
สำหรับ endpoints ที่ต้องมี Authentication ต้องส่ง Header:
```http
Authorization: Bearer <your-jwt-token>
X-Passkey: <your-passkey>
```

### ตัวอย่างการเรียกใช้งาน

#### ด้วย cURL
```bash
# เรียกข้อมูลสมาชิก
curl -X GET "http://localhost:5242/Member/display" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "X-Passkey: ibi1Nxvi2Kym0edyf2015zzz"

# เรียกรายงานโบนัสตามวันที่
curl -X GET "http://localhost:5242/Member/reportbonusbydate" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "X-Passkey: ibi1Nxvi2Kym0edyf2015zzz"
```

#### ด้วย JavaScript (Fetch API)
```javascript
async function getMemberReport() {
  const response = await fetch('http://localhost:5242/Member/display', {
    method: 'GET',
    headers: {
      'Authorization': 'Bearer ' + token,
      'X-Passkey': 'ibi1Nxvi2Kym0edyf2015zzz'
    }
  });
  const data = await response.json();
  console.log(data);
}
```

## 🎨 คุณสมบัติพิเศษ

### 1. การกรองข้อมูล
- **กรองตาม Controller**: เลือกแสดงเฉพาะ endpoints จาก controller ที่ต้องการ
- **กรองตาม Authentication**: แสดงเฉพาะ endpoints ที่ต้องมีหรือไม่ต้องมี authentication
- **ค้นหา**: ค้นหาด้วยคำค้นหาในชื่อรายงานหรือฟิลด์ข้อมูล

### 2. การคัดลอก Endpoint
- คลิกที่ endpoint ในคอลัมน์ "Endpoint" เพื่อคัดลอก URL ไปยัง clipboard
- ระบบจะแสดง notification เมื่อคัดลอกสำเร็จ

### 3. การแสดงผลฟิลด์ข้อมูล
- ฟิลด์ข้อมูลแสดงในรูปแบบ badge เพื่อให้อ่านง่าย
- สีสันช่วยแยกแยะประเภทของข้อมูล

## 🔄 การอัพเดทข้อมูล
หากมี API endpoints ใหม่เพิ่มเข้ามา สามารถอัพเดทข้อมูลได้ที่:
1. `TheStarRichyProject/Views/reports/reportcatalog.cshtml` - เพิ่มแถวใหม่ในตาราง
2. ตรวจสอบข้อมูลจาก `API_ENDPOINTS_COMPLETE.md` สำหรับ endpoints ใหม่

## 📊 สถิติ
- **ทั้งหมด**: 19 endpoints
- **MemberController**: 8 endpoints
- **ProductController**: 3 endpoints  
- **StaticController**: 4 endpoints
- **CartController**: 1 endpoint
- **KbankPaymentController**: 1 endpoint
- **LoginController**: 1 endpoint
- **MasterController**: 1 endpoint

## 🏁 สรุป
รายงานนี้ช่วยให้ผู้ใช้สามารถดูข้อมูลทั้งหมดของ API endpoints ที่มีอยู่ในระบบได้อย่างครบถ้วน พร้อมตัวอย่างการใช้งานและฟิลด์ข้อมูลที่คืนค่า ช่วยลดเวลาในการค้นหาและทดสอบ API

---

**วันที่สร้าง**: 3 กุมภาพันธ์ 2026  
**เวอร์ชัน**: 1.0  
**สถานะ**: ✅ เสร็จสมบูรณ์