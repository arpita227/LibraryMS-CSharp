using LibraryMS.Interfaces;
using LibraryMS.Models;
using LibraryMS.Services;

namespace LibraryMS.Repositories
{
    /// <summary>
    /// In-memory repository for Member entities backed by JSON persistence.
    /// </summary>
    public class MemberRepository : IRepository<Member>
    {
        private List<Member> _members = new();
        private readonly DataPersistenceService _persistence;

        public MemberRepository(DataPersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task LoadAsync()
        {
            _members = await _persistence.LoadMembersAsync();
        }

        public async Task SaveAsync()
        {
            await _persistence.SaveMembersAsync(_members);
        }

        public Task<IEnumerable<Member>> GetAllAsync() =>
            Task.FromResult<IEnumerable<Member>>(_members);

        public Task<Member?> GetByIdAsync(string id) =>
            Task.FromResult(_members.FirstOrDefault(m => m.Id == id));

        public async Task AddAsync(Member member)
        {
            _members.Add(member);
            await SaveAsync();
        }

        public async Task UpdateAsync(Member member)
        {
            var index = _members.FindIndex(m => m.Id == member.Id);
            if (index >= 0)
            {
                _members[index] = member;
                await SaveAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            _members.RemoveAll(m => m.Id == id);
            await SaveAsync();
        }

        public Task<bool> ExistsAsync(string id) =>
            Task.FromResult(_members.Any(m => m.Id == id));

        // ── LINQ-powered search methods ────────────────────────────────

        public IEnumerable<Member> SearchByName(string keyword) =>
            _members.Where(m => m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.Name);

        public IEnumerable<Member> GetActiveMembers() =>
            _members.Where(m => m.IsActive)
                    .OrderBy(m => m.Name);

        public IEnumerable<Member> GetByMembershipType(MembershipType type) =>
            _members.Where(m => m.MembershipType == type)
                    .OrderBy(m => m.Name);

        public bool EmailExists(string email) =>
            _members.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
