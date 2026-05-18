# 📚 Library Management System — C# .NET 8

A **mini Library Management System** built in **C# (.NET 8)** as a portfolio project, demonstrating core C# skills in a clean, well-structured console application.

> 📁 Source code is inside the [`LibraryMS/`](./LibraryMS) folder.

---

## ✨ Features

| Module | Operations |
|---|---|
| **📚 Books** | Add, Remove, Search by title/author/genre, View available copies |
| **👤 Members** | Register, Search, View borrow history, Deactivate |
| **🔄 Borrow/Return** | Issue books, Return with overdue fine calculation, Active & overdue view |
| **📊 Dashboard** | Live stats — book counts, member counts, overdue alerts, top 5 most borrowed |

---

## 🛠 C# Concepts Demonstrated

| Concept | Where Used |
|---|---|
| **OOP** — Classes, Encapsulation | `Book`, `Member`, `BorrowRecord` model classes |
| **Interfaces & Generics** | `IRepository<T>` generic CRUD contract |
| **LINQ** | Search, filter, sort, `GroupBy`, `DistinctBy` across all repositories |
| **async / await** | All file I/O and service operations are fully async |
| **JSON Serialization** | `System.Text.Json` — data persists across sessions |
| **Repository Pattern** | Clean separation: Models → Repositories → Services → UI |
| **Enums** | `BookStatus`, `MembershipType` for type-safe state |
| **Tuple returns** | `(bool Success, string Message)` for operation results |
| **Exception Safety** | Validation in service layer before every operation |

---

## 📁 Project Structure

```
LibraryMS/
├── Models/
│   ├── Book.cs               # Book entity with computed properties
│   ├── Member.cs             # Member entity with borrow limit logic
│   ├── BorrowRecord.cs       # Transaction record with overdue fine calc
│   └── Enums.cs              # BookStatus, MembershipType
├── Interfaces/
│   └── IRepository.cs        # Generic CRUD interface
├── Repositories/
│   ├── BookRepository.cs     # Book data access + LINQ queries
│   ├── MemberRepository.cs   # Member data access + LINQ queries
│   └── BorrowRepository.cs   # Borrow data access + GroupBy reports
├── Services/
│   ├── LibraryService.cs     # Core business logic layer
│   └── DataPersistenceService.cs  # Async JSON file I/O
├── UI/
│   └── ConsoleUI.cs          # Colour-coded console interface
└── Program.cs                # Entry point + DI composition + seed data
```

---

## 🚀 How to Run

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
cd LibraryMS
dotnet build
dotnet run
```

The app **auto-seeds 12 books & 4 members** on first launch so you can explore immediately.

---

## 💾 Data Persistence

All data is saved as human-readable **JSON files** in `LibraryMS/Data/`:
- `books.json`
- `members.json`
- `borrows.json`

Data survives application restarts — **no database required**.

---

## 💡 Business Rules

- **Standard** members → borrow up to **3 books** at a time
- **Premium** members → borrow up to **7 books** at a time
- Overdue fine: **₹5 per day** after the due date
- Books with active loans **cannot be deleted**
- Members with unreturned books **cannot be deactivated**
- **Duplicate emails** are rejected at registration

---

## 👩‍💻 Tech Stack

- Language: **C# 12**
- Framework: **.NET 8**
- Persistence: **System.Text.Json**
- Architecture: **Repository Pattern + Service Layer**
