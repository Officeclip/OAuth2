using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Net;
using OfficeClip.OpenSource.OAuth2.Lib.Provider;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MailKit;
using OfficeClip.OpenSource.OAuth2.Services.Google.People;
using OfficeClip.OpenSource.OAuth2.CSharp.Google.People;
using System.Configuration;
using devmon_library;
using System.Collections.Generic;
//using OpenNetTools.OAuth2.Services.Google.Contacts;

namespace OfficeClip.OpenSource.OAuth2.Example
{
    public partial class Auth : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected string ImageResizedHtml;
        protected void Page_Load(object sender, EventArgs e)
        {
            var element = Utils.LoadConfigurationFromWebConfig("Google");
            var client = new Google(
                                element.ClientId, 
                                element.ClientSecret, 
                                element.Scope, 
                                element.RedirectUri);
            //var element = Utils.LoadConfigurationFromWebConfig("WindowsLive");
            //var client = new WindowsLive(element.ClientId, element.ClientSecret, element.Scope, element.RedirectUri);
            //var client1 = new SmtpClient(new ProtocolLogger(@"c:\temp\smtp.log"));
            try
            {
                client.HandleAuthorizationCodeResponse();
                litAccessToken.Text = client.AccessToken;
                litRefreshToken.Text = client.RefreshToken;
                litState.Text = client.GetStateObject(string.Empty).GetValue("one");
                //var message = new MimeKit.MimeMessage();
                //message.From.Add(new MailboxAddress("SK Dutta", "skd@officeclip.com"));
                ////message.To.Add(new MailboxAddress("SK Dutta", "skdutta@gmail.com"));
                //message.To.Add(new MailboxAddress("Kim Jung", "kimjung@koreanmail.com"));
                //message.Subject = "Test Subject 210010";
                //message.Body = new TextPart("plain") { Text = @"Hey" };
                //using (client1)
                //{
                //    client1.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                //    var oauth2 = new SaslMechanismOAuth2("skd@officeclip.com", client.AccessToken);
                //    client1.Authenticate(oauth2);

                //    client1.Send(message);
                //    client1.Disconnect(true);
                //}
            }
            catch (Exception ex)
            {
                litError.Text = ex.Message;
                //litError.Text = client1.ProtocolLogger
                //return;
            }
            finally
            {
            }
            try
            {
                var peopleContact = new Contact(
                                            element.ClientId,
                                            element.ClientSecret,
                                            element.Scope.Split(" ".ToCharArray()),
                                            client.AccessToken,
                                            string.Empty
                                            );
                //var contactGroups = peopleContact.ContactGroups;

                //var contact = peopleContact.GetContact(
                //                                ConfigurationManager.AppSettings["Contact"]);
                //litFullName.Text = ObjectDumper.Dump(contact);

                //List<ContactInfo> contactInfoList = new List<ContactInfo>();

                List<string> contactInfoList = new List<string>();
                //contactInfoList.Add("people/c304565144089597399");
                //contactInfoList.Add("people/c4635318391136207354");
                //contactInfoList.Add("people/c4676380728778291940");
                //contactInfoList.Add("people/c9202708945039554846");

                //var isCreateContact = peopleContact.Delete(contactInfoList);

                var isCreateContact = peopleContact.Insert(ConfigurationManager.AppSettings["ContactGroup"]);
                if (isCreateContact == true)
                {
                    litFullName.Text = ObjectDumper.Dump(isCreateContact);
                }

                //bool isUpdateContact = peopleContact.Update(contact);
                //if (isUpdateContact == true)
                //{
                //    litFullName.Text = ObjectDumper.Dump(contact);
                //}

                //var groups = string.Join(",", peopleContact.ContactGroups);
                //litFullName.Text = ObjectDumper.Dump(contactGroups);

                //var contactList = string.Join(",", peopleContact.ContactList);
                //litFullName.Text = contactList;
                //peopleContact.CreateContact();
                //peopleContact.UpdateContact(
                //                    ConfigurationManager.AppSettings["Contact"]);
                //ContactsGroup contacts = new ContactsGroup(client.AccessToken);
                //UserInfo userInfo = client.GetUserInfo();
                //litFullName.Text = userInfo.FullName;
                //litEmail.Text = userInfo.Email;
                //ProfilePicture picture = new ProfilePicture(userInfo.PictureUrl, true);
                //ImageHtml = picture.HtmlPart;
                //picture.Resize(200);
                //ImageResizedHtml = picture.HtmlPart;
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