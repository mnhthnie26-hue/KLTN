-- =====================================================================
-- HỆ THỐNG ĐÁNH GIÁ CHẤT LƯỢNG HOẠT ĐỘNG TRƯỜNG ĐẠI HỌC
-- Database: MySQL 8.0+
-- =====================================================================

CREATE DATABASE IF NOT EXISTS quality_eval_db
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE quality_eval_db;

-- ---------------------------------------------------------------------
-- 1. PHÂN QUYỀN NGƯỜI DÙNG
-- ---------------------------------------------------------------------
CREATE TABLE roles (
    role_id       INT AUTO_INCREMENT PRIMARY KEY,
    role_code     VARCHAR(30)  NOT NULL UNIQUE,   -- ADMIN, MANAGER, LECTURER, STUDENT
    role_name     VARCHAR(100) NOT NULL,
    description   VARCHAR(255)
);

INSERT INTO roles (role_code, role_name, description) VALUES
 ('ADMIN',    'Quản trị hệ thống',  'Toàn quyền quản trị hệ thống'),
 ('MANAGER',  'Cán bộ quản lý',     'Quản lý tiêu chí, khảo sát, báo cáo'),
 ('LECTURER', 'Giảng viên',         'Tham gia khảo sát, xem báo cáo liên quan'),
 ('STUDENT',  'Sinh viên',          'Tham gia khảo sát đánh giá');

