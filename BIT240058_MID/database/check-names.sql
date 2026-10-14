-- =====================================================================
-- Script dùng khi DEMO mục 1: chứng minh database, 3 bảng và TẤT CẢ cột
-- đều có tiền tố MSSV BIT240058.
-- (Database và bảng do ứng dụng tự tạo khi chạy lần đầu - không cần tạo tay.)
-- =====================================================================
USE BIT240058_MID;
GO

-- 1. Danh sách bảng
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 2. Danh sách cột của 3 bảng
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE 'BIT240058[_]%'
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- 3. Kiểm tra: cột nào KHÔNG có tiền tố MSSV (kết quả rỗng là đạt)
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE 'BIT240058[_]%'
  AND COLUMN_NAME NOT LIKE 'BIT240058[_]%';

-- 4. Xem dữ liệu
SELECT * FROM BIT240058_DishCategories;
SELECT * FROM BIT240058_Dishes;
SELECT * FROM BIT240058_DishImages;
