using LibraryMS.Models;
using LibraryMS.Repositories;

namespace LibraryMS.Services
{
    /// <summary>
    /// Core business logic layer. Orchestrates operations across all three repositories.
    /// All public methods are async to demonstrate async/await patterns.
    /// </summary>
    public class LibraryService
    {
        private readonly BookRepository _books;
        private readonly MemberRepository _members;
        private readonly BorrowRepository _borrows;

        public LibraryService(BookRepository books, MemberRepository members, BorrowRepository borrows)
        {
            _books   = books;
            _members = members;
            _borrows = borrows;
        }

        /// <summary>Loads all data from disk on startup.</summary>
        public async Task InitializeAsync()
        {
            await Task.WhenAll(
                _books.LoadAsync(),
                _members.LoadAsync(),
                _borrows.LoadAsync()
            );
        }

        // ══════════════════════════════════════════════════════════════
        //  BOOK OPERATIONS
        // ══════════════════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> AddBookAsync(
            string title, string author, string isbn,
            string genre, int year, int copies)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                return (false, "Title and author cannot be empty.");

            var book = new Book
            {
                Title          = title.Trim(),
                Author         = author.Trim(),
                ISBN           = isbn.Trim(),
                Genre          = genre.Trim(),
                PublishedYear  = year,
                TotalCopies    = copies,
                AvailableCopies = copies,
                Status         = BookStatus.Available
            };

            await _books.AddAsync(book);
            return (true, $"Book '{book.Title}' added successfully with ID: {book.Id}");
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync() =>
            await _books.GetAllAsync();

        public async Task<Book?> GetBookByIdAsync(string id) =>
            await _books.GetByIdAsync(id);

        public IEnumerable<Book> SearchBooks(string keyword) =>
            _books.SearchByTitle(keyword)
                  .Concat(_books.SearchByAuthor(keyword))
                  .DistinctBy(b => b.Id)
                  .OrderBy(b => b.Title);

        public IEnumerable<Book> GetAvailableBooks() => _books.GetAvailableBooks();

        public IEnumerable<Book> SearchByGenre(string genre) => _books.SearchByGenre(genre);

        public IEnumerable<string> GetAllGenres() => _books.GetAllGenres();

        public async Task<(bool Success, string Message)> RemoveBookAsync(string bookId)
        {
            var book = await _books.GetByIdAsync(bookId);
            if (book == null) return (false, "Book not found.");

            var activeLoans = _borrows.GetAllActive().Any(r => r.BookId == bookId);
            if (activeLoans) return (false, "Cannot remove a book that is currently borrowed.");

            await _books.DeleteAsync(bookId);
            return (true, $"Book '{book.Title}' removed successfully.");
        }

