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
using WebCore.ENM;
using WebCore.Model.Enum;
using Helper.Page;

namespace WebCore.Services
{
    public interface IWorkStackService : IEntityService<WorkStack> { }
    public class WorkStackService : EntityService<WorkStack>, IWorkStackService
    {
        public WorkStackService() : base() { }
        public WorkStackService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
    }
}