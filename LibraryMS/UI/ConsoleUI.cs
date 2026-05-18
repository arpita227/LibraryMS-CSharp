using LibraryMS.Models;
using LibraryMS.Services;

namespace LibraryMS.UI
{
    /// <summary>
    /// All console rendering and user-interaction logic lives here,
    /// keeping it completely separate from business logic.
    /// </summary>
    public class ConsoleUI
    {
        private readonly LibraryService _service;

        // ── Colour palette ─────────────────────────────────────────────
        private const ConsoleColor Accent  = ConsoleColor.Cyan;
        private const ConsoleColor Success = ConsoleColor.Green;
        private const ConsoleColor Warning = ConsoleColor.Yellow;
        private const ConsoleColor Danger  = ConsoleColor.Red;
        private const ConsoleColor Muted   = ConsoleColor.DarkGray;
        private const ConsoleColor Header  = ConsoleColor.Magenta;

        public ConsoleUI(LibraryService service) => _service = service;

        // ══════════════════════════════════════════════════════════════
        //  ENTRY POINT
        // ══════════════════════════════════════════════════════════════

        public async Task RunAsync()
        {
            Console.Title = "📚 Library Management System";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ShowWelcomeBanner();

            bool running = true;
            while (running)
            {
                ShowMainMenu();
                var choice = ReadLine("Choose an option").Trim();

                switch (choice)
                {
                    case "1": await BookMenuAsync(); break;
                    case "2": await MemberMenuAsync(); break;
                    case "3": await BorrowMenuAsync(); break;
                    case "4": await ShowDashboardAsync(); break;
                    case "0":
                        running = false;
                        PrintColored("\n  Goodbye! Happy Reading! 📖\n", Success);
                        break;
                    default:
                        PrintColored("  Invalid option. Please try again.", Danger);
                        break;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  MAIN MENU & BANNER
        // ══════════════════════════════════════════════════════════════

        private void ShowWelcomeBanner()
        {
            Console.Clear();
            PrintColored("""

              ██╗     ██╗██████╗ ██████╗  █████╗ ██████╗ ██╗   ██╗    ███╗   ███╗███████╗
              ██║     ██║██╔══██╗██╔══██╗██╔══██╗██╔══██╗╚██╗ ██╔╝    ████╗ ████║██╔════╝
              ██║     ██║██████╔╝██████╔╝███████║██████╔╝ ╚████╔╝     ██╔████╔██║███████╗
              ██║     ██║██╔══██╗██╔══██╗██╔══██║██╔══██╗  ╚██╔╝      ██║╚██╔╝██║╚════██║
              ███████╗██║██████╔╝██║  ██║██║  ██║██║  ██║   ██║       ██║ ╚═╝ ██║███████║
              ╚══════╝╚═╝╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝       ╚═╝     ╚═╝╚══════╝
            """, Accent);

            PrintColored("                    📚  Library Management System  v1.0", Header);
            PrintColored("                         Built with C# .NET 8 | OOP · LINQ · JSON\n", Muted);
        }

        private void ShowMainMenu()
        {
            Console.WriteLine();
            PrintDivider("MAIN MENU");
            PrintMenuItem("1", "📚 Book Management");
            PrintMenuItem("2", "👤 Member Management");
            PrintMenuItem("3", "🔄 Borrow / Return");
            PrintMenuItem("4", "📊 Dashboard & Reports");
            PrintMenuItem("0", "❌ Exit");
            PrintDividerLine();
        }

        // ══════════════════════════════════════════════════════════════
        //  BOOK MENU
        // ══════════════════════════════════════════════════════════════

        private async Task BookMenuAsync()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine();
                PrintDivider("BOOK MANAGEMENT");
                PrintMenuItem("1", "View All Books");
                PrintMenuItem("2", "Search Books");
                PrintMenuItem("3", "Browse by Genre");
                PrintMenuItem("4", "View Available Books");
                PrintMenuItem("5", "Add New Book");
                PrintMenuItem("6", "Remove Book");
                PrintMenuItem("0", "← Back");
                PrintDividerLine();

                switch (ReadLine("Choose").Trim())
                {
                    case "1": await ListAllBooksAsync(); break;
                    case "2": await SearchBooksAsync(); break;
                    case "3": await BrowseByGenreAsync(); break;
                    case "4": ShowAvailableBooks(); break;
                    case "5": await AddBookAsync(); break;
                    case "6": await RemoveBookAsync(); break;
                    case "0": back = true; break;
                    default: PrintColored("  Invalid option.", Danger); break;
                }
            }
        }

        private async Task ListAllBooksAsync()
        {
            var books = (await _service.GetAllBooksAsync()).ToList();
            PrintDivider($"ALL BOOKS ({books.Count} total)");

            if (!books.Any()) { PrintColored("  No books in catalog.", Muted); return; }

            foreach (var b in books.OrderBy(b => b.Title))
                PrintBook(b);
        }

        private async Task SearchBooksAsync()
        {
            var keyword = ReadLine("Enter search keyword (title / author)");
            if (string.IsNullOrWhiteSpace(keyword)) return;

            var results = _service.SearchBooks(keyword).ToList();
            PrintDivider($"SEARCH RESULTS — \"{keyword}\" ({results.Count} found)");

            if (!results.Any()) { PrintColored("  No matching books found.", Muted); return; }
            foreach (var b in results) PrintBook(b);
        }

        private async Task BrowseByGenreAsync()
        {
            var genres = _service.GetAllGenres().ToList();
            if (!genres.Any()) { PrintColored("  No genres found.", Muted); return; }

            PrintDivider("AVAILABLE GENRES");
            for (int i = 0; i < genres.Count; i++)
                PrintColored($"  {i + 1}. {genres[i]}", Accent);

            var input = ReadLine("Enter genre name to browse");
            var books = _service.SearchByGenre(input).ToList();
            PrintDivider($"GENRE: {input.ToUpper()} ({books.Count} books)");

            if (!books.Any()) { PrintColored("  No books in this genre.", Muted); return; }
            foreach (var b in books) PrintBook(b);
        }

        private void ShowAvailableBooks()
        {
            var books = _service.GetAvailableBooks().ToList();
            PrintDivider($"AVAILABLE BOOKS ({books.Count})");
            if (!books.Any()) { PrintColored("  No books available for borrowing.", Muted); return; }
            foreach (var b in books) PrintBook(b);
        }

        private async Task AddBookAsync()
        {
            PrintDivider("ADD NEW BOOK");
            var title  = ReadLine("Title");
            var author = ReadLine("Author");
            var isbn   = ReadLine("ISBN (optional)");
            var genre  = ReadLine("Genre");
            var yearStr = ReadLine("Published Year");
            var copiesStr = ReadLine("Number of Copies");

            if (!int.TryParse(yearStr, out int year))   { PrintColored("  Invalid year.", Danger); return; }
            if (!int.TryParse(copiesStr, out int copies) || copies < 1)
            { PrintColored("  Invalid copies count.", Danger); return; }

            var (ok, msg) = await _service.AddBookAsync(title, author, isbn, genre, year, copies);
            PrintResult(ok, msg);
        }

        private async Task RemoveBookAsync()
        {
            PrintDivider("REMOVE BOOK");
            var id = ReadLine("Enter Book ID").ToUpper();
            var (ok, msg) = await _service.RemoveBookAsync(id);
            PrintResult(ok, msg);
        }

        // ══════════════════════════════════════════════════════════════
        //  MEMBER MENU
        // ══════════════════════════════════════════════════════════════

        private async Task MemberMenuAsync()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine();
                PrintDivider("MEMBER MANAGEMENT");
                PrintMenuItem("1", "View All Members");
                PrintMenuItem("2", "Search Members");
                PrintMenuItem("3", "View Member History");
                PrintMenuItem("4", "Register New Member");
                PrintMenuItem("5", "Deactivate Member");
                PrintMenuItem("0", "← Back");
                PrintDividerLine();

                switch (ReadLine("Choose").Trim())
                {
                    case "1": await ListAllMembersAsync(); break;
                    case "2": await SearchMembersAsync(); break;
                    case "3": await ShowMemberHistoryAsync(); break;
                    case "4": await RegisterMemberAsync(); break;
                    case "5": await DeactivateMemberAsync(); break;
                    case "0": back = true; break;
                    default: PrintColored("  Invalid option.", Danger); break;
                }
            }
        }

