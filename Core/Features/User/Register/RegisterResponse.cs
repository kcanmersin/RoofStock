namespace Core.Features.User.Register
{
    public class RegisterResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();

        public string Message { get; set; }
    }
}