        // ══════════════════════════════════════════════════════════════
        //  MEMBER OPERATIONS
        // ══════════════════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> RegisterMemberAsync(
            string name, string email, string phone, MembershipType type)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
                return (false, "Name and email are required.");

            if (_members.EmailExists(email))
                return (false, $"A member with email '{email}' already exists.");

            var member = new Member
            {
                Name           = name.Trim(),
                Email          = email.Trim().ToLower(),
                Phone          = phone.Trim(),
                MembershipType = type,
                IsActive       = true
            };

            await _members.AddAsync(member);
            return (true, $"Member '{member.Name}' registered with ID: {member.Id}");
        }

        public async Task<IEnumerable<Member>> GetAllMembersAsync() =>
            await _members.GetAllAsync();

        public async Task<Member?> GetMemberByIdAsync(string id) =>
            await _members.GetByIdAsync(id);

        public IEnumerable<Member> SearchMembers(string keyword) =>
            _members.SearchByName(keyword);

        public async Task<(bool Success, string Message)> DeactivateMemberAsync(string memberId)
        {
            var member = await _members.GetByIdAsync(memberId);
            if (member == null) return (false, "Member not found.");

            var hasActiveLoans = _borrows.CountActiveForMember(memberId) > 0;
            if (hasActiveLoans) return (false, "Member has unreturned books. Please return them first.");

            member.IsActive = false;
            await _members.UpdateAsync(member);
            return (true, $"Member '{member.Name}' has been deactivated.");
        }

        // ══════════════════════════════════════════════════════════════
        //  BORROW & RETURN OPERATIONS
        // ══════════════════════════════════════════════════════════════

        public async Task<(bool Success, string Message)> BorrowBookAsync(
            string bookId, string memberId, int borrowDays = 14)
        {
            var book   = await _books.GetByIdAsync(bookId);
            var member = await _members.GetByIdAsync(memberId);

            if (book == null)   return (false, "Book not found.");
            if (member == null) return (false, "Member not found.");
            if (!member.IsActive) return (false, "Inactive members cannot borrow books.");
            if (!book.IsAvailable) return (false, $"'{book.Title}' has no available copies.");

            int activeLoans = _borrows.CountActiveForMember(memberId);
            if (activeLoans >= member.BorrowLimit)
                return (false, $"Member has reached their borrow limit ({member.BorrowLimit} books).");

            // Update book stock
            book.AvailableCopies--;
            if (book.AvailableCopies == 0)
                book.Status = BookStatus.Borrowed;
            await _books.UpdateAsync(book);

            // Create borrow record
            var record = new BorrowRecord
            {
                BookId     = bookId,
                MemberId   = memberId,
                BookTitle  = book.Title,
                MemberName = member.Name,
                BorrowedOn = DateTime.Now,
                DueDate    = DateTime.Now.AddDays(borrowDays)
            };

            await _borrows.AddAsync(record);
            return (true, $"'{book.Title}' issued to {member.Name}. Due: {record.DueDate:dd-MMM-yyyy}");
        }

        public async Task<(bool Success, string Message, decimal Fine)> ReturnBookAsync(string recordId)
        {
            var record = await _borrows.GetByIdAsync(recordId);
            if (record == null)    return (false, "Borrow record not found.", 0);
            if (record.IsReturned) return (false, "This book has already been returned.", 0);

            decimal fine = record.OverdueFine;

            // Mark returned
            record.ReturnedOn = DateTime.Now;
            await _borrows.UpdateAsync(record);

            // Restore book stock
            var book = await _books.GetByIdAsync(record.BookId);
            if (book != null)
            {
                book.AvailableCopies++;
                if (book.AvailableCopies > 0)
                    book.Status = BookStatus.Available;
                await _books.UpdateAsync(book);
            }

            string msg = fine > 0
                ? $"Book returned. Overdue fine: ₹{fine:F2}"
                : "Book returned on time. Thank you!";

            return (true, msg, fine);
        }

        public IEnumerable<BorrowRecord> GetActiveBorrows() => _borrows.GetAllActive();

        public IEnumerable<BorrowRecord> GetOverdueRecords() => _borrows.GetOverdue();

        public IEnumerable<BorrowRecord> GetMemberHistory(string memberId) =>
            _borrows.GetHistoryByMember(memberId);

        // ══════════════════════════════════════════════════════════════
        //  REPORTS / STATISTICS
        // ══════════════════════════════════════════════════════════════

        public async Task<LibraryStats> GetStatsAsync()
        {
            var books   = (await _books.GetAllAsync()).ToList();
            var members = (await _members.GetAllAsync()).ToList();

            return new LibraryStats
            {
                TotalBooks          = books.Count,
                TotalCopies         = books.Sum(b => b.TotalCopies),
                AvailableCopies     = books.Sum(b => b.AvailableCopies),
                TotalMembers        = members.Count,
                ActiveMembers       = members.Count(m => m.IsActive),
                PremiumMembers      = members.Count(m => m.MembershipType == MembershipType.Premium),
                ActiveBorrows       = _borrows.GetAllActive().Count(),
                OverdueBooks        = _borrows.GetOverdue().Count(),
                TopBorrowedBooks    = _borrows.GetTopBorrowedBooks(5).ToList()
            };
        }
    }

    /// <summary>Value object carrying all library statistics for the dashboard.</summary>
    public class LibraryStats
    {
        public int TotalBooks       { get; set; }
        public int TotalCopies      { get; set; }
        public int AvailableCopies  { get; set; }
        public int TotalMembers     { get; set; }
        public int ActiveMembers    { get; set; }
        public int PremiumMembers   { get; set; }
        public int ActiveBorrows    { get; set; }
        public int OverdueBooks     { get; set; }
        public List<(string Title, int Count)> TopBorrowedBooks { get; set; } = new();
    }
}
