using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Infrastructure;
using QuanLyThuVien.Infrastructure.Persistence;

const string ExpectedDatabase = "QuanLyThuVienDB";
const string ExpectedServer = "127.0.0.1,14333";
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
if (!environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Demo seeder chỉ chạy khi DOTNET_ENVIRONMENT hoặc ASPNETCORE_ENVIRONMENT là Development.");

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .AddEnvironmentVariables()
    .Build();
var connectionString = configuration.GetConnectionString("LibraryDb")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:LibraryDb trong User Secrets hoặc biến môi trường.");
var connection = new SqlConnectionStringBuilder(connectionString);
if (!connection.DataSource.Equals(ExpectedServer, StringComparison.OrdinalIgnoreCase)
    || !connection.InitialCatalog.Equals(ExpectedDatabase, StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException($"Seeder từ chối chạy ngoài {ExpectedDatabase} trên {ExpectedServer}. Kiểm tra lại đích kết nối.");

await VerifyRecentBackupAsync(connectionString);

var services = new ServiceCollection();
services.AddInfrastructure(connectionString);
services.AddScoped<ISachService, SachService>();
services.AddScoped<ITheLoaiService, TheLoaiService>();
services.AddScoped<ITacGiaService, TacGiaService>();
services.AddScoped<IDocGiaService, DocGiaService>();
services.AddScoped<INhanVienService, NhanVienService>();
services.AddScoped<IMuonTraService, MuonTraService>();
await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
if (!await db.Database.CanConnectAsync()) throw new InvalidOperationException("Không thể kết nối tới database mục tiêu.");

var fineRate = (await scope.ServiceProvider.GetRequiredService<IMuonTraService>().GetCauHinhTienPhatAsync(CancellationToken.None)).MucPhatMoiNgay;
if (fineRate is null or <= 0) throw new InvalidOperationException("Database chưa có mức phạt/ngày hợp lệ. Seeder không tự thay đổi cấu hình này.");

var countsBefore = await ReadCountsAsync(db);
var categoriesCreated = 0;
var authorsCreated = 0;
var booksCreated = 0;
var readersCreated = 0;
var employeesCreated = 0;

var categoryService = scope.ServiceProvider.GetRequiredService<ITheLoaiService>();
var authorService = scope.ServiceProvider.GetRequiredService<ITacGiaService>();
var bookService = scope.ServiceProvider.GetRequiredService<ISachService>();
var readerService = scope.ServiceProvider.GetRequiredService<IDocGiaService>();
var employeeService = scope.ServiceProvider.GetRequiredService<INhanVienService>();
var borrowingService = scope.ServiceProvider.GetRequiredService<IMuonTraService>();

var categoryNames = new[] { "DEMO - Công nghệ thông tin", "DEMO - Khoa học", "DEMO - Văn học", "DEMO - Kinh tế", "DEMO - Kỹ năng sống" };
var categoryIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var authorIds = new List<int>();
var bookIds = new List<int>();
var readerIds = new List<int>();
var staffNames = new List<string>();

foreach (var name in categoryNames)
{
    var existing = (await categoryService.SearchAsync(name, includeDeleted: true, onlyDeleted: false, CancellationToken.None))
        .FirstOrDefault(x => x.TenTheLoai.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (existing?.IsDeleted == true) throw new InvalidOperationException($"Danh mục demo '{name}' đang bị ẩn; seeder không tự khôi phục dữ liệu.");
    if (existing is not null) categoryIds[name] = existing.MaTheLoai;
    else
    {
        var created = await categoryService.CreateAsync(new TheLoaiWriteDto { TenTheLoai = name, MoTa = "DEMO-SEED: Danh mục dữ liệu giả lập cho kiểm thử đồ án." }, CancellationToken.None);
        categoryIds[name] = created.MaTheLoai;
        categoriesCreated++;
    }
}

for (var i = 1; i <= 8; i++)
{
    var name = $"DEMO - Tác giả giả lập {i:00}";
    var existing = (await authorService.SearchAsync(name, includeDeleted: true, onlyDeleted: false, CancellationToken.None))
        .FirstOrDefault(x => x.TenTacGia.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (existing?.IsDeleted == true) throw new InvalidOperationException($"Tác giả demo '{name}' đang bị ẩn; seeder không tự khôi phục dữ liệu.");
    if (existing is not null) authorIds.Add(existing.MaTacGia);
    else
    {
        var created = await authorService.CreateAsync(new TacGiaWriteDto
        {
            TenTacGia = name, QuocTich = "Việt Nam", GioiThieu = "DEMO-SEED: Nhân vật hư cấu, không đại diện cá nhân có thật."
        }, CancellationToken.None);
        authorIds.Add(created.MaTacGia);
        authorsCreated++;
    }
}

var bookSeeds = CreateBookSeeds(categoryNames);
for (var i = 0; i < bookSeeds.Count; i++)
{
    var seed = bookSeeds[i];
    var isbn = CreateDemoIsbn(i + 1);
    var existing = await bookService.SearchAsync(isbn, null, includeDeleted: true, onlyDeleted: false, CancellationToken.None);
    var found = existing.FirstOrDefault(x => x.ISBN == isbn);
    if (found?.IsDeleted == true) throw new InvalidOperationException($"Sách demo ISBN {isbn} đang bị ẩn; seeder không tự khôi phục.");
    if (found is not null) bookIds.Add(found.MaSach);
    else
    {
        var created = await bookService.CreateAsync(new SachWriteDto
        {
            ISBN = isbn,
            TenSach = seed.Title,
            MaTheLoai = categoryIds[seed.Category],
            NamXB = 2018 + (i % 8),
            NhaXB = "Nhà xuất bản Demo",
            SoLuong = 5 + (i % 4),
            MoTa = "DEMO-SEED: Tên sách và ISBN giả lập để kiểm thử. ISBN chỉ có checksum hợp lệ, không khẳng định đã được cấp hoặc phát hành.",
            MaTacGia = seed.Authors.Select(index => authorIds[index]).Distinct().ToList()
        }, CancellationToken.None);
        bookIds.Add(created.MaSach);
        booksCreated++;
    }
}

for (var i = 1; i <= 12; i++)
{
    var name = $"DEMO - Độc giả {i:00}";
    var existing = (await readerService.SearchAsync(name, null, CancellationToken.None))
        .FirstOrDefault(x => x.HoTen.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (existing is not null) readerIds.Add(existing.MaDocGia);
    else
    {
        var created = await readerService.CreateAsync(new CreateDocGiaDto
        {
            HoTen = name,
            NgaySinh = new DateTime(1988 + (i % 15), 1 + (i % 12), 1 + (i % 27)),
            GioiTinh = i % 2 == 0 ? "Nữ" : "Nam",
            SoDienThoai = $"0000000{i:000}",
            Email = $"demo.reader{i:00}@example.invalid",
            DiaChi = "Địa chỉ giả lập - dữ liệu demo",
            TrangThai = "Hoạt động"
        }, CancellationToken.None);
        readerIds.Add(created.MaDocGia);
        readersCreated++;
    }
}

for (var i = 1; i <= 4; i++)
{
    var name = $"DEMO - Nhân viên {i:00}";
    var existing = (await employeeService.SearchAsync(name, null, null, CancellationToken.None))
        .FirstOrDefault(x => x.HoTen.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (existing is null)
    {
        await employeeService.CreateAsync(new CreateNhanVienDto
        {
            HoTen = name, ChucVu = i == 1 ? "Thủ thư" : "Nhân viên thư viện",
            Email = $"demo.employee{i:00}@example.invalid", SoDienThoai = null
        }, CancellationToken.None);
        employeesCreated++;
    }
    staffNames.Add(name);
}

var today = DateTime.Today;
var scenarios = new[]
{
    new LoanScenario(0, today, today.AddDays(10), [0], [1], []),
    new LoanScenario(1, today, today.AddDays(1), [1], [1], []),
    new LoanScenario(2, today, today, [2], [1], []),
    new LoanScenario(3, today.AddDays(-10), today.AddDays(-7), [3], [1], []),
    new LoanScenario(4, today.AddDays(-8), today.AddDays(-3), [4], [1], []),
    new LoanScenario(5, today.AddDays(-20), today.AddDays(-15), [5], [1], [new ReturnScenario(5, 1, today.AddDays(-17), false)]),
    new LoanScenario(6, today.AddDays(-23), today.AddDays(-18), [6], [1], [new ReturnScenario(6, 1, today.AddDays(-14), false)]),
    new LoanScenario(7, today.AddDays(-25), today.AddDays(-20), [7], [1], [new ReturnScenario(7, 1, today.AddDays(-17), true)]),
    new LoanScenario(8, today.AddDays(-28), today.AddDays(-23), [8], [1], [new ReturnScenario(8, 1, today.AddDays(-21), true)]),
    new LoanScenario(9, today, today.AddDays(8), [12, 13], [2, 1], [new ReturnScenario(12, 1, today, false)]),
    new LoanScenario(10, today, today.AddDays(12), [14, 15], [1, 1], []),
    new LoanScenario(11, today, today.AddDays(5), [11], [1], [])
};

var loansCreated = 0;
var loanDetailsCreated = 0;
var returnsCreated = 0;
var paymentsCreated = 0;
foreach (var scenario in scenarios)
{
    var loanKey = StableKey($"loan-{scenario.Index:00}");
    var loanExisted = await db.PhieuMuon.AsNoTracking().AnyAsync(x => x.IdempotencyKey == loanKey);
    var request = new TaoMuonTraDto
    {
        MaDocGia = readerIds[scenario.Index],
        NgayMuon = scenario.Borrowed,
        HanTra = scenario.Due,
        NguoiLap = staffNames[scenario.Index % staffNames.Count],
        IdempotencyKey = loanKey,
        ChiTiet = scenario.BookIndexes.Select((bookIndex, lineIndex) => new TaoChiTietMuonDto
        {
            MaSach = bookIds[bookIndex], SoLuong = scenario.Quantities[lineIndex]
        }).ToList()
    };
    var loan = await borrowingService.CreateAsync(request, CancellationToken.None);
    if (!loanExisted) { loansCreated++; loanDetailsCreated += scenario.BookIndexes.Length; }

    foreach (var returnSeed in scenario.Returns)
    {
        var line = loan.ChiTiet.First(x => x.MaSach == bookIds[returnSeed.BookIndex]);
        var key = StableKey($"return-{scenario.Index:00}-{returnSeed.BookIndex:00}");
        var returnExisted = await db.ChiTietTraSach.AsNoTracking().AnyAsync(x => x.IdempotencyKey == key);
        loan = await borrowingService.TraChiTietAsync(loan.MaPhieu, line.MaChiTiet, new TraChiTietDto
        {
            SoLuongXuLy = returnSeed.Quantity,
            NgayTra = returnSeed.Returned,
            TinhTrangSach = "Nguyên vẹn",
            IdempotencyKey = key
        }, CancellationToken.None) ?? throw new InvalidOperationException("Không tìm thấy chi tiết phiếu demo để trả.");
        if (!returnExisted) returnsCreated++;

        if (returnSeed.PayFine)
        {
            var fine = loan.ChiTiet.First(x => x.MaChiTiet == line.MaChiTiet).LichSuTra
                .First(x => x.SoTienPhatPhaiThu is > 0);
            if (fine.ConNo is > 0)
            {
                var paymentKey = StableKey(scenario.Index == 7 ? "payment-completion-07" : $"payment-{scenario.Index:00}");
                var paymentExisted = await db.ThuTienPhat.AsNoTracking().AnyAsync(x => x.IdempotencyKey == paymentKey);
                await borrowingService.ThuTienPhatAsync(loan.MaPhieu, new ThuTienPhatDto
                {
                    MaLanTra = fine.MaLanTra,
                    SoTien = scenario.Index == 7
                        ? fine.ConNo.Value
                        : decimal.Round(fine.SoTienPhatPhaiThu!.Value / 2m, 2, MidpointRounding.ToZero),
                    NguoiThu = staffNames[scenario.Index % staffNames.Count],
                    IdempotencyKey = paymentKey
                }, CancellationToken.None);
                if (!paymentExisted) paymentsCreated++;
            }
        }
    }
}

var countsAfter = await ReadCountsAsync(db);
Console.WriteLine($"Database: {connection.InitialCatalog} on {connection.DataSource}");
Console.WriteLine($"Full backup verified before seed; fine rate reused: {fineRate:N2} per day.");
Console.WriteLine($"Created/Skipped — categories {categoriesCreated}/{categoryNames.Length - categoriesCreated}, authors {authorsCreated}/8, books {booksCreated}/{bookSeedsCount()}, readers {readersCreated}/12, employees {employeesCreated}/4, loans {loansCreated}/{scenarios.Length}, loan lines {loanDetailsCreated}/(existing loan lines), return events {returnsCreated}/(existing events), fine payments {paymentsCreated}/(existing payments).");
Console.WriteLine("Table rows before -> after:");
foreach (var (table, before) in countsBefore)
    Console.WriteLine($"{table}: {before} -> {countsAfter[table]}");

static List<BookSeed> CreateBookSeeds(string[] categories) =>
[
    new("DEMO - Nhập môn lập trình C#", categories[0], [0, 1]),
    new("DEMO - Cấu trúc dữ liệu thực hành", categories[0], [1, 2]),
    new("DEMO - Phát triển ứng dụng .NET", categories[0], [2, 3]),
    new("DEMO - Cơ sở dữ liệu quan hệ", categories[0], [3, 4]),
    new("DEMO - An toàn thông tin căn bản", categories[0], [4, 5]),
    new("DEMO - Khám phá vũ trụ", categories[1], [5, 6]),
    new("DEMO - Sinh thái học quanh ta", categories[1], [6, 7]),
    new("DEMO - Vật lý qua thí nghiệm", categories[1], [7, 0]),
    new("DEMO - Truyện ngắn bên hiên", categories[2], [0, 2]),
    new("DEMO - Khu vườn mùa hạ", categories[2], [1, 3]),
    new("DEMO - Dòng sông ký ức", categories[2], [2, 4]),
    new("DEMO - Kinh tế học nhập môn", categories[3], [3, 5]),
    new("DEMO - Quản trị dự án nhỏ", categories[3], [4, 6]),
    new("DEMO - Tư duy tài chính cá nhân", categories[3], [5, 7]),
    new("DEMO - Giao tiếp và lắng nghe", categories[4], [6, 0]),
    new("DEMO - Thói quen học tập hiệu quả", categories[4], [7, 1])
];

static string CreateDemoIsbn(int sequence)
{
    var body = "978604" + sequence.ToString("D6");
    var sum = body.Select((c, index) => (c - '0') * (index % 2 == 0 ? 1 : 3)).Sum();
    var check = (10 - sum % 10) % 10;
    return body + check;
}

static Guid StableKey(string name) => new(SHA256.HashData(Encoding.UTF8.GetBytes("qltv-demo-seed-v1:" + name)).AsSpan(0, 16));

static async Task VerifyRecentBackupAsync(string connectionString)
{
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    await using var latest = connection.CreateCommand();
    latest.CommandText = "SELECT TOP(1) bs.backup_finish_date,mf.physical_device_name FROM msdb.dbo.backupset bs JOIN msdb.dbo.backupmediafamily mf ON bs.media_set_id=mf.media_set_id WHERE bs.database_name=DB_NAME() AND bs.type='D' ORDER BY bs.backup_finish_date DESC";
    await using var reader = await latest.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) throw new InvalidOperationException("Không tìm thấy full backup của database; dừng trước khi ghi dữ liệu.");
    var finished = reader.GetDateTime(0);
    var path = reader.GetString(1);
    await reader.CloseAsync();
    if (finished < DateTime.Now.AddDays(-7)) throw new InvalidOperationException("Full backup gần nhất đã quá 7 ngày; hãy tạo và kiểm chứng backup mới trước khi seed.");
    await using var verify = connection.CreateCommand();
    verify.CommandTimeout = 180;
    verify.CommandText = "RESTORE VERIFYONLY FROM DISK=@backupPath";
    verify.Parameters.AddWithValue("@backupPath", path);
    await verify.ExecuteNonQueryAsync();
    Console.WriteLine($"Verified recent full backup ({finished:yyyy-MM-dd HH:mm}); backup path: {path}");
}

static async Task<Dictionary<string, int>> ReadCountsAsync(LibraryDbContext db) => new()
{
    ["TheLoai"] = await db.TheLoai.CountAsync(), ["TacGia"] = await db.TacGia.CountAsync(),
    ["Sach"] = await db.Sach.CountAsync(), ["Sach_TacGia"] = await db.SachTacGia.CountAsync(),
    ["DocGia"] = await db.DocGia.CountAsync(), ["NhanVien"] = await db.NhanVien.CountAsync(),
    ["NhanVien_TrangThaiLichSu"] = await db.NhanVienTrangThaiLichSu.CountAsync(),
    ["TaiKhoan"] = await db.TaiKhoan.CountAsync(), ["TaiKhoan_Session"] = await db.TaiKhoanSession.CountAsync(),
    ["PhieuMuon"] = await db.PhieuMuon.CountAsync(), ["ChiTiet_PhieuMuon"] = await db.ChiTietPhieuMuon.CountAsync(),
    ["ChiTietTraSach"] = await db.ChiTietTraSach.CountAsync(), ["ThuTienPhat"] = await db.ThuTienPhat.CountAsync(),
    ["CauHinhTienPhat"] = await db.CauHinhTienPhat.CountAsync()
};

static int bookSeedsCount() => 16;

sealed record BookSeed(string Title, string Category, int[] Authors);
sealed record ReturnScenario(int BookIndex, int Quantity, DateTime Returned, bool PayFine);
sealed record LoanScenario(int Index, DateTime Borrowed, DateTime Due, int[] BookIndexes, int[] Quantities, ReturnScenario[] Returns);
