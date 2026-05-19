# Freelance-Job-Matching-System
Build a website to connect job seekers and freelancers.

Mình tóm tắt lại chủ đề dự án sau và phân công luôn nhé :
- Tên dự án : Freelancer Job Matching System (Xây dựng website kết nối người tìm việc và bên thuê freelancer) màu chủ đạo giống freelancerviet.vn
- Công cụ : Backend: ASP.NET Core MVC 
            Frontend: Razor View + Bootstrap (hoặc React.js)
            Database : SQL 
            Authentication : ASP.NET Identity
            Github 
- Tuần 1 và 2 : thì sẽ làm xong file RDS và USE CASE, ERD , Database, Screen flow
- Về các Actor : 
                Freelancer :Người tìm việc freelance, tạo profile, upload CV, tìm và apply job
                Employer : Nhà tuyển dụng hoặc công ty đăng job và quản lý ứng viên
                Admin :Quản trị hệ thống, quản lý user, job, category, report
                Manager : Theo dõi dashboard, thống kê, doanh thu, KPI hệ thống 
                Guest: Người chưa đăng nhập, chỉ xem homepage/job list

- 2 luồng quan trọng nhất nên làm trước
  + Post Job 
  + Apply Job

- Document : 
 RDS – Freelancer Marketplace System

* Hiếu
  Phần I.1 – User Requirements
  • Actors (Freelancer, Employer, Admin, Guest)
  • Use Case Diagram
  • Bảng mô tả Use Cases
  • Screens Flow (I.2.1)

  Các UC chính:

  * Register
  * Login
  * Manage Profile
  * Search Job
  * Apply Job
  * Post Job
  * Manage Applications
  * Manage Users

---

* Kiệt
  Phần I.2.2 – I.3 (Overall Functionalities + System High Level Design)
  • Screen Descriptions
  • Screen Authorization
  • Non-UI Functions
  • Database Schema (ERD + Table Description)
  • Code Packages (MVC structure)

  Bao gồm:

  * Users
  * Roles
  * Freelancer Profiles
  * Employer Profiles
  * Jobs
  * Applications
  * Categories
  * Skills

---

* Dương
  Phần II – Common Functions
  • UC-01: Register System
  • UC-02: Login System
  • UC-03: Forgot Password
  • Common Authentication Functions

  Viết:

  * Normal Flow
  * Alternative Flow
  * Exceptions
  * Business Rules

---

* Hiển
  Phần II – Main Features (Freelancer & Employer Features)
  • UC-04: Manage Freelancer Profile
  • UC-05: Search Job
  • UC-06: Apply Job
  • UC-07: Create/Edit/Delete Job
  • UC-08: Manage Applications

  Bao gồm:

  * Functional Description
  * Preconditions/Postconditions
  * Business Rules
  * Exception Flow

---

* Đức
  Phần III – Design Specifications + Phần IV – Appendix
  • UI Design
  • User Login Screen
  • Job List Screen
  • Job Detail Screen
  • Dashboard Screen
  • Database Access
  • SQL Commands

  Phần IV:
  • Assumptions & Dependencies
  • Limitations & Exclusions
  • Common Business Rules
  • Record of Changes
  • Final format checking toàn bộ tài liệu

  Đồng thời:
  • kiểm tra format
  • heading
  • numbering
  • screenshot UI
