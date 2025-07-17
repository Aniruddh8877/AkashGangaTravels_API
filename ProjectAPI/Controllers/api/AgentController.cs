using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Agent")]
    public class AgentController : ApiController
    {
        [HttpPost]
        [Route("saveAgent")]
        public ExpandoObject SaveAgent(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Agent model = JsonConvert.DeserializeObject<Agent>(decryptData);

                    Agent agent;
                    if (model.AgentId > 0)
                    {
                        agent = db.Agents.FirstOrDefault(x => x.AgentId == model.AgentId);
                        if (agent == null)
                        {
                            res.Message = "Agent not found.";
                            return res;
                        }
                        agent.ContactPersonName = model.ContactPersonName;
                        agent.AgentCompanyName = model.AgentCompanyName;
                        agent.MobileNo = model.MobileNo;
                        agent.Email = model.Email;
                        agent.Status = model.Status;
                    }
                    else
                    {
                        agent = model;
                        agent.AgentCode = AppData.GenterateAgentCode(db); // Custom code generation
                        db.Agents.Add(model);
                    }

                    db.SaveChanges();
                    res.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("AgentList")]
        public ExpandoObject AgentList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    var list = db.Agents.Select(a => new
                    {
                        a.AgentId,
                        a.ContactPersonName,
                        a.AgentCompanyName,
                        a.MobileNo,
                        a.Email,
                        a.AgentCode,
                        a.Status
                    }).ToList();

                    res.AgentList = list;
                    res.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("deleteAgent")]
        public ExpandoObject DeleteAgent(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Agent model = JsonConvert.DeserializeObject<Agent>(decryptData);

                    var agent = db.Agents.FirstOrDefault(x => x.AgentId == model.AgentId);
                    if (agent == null)
                    {
                        res.Message = "Agent not found.";
                        return res;
                    }

                    db.Agents.Remove(agent);
                    db.SaveChanges();
                    res.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

      

    }
}
