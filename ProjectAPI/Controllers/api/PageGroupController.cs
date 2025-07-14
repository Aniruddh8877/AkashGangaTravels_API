using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/PageGroup")]
    public class PageGroupController : ApiController
    {

        [HttpPost]
        [Route("PageGroupList")]
        public ExpandoObject PageGroupList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                PageGroup model = JsonConvert.DeserializeObject<PageGroup>(decryptData);

                var list = (from d1 in dbContext.PageGroups
                            where (model.PageGroupId == d1.PageGroupId || model.PageGroupId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.PageGroupName
                            select new
                            {
                                d1.PageGroupId,
                                d1.PageGroupName,
                                d1.Status,
                            }).ToList();

                response.PageGroupList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("savePageGroup")]
        public ExpandoObject SavePageGroup(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                PageGroup model = JsonConvert.DeserializeObject<PageGroup>(decryptData);

                PageGroup PageGroup = null;
                if (model.PageGroupId > 0)
                {
                    PageGroup = dbContext.PageGroups.Where(x => x.PageGroupId == model.PageGroupId).First();
                    PageGroup.UpdatedBy = model.UpdatedBy;
                    PageGroup.PageGroupName = model.PageGroupName;
                    PageGroup.Status = model.Status;
                }
                else
                    PageGroup = model;
                PageGroup.UpdatedDate = DateTime.Now;

                if (PageGroup.PageGroupId == 0)
                    dbContext.PageGroups.Add(PageGroup);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IX"))
                    response.Message = "This record is already exist";
                else
                    response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("deletePageGroup")]
        public ExpandoObject DeletePageGroup(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                PageGroup model = JsonConvert.DeserializeObject<PageGroup>(decryptData);
                var PageGroup = dbContext.PageGroups.Where(x => x.PageGroupId == model.PageGroupId).First();
                dbContext.PageGroups.Remove(PageGroup);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    response.Message = "This record is in use.so can't delete.";
                else
                    response.Message = ex.Message;
            }
            return response;
        }
    }
}
