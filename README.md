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

## Kiến thức phỏng vấn — Câu hỏi & Trả lời ngắn gọn

### **1. The Standard là gì?**
> **Trả lời**: `Broker → Service → Controller` — tách biệt dữ liệu, logic, API.  
> **Lợi ích**: Dễ test, dễ bảo trì, dễ thay đổi.

### **2. Tại sao không dùng `new` trong Controller?**
> **Trả lời**: Dùng DI để inject `IBookService`.  
> **Lợi ích**: Không hard-code, dễ mock trong unit test.

### **3. Middleware Pipeline hoạt động thế nào?**
> **Trả lời**: Request đi qua các middleware theo thứ tự trong `Program.cs`.  
> **Ví dụ**: `UseCors()` → `UseAuthentication()` → `MapControllers()`.

### **4. Custom Middleware dùng khi nào?**
> **Trả lời**: Khi cần xử lý chung: log, timing, auth.  
> **Ví dụ**: `RequestTimingMiddleware` ghi thời gian xử lý.

### **5. 3 Lifetime của DI là gì?**
> **Trả lời**:  
> - `Singleton`: 1 instance cho toàn app  
> - `Scoped`: 1 instance cho mỗi request  
> - `Transient`: Tạo mới mỗi lần gọi

### **6. Unit Test vs Integration Test?**
> **Trả lời**:  
> - **Unit**: Test logic riêng (`new BookService(broker)`)  
> - **Integration**: Test toàn bộ (`WebApplicationFactory`)

### **7. Attribute Routing là gì?**
> **Trả lời**: Dùng `[HttpGet("{id:int}")]` để định nghĩa route.  
> **Ví dụ**: `GET /api/books/1` → chỉ chấp nhận `id` là số.

### **8. Custom Route Constraint dùng khi nào?**
> **Trả lời**: Khi cần luật riêng cho tham số route.  
> **Ví dụ**: `year/{year:year}` → chỉ cho phép `1900–2025`.

### **9. Khi nào dùng `async/await`?**
> **Trả lời**: Khi chờ I/O (DB, API, file).  
> **Ví dụ**: `GetBookByIdAsync` → không block thread.

### **10. Minimal API có lợi gì?**
> **Trả lời**: Viết nhanh, không cần Controller.  
> **Ví dụ**: `app.MapGet("/hello", () => "Hi");`

---

## Bằng chứng thực tế (GitHub)

**Repository**: [https://github.com/thuannluitk42/StudyDotnet](https://github.com/thuannluitk42/StudyDotnet)

| File | Mục đích |
|------|--------|
| `BooksController.cs` | Full CRUD + Async |
| `YearRouteConstraint.cs` | Custom constraint |
| `ApiIntegrationTests.cs` | Test toàn bộ pipeline |
| `Book.cs` | Có `PublishedYear` |

---

**Ngày 5 (7 PM HCMC)**:  
> **Chương 8: Model Binding & Validation**  
> - `[FromBody]`, `[FromQuery]`  
> - FluentValidation  
> - Custom Model Binder
