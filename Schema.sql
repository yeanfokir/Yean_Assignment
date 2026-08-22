-- ==========================================================
-- Database Schema for dbCompanyApp
-- Student ID: 24-57480-2
-- Application: 24-57480-2_CompanyApp
-- ==========================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'dbCompanyApp')
BEGIN
    CREATE DATABASE [dbCompanyApp];
END
GO

USE [dbCompanyApp];
GO

-- 1. Create dbo.Users Table
IF OBJECT_ID(N'dbo.FK_Emp_CreatedBy', N'F') IS NOT NULL
    ALTER TABLE dbo.Emp_details DROP CONSTRAINT FK_Emp_CreatedBy;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'User' NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL
);
GO

-- 2. Create dbo.Emp_details Table
IF OBJECT_ID(N'dbo.Emp_details', N'U') IS NOT NULL
    DROP TABLE dbo.Emp_details;
GO

CREATE TABLE dbo.Emp_details (
    EmpId NVARCHAR(50) PRIMARY KEY,
    EmpName NVARCHAR(100) NOT NULL,
    EmpAge INT NOT NULL,
    EmpContact NVARCHAR(20) NULL,
    EmpGender NVARCHAR(10) NULL,
    CreatedBy INT NULL,
    CONSTRAINT FK_Emp_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID)
);
GO

-- 3. Create dbo.LoginHistory Table (Bonus feature for activity tracking)
IF OBJECT_ID(N'dbo.LoginHistory', N'U') IS NOT NULL
    DROP TABLE dbo.LoginHistory;
GO

CREATE TABLE dbo.LoginHistory (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    Username NVARCHAR(50) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE() NOT NULL,
    CONSTRAINT FK_LoginHistory_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

-- ==========================================================
-- MIGRATED DATA FROM db_users.mdb (Access Database)
-- ==========================================================

INSERT INTO dbo.Users (Username, Password, Role) VALUES
(N'admin', N'12345', N'Admin'),
(N'sayan', N'Sayan@1234', N'User'),
(N'sayanchamp', N'12345', N'User'),
(N'sachin', N'123', N'User'),
(N'sayan_admin', N'12345', N'Admin'),
(N'iftee', N'123', N'User'),
(N'ifte', N'123', N'User');
GO

-- Sample Initial Employee Data (with NULL CreatedBy representing legacy migrated records)
INSERT INTO dbo.Emp_details (EmpId, EmpName, EmpAge, EmpContact, EmpGender, CreatedBy) VALUES
(N'EMP-101', N'John Doe', 30, N'01711000001', N'Male', NULL),
(N'EMP-102', N'Jane Smith', 28, N'01811000002', N'Female', 1);
GO
