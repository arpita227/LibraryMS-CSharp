namespace LibraryMS.Models
{
    /// <summary>Represents the availability status of a book in the library.</summary>
    public enum BookStatus
    {
        Available,
        Borrowed,
        Reserved,
        Damaged,
        Lost
    }

    /// <summary>Membership tier for library members.</summary>
    public enum MembershipType
    {
        Standard,   // Can borrow up to 3 books at a time
        Premium     // Can borrow up to 7 books at a time
    }
}
