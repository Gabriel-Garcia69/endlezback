namespace Core.Dtos.User
{
    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public Guid ProfileId { get; set; }
    }
}
