﻿using Newtonsoft.Json;
using OfficeClip.OpenSource.OAuth2.Lib;
using System;
//using OfficeClip.OpenSource.OAuth2.CSharp.Google.People;
using System.Collections.Generic;
using System.Net;
using OfficeClipMS365 = OfficeClip.OpenSource.OAuth2.Lib.Provider.MS365;
//using OpenNetTools.OAuth2.Services.Google.Contacts;

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
            //var client = new SmtpClient(new ProtocolLogger(@"c:\temp\smtp.log"));
            try
            {
                var state = client.GetStateObject(string.Empty).GetValue("mode");

                if (state == "Exchange")
                {
                    client.SetExchangeToken();
                    client.HandleAuthorizationCodeResponse();
                    client.GetAccessTokenFromRefreshToken();
                    litExchangeAccessToken.Text = client.AccessToken;
                    litExchangeRefreshToken.Text = client.RefreshToken;
                    var userInfo = client.GetUserInfo();
                    litResponseString.Text = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
                }


                //litExchangeAccessToken.Text = client1.AccessToken;
                //litExchangeRefreshToken.Text = client1.RefreshToken;

                //var response = client1.GetSharedSecretAccessToken();
                //var output = response.ResponseString;
                //---------------------------
                //client.SetGraphToken();
                //client.HandleAuthorizationCodeResponse();
                //litGraphAccessToken.Text = client.AccessToken;
                //litGraphRefreshToken.Text = client.RefreshToken;
                //litClientId.Text = element.ClientId;
                //litState.Text = client.GetStateObject(string.Empty).GetValue("one");

                //var UserInfo = client.GetUserInfo();


                //var message = new MimeKit.MimeMessage();
                //message.From.Add(new MailboxAddress("SK Dutta", "xxx@xxx.com"));
                //message.To.Add(new MailboxAddress("SK Dutta", "yyy@yyy.com"));
                //message.To.Add(new MailboxAddress("Kim Jung", "zzz@zzz.com"));
                //message.Subject = "Test Subject 210010";
                //message.Body = new TextPart("plain") { Text = @"Hey" };
                //using (client1)
                //{
                //    client1.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                //    var oauth2 = new SaslMechanismOAuth2("xxx@xxx.com", client.AccessToken);
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
                //var peopleContact = new Contact(
                //                            element.ClientId,
                //                            element.ClientSecret,
                //                            element.Scope.Split(" ".ToCharArray()),
                //                            client.AccessToken,
                //                            string.Empty
                //                            );
                //var contactGroups = peopleContact.ContactGroups;

                //var contact = peopleContact.GetContact(
                //                                ConfigurationManager.AppSettings["Contact"]);
                //litFullName.Text = ObjectDumper.Dump(contact);

                //List<ContactInfo> contactInfoList = new List<ContactInfo>();

                List<string> contactInfoList = new List<string>();
                //contactInfoList.Add("people/xxx");
                //contactInfoList.Add("people/yyy");
                //contactInfoList.Add("people/zzz");
                //contactInfoList.Add("people/ttt");

                //var isCreateContact = peopleContact.Delete(contactInfoList);

                //var isCreateContact = peopleContact.Insert(ConfigurationManager.AppSettings["ContactGroup"]);
                //if (isCreateContact == true)
                //{
                //    litFullName.Text = ObjectDumper.Dump(isCreateContact);
                //}

                //var createContact = peopleContact.ContactsBatchHardCoded();

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