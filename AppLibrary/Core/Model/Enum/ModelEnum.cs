﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCore.Model.Enum
{
    public class ModelEnum
    {
        public enum FileType
        {
            NONE = 0,
            ALONE = 1,
            MULTI = 2
        }

        public enum State
        {
            NONE = -1,
            ENABLED = 1,
            DISABLE = 0
        }

        public enum DateExtension
        {
            NONE = 0,
            DASH = 1,
            SLASH = 2
        }
    }
}
