using System;
using OfficeClip.OpenSource.OAuth2.Services.Slack;

namespace OfficeClip.OpenSource.OAuth2.Example.Slack
{
    public partial class Notify : System.Web.UI.Page
    {
        protected string ImageHtml;
        protected string ImageResizedHtml;
        protected void Page_Load(object sender, EventArgs e)
        {
            var accessToken = Request.QueryString["accessToken"];
            var channelListService = new ChannelList();
            var channelList =  channelListService.GetChannelList(accessToken);
            if (!IsPostBack)
            {
                ddlChannels.Items.Clear();
                foreach (Channel channel in channelList)
                {
                    ddlChannels.Items.Add(channel.Name);
                }
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            var accessToken = Request.QueryString["accessToken"];
            Messaging messaging = new Messaging();
            messaging.Post(accessToken, ddlChannels.SelectedValue, txtMessage.Text);
        }
    }
}