using Marketplace.Models.DM;
using Marketplace.Models.DTO;
using Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
	[ApiController]
	[AllowAnonymous]
	[Route("[controller]")]
	public class Attribute : ControllerBase
	{
		private AttributeService _attributeService;

		public Attribute(AttributeService attributeService)
		{
			_attributeService = attributeService;
		}

		[HttpGet("allAttributes")]
		public async Task<IActionResult> GetAllAttributes()
		{
			var attributes = await _attributeService.GetAllAttributes();
			return Ok(new ResponseDto() { IsSuccess = true, Model = attributes, Message = $"" });
		}

		[HttpGet("filterModel")]
		public async Task<IActionResult> LoadFilterModel()
		{
			var filterModel = await _attributeService.LoadFilterModel();
			return Ok(new ResponseDto() { IsSuccess = true, Model = filterModel, Message = $"" });
		}

		[Authorize]
		[HttpPost("addEditAttribute")]
		public async Task<IActionResult> AddEditAttribute(AttributeDto attribute)
		{
			try
			{
				await _attributeService.AddEditAttribute(attribute);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"{ex.Message}" });
			}
		}

		[HttpPost("productTemplate")]
		public async Task<IActionResult> LoadProductTemplate([FromBody] long productId)
		{
			var template = await _attributeService.LoadProductTemplate(productId);
			return Ok(new ResponseDto() { IsSuccess = true, Model = template, Message = $"" });
		}

		[Authorize]
		[HttpPost("saveProductTemplate")]
		public async Task<IActionResult> SaveProductTemplate(TemplateDto template)
		{
			await _attributeService.SaveProductTemplate(template);
			return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
		}



		[HttpGet("allTemplates")]
		public async Task<IActionResult> GetAllTemplates()
		{
			var templates = await _attributeService.GetAllTemplates();
			return Ok(new ResponseDto() { IsSuccess = true, Model = templates, Message = $"" });
		}

		[Authorize]
		[HttpPost("addEditTemplate")]
		public async Task<IActionResult> AddEditTemplate(TemplateDto template)
		{
			try
			{
				await _attributeService.AddEditTemplate(template);
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"" });
			}
			catch (Exception ex)
			{
				return Ok(new ResponseDto() { IsSuccess = true, Message = $"{ex.Message}" });
			}
		}
	}
}
