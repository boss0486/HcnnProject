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
using System.Data.Common;
using System.Data;

namespace WebCore.Services
{
    public interface IWorkNotifyService : IEntityService<WorkNotify> { }
    public class WorkNotifyService : EntityService<WorkNotify>, IWorkNotifyService
    {
        public WorkNotifyService() : base() { }
        public WorkNotifyService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################

        public void AddNotify(WorkNotify notify, IDbTransaction _transaction, IDbConnection connection)
        {
            WorkNotifyService notifyService = new WorkNotifyService(connection);
            notifyService.Create(notify, _transaction);
        }

        public void UpdateNotify(string notifyId, IDbTransaction _transaction, IDbConnection connection)
        {
            WorkNotifyService notifyService = new WorkNotifyService(connection);
            var notifyObj = notifyService.GetAlls(x => x.ID == notifyId).FirstOrDefault();
            notifyObj.IsShow = false;
        }
    }
}