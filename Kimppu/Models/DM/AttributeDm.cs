namespace Marketplace.Models.DM
{
	public class AttributeDm
	{
		public long Id { get; set; }
		public int Type { get; set; }
		public string? Name { get; set; }
		public string? Metaname { get; set; }
		public string? Options { get; set; }
		public DateTime? Removed { get; set; }

		public AttributeDm() { }
	}
}
