namespace Marketplace.Models.DM
{
	public class TemplateDm
	{
		public long Id { get; set; }
		public string? Name { get; set; }
		public string? Metaname { get; set; }
		public DateTime? Removed { get; set; }

		public TemplateDm() { }
	}
}
