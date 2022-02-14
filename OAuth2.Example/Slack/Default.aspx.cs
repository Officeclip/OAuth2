using System;
using NLog;
using OfficeClip.OpenSource.OAuth2.Lib;
using OfficeClip.OpenSource.OAuth2.Lib.Provider;

namespace OfficeClip.OpenSource.OAuth2.Example.Slack
{
    public partial class Default : System.Web.UI.Page
    {
        protected string ImageHtml;
        //private static Logger logger =
        //                          LogManager.GetCurrentClassLogger();
        protected void Page_Load(object sender, EventArgs e)
        {
            //logger.Debug("Test");
            var element = Utils.LoadConfigurationFromWebConfig("Slack"); //Test Google
            var client = new OAuth2.Lib.Provider.Slack(
                                element.ClientId, 
                                element.ClientSecret, 
                                element.Scope, 
                                element.RedirectUri);

            try
            {
                State state = new State(string.Empty);
                state.Add("one", "State for one");
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