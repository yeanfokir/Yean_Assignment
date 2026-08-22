# CompanyApp - Merged Authentication & Employee Management System

**Student ID:** `24-57480-2`  
**Repository Name:** `24-57480-2_CompanyApp`  
**Target Framework:** `.NET Framework 4.8`  
**Language & Platform:** C# / Windows Forms  
**Database:** Microsoft SQL Server LocalDB (`(localdb)\MSSQLLocalDB` / `dbCompanyApp`)  

---

## Table of Contents
1. [Executive Summary & Architectural Transformation](#1-executive-summary--architectural-transformation)
2. [The Six Structural Conflicts & Solutions](#2-the-six-structural-conflicts--solutions)
3. [Unified Database Design & Access Migration](#3-unified-database-design--access-migration)
4. [The Three-File Rule & Form Import Methodology](#4-the-three-file-rule--form-import-methodology)
5. [Porting OleDb to SqlClient (User.cs & Session.cs)](#5-porting-oledb-to-sqlclient-usercs--sessioncs)
6. [Application Lifecycle & Workflow Wiring](#6-application-lifecycle--workflow-wiring)
7. [Why One Database is Superior to Two (Architectural Rationale)](#7-why-one-database-is-superior-to-two-architectural-rationale)
8. [Real Build Errors Encountered & Solutions](#8-real-build-errors-encountered--solutions)
9. [Bonus Features Implemented](#9-bonus-features-implemented)
10. [Build & Execution Guide](#10-build--execution-guide)

---

## 1. Executive Summary & Architectural Transformation

### Before the Merge:
The starting codebase consisted of two disconnected solutions:
1. **Login-and-Register App:** A .NET Framework 4.7.2 solution relying on `System.Data.OleDb`, connecting to a Microsoft Access file database (`db_users.mdb`) located locally in `bin\Debug`. Started directly at `frmRegister`.
2. **EmployeeDetails App:** A .NET Framework 4.8 solution relying on `System.Data.SqlClient`, connecting to SQL Server LocalDB (`dbEmployeeDetails`), containing a single form `Form1` and an `Employee.cs` data-access class.

### After the Merge:
The resulting solution is a single, unified enterprise desktop application (**`CompanyApp`** / **`24-57480-2_CompanyApp`**) targeting **.NET Framework 4.8** with a single centralized database (**`dbCompanyApp`**). User authentication controls entry into the system; each employee record created is permanently tied to the authenticating user via a `CreatedBy` foreign key, and clean session lifecycle management ensures zero memory leaks or orphaned background processes.

```
+---------------------------------------------------------------------------------------+
|                                    CompanyApp (.NET 4.8)                              |
|                                                                                       |
|   +---------------+      +----------------+      +---------------+                    |
|   |   frmLogin    | ---> |  frmDashboard  | ---> |  frmEmployee  | (Employee CRUD)    |
|   +---------------+      +----------------+      +---------------+                    |
|          ^                       |                                                    |
|          | (Logout / Session.Clear)                                                   |
|          +-----------------------+                                                    |
+---------------------------------------------------------------------------------------+
                                           |
                                [System.Data.SqlClient]
                                           |
                                           v
                       +---------------------------------------+
                       |        Unified SQL Server LocalDB     |
                       |              (dbCompanyApp)           |
                       |  +---------------------------------+  |
                       |  | dbo.Users                       |  |
                       |  | dbo.Emp_details (FK: CreatedBy) |  |
                       |  | dbo.LoginHistory                |  |
                       |  +---------------------------------+  |
                       +---------------------------------------+
```

---

## 2. The Six Structural Conflicts & Solutions

| Conflict # | Conflict Description | Root Cause | Engineering Resolution |
| :--- | :--- | :--- | :--- |
| **1** | **Namespace Divergence** | `Login-and-Register` vs `EmployeeDetails`. Pasting form files directly produced partial class mismatch compiler errors. | Standardized the root namespace of the unified project to `EmployeeDetails`. Updated all `.cs` and `.Designer.cs` files to use `namespace EmployeeDetails`. |
| **2** | **Data Provider Incompatibility** | `System.Data.OleDb` cannot talk to SQL Server, and `System.Data.SqlClient` cannot open `.mdb`. | Completely eliminated `System.Data.OleDb`. Replaced all OleDb connections, commands, and adapters with `System.Data.SqlClient` using parameterized queries (`@param`). |
| **3** | **Dual Database Segregation** | Two separate databases (Access `.mdb` vs SQL Server) prevented relational joins and consistency. | Created one unified SQL Server database (`dbCompanyApp`) containing `dbo.Users`, `dbo.Emp_details`, and `dbo.LoginHistory`. Linked tables via foreign key `FK_Emp_CreatedBy`. |
| **4** | **Framework Version Divergence** | .NET 4.7.2 vs .NET 4.8. | Set host project target to `.NET Framework 4.8`. Importing older framework code into v4.8 ensures binary and language compatibility. |
| **5** | **Dual Program.cs / Main() Entry Points** | Having two `Program.cs` files causes CS0017 ("Program has more than one entry point defined"). | Retained a single `Program.cs` configured with `Application.Run(new frmLogin())` so users authenticate before accessing protected features. |
| **6** | **Hidden File Dependency** | `db_users.mdb` existed only in `bin\Debug`, causing "clean solution" builds to wipe out login capabilities. | Migrated all user credentials into `dbo.Users` inside SQL Server LocalDB. Added `Schema.sql` so the database can be rebuilt deterministically anywhere. |

---

## 3. Unified Database Design & Access Migration

### Database: `dbCompanyApp`

#### 1. Table `dbo.Users`
```sql
CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'User' NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL
);
```

#### 2. Table `dbo.Emp_details`
```sql
CREATE TABLE dbo.Emp_details (
    EmpId NVARCHAR(50) PRIMARY KEY,
    EmpName NVARCHAR(100) NOT NULL,
    EmpAge INT NOT NULL,
    EmpContact NVARCHAR(20) NULL,
    EmpGender NVARCHAR(10) NULL,
    CreatedBy INT NULL,
    CONSTRAINT FK_Emp_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID)
);
```

#### 3. Table `dbo.LoginHistory` (Activity Audit Log)
```sql
CREATE TABLE dbo.LoginHistory (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    Username NVARCHAR(50) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE() NOT NULL,
    CONSTRAINT FK_LoginHistory_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
```

### Access Data Migration:
The records from `db_users.mdb` were extracted and migrated into `dbo.Users`:
- `admin`
- `sayan`
- `sayanchamp`
- `sachin`
- `sayan_admin`
- `iftee`
- `ifte`

> **Note on Foreign Key Nullability:** `CreatedBy` in `dbo.Emp_details` is intentionally `NULL`-able to accommodate pre-existing/legacy employee records where the creating user was unknown, ensuring seamless backwards compatibility without violating relational integrity.

---

## 4. The Three-File Rule & Form Import Methodology

In Windows Forms, every form comprises three coupled files:
1. **`<FormName>.cs`:** User business logic and event handlers.
2. **`<FormName>.Designer.cs`:** Control instantiations, property layout, and visual bindings.
3. **`<FormName>.resx`:** Serialized binary/XML GUI resources.

### Execution:
- All 3 files for `frmLogin`, `frmRegister`, and `frmDashboard` were copied from `Login and Register` into the host project.
- `Form1` in the host project was renamed to `frmEmployee` across its `.cs`, `.Designer.cs`, and `.resx` files.
- In `CompanyApp.csproj`, each form's files are nested using `<DependentUpon>` tags so that Visual Studio's Solution Explorer presents them as unified components.

---

## 5. Porting OleDb to SqlClient (User.cs & Session.cs)

### Security & Reliability Fixes Made:
1. **SQL Injection Removal:** Replaced unsafe string concatenation (`"WHERE username = '" + txtUsername.Text + "'..."`) with parameterized `SqlCommand.Parameters.AddWithValue()`.
2. **Connection Leak Resolution:** The original `frmLogin` opened an unmanaged class-level `OleDbConnection` that was never closed, throwing exceptions on subsequent login attempts. All queries now use `using (SqlConnection con = new SqlConnection(...))` ensuring automatic disposal and connection pooling.
3. **Registration Validation Bug Fix:** Replaced faulty `&&` validation (`if (txtUser == "" && txtPass == "" && txtConPass == "")`) with `||` / `string.IsNullOrWhiteSpace` to prevent blank submissions.

### Session Management (`Session.cs`):
```csharp
public static class Session
{
    public static int UserID { get; set; }
    public static string Username { get; set; }
    public static string Role { get; set; }

    public static void Clear()
    {
        UserID = 0;
        Username = null;
        Role = null;
    }
}
```

### Data Access Architecture (`User.cs`):
Implements the exact same design pattern as `Employee.cs`:
- `ValidateLogin(string username, string password)` $\rightarrow$ Returns `UserID` (`int`, `0` on failure) and sets Session.
- `UsernameExists(string username)` $\rightarrow$ Executes `ExecuteScalar()` query.
- `RegisterUser(string username, string password, string role)` $\rightarrow$ Returns new `UserID`.
- `LogActivity(int? userId, string username, string action)` $\rightarrow$ Logs login/logout events.

---

## 6. Application Lifecycle & Workflow Wiring

1. **Startup:** `Program.cs` launches `frmLogin` (`Application.Run(new frmLogin())`).
2. **Authentication:** On successful login, `Session.UserID` and `Session.Username` are set, `frmDashboard` is shown, and `frmLogin` is hidden.
3. **Protected CRUD Access:** Clicking "MANAGE EMPLOYEES" on `frmDashboard` opens `frmEmployee` (`ShowDialog()`).
4. **Stamping Ownership:** When adding an employee in `frmEmployee`, `employee.CreatedBy = Session.UserID` is automatically saved into the database.
5. **Relational Display:** Employee records are loaded using a `LEFT JOIN`:
   ```sql
   SELECT e.EmpId, e.EmpName, e.EmpAge, e.EmpContact, e.EmpGender,
          ISNULL(u.Username, 'Migrated / N/A') AS [CreatedBy]
   FROM dbo.Emp_details e
   LEFT JOIN dbo.Users u ON e.CreatedBy = u.UserID;
   ```
6. **Clean Logout & No Orphan Process:**
   - Clicking "LOGOUT" prompts a confirmation dialog.
   - On confirmation, `Session.Clear()` resets credentials, a fresh `frmLogin` instance is displayed, and `frmDashboard` is closed.
   - Closing `frmLogin` triggers `Application.Exit()`, terminating all threads cleanly.

---

## 7. Why One Database is Superior to Two (Architectural Rationale)

Having a single unified database eliminates data silos and guarantees ACID consistency across the entire application domain. With two independent databases (Access and SQL Server), linking an employee to the user who registered them would require complex, error-prone application-level joining across heterogeneous database drivers. In contrast, a single unified database allows high-performance declarative queries like `SELECT e.EmpName, u.Username AS CreatedBy FROM dbo.Emp_details e LEFT JOIN dbo.Users u ON e.CreatedBy = u.UserID`. A `LEFT JOIN` preserves legacy records where `CreatedBy IS NULL` while instantly showing creator usernames for active records in a single atomic query. Furthermore, a single database enables central connection pooling, automated backups, and unified security policies.

---

## 8. Real Build Errors Encountered & Solutions

### Error 1: CS0017 - Program has more than one entry point defined
- **Cause:** Copying forms and files initially retained the `Program.cs` from `Login and Register` alongside the existing `Program.cs` in `EmployeeDetails`, creating two `static void Main()` methods.
- **Solution:** Removed the duplicate `Program.cs` and unified the startup routine inside a single `Program.cs` executing `Application.Run(new frmLogin())`.

### Error 2: Partial Declarations Must Be in the Same Namespace
- **Cause:** When `frmLogin.cs` and `frmLogin.Designer.cs` were imported, their namespace was `Login_and_Register`, while the project root was `EmployeeDetails`. Changing one file without the other caused compiler errors stating that partial classes could not be merged.
- **Solution:** Performed a solution-wide namespace standardization replacing `Login_and_Register` with `EmployeeDetails` across both `.cs` and `.Designer.cs` files.

---

## 9. Bonus Features Implemented (+15 Marks)

1. **SHA-256 Password Hashing with Legacy Auto-Upgrade:**
   - User passwords are automatically hashed using SHA-256 with UTF-8 encoding.
   - Legacy plaintext accounts migrated from Access can log in smoothly; upon their first successful authentication, their password is automatically upgraded to a secure SHA-256 hash in the database.
2. **Dynamic Search by Name / ID (`LIKE @term`):**
   - Added a real-time Search box and "Search" / "Show All" buttons in `frmEmployee` using parameterized queries:
     `WHERE e.EmpName LIKE @term OR e.EmpId LIKE @term`.
3. **Confirmation Dialog on Delete:**
   - Destructive delete actions require explicit user confirmation via `MessageBoxButtons.YesNo` to prevent accidental data loss.
4. **Audit Log / Activity Tracking (`dbo.LoginHistory`):**
   - Automatically records user login and logout timestamps and actions for security tracking.

---

## 10. Build & Execution Guide

### Prerequisites:
- Visual Studio 2019 / 2022 with .NET desktop development workload.
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`).

### Setup Steps:
1. Open SQL Server Management Studio (SSMS) or Visual Studio SQL Server Object Explorer.
2. Execute [Schema.sql](file:///D:/Assignment/24-57480-2_CompanyApp/Schema.sql) to create `dbCompanyApp` and seed initial accounts.
3. Open `24-57480-2_CompanyApp.sln` in Visual Studio.
4. Build Solution (`Ctrl+Shift+B`) - Verify 0 errors, 0 warnings.
5. Press `F5` to run the application:
   - Login using `admin` / `12345` or `sayan` / `Sayan@1234`.
   - Access the Dashboard and click **"MANAGE EMPLOYEES"**.
   - Perform CRUD, Search, and verify the `CreatedBy` column in the DataGridView.
   - Click **"LOGOUT"** and verify return to Login.
