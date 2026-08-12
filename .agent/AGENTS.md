# 🤖 FASTURL.API MULTI-AGENT & CODE QUALITY PROTOCOL (v3.0)

Bộ quy định này quản lý việc lập trình, thiết kế kiến trúc và tự động kiểm thử mã nguồn dự án `FastUrl.API`.

---

## 👥 1. CƠ CẤU ĐỘI NGŨ MULTI-AGENT & VAI TRÒ
* **`sa_agent`**: Software Architect — Bảo vệ cấu trúc Clean Architecture & System Design.
* **`be_dev_agent`**: Backend Developer — Lập trình C# .NET 8, EF Core, Redis, PostgreSQL.
* **`qa_tester`**: QA / Tester — Kiểm thử Unit Tests, xUnit, Architecture Tests.
* **`sec_agent`**: Security Specialist — Rà soát lỗ hổng SAST (SQLi, XSS, Log Injection).

---

## 🔒 2. BỘ QUY TẮC PHÁT TRIỂN & BẢO MẬT (RULES 7.0 - 7.5)

### Rule 7.0: Local Inspection First
1. **Khởi Chạy Local Inspection**:
   - AI Agent **bắt buộc phải chạy `dotnet build` tại local** trước khi trình bày code cho User.
   - Quá trình build sẽ tự động kích hoạt `SonarAnalyzer.CSharp`, `SecurityCodeScan` và `Roslyn Analyzers`.

2. **Chỉ Push / Handoff Khi Local Clean 100%**:
   - AI tuyệt đối không commit hoặc push code dính lỗi biên dịch hay lỗi bảo mật chưa được xử lý.

### Rule 7.1: Tiered Warning Auto-Fix Protocol (Phân Loại Tự Động Sửa Warnings)
* **🟢 Nhóm A (Safe Auto-Fix - Tự động sửa 100%)**:
  - **Bao gồm**: Các cảnh báo Performance, Code Style, Formatting, Unused Variables (e.g., `S1481`, `CA1848`, `CA1805`, `S3267`).
  - **Quy tắc**: AI Agent **tự động sửa 100%** và chạy lại `dotnet test`. Nếu 100% Unit Tests pass -> Chấp nhận cho qua!

* **🟡 Nhóm B (Architectural Review - Phân tích rủi ro & đề xuất)**:
  - **Bao gồm**: Các cảnh báo đụng chạm API Contract, Database Schema, EF Core Entity Types, Exception Hierarchy (e.g., `CA1056`, `CA1054`, `CA1032`).
  - **Quy tắc**: AI Agent phải đánh giá rủi ro: Nếu việc sửa cảnh báo làm vỡ API Contract hoặc EF Core Migration, AI sẽ giữ nguyên kiểu dữ liệu và dùng `[SuppressMessage]` kèm lời giải thích rõ ràng cho User.

### Rule 7.2: Local AI Code Review Protocol
- Trước khi commit/push, AI Agent (vai trò `qa_tester` / `sec_agent`) thực hiện bước Local AI Review:
  1. Đọc `git diff` các file vừa thay đổi tại local.
  2. Kiểm tra tuân thủ Clean Architecture, Security (SQLi, Log Injection), và Naming Rules.
  3. Nếu phát hiện vi phạm, AI tự động fix hoặc cảnh báo cho User ngay tại Local Terminal.

### Rule 7.3: Fast-Path for Documentation & Non-Code Edits (Bỏ Qua Inspection Cho File Phi-Code)
- **Điều kiện**: Khi commit CHỈ chứa các thay đổi thuộc loại:
  - File Markdown (`*.md`), Tài liệu (`docs/**`), File cấu hình Agent (`.agent/**`), tệp hình ảnh (`*.png`, `*.jpg`), hoặc `.gitignore`.
- **Hành vi**:
  - ❌ AI Agent **bỏ qua không chạy** `dotnet build` & `dotnet test`.
  - ❌ AI Agent **bỏ qua không gọi** AI Reviewer nặng.
  - ✅ Cho phép Commit & Push trực tiếp qua **Fast-Path** để tiết kiệm thời gian và tài nguyên!

### Rule 7.4: Conventional Commits Standard (Chuẩn Format Commit Message)
- Tất cả commit messages phải tuân theo định dạng: `<type>(<scope>): <short description>`.
- Các tiền tố hợp lệ:
  - `feat`: Tính năng mới (Feature).
  - `fix`: Sửa lỗi bug (Bug fix).
  - `docs`: Sửa tài liệu markdown hoặc comment.
  - `refactor`: Refactor code không đổi tính năng.
  - `test`: Thêm/sửa unit tests.
  - `chore`: Thay đổi cấu hình build, CI/CD, dependencies.

### Rule 7.5: Why-Only Code Comment Standard (Chuẩn Viết Comment)
- **Public API Controllers & Interfaces**: Bắt buộc có XML Doc Comments (`/// <summary>`) để Swagger UI tự sinh tài liệu.
- **Internal Method Body**: Cấm comment diễn giải "WHAT" (code làm gì). **Chỉ comment giải thích "WHY" (Lý do thiết kế/đánh đổi kỹ thuật ngầm)**.

---

## 🛠️ 3. QUY TRÌNH "LOCAL BUILD FIRST"
- **Step 1**: Write/Edit C# Code.
- **Step 2**: Run `dotnet build -c Release` (Pass 0 Errors).
- **Step 3**: Run `dotnet test -c Release` (Pass 100% Unit Tests).
- **Step 4**: Commit & Push to GitHub.

---

## 🤖 4. QUY TRÌNH AI CODE REVIEW TRÊN PULL REQUEST (AI REVIEWER WORKFLOW)

Mọi Pull Request đẩy lên GitHub đều trải qua luồng thẩm định tự động của **AI Code Reviewer (CodeRabbit AI / Antigravity Agent)**:

1. **Trigger Tự Động**:
   - Mỗi khi PR được tạo từ `feature/*` hoặc `fix/*` vào `main`, bot AI Code Reviewer tự động đọc diff mã nguồn.
2. **Tiêu Chí Thẩm Định Của AI Reviewer**:
   - Tóm tắt PR (PR Summary & Walkthrough).
   - Kiểm tra tuân thủ Rules 7.0 - 7.5 (Sonar, Roslyn & Security warnings).
   - Đánh giá kiến trúc Clean Architecture & tối ưu hiệu năng C#.
   - Đưa ra nhận xét trực tiếp trên từng dòng code (Line-by-line Actionable Comments).
3. **Điều Kiện Merge (Quality Gate Approval)**:
   - PR chỉ được merge khi AI Code Reviewer không tìm thấy lỗi Critical/High Security và toàn bộ Required Status Checks (CI + CodeQL) báo XANH 🟢!
