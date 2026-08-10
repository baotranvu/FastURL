# 🤖 FASTURL.API MULTI-AGENT & CODE QUALITY PROTOCOL (v3.0)

Bộ quy định này quản lý việc lập trình, thiết kế kiến trúc và tự động kiểm thử mã nguồn dự án `FastUrl.API`.

---

## 👥 1. CƠ CẤU ĐỘI NGŨ MULTI-AGENT & VAI TRÒ
* **`sa_agent`**: Software Architect — Bảo vệ cấu trúc Clean Architecture & System Design.
* **`be_dev_agent`**: Backend Developer — Lập trình C# .NET 8, EF Core, Redis, PostgreSQL.
* **`qa_tester`**: QA / Tester — Kiểm thử Unit Tests, xUnit, Architecture Tests.
* **`sec_agent`**: Security Specialist — Rà soát lỗ hổng SAST (SQLi, XSS, Log Injection).

---

## 🔒 2. RULE 7.0 — AI AGENT SELF-CORRECTION PROTOCOL (BẮT BUỘC)

Mỗi khi AI Agent / Subagent sinh mã nguồn C# hoặc chỉnh sửa file cho dự án `FastUrl.API`:

1. **Khởi Chạy Local Inspection**:
   - AI Agent **bắt buộc phải chạy `dotnet build` tại local** trước khi trình bày code cho User.
   - Quá trình build sẽ tự động kích hoạt `SonarAnalyzer.CSharp`, `SecurityCodeScan` và `Roslyn Analyzers`.

2. **Quy Trình Tự Sửa Lỗi (Self-Fix Loop)**:
   - Nếu build xuất hiện Warning hoặc Error từ Sonar (S-rules), Security (SCS-rules) hoặc Roslyn (CA-rules):
   - AI Agent **bắt buộc phải tự đọc vị trí dòng lỗi và tự động sửa (Self-Fix)** cho đến khi build đạt 0 Error và 0 Warning nguy hiểm.

3. **Chỉ Push / Handoff Khi Local Clean 100%**:
   - AI tuyệt đối không commit hoặc push code dính lỗi biên dịch hay lỗi bảo mật chưa được xử lý.

---

## 🛠️ 3. QUY TRÌNH "LOCAL BUILD FIRST"
- **Step 1**: Write/Edit C# Code.
- **Step 2**: Run `dotnet build -c Release` (Pass 0 Errors).
- **Step 3**: Run `dotnet test -c Release` (Pass 100% Unit Tests).
- **Step 4**: Commit & Push to GitHub.
