﻿using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Net;
using OfficeClip.OpenSource.OAuth2.Lib.Provider;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MailKit;
using System.Management.Instrumentation;
using devmon_library;
using NLog;
//using OpenNetTools.OAuth2.Services.Google.Contacts;

namespace OfficeClip.OpenSource.OAuth2.Example.Slack
{
    public partial class Auth : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected string ImageResizedHtml;
        //private static Logger logger =
        //                          LogManager.GetCurrentClassLogger();
        protected void Page_Load(object sender, EventArgs e)
        {
            //logger.Debug("Test");
            var element = Utils.LoadConfigurationFromWebConfig("Slack");
            var client = new OAuth2.Lib.Provider.Slack(
                                    element.ClientId,
                                    element.ClientSecret,
                                    element.Scope,
                                    element.RedirectUri);
            try
            {
                client.HandleAuthorizationCodeResponse();
                litAccessToken.Text = client.AccessToken;
                litChannelName.Text = client.GetChannelName();
                litTeamName.Text = client.GetTeamName();
                litWebhook.Text = client.GetWebhookUrl();
            }
            finally
            {

            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            Response.Redirect("http://localhost/authorize/Slack/Notify.aspx?accessToken=" + litAccessToken.Text);
        }
    }
}