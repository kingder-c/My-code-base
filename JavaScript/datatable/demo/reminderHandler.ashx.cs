using Example.Model;
using Smart.Water.Data.DataAccess;
using Smart.Water.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Smart.Water.Web.FireHydrant.HydrantReminder
{
    /// <summary>
    /// reminderHandler 的摘要说明
    /// </summary>
    public class reminderHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string action = context.Request.QueryString["Action"];
            string result = string.Empty;
            switch (action)
            {
                case "List":
                    result = this.HandleListRequest(context);
                    break;
            }
            context.Response.Write(result);
        }
        private string HandleListRequest(HttpContext context)
        {
            string result = string.Empty;
            DHydrant bll = new DHydrant();
            string iDisplayStart = context.Request["iDisplayStart"];
            string iDisplayLength = context.Request["iDisplayLength"];
            string briName = context.Request["name"];
            string echo = context.Request["sEcho"];
            int startIndex = 0, pagesize = 7;
            int.TryParse(iDisplayStart, out startIndex);
            int.TryParse(iDisplayLength, out pagesize);
            int count = 0;
            //string where = string.Empty;
            //if (!string.IsNullOrEmpty(briName))
            //{
            //    where += " AND ID = " + briName;
            //}

            //where += "  order by WHSJ desc";
            FormatedList<List<MHYDRANT>> model = new FormatedList<List<MHYDRANT>>();
            try
            {
                List<MHYDRANT> exceed=new List<MHYDRANT>();
                //筛选超期的选项
                foreach(MHYDRANT i in bll.GetList(null))
                {
                    if(i.LASTMAINTENTIME!=null)
                    {
                        if (briName!=null&&i.CODE.IndexOf(briName) == -1)
                            continue;
                        if (DateTime.Now.Subtract((DateTime)i.LASTMAINTENTIME).TotalDays > (365*double.Parse(i.MAINTENCYCLE.ToString())-7))
                        {
                            i.EXISTCODE5 =(DateTime.Now.Subtract((DateTime)i.LASTMAINTENTIME).TotalDays - 365 * double.Parse(i.MAINTENCYCLE.ToString())).ToString();
                            exceed.Add(i);
                        }
                    }
                    
                }
                count = exceed.Count();
                model.sEcho = int.Parse(echo);
                model.iTotalRecords = count;
                model.iTotalDisplayRecords = count;
                //model.aaData = bll.GetList(where, startIndex, pagesize);
                model.aaData = GetPage(exceed, startIndex, pagesize);
                result = Robin.EntLib.Common.Helper.JsonHelper.SerializeObject(model);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }
        /// <summary>
        /// 获取分页数据
        /// 作者：张健
        /// 时间2018/02/05
        /// </summary>
        /// <param name="allData">全部的数据</param>
        /// <param name="start">开始的条目号</param>
        /// <param name="size">每页的条目数量</param>
        /// <returns>当页的数据</returns>
        public List<MHYDRANT> GetPage(List<MHYDRANT> allData,int start,int size)
        {
            List<MHYDRANT> result=new List<MHYDRANT>();
            for (int i=start;i<allData.Count; i++)
            {
                if(i<start+size)
                   result.Add(allData[i]);
            }
            return result;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}