using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using System.Collections.Generic;
using Helper;
using System.Web;
using WebCore.Entities;
using WebCore.Model.Entities;
using Helper.Page;
using Helper.File;

namespace WebCore.Services
{
    public interface IAttachmentSharedService : IEntityService<AttachmentShared> { }
    public class AttachmentSharedService : EntityService<AttachmentShared>, IAttachmentSharedService
    {
        public AttachmentSharedService() : base() { }
        public AttachmentSharedService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
    }
}
