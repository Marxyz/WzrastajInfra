namespace Domain.Models
{
    public class Error
    {
        public Error(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}