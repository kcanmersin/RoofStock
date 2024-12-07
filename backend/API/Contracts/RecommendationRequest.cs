using System.ComponentModel;

namespace API.Contracts
{
    public class RecommendationRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid UserId { get; set; }
    }
}
