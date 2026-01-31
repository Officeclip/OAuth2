using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using OfficeClipMS365 = OfficeClip.OpenSource.OAuth2.Lib.Provider.MS365;

namespace OfficeClip.OpenSource.OAuth2.Example
{
    public partial class _default : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected void Page_Load(object sender, EventArgs e)
        {
            var element = Utils.LoadConfigurationFromWebConfig("MS365"); //Test Google
            var client = new OfficeClipMS365(
                                element.ClientId,
                                element.ClientSecret,
                                element.Scope,
                                element.RedirectUri,
                                element.TenantId);
            //client.ForceRefreshToken = true;
            //var element = Utils.LoadConfigurationFromWebConfig("WindowsLive"); // Test Live
            //var client = new WindowsLive(element.ClientId, element.ClientSecret, element.Scope, element.RedirectUri);
            //client.ForceRefreshToken = true;
            try
            {
                var mode =
                        string.IsNullOrEmpty(Request.QueryString["mode"])
                        ? "Exchange"
                        : Request.QueryString["mode"];
                State state = new State(string.Empty);
                state.Add("mode", mode);
                var nonceString = DateTime.Now.Ticks.ToString();
                Session["nonceString"] = nonceString;
                state.Add("nonce", nonceString);
                client.Authenticate(state);
            }
            catch (Exception ex)
            {
                litError.Text = ex.Message;
                return;
            }
            //try
            //{
            //    UserInfo userInfo = client.GetUserInfo();
            //    litFullName.Text = userInfo.FullName;
            //    ProfilePicture picture = new ProfilePicture(userInfo.PictureUrl, true);
            //    ImageHtml = picture.HtmlPart;
            //}
            //catch (Exception ex)
            //{
            //    litError.Text = ex.Message;
            //}
            //DomainUsers googleDomainUsers = new DomainUsers(client.AccessToken);
            //litDirectoryString.Text = googleDomainUsers.ToJsonString();

            //state.Add("two", "http://www.yahoo.com");
            //string stateString = state.ToString();
            //State state1 = new State(stateString);
            //string value = state1.GetValue("one");
        }
    }
}