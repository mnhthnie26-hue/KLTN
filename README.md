# Hệ thống đánh giá chất lượng hoạt động Trường Đại học
**Trường Đại học Công Thương TP. Hồ Chí Minh – Khoa Công nghệ Thông tin**

Ứng dụng web quản lý và đánh giá chất lượng các loại hình hoạt động của trường:
tiêu chí/KPI, khảo sát trực tuyến, phân tích (thống kê + AHP), thống kê/báo cáo,
và quản lý người dùng theo vai trò.

## 1. Kiến trúc hệ thống

| Thành phần   | Công nghệ                                   |
|--------------|----------------------------------------------|
| Frontend     | HTML5 / CSS3 / JavaScript (thuần, không cần build) |
| Backend API  | C# ASP.NET Core 8 Web API                    |
| Database     | MySQL 8.0+                                    |
| ORM          | Entity Framework Core + Pomelo MySQL Provider |
| Xác thực     | JWT Bearer Token + BCrypt (mã hoá mật khẩu)   |

```
project/
├── backend/QualityEvalAPI/     # Mã nguồn C# ASP.NET Core Web API
│   ├── Controllers/            # Auth, Criteria, Survey, Evaluation, Statistics, Users
│   ├── Models/                 # Entity mapping tới MySQL
│   ├── DTOs/                   # Request/Response models
│   ├── Data/AppDbContext.cs    # EF Core DbContext
│   ├── Services/                # AuthService (JWT), AhpService (thuật toán AHP)
│   └── Program.cs
├── database/schema.sql         # Script tạo toàn bộ database MySQL + dữ liệu mẫu
└── frontend/                   # Giao diện web (HTML/CSS/JS)
    ├── login.html, dashboard.html, criteria.html, survey.html,
    │   survey-take.html, evaluation.html, reports.html,
    │   users.html, change-password.html
    └── assets/{css,js,images}
```

## 2. Cài đặt Database (MySQL)

1. Cài MySQL Server 8.0+ (hoặc dùng XAMPP/WAMP/Docker).
2. Chạy script tạo CSDL:
   ```bash
   mysql -u root -p < database/schema.sql
   ```
3. Script sẽ tạo database `quality_eval_db`, đầy đủ các bảng và một số dữ liệu mẫu
   (nhóm tiêu chí, tiêu chí, kỳ đánh giá).
4. Tài khoản quản trị mặc định:
   - **Username:** `admin`
   - **Password:** `Admin@123`
   - ⚠️ Đổi mật khẩu ngay sau lần đăng nhập đầu tiên (trang "Đổi mật khẩu").

## 3. Cài đặt & chạy Backend (C# ASP.NET Core Web API)

**Yêu cầu:** .NET 8 SDK (`dotnet --version` để kiểm tra).

1. Mở `backend/QualityEvalAPI/appsettings.json`, sửa lại chuỗi kết nối MySQL:
   ```json
   "ConnectionStrings": {
     "MySqlConnection": "Server=localhost;Port=3306;Database=quality_eval_db;User=root;Password=MẬT_KHẨU_CỦA_BẠN;"
   }
   ```
   Đồng thời đổi `"Jwt:Key"` thành một chuỗi bí mật ngẫu nhiên dài (≥32 ký tự) cho môi trường thực tế.

2. Khôi phục package và chạy:
   ```bash
   cd backend/QualityEvalAPI
   dotnet restore
   dotnet run
   ```
3. Mặc định API chạy tại `https://localhost:7080` (hoặc cổng do `dotnet run` in ra console —
   cập nhật lại `API_BASE_URL` trong `frontend/assets/js/api.js` cho khớp).
4. Truy cập Swagger để test API: `https://localhost:7080/swagger`

### Danh sách API chính

| Chức năng | Method & Endpoint |
|---|---|
| Đăng nhập | `POST /api/auth/login` |
| Đăng xuất | `POST /api/auth/logout` |
| Đổi mật khẩu | `POST /api/auth/change-password` |
| **01. Tiêu chí/KPI** | `GET/POST/PUT/DELETE /api/criteria`, `/api/criteria-groups` |
| **02. Khảo sát** | `GET/POST/PUT/DELETE /api/surveys`, `POST /api/surveys/{id}/responses` |
| **03. Phân tích & đánh giá** | `POST /api/evaluation/compute` (tính điểm KPI), `POST /api/evaluation/ahp` (thuật toán AHP) |
| **04. Thống kê & báo cáo** | `GET /api/statistics/dashboard`, `POST /api/statistics/rankings/{periodId}/generate`, `GET /api/statistics/reports/{periodId}/export-csv` |
| **05. Người dùng** | `GET/POST/PUT/DELETE /api/users` (chỉ ADMIN) |

## 4. Chạy Frontend (HTML/CSS/JS)

Không cần build — chỉ cần một web server tĩnh đơn giản (do trình duyệt chặn `fetch`
từ `file://`):

- **VS Code:** cài extension "Live Server" → chuột phải `frontend/login.html` → "Open with Live Server".
- **Python:** 
  ```bash
  cd frontend
  python -m http.server 5500
  ```
  rồi mở `http://localhost:5500/login.html`.

Nhớ cấu hình đúng cổng frontend trong `backend/QualityEvalAPI/appsettings.json` mục
`Cors:AllowedOrigins` để backend chấp nhận request từ frontend.

## 5. Phân quyền người dùng (role-based access)

| Vai trò | Quyền hạn |
|---|---|
| **ADMIN** | Toàn quyền: quản lý người dùng, tiêu chí, khảo sát, đánh giá, báo cáo |
| **MANAGER** (Cán bộ quản lý) | Quản lý tiêu chí, khảo sát, chạy phân tích/đánh giá, xem báo cáo |
| **LECTURER** (Giảng viên) | Tham gia khảo sát, xem báo cáo/thống kê |
| **STUDENT** (Sinh viên) | Tham gia khảo sát, xem tổng quan |

Phân quyền được áp dụng ở **hai lớp**: (1) Backend dùng `[Authorize(Roles = "...")]`
trên từng Controller/Action; (2) Frontend ẩn/hiện menu và chặn truy cập trang theo
`localStorage` role sau khi đăng nhập (chỉ mang tính UX, lớp bảo vệ thật sự nằm ở backend).

## 6. Phương pháp phân tích chất lượng đã cài đặt

- **Trung bình có trọng số:** điểm mỗi tiêu chí = trung bình phản hồi khảo sát × trọng số tiêu chí (`EvaluationController.ComputeScores`).
- **AHP (Analytic Hierarchy Process):** người dùng nhập ma trận so sánh cặp theo thang Saaty (1/9 → 9),
  hệ thống tự tính **trọng số ưu tiên (Eigenvector xấp xỉ)** và **tỷ số nhất quán CR**
  (`AhpService.Compute`). Nếu CR < 0.1 (nhất quán), trọng số tiêu chí sẽ được tự động cập nhật.
- Kiến trúc đã chừa sẵn để mở rộng phương pháp **Machine Learning** (ví dụ hồi quy dự đoán xu hướng
  chất lượng) ở tầng `Services/` khi cần.

## 7. Việc tiếp theo có thể mở rộng
- Thêm API quản lý `evaluation_periods` / `evaluation_targets` (hiện đang dùng ID mẫu trên UI).
- Xuất báo cáo dạng PDF/Excel (hiện đã có xuất CSV).
- Thêm refresh token, khoá tài khoản sau nhiều lần đăng nhập sai.
- Triển khai mô hình Machine Learning dự đoán xu hướng chất lượng theo thời gian.

---
Nếu cần bổ sung tính năng gì tiếp theo, cứ nói nhé!
