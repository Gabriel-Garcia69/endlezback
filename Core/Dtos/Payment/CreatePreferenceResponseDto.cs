namespace Core.Dtos.Payment
{
    public class CreatePreferenceResponseDto
    {
        public string InitPoint { get; set; } = null!;
        public string SandboxInitPoint { get; set; } = null!;
        public Guid OrderId { get; set; }
    }
}