        private async Task ListAllMembersAsync()
        {
            var members = (await _service.GetAllMembersAsync()).ToList();
            PrintDivider($"ALL MEMBERS ({members.Count} total)");
            if (!members.Any()) { PrintColored("  No members registered.", Muted); return; }
            foreach (var m in members.OrderBy(m => m.Name)) PrintMember(m);
        }

        private async Task SearchMembersAsync()
        {
            var keyword = ReadLine("Enter member name");
            var results = _service.SearchMembers(keyword).ToList();
            PrintDivider($"MEMBER SEARCH — \"{keyword}\" ({results.Count} found)");
            if (!results.Any()) { PrintColored("  No members found.", Muted); return; }
            foreach (var m in results) PrintMember(m);
        }

        private async Task ShowMemberHistoryAsync()
        {
            var id = ReadLine("Enter Member ID").ToUpper();
            var member = await _service.GetMemberByIdAsync(id);
            if (member == null) { PrintColored("  Member not found.", Danger); return; }

            var history = _service.GetMemberHistory(id).ToList();
            PrintDivider($"BORROW HISTORY — {member.Name} ({history.Count} records)");
            if (!history.Any()) { PrintColored("  No borrow history found.", Muted); return; }
            foreach (var r in history) PrintRecord(r);
        }

