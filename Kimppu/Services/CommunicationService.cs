using Marketplace.Repository;

namespace Marketplace.Services
{
	public class CommunicationService
	{
		private readonly RepositorySettings _repositorySettings;

		public CommunicationService(RepositorySettings repositorySettings)
		{
			_repositorySettings = repositorySettings;
		}

		internal async Task SendEmailCode(string email, string code)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(code))
				{
					var html = $@"
            <div style=\""margin:0;padding:0; font-family:'Google Sans',Roboto,RobotoDraft,Helvetica,Arial,sans-serif;border-bottom:thin solid #dadce0;color:rgba(0,0,0,0.87);line-height:32px;padding-bottom:24px;text-align:center;word-break:break-word\""
                bgcolor='#FFFFFF'>
                <table width='100%' height='100%' style='min-width:348px' border='0' cellspacing='0' cellpadding='0' lang='en'>
                    <tbody>
                        <tr height='32' style='height:32px'>
                            <td></td>
                        </tr>
                        <tr align='center'>
                            <td>
                                <div>
                                    <div></div>
                                </div>
                                <table border='0' cellspacing='0' cellpadding='0'
                                    style='padding-bottom:20px;max-width:516px;min-width:220px'>
                                    <tbody>
                                        <tr>
                                            <td width='8' style='width:8px'></td>
                                            <td>
                                                <div style='border-style:solid;border-width:thin;border-color:#dadce0;border-radius:8px;padding:40px 20px'
                                                    align='center'>
                                                    <h1>Kenobo</h1>
                                                    <div
                                                        style='border-bottom:thin solid #dadce0;color:rgba(0,0,0,0.87);line-height:32px;padding-bottom:24px;text-align:center;word-break:break-word'>
                                                        <div style='font-size:24px'>Verify it's You</div>
                                                    </div>
                                                    <div
                                                        style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;font-size:14px;color:rgba(0,0,0,0.87);line-height:20px;padding-top:20px;text-align:left'>
                                                        Here is Your verification code to proceed with signing in<br>
                                                        <div
                                                            style='text-align:center;font-size:36px;margin-top:20px;line-height:44px'>{code}</div>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
						";
					await _repositorySettings.SendEmail("Verification code", email, $"{code} is your Kenobo verification code", string.Empty, html);
				}
			}
			catch (Exception ex)
			{
				int k = 0;
			}
		}
	}
}