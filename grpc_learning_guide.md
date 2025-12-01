# 📚 gRPC Learning Guide - From Theory to Practice

## 🎯 **Part 1: Lý Thuyết (Theory)**

### **1.1 gRPC Là Gì?**

**gRPC** = **g**oogle **R**emote **P**rocedure **C**all

- Framework RPC hiện đại từ Google
- Client gọi method trên server như gọi local function
- Sử dụng **HTTP/2** và **Protocol Buffers (ProtoBuf)**

**Ví dụ đơn giản:**
```csharp
// Thay vì REST:
var response = await httpClient.GetAsync("http://bookapi/api/books/1");
var book = await response.Content.ReadFromJsonAsync<Book>();

// gRPC:
var book = await grpcClient.GetBookAsync(new GetBookRequest { Id = 1 });
```

---

### **1.2 Tại Sao gRPC Nhanh Hơn REST?**

| Feature | REST (JSON) | gRPC (ProtoBuf) |
|---------|-------------|-----------------|
| **Protocol** | HTTP/1.1 | HTTP/2 |
| **Format** | JSON (text) | Binary (ProtoBuf) |
| **Size** | ~500 bytes | ~50 bytes (10x nhỏ hơn) |
| **Speed** | Baseline | 2-10x nhanh hơn |
| **Type Safety** | Runtime | Compile-time |
| **Streaming** | ❌ | ✅ |

**HTTP/2 Benefits:**
- Multiplexing: Nhiều requests trên 1 connection
- Header compression: Giảm overhead
- Binary framing: Parse nhanh hơn text

---

### **1.3 Protocol Buffers (ProtoBuf)**

**ProtoBuf** là ngôn ngữ định nghĩa data structure, platform-independent.

**Example `.proto` file:**
```protobuf
syntax = "proto3";

message Book {
  int32 id = 1;
  string title = 2;
  string author = 3;
  double price = 4;
}

service BookService {
  rpc GetBook (GetBookRequest) returns (BookResponse);
}

message GetBookRequest {
  int32 id = 1;
}

message BookResponse {
  Book book = 1;
}
```

**Tool generate C# code:**
```csharp
// Auto-generated
public class Book {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public double Price { get; set; }
}
```

---

### **1.4 Các Loại gRPC Calls**

#### **1. Unary RPC** (Request-Response)
```protobuf
rpc GetBook (GetBookRequest) returns (BookResponse);
```
- Client gửi 1 request → Server trả 1 response
- Giống REST GET/POST

#### **2. Server Streaming**
```protobuf
rpc GetBooksStream (GetBooksRequest) returns (stream BookResponse);
```
- Client gửi 1 request → Server trả stream responses
- Use case: Download large dataset, real-time updates

#### **3. Client Streaming**
```protobuf
rpc UploadBooks (stream UploadBookRequest) returns (UploadResponse);
```
- Client gửi stream requests → Server trả 1 response
- Use case: Upload files, batch processing

#### **4. Bi-directional Streaming**
```protobuf
rpc Chat (stream ChatMessage) returns (stream ChatMessage);
```
- Client và Server đều stream
- Use case: Chat, real-time collaboration

---

### **1.5 gRPC vs REST vs RabbitMQ**

| Scenario | Best Choice | Why |
|----------|-------------|-----|
| **Get book details** | gRPC | Sync, fast, type-safe |
| **Create order** | REST | Simple, browser-friendly |
| **Notify after order** | RabbitMQ | Async, decoupled |
| **Real-time updates** | gRPC Streaming | Bi-directional, low latency |
| **Public API** | REST | Standard, widely supported |
| **Internal services** | gRPC | Performance, type-safety |

---

## 🛠️ **Part 2: Thực Hành (Practice)**

### **2.1 Architecture Overview**

```
┌─────────────┐         gRPC          ┌─────────────┐
│             │ ──────────────────────>│             │
│  OrderApi   │  GetBook(id)          │   BookApi   │
│  (Client)   │ <──────────────────────│  (Server)   │
│             │  BookResponse         │             │
└─────────────┘                        └─────────────┘
      │                                       │
      │ RabbitMQ                             │
      │ (Async)                              │
      └──────────────────────────────────────┘
```

**Flow:**
1. User tạo order qua OrderApi
2. OrderApi **gọi gRPC** tới BookApi để check stock (sync)
3. Nếu có stock, tạo order
4. OrderApi **publish RabbitMQ** event (async)
5. BookApi consume event để update stock

---

### **2.2 Implementation Steps**

#### **Step 1: Setup BookApi gRPC Server**
1. Add NuGet packages
2. Create `.proto` file
3. Implement gRPC service
4. Map gRPC endpoint

#### **Step 2: Setup OrderApi gRPC Client**
1. Add NuGet packages
2. Reference `.proto` file
3. Configure gRPC client
4. Call gRPC service

#### **Step 3: Add Streaming**
1. Implement server streaming
2. Test real-time updates

#### **Step 4: Add Resilience**
1. Add Polly retry for gRPC
2. Configure deadline (timeout)
3. Test failure scenarios

#### **Step 5: Deploy to Docker**
1. Expose gRPC port (5001)
2. Update docker-compose.yml
3. Test in Swarm

---

## 📊 **Part 3: So Sánh Performance**

### **Expected Results:**

| Metric | REST | gRPC | Improvement |
|--------|------|------|-------------|
| **Response Time** | 50ms | 10ms | 5x faster |
| **Payload Size** | 500 bytes | 50 bytes | 10x smaller |
| **Throughput** | 1000 req/s | 5000 req/s | 5x higher |
| **CPU Usage** | 80% | 40% | 50% less |

---

## 🎯 **Part 4: Best Practices**

### **When to Use gRPC:**
✅ Internal microservices communication  
✅ High-performance requirements  
✅ Real-time streaming  
✅ Type-safe contracts  

### **When NOT to Use gRPC:**
❌ Public-facing APIs (browsers don't support HTTP/2 well)  
❌ Simple CRUD operations (REST is simpler)  
❌ Legacy systems integration  

### **Production Patterns:**
1. **Deadline**: Set timeout cho mỗi call
2. **Retry**: Auto retry khi transient failures
3. **Circuit Breaker**: Prevent cascade failures
4. **Load Balancing**: Distribute calls across replicas
5. **mTLS**: Secure communication

---

## 📝 **Next Steps**

Chúng ta sẽ implement:
1. ✅ **Theory** - Đã học xong
2. 🔄 **BookApi gRPC Server** - Tiếp theo
3. 🔄 **OrderApi gRPC Client**
4. 🔄 **Streaming & Resilience**
5. 🔄 **Docker Deployment**

Sẵn sàng code chưa? 😊