        private async Task RegisterMemberAsync()
        {
            PrintDivider("REGISTER NEW MEMBER");
            var name  = ReadLine("Full Name");
            var email = ReadLine("Email");
            var phone = ReadLine("Phone");
            Console.WriteLine("  Membership Type: [1] Standard  [2] Premium");
            var typeInput = ReadLine("Choose type");
            var type = typeInput == "2" ? MembershipType.Premium : MembershipType.Standard;

            var (ok, msg) = await _service.RegisterMemberAsync(name, email, phone, type);
            PrintResult(ok, msg);
        }

        private async Task DeactivateMemberAsync()
        {
            PrintDivider("DEACTIVATE MEMBER");
            var id = ReadLine("Enter Member ID").ToUpper();
            var (ok, msg) = await _service.DeactivateMemberAsync(id);
            PrintResult(ok, msg);
        }

        // ══════════════════════════════════════════════════════════════
        //  BORROW / RETURN MENU
        // ══════════════════════════════════════════════════════════════

        private async Task BorrowMenuAsync()
        {
            bool back = false;
            while (!back)
            {
                Console.WriteLine();
                PrintDivider("BORROW / RETURN");
                PrintMenuItem("1", "Issue Book to Member");
                PrintMenuItem("2", "Return a Book");
                PrintMenuItem("3", "View All Active Borrows");
                PrintMenuItem("4", "View Overdue Books");
                PrintMenuItem("0", "← Back");
                PrintDividerLine();

                switch (ReadLine("Choose").Trim())
                {
                    case "1": await IssueBookAsync(); break;
                    case "2": await ReturnBookAsync(); break;
                    case "3": ShowActiveBorrows(); break;
                    case "4": ShowOverdueBooks(); break;
                    case "0": back = true; break;
                    default: PrintColored("  Invalid option.", Danger); break;
                }
            }
        }

        private async Task IssueBookAsync()
        {
            PrintDivider("ISSUE BOOK");
            var bookId   = ReadLine("Enter Book ID").ToUpper();
            var memberId = ReadLine("Enter Member ID").ToUpper();
            var daysStr  = ReadLine("Borrow duration in days [default: 14]");

            int days = string.IsNullOrWhiteSpace(daysStr) ? 14
                       : int.TryParse(daysStr, out int d) ? d : 14;

            var (ok, msg) = await _service.BorrowBookAsync(bookId, memberId, days);
            PrintResult(ok, msg);
        }

        private async Task ReturnBookAsync()
        {
            PrintDivider("RETURN BOOK");
            ShowActiveBorrows();
            var recordId = ReadLine("\n  Enter Borrow Record ID").ToUpper();
            var (ok, msg, fine) = await _service.ReturnBookAsync(recordId);
            PrintResult(ok, msg);
            if (fine > 0) PrintColored($"  💰 Fine Amount: ₹{fine:F2}", Warning);
        }

        private void ShowActiveBorrows()
        {
            var records = _service.GetActiveBorrows().ToList();
            PrintDivider($"ACTIVE BORROWS ({records.Count})");
            if (!records.Any()) { PrintColored("  No active borrows.", Muted); return; }
            foreach (var r in records) PrintRecord(r);
        }

