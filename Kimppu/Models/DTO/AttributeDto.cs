using Marketplace.Models.DM;

namespace Marketplace.Models.DTO
{
	public enum AttributeType
	{
		TEXT = 0,
		INTEGER = 1,
		DECIMAL = 2,
		BOOLEAN = 3,
		DATETIME = 4,
		DROPDOWN = 5,
		MEDIAGALLERY = 6
	}

	public class AttributeDto
	{
		public long Id { get; set; }
		public int Type { get; set; }
		public string? Name { get; set; }
		public string? Metaname { get; set; }
		public string? Options { get; set; }
		public string? Removed { get; set; }
		public bool Selected { get; set; }
		public string? Value { get; set; }
		public List<AttributeDto> ChildAttributes { get; set; } = new List<AttributeDto>();

		public AttributeDto() { }

		public AttributeDto(AttributeDm attributeDM, string? value = null)
		{
			if (attributeDM != null)
			{
				Id = attributeDM.Id;
				Type = attributeDM.Type;
				Name = attributeDM.Name;
				Metaname = attributeDM.Metaname;
				Options = attributeDM.Options;

				switch ((AttributeType)Type)
				{
					case AttributeType.TEXT:
						Value = string.Empty;
						break;

					case AttributeType.INTEGER:
						Value = "0";
						break;

					case AttributeType.DECIMAL:
						Value = "0.0";
						break;

					case AttributeType.BOOLEAN:
						Value = "0";
						break;

					case AttributeType.DATETIME:
						Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
						break;

					case AttributeType.DROPDOWN:
						Value = "[]";
						break;

					case AttributeType.MEDIAGALLERY:
						Value = "[]";
						break;
				}

				if (value != null)
				{
					Value = value;
				}
			}
		}
	}
}
