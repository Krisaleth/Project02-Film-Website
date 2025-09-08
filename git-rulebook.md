## 1 Git & Nhánh

- MUST: Không push trực tiếp lên main. Tất cả thay đổi qua PR.

- MUST: Đặt tên nhánh theo kiểu:
feature/<module>-<short-desc> · fix/<issue-id>-<short-desc> · chore/<task>

- MUST: PR chỉ merge khi CI xanh + review ≥ 1. Ưu tiên Squash & merge.

- SHOULD: Rebase nhánh của bạn lên origin/main trước khi push (giảm conflict, lịch sử gọn).

- MUST: Commit theo Conventional Commit: feat:, fix:, refactor:, docs:, test:, chore:.

## 2 Chất lượng mã & Build

- MUST: Bật nullable & warnings-as-errors trên toàn solution.

- MUST: Chạy dotnet build + dotnet test trước khi push (pre-push hook/CI).

- SHOULD: Format code chuẩn .editorconfig; dùng dotnet format trong CI.

- SHOULD: Thêm Roslyn analyzers (Microsoft + StyleCop) và sửa các warning.

- MUST: Không dùng async void (trừ event handlers); method async phải có hậu tố Async.

## 3 Kiến trúc & Tổ chức Solution

- MUST: Theo Clean-ish layering:
Project.Api (ASP.NET) · Project.Application (use cases) · Project.Domain (entities) · Project.Infrastructure (EF Core, external services) · Project.Tests (unit/integration).

- MUST: DI qua IServiceCollection (không new trực tiếp trong controller).

- MUST: Tách cấu hình qua IOptions<T>; không đọc trực tiếp Environment.GetEnvironmentVariable trong business code.

- SHOULD: Controller mỏng, logic nằm ở Application/Domain.

- MUST: DTO/ViewModel không lộ entity nội bộ. Dùng AutoMapper (tuỳ chọn).

## 4 API & Middleware

- MUST: Bật Swagger/OpenAPI ở Dev; tắt/đặt bảo vệ ở Prod.

- MUST: Global exception handling (Middleware) → trả JSON lỗi chuẩn.

- MUST: Validation bằng DataAnnotations/FluentValidation; trả 400 với message rõ ràng.

- SHOULD: Chuẩn REST: danh từ số nhiều, mã trạng thái đúng, không trả 200 cho lỗi.

- MUST: Logging qua ILogger<T>; cấm Console.WriteLine trong code prod.

- MUST: Chuẩn hóa response: có traceId (từ HttpContext.TraceIdentifier) khi lỗi.

## 5 Bảo mật

- MUST: Không commit secrets. Dùng Secret Manager (dev) / KeyVault/Env vars (prod).

- MUST: Bật HTTPS redirection; HSTS ở Prod.

- SHOULD: Thêm security headers (Content-Security-Policy, X-Content-Type-Options, X-Frame-Options).

- MUST: JWT cookie/http-only hoặc Bearer token; tuyệt đối không tự chế hash password (dùng ASP.NET Identity hoặc PBKDF2 chuẩn).

- SHOULD: Rate limit cho endpoints nhạy cảm (login, file upload).

## 6 EF Core & Database

- MUST: Migration đi kèm thay đổi model. Không sửa DB thủ công ngoài migration.

- MUST: Không Database.EnsureCreated trong prod; dùng dotnet ef database update.

- SHOULD: Seed data chỉ cho dev/test; prod seed qua script được duyệt.

- MUST: Transaction khi thay đổi nhiều bảng liên quan; tránh SaveChanges nhiều lần không cần thiết.

- SHOULD: Dùng AsNoTracking() cho query chỉ đọc; bật cancellationToken cho DB calls.

- Quy tắc đặt migration
```bash
dotnet ef migrations add <Area>_<WhatChanged>   # ví dụ: User_AddRefreshToken
```
## 7 Testing

- MUST: Unit test cho business-critical logic; integration test tối thiểu cho API chính.

- SHOULD: Mục tiêu coverage ≥ 60% cho Application/Domain (không lấy số tuyệt đối, ưu tiên risk-based).

- MUST: Test phải deterministic (không phụ thuộc thời gian/môi trường mạng). Dùng fixture/mocks.

- MAY: Contract test/Smoke test sau deploy.

## 8 Logging, Observability

- MUST: Log theo cấp độ: Information cho luồng bình thường; Warning cho tình huống bất thường; Error khi thất bại; không log PII/secret.

- SHOULD: Correlation ID xuyên suốt (propagate traceparent nếu có).

- MAY: OpenTelemetry + exporter (OTLP) nếu hạ tầng cho phép.

## 9 Cấu hình môi trường

- MUST: Dùng profile appsettings.{Environment}.json; giá trị secret lấy từ env/KeyVault.

- MUST: Tách BuildConfig (Debug/Release) và ASPNETCORE_ENVIRONMENT (Development/Staging/Production).

- SHOULD: Feature flags cho tính năng thử nghiệm (Microsoft.FeatureManagement).

## 10 CI tối thiểu (GitHub Actions mẫu)

- MUST: PR → chạy build, format, test. Merge bị chặn nếu fail.

.github/workflows/ci.yml:
```bash
name: ci
on:
  pull_request:
    branches: [ main ]
jobs:
  build_test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet format --verify-no-changes
      - run: dotnet test --no-build -c Release --collect:"XPlat Code Coverage"
```
## 11 Hook trước khi push (khuyến nghị)

- SHOULD: Chặn push khi build/test fail.

.git/hooks/pre-push (Linux/macOS, nhớ chmod +x):
```bash
#!/usr/bin/env bash
dotnet build -c Release || exit 1
dotnet test -c Release || exit 1
```

PowerShell (Windows – .git/hooks/pre-push):
```bash
dotnet build -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet test -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }
```
## 12 Tài liệu & Versioning

- MUST: Mỗi PR cập nhật CHANGELOG.md theo SemVer (patch/minor/major).

- SHOULD: README phải có: kiến trúc, cách chạy dev, migrate DB, biến môi trường, demo API.

- MAY: Tự động gán version app từ git tag trong CI.