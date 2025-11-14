# Study .NET - Mastering ASP.NET Core 10

------------------------------------------------

## Tổng kết 4 buổi học — Kiến trúc The Standard

| Ngày | Chương | Nội dung chính | Thành tựu |
|------|--------|----------------|----------|
| **Day 1** | **Ch3** | **First Book API** | - The Standard: `Broker → Service → Controller`<br>- .NET 10 RC2 + VS 2026 Preview<br>- Swagger + XML Comments<br>- `POST /api/books` → `201 Created` |
| **Day 2** | **Ch4** | **Pipeline** | - Custom Middleware: `RequestTimingMiddleware`<br>- Built-in: `UseCors`, `UseExceptionHandler`<br>- `UseRequestTiming()` logs request duration |
| **Day 3** | **Ch6** | **Dependency Injection** | - Unit Test: `BookServiceTests` (xUnit)<br>- Integration Test: `WebApplicationFactory<Program>`<br>- .NET 10 DI Diagnostics<br>- `AddSingleton`, `AddScoped`, `AddTransient` |
| **Day 4** | **Ch7** | **Routing & Endpoints** | - Full CRUD Async Controller<br>- Attribute Routing: `{id:int}`<br>- Custom Route Constraint: `year/{year:year}` → `1800` → `404`<br>- Minimal API: `MapGet("/hello")`<br>- `GetBooksByYear` full The Standard<br>- Integration Tests: `GET /api/books/1`, `GET /api/books/year/1800` |

---

## Kiến trúc The Standard — 100% hoàn chỉnh
<img width="453" height="234" alt="Screenshot_1" src="https://github.com/user-attachments/assets/108ff0ee-33fa-432f-b7c0-620ffbe30d99" />

- Tất cả đều **DI**, **testable**, **async khi cần**, **sync khi nhanh**.

---

## Công cụ & Công nghệ

| Công cụ | Phiên bản |
|--------|----------|
| .NET | **10.0 RC2** |
| IDE | Visual Studio 2026 Preview |
| Testing | xUnit + `Microsoft.AspNetCore.Mvc.Testing` |
| API Documentation | Swagger + XML Comments |
| Source Control | Git + GitHub |

---

### **DAY 1 — CHƯƠNG 3: FIRST BOOK API**

| Câu hỏi | Trả lời |
|--------|--------|
| **Bạn đã học gì ở Day 1?** | Xây API đầu tiên với The Standard |
| **The Standard gồm mấy lớp?** | 3 lớp: `Broker`, `Service`, `Controller` |
| **Swagger dùng để làm gì?** | Tạo tài liệu API tự động |

| Câu hỏi | Trả lời |
|--------|--------|
| **Tại sao không `new BookService()` trong Controller?** | Dùng DI để inject `IBookService` → dễ test |
| **XML Comments có ích gì trong Swagger?** | Hiển thị mô tả API, tham số, response |

| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao để `POST /api/books` trả về `201 Created` với URL chi tiết?** | Dùng `CreatedAtAction(nameof(Get), new { id = book.Id }, book)` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Nếu `IBookService` có 2 implementation (`InMemory` và `SqlServer`), bạn chọn cái nào khi start app?** | Dùng `appsettings.json` + `IConfiguration` → `services.AddScoped<IBookService>(sp => config["Storage"] == "sql" ? new SqlBookService(...) : new InMemoryBookService(...))` |
| **Làm sao để `POST /api/books` trả về `Location: /api/books/1` ngay cả khi `Get` chưa tồn tại?** | Dùng `CreatedAtRoute("GetBook", new { id = book.Id }, book)` |

---

### **DAY 2 — CHƯƠNG 4: PIPELINE**

| Câu hỏi | Trả lời |
|--------|--------|
| **Middleware là gì?** | Hàm xử lý request/response theo thứ tự |
| **Bạn đã viết middleware nào?** | `RequestTimingMiddleware` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao log thời gian xử lý request?** | Dùng `Stopwatch` trong middleware |
| **Thứ tự `UseCors()` và `MapControllers()`?** | `UseCors()` trước `MapControllers()` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Nếu `UseExceptionHandler()` đặt sau `MapControllers()`, điều gì xảy ra?** | Lỗi không được bắt → trả về HTML 500 |

| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao để `RequestTimingMiddleware` chỉ chạy với `/api/books` mà không chạy với `/health`?** | Dùng `if (!context.Request.Path.StartsWithSegments("/health")) { await _next(context); }` |
| **Nếu có 2 middleware cùng log, làm sao tránh log 2 lần?** | Dùng `context.Items["Logged"] = true` để đánh dấu |

---

### **DAY 3 — CHƯƠNG 6: DEPENDENCY INJECTION**

