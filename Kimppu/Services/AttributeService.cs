using Dapper;
using Marketplace.Models.DM;
using Marketplace.Models.DTO;
using Marketplace.Repository;
using System.Xml.Serialization;

namespace Marketplace.Services
{
	public class AttributeService
	{
		private readonly RepositorySettings _repositorySettings;
		private readonly AuthService _authService;

		private int _cacheIntervalSec = 60;
		private long _lastUpdateTicks;
		private IEnumerable<AttributeDm> _allAttributes;
		private IEnumerable<TemplateDm> _allTemplates;
		private IEnumerable<AttributeTemplateDm> _allAttributeTemplates;

		public AttributeService(RepositorySettings repositorySettings, AuthService authService)
		{
			_repositorySettings = repositorySettings;
			_authService = authService;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="force"></param>
		/// <returns></returns>
		private async Task CheckCache(bool force = false)
		{
			if (DateTime.UtcNow.AddSeconds(-_cacheIntervalSec).Ticks > _lastUpdateTicks || force)
			{
				var templatetConnection = _repositorySettings.CreateTemplateConnection();
				_allAttributes = await templatetConnection.QueryAsync<AttributeDm>("Select * from attribute where removed is null");
				_allTemplates = await templatetConnection.QueryAsync<TemplateDm>("Select * from template where removed is null");
				_allAttributeTemplates = await templatetConnection.QueryAsync<AttributeTemplateDm>("Select * from templateAttribute");

				_lastUpdateTicks = DateTime.UtcNow.Ticks;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		internal async Task<FilterDto> LoadFilterModel()
		{
			await CheckCache();
			var filterModel = new FilterDto();

			var designCollectionAttribute = _allAttributes.FirstOrDefault(a => a.Metaname == "design-collection");

			if (designCollectionAttribute != null)
			{
				filterModel.Attributes = new List<AttributeDto> {new AttributeDto(designCollectionAttribute) };			
			}
			return filterModel;			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal async Task<List<AttributeDto>> GetAllAttributes()
		{
			await CheckCache();
			var attributes = new List<AttributeDto>();
			foreach (var attribute in _allAttributes) attributes.Add(new AttributeDto(attribute));
			return attributes;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal async Task<List<TemplateDto>> GetAllTemplates()
		{
			await CheckCache();
			var templates = new List<TemplateDto>();
			foreach (var template in _allTemplates) templates.Add(new TemplateDto(template, _allAttributeTemplates, _allAttributes));
			return templates;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		internal async Task AddEditTemplate(TemplateDto template)
		{
			if (template == null)
			{
				throw new ArgumentException("Template is nothing");
			}

			var templatetConnection = _repositorySettings.CreateTemplateConnection();
			var templateParams = new
			{
				id = template.Id,
				name = template.Name,
				metaName = template.Metaname,
				removed = template.Removed
			};

			if (template.Id > 0)
			{
				await templatetConnection.ExecuteAsync(@"update [template] set name=@name, metaname=@metaName, removed = @removed where id = @id", templateParams);
			}
			else
			{
				var insertedId = await templatetConnection.QueryFirstAsync<long>(@"insert into [template] (name, metaname) values (@name, @metaName); select SCOPE_IDENTITY()", templateParams);
				templateParams = new
				{
					id = insertedId,
					name = template.Name,
					metaName = template.Metaname,
					removed = template.Removed
				};
			}

			if (template.Attributes != null && template.Attributes.Count > 0)
			{
				var insertList = new List<object>();
				foreach (var attribute in template.Attributes)
				{
					insertList.Add(new
					{
						templateId = templateParams.id,
						attributeId = attribute.Id
					});
				}
				await templatetConnection.ExecuteAsync(@"delete templateAttribute where templateId = @id", templateParams);
				await templatetConnection.ExecuteAsync(@"insert into templateAttribute (templateId, attributeId) values (@templateId, @attributeId)", insertList);
			}
			else
			{
				await templatetConnection.ExecuteAsync(@"delete templateAttribute where templateId = @id", templateParams);
			}

			await CheckCache(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		internal async Task<TemplateDto> LoadProductTemplate(long productId)
		{
			var template = new TemplateDto();
			if (productId == 0)
			{
				throw new ArgumentException("Product id is nothing");
			}

			var templatetConnection = _repositorySettings.CreateTemplateConnection();
			var productAttributes = await templatetConnection.QueryAsync<ProductAttributeDm>(@"select * from productAttribute where productId = @id", new { id = productId });

			if (productAttributes != null && productAttributes.Count() > 0)
			{
				var templateDm = _allTemplates.FirstOrDefault(t => t.Id == productAttributes.FirstOrDefault()?.TemplateId);
				if (templateDm != null)
				{
					template = new TemplateDto(templateDm);
					foreach (var productAttribute in productAttributes)
					{
						var attributeDm = _allAttributes.FirstOrDefault(a => a.Id == productAttribute.AttributeId);
						if (attributeDm != null)
						{
							template.Attributes?.Add(new AttributeDto(attributeDm, productAttribute.Value));
						}
					}
				}
			}
			return template;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		internal async Task SaveProductTemplate(TemplateDto template)
		{
			if (template == null || template.Id == 0 || template.ProductId == 0)
			{
				throw new ArgumentException("Template has errors");
			}

			var templatetConnection = _repositorySettings.CreateTemplateConnection();
			var templateExists = _allTemplates.Where(t => t.Id == template.Id).Any();

			if (templateExists)
			{
				await templatetConnection.ExecuteAsync(@"delete productAttribute where productId = @productId", new { productId = template.ProductId });

				if (template.Attributes != null && template.Attributes.Count > 0)
				{
					var insertList = new List<object>();
					foreach (var attribute in template.Attributes)
					{
						insertList.Add(new
						{
							productId = template.ProductId,
							templateId = template.Id,
							attributeId = attribute.Id,
							value = attribute.Value
						});
					}
					await templatetConnection.ExecuteAsync(@"insert into productAttribute (productId, templateId, attributeId, [value]) values (@productId, @templateId, @attributeId, @value)", insertList);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		internal async Task AddEditAttribute(AttributeDto attribute)
		{
			if (attribute == null)
			{
				throw new ArgumentException("Attribute is nothing");
			}

			var templatetConnection = _repositorySettings.CreateTemplateConnection();
			var attributeParams = new
			{
				id = attribute.Id,
				type = attribute.Type,
				name = attribute.Name,
				metaName = attribute.Metaname,
				options = attribute.Options,
				removed = attribute.Removed
			};

			if (attribute.Id > 0)
			{
				await templatetConnection.ExecuteAsync(@"update [attribute] set [type]=@type, [name]=@name, metaname=@metaName, options=@options, removed = @removed where id = @id", attributeParams);
			}
			else
			{
				await templatetConnection.ExecuteAsync(@"insert into [attribute] ([type], [name], metaName, options) values (@type, @name, @metaName, @options)", attributeParams);
			}
			await CheckCache(true);
		}


	}
}
