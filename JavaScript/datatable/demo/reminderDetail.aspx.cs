using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Smart.Water.Data.DataAccess;
using Smart.Water.Data.Model;

namespace Smart.Water.Web.FireHydrant.HydrantReminder
{
    public partial class reminderDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string Id = Request.QueryString["Id"].ToString();
                detalInfo(Id);
            }
        }

        private void detalInfo(string id)
        {
            try
            {
                DHydrant bll = new DHydrant();
                MHYDRANT model = new MHYDRANT();
                model = bll.GetModel(id);
                CODE.Value = model.CODE;
                CALIBER.Value = model.CALIBER == null ? string.Empty : model.CALIBER.ToString();
                MANUFACTURER.Value = model.MANUFACTURER;
                ACTIVATETIME.Value = model.ACTIVATETIME == null ? string.Empty : model.ACTIVATETIME.Value.ToString("yyyy-MM-dd");
                BURYMODE.Value = model.BURYMODE;
                STATUES.Value = model.STATUS;
                LASTMAINTENTIME1.Value = model.LASTMAINTENTIME == null ? string.Empty : model.LASTMAINTENTIME.Value.ToString("yyyy-MM-dd");
                PHONENUMBER1.Value = model.PHONENUMBER;
                WELLDEPTH1.Value = model.WELLDEPTH == null ? string.Empty : model.WELLDEPTH.ToString();
                MAINTENCYCLE1.Value = model.MAINTENCYCLE==null?string.Empty:model.MAINTENCYCLE.ToString();
                MAINTENUNITNAME1.Value = model.MAINTENUNITNAME;
                CONTENT1.Value = model.CONTENT;
                ADDRESS1.Value = model.ADDRESS;
                REMARK1.Value = model.REMARK;
            }
            catch (Exception ex)
            {

            }
        }
    }
}