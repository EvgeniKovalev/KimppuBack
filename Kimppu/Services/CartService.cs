using Marketplace.Repository;
using Dapper;
using Marketplace.Models.DATA;
using Marketplace.Models.DM;
using Marketplace.Models.DTO;
using System.Data;

namespace Marketplace.Services
{
	public class CartService
	{
		private readonly RepositorySettings _repositorySettings;
		private readonly CommunicationService _communicationService;

		public CartService(RepositorySettings repositorySettings, CommunicationService communicationService)
		{
			_repositorySettings = repositorySettings;
			_communicationService = communicationService;
		}

		internal async Task<CartDto> LoadCart(string visitorToken)
		{
			var cart = new CartDto();
			var dbConnection = _repositorySettings.CreateCartConnection();
			var query = @$"select p.Id, p.Name, p.SeoLink, p.Description, p.CoverImages,p.Price, cp.Amount from CartProduct cp 
				inner join Cart c on c.Id = cp.CartId
				inner join Product p on p.Id = cp.ProductId 
				where c.VisitorToken = '{visitorToken}' and p.Removed is null";

			var cartProducts = await dbConnection.QueryAsync<ProductDm>(query);
			if (cartProducts != null)
			{
				cart.AdoptProducts(cartProducts);
			}
			return cart;
		}

		internal async Task<int> getProductCount(string visitorToken)
		{
			int amount = 0;
			var cartEntity = await GetCart(visitorToken);
			if (cartEntity != null)
			{
				amount = cartEntity.ProductCount;
			}
			return amount;
		}

		internal async Task<int> AddProduct(string visitorToken, long productId, int amount)
		{
			var dbConnection = _repositorySettings.CreateCartConnection();
			var cartEntity = await GetCart(visitorToken, dbConnection);

			if (cartEntity == null)
			{
				var newCartParams = new { visitorToken = visitorToken, created = DateTime.UtcNow };
				await dbConnection.ExecuteAsync("insert into Cart (VisitorToken, Created) values (@visitorToken, @created)", newCartParams);
				cartEntity = await GetCart(visitorToken, dbConnection);
			}

			var parameters = new { cartId = cartEntity.Id, productId = productId, amount = amount };
			var insertQuery = @"
				if not exists (select * from CartProduct WHERE cartId = @cartId and productId = @productId)
					insert into CartProduct (cartId, productId, amount) values (@cartId, @productId, @amount)	
				ELSE
					update CartProduct set Amount = Amount + @amount where cartId = @cartId and productId = @productId";

			await dbConnection.ExecuteAsync(insertQuery, parameters);
			cartEntity = await GetCart(visitorToken, dbConnection);
			return cartEntity.ProductCount;
		}

		internal async Task<int> RemoveProduct(string visitorToken, long productId, int amount)
		{
			var dbConnection = _repositorySettings.CreateCartConnection();
			var cartEntity = await GetCart(visitorToken, dbConnection);

			return cartEntity.ProductCount;
		}

		internal async Task AssignCartToUser(string previousUserGuid, string? newUserGuid)
		{
			if (!string.IsNullOrWhiteSpace(newUserGuid))
			{
				var dbConnection = _repositorySettings.CreateCartConnection();
				var cartEntity = await GetCart(previousUserGuid, dbConnection);

				if (cartEntity != null)
				{
					await dbConnection.ExecuteAsync($"update Cart set visitorToken = '{newUserGuid}' where visitorToken = '{previousUserGuid}'");
				}
			}
		}


		private async Task<CartDm?> GetCart(string visitorToken, IDbConnection dbConnection = null)
		{
			if (dbConnection == null)
			{
				dbConnection = _repositorySettings.CreateCartConnection();
			}

			var query = $"select c.id, visitorToken, (select sum(amount) from cartproduct where CartId = c.id) as ProductCount from Cart c where visitorToken = '{visitorToken}'";
			var cart = await dbConnection.QueryFirstOrDefaultAsync<CartDm>(query);
			return cart;
		}
	}
}
