var asTable;
/*列表信息*/
$( function () {
    asTable = $( '#reminderTable' ).dataTable( {
        "oLanguage": {
            "sUrl": "../../js/jQuery/Plugins/DataTables/dataTables.chinese.txt"
        },
        "bServerSide": true,
        "sAjaxSource": "reminderHandler.ashx?Action=List",      //mvc后台ajax调用接口
        'bPaginate': true,                      //是否分页。
        "bProcessing": true,                    //当datatable获取数据时候是否显示正在处理提示信息。
        'bFilter': false,                       //是否使用内置的过滤功能。
        "bSort": false,
        "iDisplayLength": 10,  //每页显示的行数
        "bLengthChange": false,//允许用户从一个选择菜单中格式化页面大小(改变页面显示的数据数量)
        'sPaginationType': 'full_numbers',      //分页样式
        "fnServerParams": function ( aoData ) {
            aoData.push( { "name": "name", "value": $( "#txtName" ).val() } );
        },
        "columns": [
           { "mData": "ID", "sTitle": "ID", "bVisible": false, "bSearchable": false, "bSortable": false },
             { "mData": "CODE", "sTitle": "管点编号", "width": "10%" },
                 { "mData": "BURYMODE", "sTitle": "埋设方式", "width": "10%" },
            {
                "mData": "LASTMAINTENTIME", "sTitle": "最后维保时间", "sWidth": "14%",
                "mRender": function ( data, type, full ) {
                    if ( data ) {
                        var dtStart = full.WHSJ;
                        var dtStartWrapper = moment( dtStart );
                        return dtStartWrapper.format( 'YYYY-MM-DD' );
                    }
                    else {
                        return "";
                    }
                }
            },
            { "mData": "MAINTENCYCLE", "sTitle": "维保周期", "width": "10%" },
            {
                "mData": "CHARGE", "sTitle": "负责人", "sWidth": "8%"
            },
            {
                "mData": "PHONENUMBER", "sTitle": "负责人电话", "sWidth": "10%"
            },

            { "mData": "MAINTENUNITNAME", "sTitle": "维保单位", "width": "10%" },
            {
                "mData": "EXISTCODE5", "sTitle": "状态", "width": "10%",
                "mRender": function ( data, type, full ) {
                    if ( data ) {
                        var day=(Number)(full.EXISTCODE5);
                        if ( day > 0 )
                            return '<span style="color:red">超期' + day.toFixed( 0 ) + '天</span>'
                        else
                            return '<span style="color:black">剩余' + -day.toFixed( 0 ) + '天</span>'
                    }
                    else {
                        return "";
                    }
                }
            },
            {
                "sTitle": "操作", "width": "8%",
                "mRender": function ( data, type, full ) {
                    var html = "<a href='#' onclick='detail(\"" + full.ID + "\")'><i class='fa fa-eye fa-lg' title='查看'></i></a> ";
                    html += "&nbsp;<a href='#' onclick='locMonitorPoint(\"" + full.X + "\",\"" + full.Y + "\",\"" + full.CODE + "\",\"" + full.LASTMAINTENTIME + "\",\"" + full.EXISTCODE5 + "\",\"" + full.CALIBER + "\",\"" + full.BURYMODE + "\")'><i class='fa fa-map-marker fa-lg' title='定位'></i></a>";
                    return html;
                }
            },
        ]
    } );
    $( "#btnQuery" ).click( function () {
        debugger;
        if ( asTable != undefined && asTable != null ) {
            asTable.fnClearTable( 0 );
            asTable.fnDraw();
        }
    } );
} );

/*查看详情*/
function detail( id ) {
    parent.Robin.Window.InfoPanel( "维保提醒详情", '<iframe src="../FireHydrant/HydrantReminder/reminderDetail.aspx?Id=' + id + '" width="100%" height="100%" frameborder="0"   tabindex="9999"></iframe>', {
        id: 'detail',
        minimizeOthers: false,
        contentSize: { width: 900, height: 500 },
        minimizeOthers: false,
        paneltype: 'hint',
        theme: '#EF681E'
    } );
}
/*定位*/
function locMonitorPoint( X, Y, POINTNO, LASTYHTIME, STATUS, SIZES, EMBED ) {
    var paremeter = { "X": X, "Y": Y, "POINTNO": POINTNO, "LASTYHTIME": LASTYHTIME, "STATUS": STATUS, "SIZES": SIZES, "EMBED": EMBED };
    $.each( top.Robin.Map.Map2DControl.graphicsLayerIds, function ( i, item ) {
        top.Robin.Map.GetGraphicLayer( top.Robin.Map.Map2DControl, item ).clear();
    } );
    var listPanel = parent.Robin.Window.GetInfoPanelByID( "assert" );
    if ( listPanel != null ) {
        listPanel.smallify();
    }
    var symbolConfig = {
        font:
            { 'size': "13", 'style': 'normal' },
        color: [0, 89, 149],
        pic: {
            src: '../images/manhole/gstation.png',
            width: 30,
            height: 30
        },
        offset: {
            x: 0,
            y: -30
        }
    }
    var tpname = "";
    symbolConfig.pic.src = "";
    //判断展示文件名
    if ( STATUS == "正常" ) {
        tpname = "bz-normal";
    }
    if ( STATUS == "在建" ) {
        tpname = "bz-construction";
    }
    if ( STATUS == "作废" ) {
        tpname = "bz-discard";
    }

    var StationListLayer = top.Robin.Map.GetGraphicLayer( top.Robin.Map.Map2DControl, tpname );
    top.Robin.Map.ShowGraphic( StationListLayer, null, paremeter.X, paremeter.Y, "../images/hydrant/" + tpname + ".png", null, symbolConfig );
    var p = top.Robin.Map.GetPoint( paremeter.X, paremeter.Y );//获取标注点位置
    top.Robin.Portal.MapTool.flyMap( top.Robin.Map.Map2DControl, p );

    top.Robin.Portal.MapTool.data = paremeter;//在此处传递数据
    top.Robin.Map.MapWindow.CloseAll();
    //点击图标显示弹窗
    var windowInfo = {
        POINTX: paremeter.X,
        POINTY: paremeter.Y
    }
    top.Robin.Portal.ShowMapWindow( windowInfo, "", "" );


}
