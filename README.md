# Study .NET - Mastering ASP.NET Core 10
**Web Applications Made Easy with the Biggest Update Yet**  
> To my family, whose love and encouragement have always been the strongest foundation I could build on."_

----------------------------------------------------------------------------------------------------------

## Tổng kết 4 buổi học — The Standard được sống trọn vẹn

| Ngày      | Chương  | Nội dung chính           | Thành tựu |
|-----------|---------|--------------------------|---------|
| **Day 1** | **Ch3** | **First Book API**       | - The Standard: `Broker → Service → Controller`<br>- .NET 10 RC2 + VS 2026 Preview<br>- Swagger + XML Docs<br>- `POST /api/books` → `Created()` |
| **Day 2** | **Ch4** | **Pipeline Mastery**     | - Custom Middleware: `RequestTimingMiddleware`<br>- Built-in: `CORS`, `Exception Handler`<br>- XML Documentation enabled<br>- `UseRequestTiming()` log thời gian |
| **Day 3** | **Ch6** | **Dependency Injection** | - Unit Test: `BookServiceTests` (xUnit)<br>- Integration Test: `WebApplicationFactory`<br>- .NET 10 DI Diagnostics<br>- `Singleton`, `Scoped`, `Transient` — hiểu như hơi thở |
| **Day 4** | **Ch7** | **Routing & Endpoints**  | - **Full CRUD Async Controller**<br>- **Attribute Routing**: `{id:int}`<br>- **Custom Route Constraint**: `year/{year:year}` → `1800` → 404<br>- **Minimal API**: `/hello`, `/echo`<br>- **`GetBooksByYear` full The Standard**<br>- **Integration Tests Passed 100%** |

---

## Kiến trúc The Standard — Đã hoàn chỉnh 100%

┌─────────────────┐
│   Controller    │ ← Nhận request, trả response
├─────────────────┤
│     Service     │ ← Logic nghiệp vụ
├─────────────────┤
│     Broker      │ ← Truy cập dữ liệu (InMemory)
└─────────────────┘

**Tất cả đều DI, testable, async khi cần, sync khi nhanh.**

------------------------------------------------------------

## Công cụ & Công nghệ

| Công cụ        | Phiên bản |
|----------------|----------|
| .NET           | **10.0 RC2** (pioneer!) |
| IDE            | Visual Studio 2026 Preview |
| Test           | xUnit + `Microsoft.AspNetCore.Mvc.Testing` |
| API Docs       | Swagger + XML Comments |
| Source Control | Git + GitHub |

------------------------------------------------------------
