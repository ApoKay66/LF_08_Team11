using BrainBusters.Models;

namespace BrainBuster.Web.Models
{
    public class TurnViewModel
    {
        public string? PlayerName { get; set; }
        public Question? Question { get; set; }
        public List<Answer>? ShuffledAnswers { get; set; }
    }
}
