using Marketplace.Models.DM;

namespace Marketplace.Models.DTO
{
	public class TemplateDto
	{
		public long Id { get; set; }
		public long ProductId { get; set; }
		
		public string? Name { get; set; }
		public string? Metaname { get; set; }
		public List<AttributeDto>? Attributes { get; set; } = new List<AttributeDto>();
		public string? Removed { get; set; }

		public TemplateDto()
		{
		}

		public TemplateDto(TemplateDm templateDM, IEnumerable<AttributeTemplateDm> allAttributeTemplates = null, IEnumerable<AttributeDm> _allAttributes = null)
		{
			if (templateDM != null)
			{
				Id = templateDM.Id;
				Name = templateDM.Name;
				Metaname = templateDM.Metaname;

				if (allAttributeTemplates != null && _allAttributes != null)
				{
					Attributes = new List<AttributeDto>();
					var attributeIds = allAttributeTemplates.Where(at => at.TemplateId == Id).Select(at => at.AttributeId).ToList();
					foreach (var attributeDm in _allAttributes.Where(a => attributeIds.Contains(a.Id)))
					{
						Attributes.Add(new AttributeDto(attributeDm));
					}
				}
			}
		}
	}
}
