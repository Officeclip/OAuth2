using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Net;
using OfficeClip.OpenSource.OAuth2.Lib.Provider;
//using OpenNetTools.OAuth2.Services.Google.Contacts;

namespace OfficeClip.OpenSource.OAuth2.Example
{
    public partial class auth : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected string ImageResizedHtml;
        protected void Page_Load(object sender, EventArgs e)
        {
            var element = Utils.LoadConfigurationFromWebConfig("Google");
            var client = new Google(element.ClientId, element.ClientSecret, element.Scope, element.RedirectUri);
            //var element = Utils.LoadConfigurationFromWebConfig("WindowsLive");
            //var client = new WindowsLive(element.ClientId, element.ClientSecret, element.Scope, element.RedirectUri);
            try
            {
                client.HandleAuthorizationCodeResponse();
                litAccessToken.Text = client.AccessToken;
                litState.Text = client.GetStateObject(string.Empty).GetValue("one");
            }
            catch (Exception ex)
            {
                litError.Text = ex.Message;
                return;
            }
            try
            {
                UserInfo userInfo = client.GetUserInfo();
                litFullName.Text = userInfo.FullName;
                litEmail.Text = userInfo.Email;
                ProfilePicture picture = new ProfilePicture(userInfo.PictureUrl, true);
                ImageHtml = picture.HtmlPart;
                picture.Resize(200);
                ImageResizedHtml = picture.HtmlPart;
                //DomainUsers googleDomainUsers = new DomainUsers(client.AccessToken);
                //litDirectoryString.Text = googleDomainUsers.ToJsonString();
            }
            catch (WebException webEx)
            {
                HttpError httpError = new HttpError(webEx.Response);
                litError.Text = httpError.StatusDescription;
            }
            catch (Exception ex)
            {
                litError.Text = ex.Message;
            }

            //CalendarList calendarList = new CalendarList(client.AccessToken);
            //litCalendarString.Text = calendarList.ToJsonString();

            //ContactsGroup contactGroup = new ContactsGroup(client.AccessToken);
            //litContactString.Text = contactGroup.ToJsonString();
        }
    }
}