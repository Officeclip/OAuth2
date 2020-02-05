using System;
using OfficeClip.OpenSource.OAuth2.Lib;

namespace OfficeClip.OpenSource.Example
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ProfilePicture profilePicture = new ProfilePicture("<Picture Url>", false);
        }
    }

}