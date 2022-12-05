using System.Text.Json.Serialization;

namespace CSVToJson.Models
{

	public class Contact
	{
		public string? ReferenceId { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Type? Type { get; set; }
		public List<Message> Messages { get; set; } = new();
	}
}