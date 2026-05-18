using LibraryMS.Interfaces;
using LibraryMS.Models;
using LibraryMS.Services;

namespace LibraryMS.Repositories
{
    /// <summary>
    /// In-memory repository for Book entities backed by JSON persistence.
    /// Implements the generic IRepository interface.
    /// </summary>
    public class BookRepository : IRepository<Book>
    {
        private List<Book> _books = new();
        private readonly DataPersistenceService _persistence;

        public BookRepository(DataPersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task LoadAsync()
        {
            _books = await _persistence.LoadBooksAsync();
        }

        public async Task SaveAsync()
        {
            await _persistence.SaveBooksAsync(_books);
        }

        public Task<IEnumerable<Book>> GetAllAsync() =>
            Task.FromResult<IEnumerable<Book>>(_books);

        public Task<Book?> GetByIdAsync(string id) =>
            Task.FromResult(_books.FirstOrDefault(b => b.Id == id));

        public async Task AddAsync(Book book)
        {
            _books.Add(book);
            await SaveAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            var index = _books.FindIndex(b => b.Id == book.Id);
            if (index >= 0)
            {
                _books[index] = book;
                await SaveAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            _books.RemoveAll(b => b.Id == id);
            await SaveAsync();
        }

        public Task<bool> ExistsAsync(string id) =>
            Task.FromResult(_books.Any(b => b.Id == id));

        // ── LINQ-powered search methods ────────────────────────────────

        public IEnumerable<Book> SearchByTitle(string keyword) =>
            _books.Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                  .OrderBy(b => b.Title);

        public IEnumerable<Book> SearchByAuthor(string keyword) =>
            _books.Where(b => b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                  .OrderBy(b => b.Author);

        public IEnumerable<Book> SearchByGenre(string genre) =>
            _books.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                  .OrderBy(b => b.Title);

        public IEnumerable<Book> GetAvailableBooks() =>
            _books.Where(b => b.IsAvailable)
                  .OrderBy(b => b.Title);

        public IEnumerable<string> GetAllGenres() =>
            _books.Select(b => b.Genre)
                  .Distinct(StringComparer.OrdinalIgnoreCase)
                  .OrderBy(g => g);
    }
}
