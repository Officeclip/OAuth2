using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using OfficeClipMS365 = OfficeClip.OpenSource.OAuth2.Lib.Provider.MS365;

namespace OfficeClip.OpenSource.OAuth2.Example.MS365
{
    public partial class Auth : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected string ImageResizedHtml;
        protected void Page_Load(object sender, EventArgs e)
        {
            var element = Utils.LoadConfigurationFromWebConfig("MS365");
            var client = new OfficeClipMS365(
                                element.ClientId,
                                element.ClientSecret,
                                element.Scope,
                                element.RedirectUri,
                                element.TenantId);
            try
            {
                var state = client.GetStateObject(string.Empty).GetValue("mode");
                UserInfo userInfo = null;
                if (state == "Exchange")
                {
                    client.SetExchangeToken();
                    client.HandleAuthorizationCodeResponse();
                   // client.GetAccessTokenFromRefreshToken(); // remove by suggestion from aistudio.google.com
                    litExchangeAccessToken.Text = client.AccessToken;
                    litExchangeRefreshToken.Text = client.RefreshToken;
                    userInfo = client.GetUserInfo();
                    litResponseString.Text = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                }
                if (userInfo == null)
                {
                    throw new Exception("Could not extract UserInfo");
                }

                TestSmtpSettings(client, userInfo);
            }
            catch (Exception ex)
            {
                litError.Text = ex.Message;
            }
            finally
            {
            }
        }

        private static void TestSmtpSettings(OfficeClipMS365 client, UserInfo userInfo)
        {
            var smtpClient = new SmtpClient(new ProtocolLogger(@"c:\temp\smtp.log"));
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MailboxAddress(userInfo.FullName, userInfo.Email));
            message.To.Add(new MailboxAddress("SK Dutta", "skd@officeclip.com"));
            message.Subject = "Test Subject 210010";
            message.Body = new TextPart("plain") { Text = @"Hey" };
            using (smtpClient)
            {
                smtpClient.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                var oauth2 = new SaslMechanismOAuth2(userInfo.Email, client.AccessToken);
                smtpClient.Authenticate(oauth2);

                smtpClient.Send(message);
                smtpClient.Disconnect(true);
            }
        }

        private static string TestIMapSettings(OfficeClipMS365 client, UserInfo userInfo)
        {
            ImapClient imapClient = null;
            var email = userInfo.Email;
            try
            {
                imapClient = new ImapClient(new ProtocolLogger(@"c:\temp\imap.log"));
                imapClient.Connect("outlook.office365.com", 993, SecureSocketOptions.SslOnConnect);
                var oauth2 = new SaslMechanismOAuth2(userInfo.Email, client.AccessToken);
                imapClient.Authenticate(oauth2);
                imapClient.Inbox.Open(FolderAccess.ReadOnly);
                var messages = imapClient.Inbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.InternalDate);
                var returnString = "";
                foreach (var item in messages)
                {
                    returnString += $"{item.UniqueId}: {item.InternalDate} <br/>";
                }
                imapClient.Disconnect(true);
                return returnString;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                if (imapClient != null)
                {
                    imapClient.Disconnect(true);
                    imapClient.Dispose();
                }
            }
        }
        
    }
}