| Câu hỏi | Trả lời |
|--------|--------|
| **DI là gì?** | Inject dependency thay vì `new` |
| **3 lifetime của DI là gì?** | `Singleton`, `Scoped`, `Transient` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Unit Test cần DI không?** | Không, có thể `new BookService(broker)` |
| **Integration Test dùng gì?** | `WebApplicationFactory<Program>` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao phát hiện lỗi DI (circular dependency) ngay khi start?** | Dùng `.NET 10 DI Diagnostics` → `builder.Services.AddDiagnostics()` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Nếu `BookService` cần `ILogger<BookService>` và `IStorageBroker`, nhưng `InMemoryStorageBroker` cũng cần `ILogger`, có circular dependency không?** | Không, vì `ILogger` là built-in, không gây vòng |
| **Làm sao mock `IStorageBroker` trong `WebApplicationFactory` để test với dữ liệu giả?** | Dùng `ConfigureTestServices(services => services.AddScoped<IStorageBroker, FakeStorageBroker>())` |

---

### **DAY 4 — CHƯƠNG 7: ROUTING & ENDPOINTS**

| Câu hỏi | Trả lời |
|--------|--------|
| **Attribute Routing là gì?** | `[HttpGet("{id:int}")]` |
| **Minimal API viết thế nào?** | `app.MapGet("/hello", () => "Hi")` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Custom Route Constraint dùng khi nào?** | Khi cần kiểm tra tham số route |
| **Bạn đã viết constraint nào?** | `year/{year:year}` → `1800` → `404` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao test `GET /api/books/year/1800` trả về `404`?** | Dùng `WebApplicationFactory` + `client.GetAsync()` + `Assert.Equal(404, response.StatusCode)` |

| Câu hỏi | Trả lời |
|--------|--------|
| **Nếu `GetById(int id)` và `GetBooksByYear(int year)` đều dùng `{id:int}`, làm sao phân biệt?** | Dùng route khác: `[HttpGet("{id:int}")]`, `[HttpGet("year/{year:year}")]` |
| **Làm sao để `GetBooksByYear` trả về `200` với danh sách rỗng nếu không có sách, thay vì `404`?** | Dùng `Ok(books)` — `404` chỉ khi route không hợp lệ |

---

### **DAY 6: — CHƯƠNG 9: AUTHENTICATION & AUTHORIZATION**

| Câu hỏi | Trả lời |
|--------|--------|
| **JWT là gì??** | JSON Web Token — chuỗi mã hóa gồm Header, Payload, Signature. Dùng để xác thực stateless. |
| **Luồng JWT hoạt động thế nào??** | Client → Login → Server tạo token → Client lưu → Gọi API với Bearer <token> → Middleware validate → Cho phép. |
| **Tại sao không dùng Session??** | Stateless, scale tốt, không cần server lưu trạng thái. |
| **Claims dùng để làm gì??** | Lưu thông tin user: sub, email, role, name → Dùng cho [Authorize(Roles = "Admin")] |
| **Token hết hạn thì sao??** | 401 Unauthorized → Client gọi "/refresh" để lấy token mới. |
| **Làm sao bảo vệ API??** | Dùng [Authorize] + AddAuthentication("Bearer").AddJwtBearer(). |
| **Validate token ở đâu??** | Middleware → JwtBearer → TokenValidationParameters |
| **Key bí mật để làm gì??** | Ký token (SigningCredentials) → Server validate chữ ký. |
| **Refresh Token khác gì Access Token??** | Access: ngắn hạn (60p), Refresh: dài hạn (7 ngày), dùng để cấp lại Access. |
| **Tại sao không lưu password trong code???** | Dùng Identity + Hashing (PBKDF2) → Không bao giờ lưu plain text. |

## Luồng hoạt động — JWT Authentication (Mabrouk’s Pattern)

```mermaid
flowchart TD
    A["CLIENT"] -->|"POST /api/auth/login"| B["AuthController.Login()"]
    B --> C["IAuthService.LoginAsync()"]
    C --> D{{"Validate Email + Password"}}
    D -->|Fail| E["throw UnauthorizedAccessException"]
    D -->|Success| F["Generate JWT Token"]
    F --> G["Claims: sub, email, role"]
    G --> H["JwtSecurityToken + SigningCredentials"]
    H --> I["WriteToken → string"]
    I --> J["Response: 200 + { token, expires }"]
    
    J --> K["CLIENT lưu token"]
    K --> L["Gọi API với Header: Authorization: Bearer <token>"]
    L --> M["Middleware: JwtBearer"]
    M --> N["Validate: Issuer, Audience, Lifetime, SigningKey"]
    N -->|Fail| O["401 Unauthorized"]
    N -->|Success| P["[Authorize] → Gọi API được bảo vệ"]
    P --> Q["200 OK + Data"]


---
