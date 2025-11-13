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

## CÂU HỎI PHỎNG VẤN — DỰA TRÊN 4 NGÀY HỌC CỦA BẠN  
### **Cấp độ: Dễ → Trung bình → Khó**

---

### **DAY 1 — CHƯƠNG 3: FIRST BOOK API**

#### **Dễ**
| Câu hỏi | Trả lời |
|--------|--------|
| **Bạn đã học gì ở Day 1?** | Xây API đầu tiên với The Standard |
| **The Standard gồm mấy lớp?** | 3 lớp: `Broker`, `Service`, `Controller` |
| **Swagger dùng để làm gì?** | Tạo tài liệu API tự động |

#### **Trung bình**
| Câu hỏi | Trả lời |
|--------|--------|
| **Tại sao không `new BookService()` trong Controller?** | Dùng DI để inject `IBookService` → dễ test |
| **XML Comments có ích gì trong Swagger?** | Hiển thị mô tả API, tham số, response |

#### **Khó**
| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao để `POST /api/books` trả về `201 Created` với URL chi tiết?** | Dùng `CreatedAtAction(nameof(Get), new { id = book.Id }, book)` |

---

### **DAY 2 — CHƯƠNG 4: PIPELINE**

#### **Dễ**
| Câu hỏi | Trả lời |
|--------|--------|
| **Middleware là gì?** | Hàm xử lý request/response theo thứ tự |
| **Bạn đã viết middleware nào?** | `RequestTimingMiddleware` |

#### **Trung bình**
| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao log thời gian xử lý request?** | Dùng `Stopwatch` trong middleware |
| **Thứ tự `UseCors()` và `MapControllers()`?** | `UseCors()` trước `MapControllers()` |

#### **Khó**
| Câu hỏi | Trả lời |
|--------|--------|
| **Nếu `UseExceptionHandler()` đặt sau `MapControllers()`, điều gì xảy ra?** | Lỗi không được bắt → trả về HTML 500 |

---

### **DAY 3 — CHƯƠNG 6: DEPENDENCY INJECTION**

#### **Dễ**
| Câu hỏi | Trả lời |
|--------|--------|
| **DI là gì?** | Inject dependency thay vì `new` |
| **3 lifetime của DI là gì?** | `Singleton`, `Scoped`, `Transient` |

#### **Trung bình**
| Câu hỏi | Trả lời |
|--------|--------|
| **Unit Test cần DI không?** | Không, có thể `new BookService(broker)` |
| **Integration Test dùng gì?** | `WebApplicationFactory<Program>` |

#### **Khó**
| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao phát hiện lỗi DI (circular dependency) ngay khi start?** | Dùng `.NET 10 DI Diagnostics` → `builder.Services.AddDiagnostics()` |

---

### **DAY 4 — CHƯƠNG 7: ROUTING & ENDPOINTS**

#### **Dễ**
| Câu hỏi | Trả lời |
|--------|--------|
| **Attribute Routing là gì?** | `[HttpGet("{id:int}")]` |
| **Minimal API viết thế nào?** | `app.MapGet("/hello", () => "Hi")` |

#### **Trung bình**
| Câu hỏi | Trả lời |
|--------|--------|
| **Custom Route Constraint dùng khi nào?** | Khi cần kiểm tra tham số route (ví dụ: năm từ 1900–2025) |
| **Bạn đã viết constraint nào?** | `year/{year:year}` → `1800` → `404` |

#### **Khó**
| Câu hỏi | Trả lời |
|--------|--------|
| **Làm sao test `GET /api/books/year/1800` trả về `404`?** | Dùng `WebApplicationFactory` + `client.GetAsync()` + `Assert.Equal(404, response.StatusCode)` |
| **Nếu 2 route trùng, cái nào được chọn?** | Route cụ thể hơn (có constraint) hoặc thứ tự đăng ký |

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
