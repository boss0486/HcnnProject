using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebCore.Entities;
using WebCore.Model.Enum;

namespace WebCore.Services
{
    public interface IAttachmentIngredientService : IEntityService<AttachmentIngredient> { }
    public class AttachmentIngredientService : EntityService<AttachmentIngredient>, IAttachmentIngredientService
    {
        public AttachmentIngredientService() : base() { }
        public AttachmentIngredientService(System.Data.IDbConnection db) : base(db) { }

        public bool UpdateFile(int typeId, string id, IEnumerable<string> fileModels = null, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            if (fileModels == null)
                return false;
            // 
            if (connection == null)
                connection = DbConnect.Connection.CMS;
            // 
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(connection);
            // lay tat ca file dinh kem of id
            string query = "SELECT * FROM AttachmentIngredient WHERE ForID = @ForID AND TypeID = @TypeID ";
            List<string> attachmentDb = attachmentIngredientService.Query<AttachmentIngredient>(query, new { ForID = id, TypeID = typeId }, transaction: transaction).Select(m => m.FileID).ToList();
            //
            if (attachmentDb.Count() > 0)
            {
                List<string> fileNews = fileModels.Except<string>(attachmentDb).ToList();
                List<string> fileDeletes = attachmentDb.Except<string>(fileModels).ToList();
                // add   
                foreach (var item in fileNews)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        AttachmentIngredient attachmentIngredient = attachmentIngredientService.GetAlls(m => m.FileID == item && m.TypeID == typeId && m.ForID == id, transaction: transaction).FirstOrDefault();
                        if (attachmentIngredient == null)
                        {
                            string guid = attachmentIngredientService.Create<string>(new Entities.AttachmentIngredient()
                            {
                                ForID = id,
                                FileID = item,
                                CategoryID = null,
                                TypeID = typeId
                            }, transaction: transaction);
                        }
                    }  
                }
                // delete  
                attachmentIngredientService.Execute("DELETE AttachmentIngredient WHERE FileID IN ('" + String.Join("','", fileDeletes) + "')", transaction: transaction);
            }
            else
            {
                // add 
                foreach (var item in fileModels)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        AttachmentIngredient attachmentIngredient = attachmentIngredientService.GetAlls(m => m.FileID == item && m.TypeID == typeId && m.ForID == id, transaction: transaction).FirstOrDefault();
                        if (attachmentIngredient == null)
                        {
                            string guid = attachmentIngredientService.Create<string>(new Entities.AttachmentIngredient()
                            {
                                ForID = id,
                                FileID = item,
                                CategoryID = null,
                                TypeID = typeId
                            }, transaction: transaction);
                        }
                    }
                }
            }
            // 
            return true;
        }
        public bool RemoveAllFileByForID(string id, IEnumerable<string> fileModels = null, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            if (fileModels == null)
                return false;
            // 
            if (connection == null)
                connection = DbConnect.Connection.CMS;
            // 
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(connection);
            attachmentIngredientService.Execute($"DELETE AttachmentIngredient WHERE ForID = @ForID", new { ForID = id }, transaction: transaction);
            return true;
        } 
        public static List<ViewAttachment> GetFileByForID(int typeId, string forId, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            if (string.IsNullOrWhiteSpace(forId))
                return null;
            //
            if (connection == null)
                connection = DbConnect.Connection.CMS;
            // 
            string query = string.Empty;
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT a.*, ai.TypeID FROM Attachment as a
                INNER JOIN AttachmentIngredient as ai ON ai.FileID = a.ID
                WHERE ai.ForID = @ForID AND ai.TypeID = @TypeID";
          //  ProductService productService = new ProductService(connection);
   
            
            return null;
        }
    }
}
