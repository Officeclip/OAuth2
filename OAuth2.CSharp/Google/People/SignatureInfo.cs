﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.CSharp.Google.People
{
    public class SignatureInfo
    {
        public string SId
        {
            get; set;
        }
        public string ETag
        {
            get; set;
        }
        public int ErrorNumber
        {
            get; set;
        } = 0;
        public string ErrorMessage
        {
            get; set;
        }
        public string UpdateTime
        {
            get; set;
        }
    }
}
