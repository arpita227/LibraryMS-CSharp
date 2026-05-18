using System.Text.Json;
using LibraryMS.Models;

namespace LibraryMS.Services
{
    /// <summary>
    /// Handles reading and writing of all data to JSON files.
    /// Demonstrates async file I/O and System.Text.Json serialization.
    /// </summary>
    public class DataPersistenceService
    {
        private readonly string _dataDir;
        private readonly string _booksFile;
        private readonly string _membersFile;
        private readonly string _borrowsFile;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public DataPersistenceService()
        {
            _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _booksFile = Path.Combine(_dataDir, "books.json");
            _membersFile = Path.Combine(_dataDir, "members.json");
            _borrowsFile = Path.Combine(_dataDir, "borrows.json");
            EnsureDataDirectory();
        }

        private void EnsureDataDirectory()
        {
            if (!Directory.Exists(_dataDir))
                Directory.CreateDirectory(_dataDir);
        }

        // ── Books ──────────────────────────────────────────────────────

        public async Task<List<Book>> LoadBooksAsync()
        {
            if (!File.Exists(_booksFile)) return new List<Book>();
            var json = await File.ReadAllTextAsync(_booksFile);
            return JsonSerializer.Deserialize<List<Book>>(json, _jsonOptions) ?? new List<Book>();
        }

        public async Task SaveBooksAsync(List<Book> books)
        {
            var json = JsonSerializer.Serialize(books, _jsonOptions);
            await File.WriteAllTextAsync(_booksFile, json);
        }

        // ── Members ────────────────────────────────────────────────────

        public async Task<List<Member>> LoadMembersAsync()
        {
            if (!File.Exists(_membersFile)) return new List<Member>();
            var json = await File.ReadAllTextAsync(_membersFile);
            return JsonSerializer.Deserialize<List<Member>>(json, _jsonOptions) ?? new List<Member>();
        }

        public async Task SaveMembersAsync(List<Member> members)
        {
            var json = JsonSerializer.Serialize(members, _jsonOptions);
            await File.WriteAllTextAsync(_membersFile, json);
        }

        // ── Borrow Records ─────────────────────────────────────────────

        public async Task<List<BorrowRecord>> LoadBorrowsAsync()
        {
            if (!File.Exists(_borrowsFile)) return new List<BorrowRecord>();
            var json = await File.ReadAllTextAsync(_borrowsFile);
            return JsonSerializer.Deserialize<List<BorrowRecord>>(json, _jsonOptions) ?? new List<BorrowRecord>();
        }

        public async Task SaveBorrowsAsync(List<BorrowRecord> records)
        {
            var json = JsonSerializer.Serialize(records, _jsonOptions);
            await File.WriteAllTextAsync(_borrowsFile, json);
        }
    }
}
