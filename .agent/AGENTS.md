# 🤖 FASTURL.API MULTI-AGENT & CODE QUALITY PROTOCOL (v3.0)

Bộ quy định này quản lý việc lập trình, thiết kế kiến trúc và tự động kiểm thử mã nguồn dự án `FastUrl.API`.

---

## 👥 1. CƠ CẤU ĐỘI NGŨ MULTI-AGENT & VAI TRÒ
* **`sa_agent`**: Software Architect — Bảo vệ cấu trúc Clean Architecture & System Design.
* **`be_dev_agent`**: Backend Developer — Lập trình C# .NET 8, EF Core, Redis, PostgreSQL.
* **`qa_tester`**: QA / Tester — Kiểm thử Unit Tests, xUnit, Architecture Tests.
* **`sec_agent`**: Security Specialist — Rà soát lỗ hổng SAST (SQLi, XSS, Log Injection).

---

## 🔒 2. RULE 7.0 & 7.1 — AI AGENT SELF-CORRECTION & TIERED AUTO-FIX PROTOCOL (BẮT BUỘC)

Mỗi khi AI Agent / Subagent sinh mã nguồn C# hoặc chỉnh sửa file cho dự án `FastUrl.API`:

### Rule 7.0: Local Inspection First
1. **Khởi Chạy Local Inspection**:
   - AI Agent **bắt buộc phải chạy `dotnet build` tại local** trước khi trình bày code cho User.
   - Quá trình build sẽ tự động kích hoạt `SonarAnalyzer.CSharp`, `SecurityCodeScan` và `Roslyn Analyzers`.

2. **Chỉ Push / Handoff Khi Local Clean 100%**:
   - AI tuyệt đối không commit hoặc push code dính lỗi biên dịch hay lỗi bảo mật chưa được xử lý.

### Rule 7.1: Tiered Warning Auto-Fix Protocol (Phân Loại Tự Động Sửa Warnings)
Khi `dotnet build` trả về các cảnh báo (Warnings), AI Agent phân loại và xử lý theo 2 nhóm:

* **🟢 Nhóm A (Safe Auto-Fix - Tự động sửa 100%)**:
  - **Bao gồm**: Các cảnh báo Performance, Code Style, Formatting, Unused Variables (e.g., `S1481`, `CA1848`, `CA1805`, `S3267`).
  - **Quy tắc**: AI Agent **tự động sửa 100%** và chạy lại `dotnet test`. Nếu 100% Unit Tests pass -> Chấp nhận cho qua!

* **🟡 Nhóm B (Architectural Review - Phân tích rủi ro & đề xuất)**:
  - **Bao gồm**: Các cảnh báo đụng chạm API Contract, Database Schema, EF Core Entity Types, Exception Hierarchy (e.g., `CA1056`, `CA1054`, `CA1032`).
  - **Quy tắc**: AI Agent phải đánh giá rủi ro: Nếu việc sửa cảnh báo làm vỡ API Contract hoặc EF Core Migration, AI sẽ giữ nguyên kiểu dữ liệu và dùng `[SuppressMessage]` kèm lời giải thích rõ ràng cho User.

---

## 🛠️ 3. QUY TRÌNH "LOCAL BUILD FIRST"
- **Step 1**: Write/Edit C# Code.
- **Step 2**: Run `dotnet build -c Release` (Pass 0 Errors).
- **Step 3**: Run `dotnet test -c Release` (Pass 100% Unit Tests).
- **Step 4**: Commit & Push to GitHub.
