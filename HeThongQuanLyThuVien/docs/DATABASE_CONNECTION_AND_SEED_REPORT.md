# Database Connection, Docker, and Demo Seed Audit

Audit date: 2026-10-25  
Branch: `feat/backend-nhan`

## A. Connection and SQL Server Docker

- **PASS** — SQL Server 2022 Developer container `qltv-sqlserver` is running and SQL Server reports ready.
- **PASS** — Host port mapping is `127.0.0.1:14333` to container port `1433`; the named volume `hethongquanlythuvien_qltv_sql_data` is mounted at `/var/opt/mssql`.
- **PASS** — API Development configuration resolves `ConnectionStrings:LibraryDb` from User Secrets. The resolved target is database `QuanLyThuVienDB` at `127.0.0.1,14333`. Credentials are deliberately omitted from this report.
- **PASS** — A .NET SQL client connected to that target and confirmed the database exists. The API started on `http://localhost:5058` and queried SQL Server successfully.
- **PASS** — The latest full backup, `/var/opt/mssql/data/QuanLyThuVienDB_PreModule6_20261025.bak`, was created 2026-10-25 02:30 and passed `RESTORE VERIFYONLY` before either seed execution.
- **PASS** — `DBCC CHECKCONSTRAINTS` returned no violations before seeding. The live schema contains 14 application tables and their foreign keys; no database migration or schema change was run.
- **NOT RUN** — SQL container restart/persistence test. The persistent named volume mapping is confirmed from Docker configuration, but the container was intentionally not restarted during this audit.

### SSMS connection

This backend uses the SQL Server in Docker. Connect SSMS to `127.0.0.1,14333`, choose SQL Server Authentication, enable Trust Server Certificate, and select `QuanLyThuVienDB`. Read the `sa` password locally from the project's `.env`; it is not included here. The earlier SSMS instance was not directly inspected, so a connection to a separate Windows/SQLEXPRESS instance is a likely explanation for differing data, but is not confirmed.

Use this read-only query in SSMS to verify which server/database the query window is using:

```sql
SELECT @@SERVERNAME AS ServerName,
       DB_NAME() AS DatabaseName,
       SERVERPROPERTY('MachineName') AS MachineName,
       SERVERPROPERTY('InstanceName') AS InstanceName;
```

## B. Demo seeder

- Added an explicit, opt-in seeder at `database/seeds/QuanLyThuVien.DemoSeeder.csproj`; the API does not seed automatically.
- Guardrails restrict it to Development, server `127.0.0.1,14333`, and database `QuanLyThuVienDB`; it refuses to write unless a full backup no older than seven days passes `RESTORE VERIFYONLY`.
- The seeder uses the existing Application services for catalog, books, readers, employees, and loan/return/payment workflows. It reuses the configured fine rate (`4.00` per day), creates no login accounts, prints no secrets, and does not delete or overwrite pre-existing rows.
- Seed rows use fictional names/contact data and visibly marked `DEMO -` books. Running twice is idempotent.
- First successful seed added 5 categories, 8 authors, 16 books, 12 readers, 4 employees, 12 loans, 14 loan details, 5 return events, and one payment. A follow-up pass completed an outstanding remainder left by an earlier partial demo payment. A subsequent idempotency run added zero rows.

| Table | Before first seed | Current |
| --- | ---: | ---: |
| TheLoai | 5 | 10 |
| TacGia | 6 | 14 |
| Sach | 3 | 19 |
| Sach_TacGia | 4 | 36 |
| DocGia | 1 | 13 |
| NhanVien | 2 | 6 |
| NhanVien_TrangThaiLichSu | 4 | 4 |
| TaiKhoan | 2 | 2 |
| TaiKhoan_Session | 16 | 16 |
| PhieuMuon | 1 | 13 |
| ChiTiet_PhieuMuon | 1 | 15 |
| ChiTietTraSach | 1 | 6 |
| ThuTienPhat | 0 | 3 |
| CauHinhTienPhat | 1 | 1 |

The existing fine-rate row and existing accounts were left unchanged. Existing hidden catalog items were not restored. Current active API counts are lower than table totals because soft-deleted categories/authors are excluded.

## C. API and build checks

- **PASS** — API process launched using the `http` profile on `http://localhost:5058`.
- **PASS** — `GET /api/dev/ping`: 200.
- **PASS** — `GET /api/sach`: 200, 19 active books.
- **PASS** — `GET /api/theloai`: 200, 9 active categories.
- **PASS** — `GET /api/tacgia`: 200, 12 active authors.
- **PASS** — `dotnet build QuanLyThuVien.slnx --no-restore -m:1 -v:q`: 0 warnings, 0 errors.
- **PASS with warning** — Demo seeder build succeeded. NuGet emitted `NU1900` because the sandbox could not reach `https://api.nuget.org/v3/index.json` for vulnerability metadata; there were no compile errors.
- **NOT RUN** — Authenticated reader, employee, loan, dashboard, and report requests. Their endpoints challenged unauthenticated requests with 401. No bootstrap/test credentials are configured in User Secrets, and authentication was not weakened for this audit.
- **NOT RUN** — WinForms GUI interaction and SQL Server restart persistence. API integration reads are confirmed, but no GUI test is claimed.

## D. Security and limitations

- Existing administrator and librarian accounts were preserved. No demo credentials were created or reported.
- Administrative endpoint authorization remains enabled. Provision test credentials through the project's intended account administration/bootstrap process before testing protected endpoints.
- The seeder is for local Development/demo use only. It should not be run against production.
- SSMS's prior target could not be established without inspecting the user's live SSMS session; verify it with the query above.

## E. Files added

- `database/seeds/Program.cs`
- `database/seeds/QuanLyThuVien.DemoSeeder.csproj`
- `database/seeds/README.md`
- `docs/DATABASE_CONNECTION_AND_SEED_REPORT.md`

