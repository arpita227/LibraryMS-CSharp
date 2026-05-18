using LibraryMS.Interfaces;
using LibraryMS.Models;
using LibraryMS.Services;

namespace LibraryMS.Repositories
{
    /// <summary>
    /// In-memory repository for BorrowRecord entities backed by JSON persistence.
    /// </summary>
    public class BorrowRepository : IRepository<BorrowRecord>
    {
        private List<BorrowRecord> _records = new();
        private readonly DataPersistenceService _persistence;

        public BorrowRepository(DataPersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task LoadAsync()
        {
            _records = await _persistence.LoadBorrowsAsync();
        }

        public async Task SaveAsync()
        {
            await _persistence.SaveBorrowsAsync(_records);
        }

        public Task<IEnumerable<BorrowRecord>> GetAllAsync() =>
            Task.FromResult<IEnumerable<BorrowRecord>>(_records);

        public Task<BorrowRecord?> GetByIdAsync(string id) =>
            Task.FromResult(_records.FirstOrDefault(r => r.Id == id));

        public async Task AddAsync(BorrowRecord record)
        {
            _records.Add(record);
            await SaveAsync();
        }

        public async Task UpdateAsync(BorrowRecord record)
        {
            var index = _records.FindIndex(r => r.Id == record.Id);
            if (index >= 0)
            {
                _records[index] = record;
                await SaveAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            _records.RemoveAll(r => r.Id == id);
            await SaveAsync();
        }

        public Task<bool> ExistsAsync(string id) =>
            Task.FromResult(_records.Any(r => r.Id == id));

        // ── LINQ-powered query methods ─────────────────────────────────

        public IEnumerable<BorrowRecord> GetActiveByMember(string memberId) =>
            _records.Where(r => r.MemberId == memberId && !r.IsReturned)
                    .OrderBy(r => r.DueDate);

        public IEnumerable<BorrowRecord> GetHistoryByMember(string memberId) =>
            _records.Where(r => r.MemberId == memberId)
                    .OrderByDescending(r => r.BorrowedOn);

        public IEnumerable<BorrowRecord> GetAllActive() =>
            _records.Where(r => !r.IsReturned)
                    .OrderBy(r => r.DueDate);

        public IEnumerable<BorrowRecord> GetOverdue() =>
            _records.Where(r => r.IsOverdue)
                    .OrderBy(r => r.DueDate);

        public int CountActiveForMember(string memberId) =>
            _records.Count(r => r.MemberId == memberId && !r.IsReturned);

        /// <summary>Returns the top N most borrowed books using LINQ GroupBy.</summary>
        public IEnumerable<(string Title, int Count)> GetTopBorrowedBooks(int top = 5) =>
            _records.GroupBy(r => r.BookTitle)
                    .Select(g => (Title: g.Key, Count: g.Count()))
                    .OrderByDescending(x => x.Count)
                    .Take(top);
    }
}