        private void ShowOverdueBooks()
        {
            var records = _service.GetOverdueRecords().ToList();
            PrintDivider($"OVERDUE BOOKS ({records.Count})");
            if (!records.Any()) { PrintColored("  No overdue books. Great!", Success); return; }
            foreach (var r in records)
            {
                PrintColored($"  ⚠  {r}", Danger);
                PrintColored($"     Fine so far: ₹{r.OverdueFine:F2}", Warning);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  DASHBOARD
        // ══════════════════════════════════════════════════════════════

        private async Task ShowDashboardAsync()
        {
            var s = await _service.GetStatsAsync();

            PrintDivider("📊 LIBRARY DASHBOARD");
            Console.WriteLine();

            PrintStatRow("📚 Total Book Titles",   s.TotalBooks.ToString());
            PrintStatRow("📦 Total Copies",         s.TotalCopies.ToString());
            PrintStatRow("✅ Available Copies",      s.AvailableCopies.ToString());
            PrintStatRow("👤 Total Members",         s.TotalMembers.ToString());
            PrintStatRow("✔  Active Members",        s.ActiveMembers.ToString());
            PrintStatRow("⭐ Premium Members",       s.PremiumMembers.ToString());
            PrintStatRow("🔄 Currently Borrowed",   s.ActiveBorrows.ToString());

            var overdueColor = s.OverdueBooks > 0 ? Danger : Success;
            Console.Write("     ⚠  Overdue Books          : ");
            PrintColored(s.OverdueBooks.ToString(), overdueColor, newLine: true);

            if (s.TopBorrowedBooks.Any())
            {
                Console.WriteLine();
                PrintColored("  🏆 Top 5 Most Borrowed Books:", Header);
                int rank = 1;
                foreach (var (title, count) in s.TopBorrowedBooks)
                    PrintColored($"     {rank++}. {title,-35} {count} borrow(s)", Accent);
            }

            PrintDividerLine();
        }

        // ══════════════════════════════════════════════════════════════
        //  DISPLAY HELPERS
        // ══════════════════════════════════════════════════════════════

        private static void PrintBook(Book b)
        {
            var statusColor = b.IsAvailable ? Success : Warning;
            Console.Write($"  [{b.Id}] ");
            PrintColored($"\"{b.Title}\"", Accent, newLine: false);
            Console.Write($" by {b.Author} ({b.PublishedYear}) — {b.Genre} | Copies: ");
            PrintColored($"{b.AvailableCopies}/{b.TotalCopies}", statusColor, newLine: true);
        }

        private static void PrintMember(Member m)
        {
            var color = m.IsActive ? Success : Muted;
            Console.Write($"  [{m.Id}] ");
            PrintColored(m.Name, color, newLine: false);
            Console.Write($" | {m.Email} | ");
            PrintColored(m.MembershipType.ToString(), m.MembershipType == MembershipType.Premium ? Warning : Accent, newLine: false);
            Console.WriteLine($" | Active: {m.IsActive}");
        }

        private static void PrintRecord(BorrowRecord r)
        {
            var color = r.IsOverdue ? Danger : r.IsReturned ? Muted : Success;
            PrintColored($"  [{r.Id}] \"{r.BookTitle}\" → {r.MemberName} | Due: {r.DueDate:dd-MMM-yyyy}" +
                (r.IsReturned ? $" | ✅ Returned: {r.ReturnedOn:dd-MMM-yyyy}" :
                 r.IsOverdue  ? " | ⚠ OVERDUE" : ""), color);
        }

        private static void PrintResult(bool ok, string msg) =>
            PrintColored($"\n  {(ok ? "✅" : "❌")} {msg}\n", ok ? Success : Danger);

        private static void PrintMenuItem(string key, string label) =>
            Console.WriteLine($"    [{key}] {label}");

        private static void PrintStatRow(string label, string value)
        {
            Console.Write($"     {label,-30}: ");
            PrintColored(value, Accent, newLine: true);
        }

        private static void PrintDivider(string title)
        {
            Console.WriteLine();
            Console.Write("  ┌─ ");
            PrintColored(title, Header, newLine: false);
            Console.WriteLine(" " + new string('─', Math.Max(0, 55 - title.Length)));
        }

        private static void PrintDividerLine() =>
            Console.WriteLine("  └" + new string('─', 60));

        private static void PrintColored(string text, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ResetColor();
        }

        private static string ReadLine(string prompt)
        {
            Console.ForegroundColor = Muted;
            Console.Write($"\n  ➤ {prompt}: ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine() ?? "";
            Console.ResetColor();
            return input;
        }
    }
}
