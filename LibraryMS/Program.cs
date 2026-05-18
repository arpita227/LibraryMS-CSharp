using LibraryMS.Repositories;
using LibraryMS.Services;
using LibraryMS.UI;

// ── Dependency composition ────────────────────────────────────────────────────
var persistence    = new DataPersistenceService();
var bookRepo       = new BookRepository(persistence);
var memberRepo     = new MemberRepository(persistence);
var borrowRepo     = new BorrowRepository(persistence);
var libraryService = new LibraryService(bookRepo, memberRepo, borrowRepo);

// ── Load existing data from disk ──────────────────────────────────────────────
await libraryService.InitializeAsync();

// ── Seed demo data if the catalog is empty ────────────────────────────────────
await SeedDemoDataAsync(libraryService);

// ── Launch UI ─────────────────────────────────────────────────────────────────
var ui = new ConsoleUI(libraryService);
await ui.RunAsync();


// ═════════════════════════════════════════════════════════════════════════════
//  SEED  — only runs when the catalog is completely empty
// ═════════════════════════════════════════════════════════════════════════════
static async Task SeedDemoDataAsync(LibraryService svc)
{
    var existing = (await svc.GetAllBooksAsync()).ToList();
    if (existing.Any()) return; // already seeded

    // --- Books ---
    await svc.AddBookAsync("Clean Code",                      "Robert C. Martin",    "9780132350884", "Technology",    2008, 3);
    await svc.AddBookAsync("The Pragmatic Programmer",        "Andrew Hunt",         "9780201616224", "Technology",    1999, 2);
    await svc.AddBookAsync("Design Patterns",                 "Gang of Four",        "9780201633610", "Technology",    1994, 2);
    await svc.AddBookAsync("Atomic Habits",                   "James Clear",         "9780735211292", "Self-Help",     2018, 4);
    await svc.AddBookAsync("The 7 Habits of Highly Effective People", "Stephen Covey","9780743269513","Self-Help",    1989, 3);
    await svc.AddBookAsync("Deep Work",                       "Cal Newport",         "9781455586691", "Self-Help",     2016, 2);
    await svc.AddBookAsync("1984",                            "George Orwell",       "9780451524935", "Fiction",       1949, 5);
    await svc.AddBookAsync("To Kill a Mockingbird",           "Harper Lee",          "9780061935466", "Fiction",       1960, 3);
    await svc.AddBookAsync("The Great Gatsby",                "F. Scott Fitzgerald", "9780743273565", "Fiction",       1925, 2);
    await svc.AddBookAsync("Sapiens",                         "Yuval Noah Harari",   "9780062316097", "History",       2011, 3);
    await svc.AddBookAsync("A Brief History of Time",         "Stephen Hawking",     "9780553380163", "Science",       1988, 2);
    await svc.AddBookAsync("The Selfish Gene",                "Richard Dawkins",     "9780198788607", "Science",       1976, 2);

    // --- Members ---
    await svc.RegisterMemberAsync("Alice Johnson",  "alice@example.com",  "9876543210", LibraryMS.Models.MembershipType.Premium);
    await svc.RegisterMemberAsync("Bob Smith",      "bob@example.com",    "9123456780", LibraryMS.Models.MembershipType.Standard);
    await svc.RegisterMemberAsync("Clara Mendes",   "clara@example.com",  "9988776655", LibraryMS.Models.MembershipType.Standard);
    await svc.RegisterMemberAsync("David Lee",      "david@example.com",  "9871234560", LibraryMS.Models.MembershipType.Premium);
}