CREATE TABLE users (
    user_id        INT AUTO_INCREMENT PRIMARY KEY,
    username       VARCHAR(50)  NOT NULL UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,
    full_name      VARCHAR(150) NOT NULL,
    email          VARCHAR(150) UNIQUE,
    phone          VARCHAR(20),
    role_id        INT NOT NULL,
    department     VARCHAR(150),          -- Khoa / phòng ban
    student_code   VARCHAR(30),           -- MSSV (nếu là sinh viên)
    staff_code     VARCHAR(30),           -- Mã CBGV (nếu là giảng viên/cán bộ)
    status         TINYINT NOT NULL DEFAULT 1,   -- 1: active, 0: locked
    created_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

-- Tài khoản quản trị mặc định: username = admin | password = Admin@123
-- (hash BCrypt bên dưới tương ứng với mật khẩu "Admin@123" — hãy đổi ngay sau khi đăng nhập lần đầu)
INSERT INTO users (username, password_hash, full_name, email, role_id, status)
VALUES ('admin', '$2b$11$Qy3V.Z1K38uDgsktegoOsuyGlqEstlp9rqyDybaR/Lw6MY9N2GLdy', 'Quản trị viên', 'admin@huit.edu.vn', 1, 1);

-- ---------------------------------------------------------------------
-- 2. QUẢN LÝ TIÊU CHÍ / KPI ĐÁNH GIÁ  (Chức năng 01)
-- ---------------------------------------------------------------------
CREATE TABLE criteria_groups (
    group_id      INT AUTO_INCREMENT PRIMARY KEY,
    group_code    VARCHAR(30) NOT NULL UNIQUE,
    group_name    VARCHAR(200) NOT NULL,     -- VD: Đào tạo, NCKH, Cơ sở vật chất, Dịch vụ SV
    description   VARCHAR(500),
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE criteria (
    criteria_id    INT AUTO_INCREMENT PRIMARY KEY,
    group_id       INT NOT NULL,
    criteria_code  VARCHAR(30) NOT NULL UNIQUE,
    criteria_name  VARCHAR(255) NOT NULL,
    description    VARCHAR(500),
    weight         DECIMAL(6,4) NOT NULL DEFAULT 1.0000,   -- trọng số (0..1)
    scale_min      DECIMAL(6,2) NOT NULL DEFAULT 1,        -- thang điểm nhỏ nhất
    scale_max      DECIMAL(6,2) NOT NULL DEFAULT 5,        -- thang điểm lớn nhất
    is_active      TINYINT NOT NULL DEFAULT 1,
    created_by     INT,
    created_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_criteria_group FOREIGN KEY (group_id) REFERENCES criteria_groups(group_id),
    CONSTRAINT fk_criteria_creator FOREIGN KEY (created_by) REFERENCES users(user_id)
);

-- ---------------------------------------------------------------------
-- 3. QUẢN LÝ KHẢO SÁT  (Chức năng 02)
-- ---------------------------------------------------------------------
CREATE TABLE surveys (
    survey_id     INT AUTO_INCREMENT PRIMARY KEY,
    title         VARCHAR(255) NOT NULL,
    description   TEXT,
    target_role   VARCHAR(30),             -- STUDENT / LECTURER / ALL
    start_date    DATETIME,
    end_date      DATETIME,
    status        VARCHAR(20) NOT NULL DEFAULT 'DRAFT',  -- DRAFT, OPEN, CLOSED
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_survey_creator FOREIGN KEY (created_by) REFERENCES users(user_id)
);

CREATE TABLE survey_questions (
    question_id    INT AUTO_INCREMENT PRIMARY KEY,
    survey_id      INT NOT NULL,
    criteria_id    INT NULL,                 -- liên kết tới tiêu chí/KPI (nếu có)
    question_text  VARCHAR(500) NOT NULL,
    question_type  VARCHAR(20) NOT NULL DEFAULT 'SCALE',  -- SCALE, TEXT, SINGLE_CHOICE
    options_json   JSON NULL,                -- danh sách lựa chọn nếu SINGLE_CHOICE
    order_no       INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_question_survey FOREIGN KEY (survey_id) REFERENCES surveys(survey_id) ON DELETE CASCADE,
    CONSTRAINT fk_question_criteria FOREIGN KEY (criteria_id) REFERENCES criteria(criteria_id)
);

CREATE TABLE survey_responses (
    response_id    INT AUTO_INCREMENT PRIMARY KEY,
    survey_id      INT NOT NULL,
    user_id        INT NULL,                 -- có thể ẩn danh -> NULL
    submitted_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_response_survey FOREIGN KEY (survey_id) REFERENCES surveys(survey_id) ON DELETE CASCADE,
    CONSTRAINT fk_response_user FOREIGN KEY (user_id) REFERENCES users(user_id)
);

CREATE TABLE survey_answers (
    answer_id      INT AUTO_INCREMENT PRIMARY KEY,
    response_id    INT NOT NULL,
    question_id    INT NOT NULL,
    answer_value   VARCHAR(500),             -- điểm số hoặc lựa chọn
    answer_text    TEXT,                     -- câu trả lời tự luận
    CONSTRAINT fk_answer_response FOREIGN KEY (response_id) REFERENCES survey_responses(response_id) ON DELETE CASCADE,
    CONSTRAINT fk_answer_question FOREIGN KEY (question_id) REFERENCES survey_questions(question_id)
);

-- ---------------------------------------------------------------------
-- 4. PHÂN TÍCH & ĐÁNH GIÁ CHẤT LƯỢNG  (Chức năng 03)
-- ---------------------------------------------------------------------
-- Điểm được tính toán (theo kỳ đánh giá / đối tượng đánh giá: khoa, GV, môn học...)
CREATE TABLE evaluation_periods (
    period_id     INT AUTO_INCREMENT PRIMARY KEY,
    period_name   VARCHAR(150) NOT NULL,     -- VD: Học kỳ 1 - 2026
    start_date    DATE,
    end_date      DATE,
    status        VARCHAR(20) NOT NULL DEFAULT 'OPEN'
);

CREATE TABLE evaluation_targets (
    target_id     INT AUTO_INCREMENT PRIMARY KEY,
    target_type   VARCHAR(30) NOT NULL,      -- DEPARTMENT, LECTURER, COURSE, SERVICE...
    target_name   VARCHAR(255) NOT NULL,
    ref_id        INT NULL                    -- id tham chiếu (user_id giảng viên, v.v.)
);

CREATE TABLE evaluation_scores (
    score_id      INT AUTO_INCREMENT PRIMARY KEY,
    period_id     INT NOT NULL,
    target_id     INT NOT NULL,
    criteria_id   INT NOT NULL,
    raw_score     DECIMAL(8,4) NOT NULL,      -- điểm trung bình thô từ khảo sát
    weighted_score DECIMAL(8,4) NOT NULL,     -- raw_score * weight
    method        VARCHAR(30) NOT NULL DEFAULT 'AVERAGE', -- AVERAGE, AHP, ML
    computed_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_score_period FOREIGN KEY (period_id) REFERENCES evaluation_periods(period_id),
    CONSTRAINT fk_score_target FOREIGN KEY (target_id) REFERENCES evaluation_targets(target_id),
    CONSTRAINT fk_score_criteria FOREIGN KEY (criteria_id) REFERENCES criteria(criteria_id)
);

-- Ma trận so sánh cặp AHP (lưu theo group tiêu chí)
CREATE TABLE ahp_matrices (
    matrix_id     INT AUTO_INCREMENT PRIMARY KEY,
    group_id      INT NOT NULL,
    criteria_a_id INT NOT NULL,
    criteria_b_id INT NOT NULL,
    comparison_value DECIMAL(6,3) NOT NULL,   -- Saaty scale 1/9 .. 9
    created_by    INT,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ahp_group FOREIGN KEY (group_id) REFERENCES criteria_groups(group_id),
    CONSTRAINT fk_ahp_ca FOREIGN KEY (criteria_a_id) REFERENCES criteria(criteria_id),
    CONSTRAINT fk_ahp_cb FOREIGN KEY (criteria_b_id) REFERENCES criteria(criteria_id)
);

-- ---------------------------------------------------------------------
-- 5. THỐNG KÊ, BÁO CÁO, XẾP HẠNG  (Chức năng 04)
-- ---------------------------------------------------------------------
CREATE TABLE quality_rankings (
    ranking_id    INT AUTO_INCREMENT PRIMARY KEY,
    period_id     INT NOT NULL,
    target_id     INT NOT NULL,
    total_score   DECIMAL(8,4) NOT NULL,
    rank_no       INT,
    rank_label    VARCHAR(30),   -- Xuất sắc, Tốt, Khá, Trung bình, Yếu
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_rank_period FOREIGN KEY (period_id) REFERENCES evaluation_periods(period_id),
    CONSTRAINT fk_rank_target FOREIGN KEY (target_id) REFERENCES evaluation_targets(target_id)
);

CREATE TABLE reports (
    report_id     INT AUTO_INCREMENT PRIMARY KEY,
    period_id     INT NOT NULL,
    title         VARCHAR(255) NOT NULL,
    file_path     VARCHAR(500),     -- đường dẫn file PDF/Excel xuất ra
    generated_by  INT,
    generated_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_report_period FOREIGN KEY (period_id) REFERENCES evaluation_periods(period_id),
    CONSTRAINT fk_report_user FOREIGN KEY (generated_by) REFERENCES users(user_id)
);

-- ---------------------------------------------------------------------
-- 6. NHẬT KÝ HỆ THỐNG (đăng nhập/đăng xuất, audit)
-- ---------------------------------------------------------------------
CREATE TABLE login_logs (
    log_id       INT AUTO_INCREMENT PRIMARY KEY,
    user_id      INT NOT NULL,
    action       VARCHAR(20) NOT NULL,  -- LOGIN, LOGOUT, CHANGE_PASSWORD
    ip_address   VARCHAR(50),
    created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_log_user FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- ---------------------------------------------------------------------
-- DỮ LIỆU MẪU (tuỳ chọn)
-- ---------------------------------------------------------------------
INSERT INTO criteria_groups (group_code, group_name, description) VALUES
 ('DT',  'Đào tạo',              'Chất lượng chương trình, giảng dạy'),
 ('NCKH','Nghiên cứu khoa học',  'Hoạt động nghiên cứu, công bố khoa học'),
 ('CSVC','Cơ sở vật chất',       'Phòng học, thư viện, thiết bị'),
 ('DVSV','Dịch vụ sinh viên',    'Hỗ trợ, tư vấn, hoạt động ngoại khóa');

INSERT INTO criteria (group_id, criteria_code, criteria_name, weight, scale_min, scale_max) VALUES
 (1, 'DT01', 'Nội dung chương trình đào tạo phù hợp thực tiễn', 0.3, 1, 5),
 (1, 'DT02', 'Phương pháp giảng dạy của giảng viên',            0.3, 1, 5),
 (1, 'DT03', 'Tài liệu học tập đầy đủ, cập nhật',                0.2, 1, 5),
 (2, 'NC01', 'Số lượng đề tài/công bố khoa học',                 0.5, 1, 5),
 (3, 'CS01', 'Chất lượng phòng học, trang thiết bị',             0.4, 1, 5),
 (4, 'DV01', 'Chất lượng tư vấn, hỗ trợ sinh viên',               0.4, 1, 5);

INSERT INTO evaluation_periods (period_name, start_date, end_date, status)
VALUES ('Học kỳ 1 - Năm học 2026-2027', '2026-09-01', '2027-01-15', 'OPEN');
