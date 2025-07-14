using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Page")]
    public class PageController : ApiController
    {

        [HttpPost]
        [Route("PageList")]
        public ExpandoObject PageList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Page model = JsonConvert.DeserializeObject<Page>(decryptData);

                var list = (from d1 in dbContext.Pages
                            join d2 in dbContext.PageGroups on d1.PageGroupId equals d2.PageGroupId
                            where (model.PageId == d1.PageId || model.PageId == 0)
                           && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.PageName
                            select new
                            {
                                d1.PageId,
                                d1.PageGroupId,
                                d2.PageGroupName,
                                d1.PageName,
                                d1.PageUrl,
                                d1.Status,
                            }).ToList();

                response.PageList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("savePage")]
        public ExpandoObject SavePage(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Page model = JsonConvert.DeserializeObject<Page>(decryptData);

                Page Page = null;
                if (model.PageId > 0)
                {
                    Page = dbContext.Pages.Where(x => x.PageId == model.PageId).First();
                    Page.UpdatedBy = model.UpdatedBy;
                    Page.PageGroupId = model.PageGroupId;
                    Page.PageName = model.PageName;
                    Page.PageUrl = model.PageUrl;
                    Page.Status = model.Status;
                }
                else
                    Page = model;
                Page.UpdatedDate = DateTime.Now;

                if (Page.PageId == 0)
                    dbContext.Pages.Add(Page);
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
        [Route("deletePage")]
        public ExpandoObject DeletePage(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Page model = JsonConvert.DeserializeObject<Page>(decryptData);
                var Page = dbContext.Pages.Where(x => x.PageId == model.PageId).First();
                dbContext.Pages.Remove(Page);
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
