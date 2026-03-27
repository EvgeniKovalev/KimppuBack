using Dapper;
using Marketplace.Models.DM;
using Marketplace.Models.DTO;
using Marketplace.Repository;
using Marketplace.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace
{
	public enum AccessRightEnum
	{
		PRODUCTS = 1,
		PROPERTIES = 2

		/*
		Product
			- Add
			- Edit
			- Remove
		
		Property
			- Add
			- Edit
			- Remove

		User
			- Add
			- Edit
			- Remove

		 */
	}

	public class AuthService
	{
		private readonly RepositorySettings _repositorySettings;
		private readonly CartService _cartService;
		private readonly CommunicationService _communicationService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="repositorySettings"></param>
		/// <param name="cartService"></param>
		/// <param name="communicationService"></param>
		public AuthService(RepositorySettings repositorySettings, CartService cartService, CommunicationService communicationService)
		{
			_repositorySettings = repositorySettings;
			_cartService = cartService;
			_communicationService = communicationService;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserToken"></param>
		/// <returns></returns>
		internal async Task<string> InitVisitorSession(string currentUserToken)
		{
			var newUserToken = Guid.NewGuid().ToString();
			await _cartService.AssignCartToUser(currentUserToken, newUserToken);

			var tokenHandler = new JwtSecurityTokenHandler();

			var claims = new List<Claim> {
				new (JwtRegisteredClaimNames.Sid, newUserToken),
				new (JwtRegisteredClaimNames.Aud, _repositorySettings.GetAudience()),
				new (JwtRegisteredClaimNames.Iss, _repositorySettings.GetAudienceIssuer())
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddDays(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_repositorySettings.GetJwtKey()), SecurityAlgorithms.HmacSha256)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginModel"></param>
		/// <param name="visitorToken"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		internal async Task<string> Login(LoginDto loginModel, string visitorToken)
		{
			if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password))
			{
				throw new ArgumentNullException("model_error");
			}

			var user = await GetUser(loginModel.Email);
			if (user == null || !loginModel.Password.Equals(user.Password))
			{
				throw new ArgumentNullException("password_mismatch");
			}

			await _cartService.AssignCartToUser(visitorToken, user.Guid);

			var tokenHandler = new JwtSecurityTokenHandler();
			var claims = new List<Claim> {
				new (JwtRegisteredClaimNames.Sid, user.Guid),
				new (JwtRegisteredClaimNames.Sub, user.Guid),
				new (JwtRegisteredClaimNames.Aud, _repositorySettings.GetAudience()),
				new (JwtRegisteredClaimNames.Iss, _repositorySettings.GetAudienceIssuer())
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddDays(5),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_repositorySettings.GetJwtKey()), SecurityAlgorithms.HmacSha256)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginModel"></param>
		/// <returns></returns>
		internal async Task<string> ApplyEmail(LoginDto loginModel)
		{
			var claims = new List<Claim>();
			var user = await GetUser(loginModel.Email);

			if (user != null)
			{
				if (!string.IsNullOrWhiteSpace(user.Password) && string.IsNullOrWhiteSpace(user.Code))
				{
					claims.Add(new(JwtRegisteredClaimNames.Sub, user.Guid));
				}
				else
				{
					await RenewPasscode(user.Id);
					user = await GetUser(loginModel.Email);
					_ = _communicationService.SendEmailCode(loginModel.Email, user.Code);
					claims.Add(new(JwtRegisteredClaimNames.Sub, "swoop_secure_login"));
				}
			}
			else
			{
				user = await EstablishUser(loginModel.Email);

				_ = _communicationService.SendEmailCode(loginModel.Email, user.Code);
				claims.Add(new(JwtRegisteredClaimNames.Sub, "swoop_secure_login"));
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Expires = DateTime.UtcNow.AddMinutes(1),
				Subject = new ClaimsIdentity(claims)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginModel"></param>
		/// <returns></returns>
		internal async Task<string> ApplyCode(LoginDto loginModel)
		{
			if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Code))
			{
				throw new ArgumentException("model_error");
			}

			var user = await GetUser(loginModel.Email);
			if (user == null)
			{
				throw new ArgumentException("user_not_found");
			}

			if (user.Code?.ToLower() != loginModel.Code?.ToLower())
			{
				throw new ArgumentException("code_mismatch");
			}
			await ClearUserCode(user.Id);

			var claims = new List<Claim>();
			claims.Add(new(JwtRegisteredClaimNames.Sub, "swoop_secure_password_renew"));
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Expires = DateTime.UtcNow.AddMinutes(1),
				Subject = new ClaimsIdentity(claims)
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginModel"></param>
		/// <returns></returns>
		internal async Task<string> ForgotPassword(LoginDto loginModel)
		{
			if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email))
			{
				throw new ArgumentException("model_error");
			}

			var user = await GetUser(loginModel.Email);
			if (user == null)
			{
				throw new ArgumentException("user_not_found");
			}

			await RenewPasscode(user.Id);

			var claims = new List<Claim>();
			claims.Add(new(JwtRegisteredClaimNames.Sub, "swoop_secure_password_forgot"));
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Expires = DateTime.UtcNow.AddMinutes(1),
				Subject = new ClaimsIdentity(claims)
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginModel"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		internal async Task ResetPassword(LoginDto loginModel)
		{
			if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password) || !loginModel.Password.Equals(loginModel.PasswordAgain))
			{
				throw new ArgumentException("passwords_mismatch");
			}

			var dbConnection = _repositorySettings.CreateUserConnection();
			await dbConnection.ExecuteAsync("update [user] set password = @newPassword where username = @email", new { newPassword = loginModel.Password, email = loginModel.Email });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		internal async Task<List<AccessRightEnum>> GetUserRights(string? userGuid)
		{
			var accessRights = new List<AccessRightEnum>();
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (!string.IsNullOrWhiteSpace(userGuid))
			{
				var accessRightIds = await dbConnection.QueryAsync<int>("select uar.AccessRightId from UserAccessRight uar inner join [user] u on uar.userId = u.Id where u.Guid = @userGuid", new { userGuid = userGuid });
				if (accessRightIds != null && accessRightIds.Count() > 0)
				{
					foreach (var accessRightId in accessRightIds)
					{
						if (Enum.IsDefined(typeof(AccessRightEnum), accessRightId))
						{
							accessRights.Add(((AccessRightEnum)accessRightId));
						}
					}
				}
			}
			return accessRights;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userGuid"></param>
		/// <param name="rights"></param>
		/// <returns></returns>
		internal async Task<bool> UserHasRights(string? userGuid, List<AccessRightEnum> rightsToCheck)
		{
			if (!string.IsNullOrWhiteSpace(userGuid) && rightsToCheck != null && rightsToCheck.Count > 0)
			{
				var userRights = await GetUserRights(userGuid);
				foreach (var right in rightsToCheck)
				{
					if (!userRights.Contains(right))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}








		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <param name="guid"></param>
		/// <returns></returns>
		private async Task<UserDm?> GetUser(string? email = null, string? guid = null)
		{
			UserDm? user = null;
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (!string.IsNullOrWhiteSpace(email))
			{
				user = await dbConnection.QueryFirstOrDefaultAsync<UserDm?>("select * from [user] where username = @email and removed is null", new { email = email });
			}

			if (!string.IsNullOrWhiteSpace(guid))
			{
				user = await dbConnection.QueryFirstOrDefaultAsync<UserDm?>("select * from [user] where guid = @guid and removed is null", new { guid = guid });
			}

			return user;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private async Task<UserDm?> GetUser(int id)
		{
			UserDm? user = null;
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (id > 0)
			{
				user = await dbConnection.QueryFirstOrDefaultAsync<UserDm?>("select * from [user] where id = @id and removed is null", new { id = id });
			}
			return user;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		private async Task<UserDm?> EstablishUser(string? email)
		{
			UserDm? user = null;
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (!string.IsNullOrWhiteSpace(email))
			{
				var newUserParams = new { guid = $"usr-{Guid.NewGuid().ToString()}", username = email, created = DateTime.UtcNow, code = Utils.GenerateRandomCode() };
				var userId = await dbConnection.QueryFirstOrDefaultAsync<int>("insert into [user] (guid, username, created, code) output Inserted.ID values (@guid, @username, @created, @code)", newUserParams);
				user = await GetUser(userId);
			}
			return user;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task RenewPasscode(long userId)
		{
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (userId > 0)
			{
				await dbConnection.ExecuteAsync("update [user] set password = '', code = @newCode where id = @id", new { newCode = Utils.GenerateRandomCode(), id = userId });
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task ClearUserCode(long userId)
		{
			var dbConnection = _repositorySettings.CreateUserConnection();
			if (userId > 0)
			{
				await dbConnection.QueryFirstOrDefaultAsync<UserDm>("update [user] set code = '' where id = @id", new { id = userId });
			}
		}




	}
}
