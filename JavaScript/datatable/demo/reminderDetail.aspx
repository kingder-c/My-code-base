<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="reminderDetail.aspx.cs" Inherits="Smart.Water.Web.FireHydrant.HydrantReminder.reminderDetail" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>维保提醒详情</title>
    <link href="../../js/Bootstrap/4.0.0-alpha.2/css/bootstrap.min.css" rel="stylesheet" />
    <link href="../../js/jQuery/Plugins/validation/validation.css" rel="stylesheet" />
    <script src="../../js/jQuery/jquery.min.js"></script>
    <script src="../../js/jQuery/Plugins/tether/tether.min.js"></script>
    <script src="../../js/Bootstrap/4.0.0-alpha.2/js/bootstrap.min.js"></script>
    <script src="../../js/jQuery/Plugins/DatePicker/WdatePicker.js"></script>
    <script src="../../js/jQuery/Plugins/validation/jquery.validate.js"></script>
       <script src="../../js/jQuery/Plugins/validation/jquery-validate.bootstrap-tooltip.min.js"></script>
    <style>
        .inputText{
            height:30px;
        }
    </style>
    <script>
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
<div>
            <div class=" form-group">
                <table class="table table-bordered">
                    <tr>
                        <td>管点编号</td>
                        <td>
                            <input type="text" class="form-control inputText" runat="server" id="CODE"  disabled="disabled" />
                        </td>
                        <td>口径</td>
                        <td>
                            <input type="text" class="form-control inputText" runat="server" id="CALIBER" disabled="disabled"  />
                        </td>
                    </tr>
                    <tr>
                        <td>生产厂家</td>
                        <td>
                            <input type="text" class="form-control inputText" runat="server" id="MANUFACTURER" disabled="disabled" />                          
                        </td>
                        <td>建成年月</td>
                        <td>
                            <input type="text" class="form-control inputText" runat="server" id="ACTIVATETIME" disabled="disabled" />
                        </td>
                    </tr>
                    <tr>
                        <td>埋设方式</td>
                        <td>
                            <input type="text" class="form-control inputText" runat="server" id="BURYMODE" disabled="disabled"  />
                        </td>                    
                        <td>使用状态</td>
                        <td >
                            <input class="form-control inputText" runat="server" id="STATUES" disabled="disabled" >
                        </td>
                    </tr>
                       <tr>                      
                        <td>最后维保时间</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="LASTMAINTENTIME1" disabled="disabled" />
                        </td>
                        <td>负责人</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="CHARGE1" disabled="disabled"/>
                        </td>
                    </tr>
                     <tr>
                        <td>负责人电话</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="PHONENUMBER1" disabled="disabled"/>
                        </td>
                        <td>井深</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="WELLDEPTH1" disabled="disabled"/>
                        </td>
                    </tr>
                     <tr>
                        <td>维保周期</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="MAINTENCYCLE1" disabled="disabled"/>
                        </td>
                        <td>维保单位名称</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="MAINTENUNITNAME1" disabled="disabled"/>
                        </td>
                    </tr>
                     <tr>
                        <td>内容展示</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="CONTENT1" disabled="disabled"/>
                        </td>
                        <td>地址</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="ADDRESS1" disabled="disabled"/>
                        </td>
                    </tr>
                    <tr>
                        <td>备注</td>
                        <td>
                            <input class="form-control inputText" runat="server" id="REMARK1" disabled="disabled"/>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </form>
</body>
</html>
