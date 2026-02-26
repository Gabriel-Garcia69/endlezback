
namespace Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Password { get; set; }
        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
        public ICollection<CustomerAddress> CustomerAddresses { get; set; } = [];

    }
}
