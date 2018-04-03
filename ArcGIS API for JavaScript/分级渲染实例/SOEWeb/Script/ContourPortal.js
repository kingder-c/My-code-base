var StartPoint = "";
var EndPoint = "";
var app = {};
var datArr = [];
require([
    "esri/map", "esri/geometry/Point", "esri/request",
    "esri/layers/ArcGISDynamicMapServiceLayer",
    "dojo/domReady!"
],
    function (
        Map, Point, esriRequest,
        ArcGISDynamicMapServiceLayer
    ) {

        app.map = new Map("map");

        var DynamicMapServiceLayer = new ArcGISDynamicMapServiceLayer("http://172.30.16.198:6080/arcgis/rest/services/SuZhouDem/MapServer");
        var DynamicMapServiceLayer02 = new ArcGISDynamicMapServiceLayer("http://172.30.16.198:6080/arcgis/rest/services/WaterDepthPoint/MapServer");
        var DynamicMapServiceLayer03 = new ArcGISDynamicMapServiceLayer("http://172.30.16.198:6080/arcgis/rest/services/rainfall/MapServer");

        app.map.addLayer(DynamicMapServiceLayer);
        app.map.addLayer(DynamicMapServiceLayer02);
        app.map.addLayer(DynamicMapServiceLayer03);

        app.map.on("click", function (evt) {
            if (StartPoint == "") {
                StartPoint = evt.mapPoint;
            }
            else {
                EndPoint = evt.mapPoint;
            }
        });
        app.map.on("dbl-click", function (evt) {
            var ptn = {};
            ptn.x = evt.mapPoint.x;
            ptn.y = evt.mapPoint.y;
            ptn.value = Number(prompt("请输入该点的值:", "1.0"));
            if (ptn.value != null) {

                datArr.push(ptn);
            }
            else { }
        })





    });

/**
  * /降雨量等值线
  */
function ContourLine() {
    require([
        "esri/map", "esri/geometry/Point", "esri/request", "esri/geometry/Polyline", "esri/symbols/CartographicLineSymbol", "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer", "esri/config",
        "dojo/domReady!"
    ], function (esriRequest, Polyline, CartographicLineSymbol, Graphic, esriConfig) {
        var content = {
            'rainfall': "[{'x':385012.49519999977,'y':3638396.1798999999,'value':52.1}," +
            "{'x':389499.83750000037,'y':3637443.6779999994,'value':49.1}," +
            "{'x':390791.00679999962,'y':3640512.8508000001,'value':51.7}," +
            "{'x':388526.16889999993,'y':3641105.5186999999,'value':42.1}," +
            "{'x':385837.99689999968,'y':3640618.6843999997,'value':55.1}," +
            "{'x':385245.32899999991,'y':3642502.5215000007,'value':55.1}," +
            "{'x':386853.99890000001,'y':3644513.3587999996,'value':57.1}," +
            "{'x':390600.5064000003,'y':3643328.0230999999,'value':49.1}," +
            "{'x':388822.5027999999,'y':3644217.0249000005,'value':47.1}," +
            "{'x':390113.67210000008,'y':3645423.5273000002,'value':60.1}," +
            "{'x':388145.16810000036,'y':3646143.1953999996,'value':58.1}," +
            "{'x':386959.83239999972,'y':3646524.1962000001,'value':57.1}," +
            "{'x':389965.50509999972,'y':3647413.1980000008,'value':55.1}," +
            "{'x':392755.27740000002,'y':3646016.1952,'value':59.1}," +
            "{'x':392501.27680000011,'y':3641655.8530999999,'value':51}," +
            "{'x':392818.77749999985,'y':3639010.0144999996,'value':51.1}]", 'f': "json"
        };
        var soeurl = "http://172.30.16.198:6080/arcgis/rest/services/SuZhouDem/MapServer/exts/ContourRestSOE2/RainfallContourLine";
        //var rquest = esriRequest({
        //    url: soeurl,
        //    content: content,
        //    handleAs: "json",
        //    callbackParamName: "callback"
        //});

        StartPoint = "";
        EndPoint = "";

        app.map.graphics.clear();

        esri.request({
            url: soeurl,
            content: content,
            handleAs: "json",
            callbackParamName: "callback",
            load: drawPolyline,
            error: function (error) {
                console.log(error);
            }
        });




    });
}
//画降雨量等值线
function drawPolyline(response) {
    require([//"esri/map",
        //"esri/geometry/Point",
        "esri/request",
        "esri/geometry/Polyline",
        "esri/symbols/CartographicLineSymbol",
        "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer",
        "esri/config",
        "dojo/domReady!"], function (esriRequest, Polyline, CartographicLineSymbol, Graphic, esriConfig) {

            app.map.graphics.clear();
            var valueArr = [];
            if (response.state == "success") {
                for (var i = 0; i < response.ContourLine.length; i++) {
                    if (valueArr.indexOf(response.ContourLine[i].Contour) == -1) {  //判断在s数组中是否存在，不存在则push到s数组中
                        valueArr.push(response.ContourLine[i].Contour);
                    }
                }
                var colors = setColor(valueArr);//设置颜色

                for (var i = 0; i < response.ContourLine.length; i++) {
                    var line = new esri.geometry.Polyline(response.ContourLine[i].GeometryLine);
                    //var symbol = new esri.symbol.SimpleLineSymbol(esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color([255, 0, 0]), 2);

                    var symbol = new esri.symbol.SimpleLineSymbol();
                    symbol.setColor(getColor(response.ContourLine[i].Contour, colors));

                    var graphic = new esri.Graphic(line, symbol);
                    app.map.graphics.add(graphic);
                }
                addLegend(colors);//增加图例
            }
            else {
                alert(response.state);
            }

        });
}

/**
 * /降水量等值面
 */
function ContourPlane() {
    require([
        "esri/map", "esri/geometry/Point", "esri/request", "esri/geometry/Polyline", "esri/symbols/CartographicLineSymbol", "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer", "esri/config",
        "dojo/domReady!"
    ], function (esriRequest, Polyline, CartographicLineSymbol, Graphic, esriConfig) {
        //var content = {
        //    'rainfall': "[{'x':385012.49519999977,'y':3638396.1798999999,'value':52.1}," +
        //    "{'x':389499.83750000037,'y':3637443.6779999994,'value':49.1}," +
        //    "{'x':390791.00679999962,'y':3640512.8508000001,'value':51.7}," +
        //    "{'x':388526.16889999993,'y':3641105.5186999999,'value':42.1}," +
        //    "{'x':385837.99689999968,'y':3640618.6843999997,'value':55.1}," +
        //    "{'x':385245.32899999991,'y':3642502.5215000007,'value':55.1}," +
        //    "{'x':386853.99890000001,'y':3644513.3587999996,'value':57.1}," +
        //    "{'x':390600.5064000003,'y':3643328.0230999999,'value':49.1}," +
        //    "{'x':388822.5027999999,'y':3644217.0249000005,'value':47.1}," +
        //    "{'x':390113.67210000008,'y':3645423.5273000002,'value':60.1}," +
        //    "{'x':388145.16810000036,'y':3646143.1953999996,'value':58.1}," +
        //    "{'x':386959.83239999972,'y':3646524.1962000001,'value':57.1}," +
        //    "{'x':389965.50509999972,'y':3647413.1980000008,'value':55.1}," +
        //    "{'x':392755.27740000002,'y':3646016.1952,'value':59.1}," +
        //    "{'x':392501.27680000011,'y':3641655.8530999999,'value':51}," +
        //    "{'x':392818.77749999985,'y':3639010.0144999996,'value':51.1}]", 'f': "json"
        //};
        var content = new Object;
        content.rainfall = JSON.stringify(datArr);
        content.f = "json";
        var soeurl = "http://172.30.16.198:6080/arcgis/rest/services/SuZhouDem/MapServer/exts/ContourRestSOE2/RainfallContourPlane";
        //var rquest = esriRequest({
        //    url: soeurl,
        //    content: content,
        //    handleAs: "json",
        //    callbackParamName: "callback"
        //});

        StartPoint = "";
        EndPoint = "";

        app.map.graphics.clear();

        esri.request({
            url: soeurl,
            content: content,
            handleAs: "json",
            callbackParamName: "callback",
            load: drawPolygon,
            error: function (error) {
                console.log(error);
            }
        });




    });
}
//画降雨量等值面
function drawPolygon(response) {
    require(["esri/map",
        "esri/layers/GraphicsLayer",
        //"esri/geometry/Point",
        //"esri/request",
        "esri/geometry/Polyline",
        "esri/symbols/CartographicLineSymbol",
        "esri/graphic",
        //"esri/layers/ArcGISDynamicMapServiceLayer",
        //"esri/config",
        "esri/symbols/SimpleFillSymbol",
        "esri/symbols/CartographicLineSymbol",
        "esri/symbols/SimpleLineSymbol",
        "esri/Color",
        "esri/InfoTemplate",
        "esri/symbols/FillSymbol",
        "dojo/domReady!"], function (esriRequest, GraphicsLayer, Polyline, CartographicLineSymbol, Graphic, SimpleFillSymbol, CartographicLineSymbol, SimpleLineSymbol, Color, InfoTemplate, FillSymbol, esriConfig) {

            app.map.graphics.clear();
            var valueArr = [];
            if (response.state == "success") {
                var pGraphicsLayer = new GraphicsLayer();
                for (var i = 0; i < response.ContourPlane.length; i++) {
                    if (valueArr.indexOf(response.ContourPlane[i].ReclassifyMedian) == -1) {  //判断在s数组中是否存在，不存在则push到s数组中
                        valueArr.push(response.ContourPlane[i].ReclassifyMedian);
                    }
                }
                var colors = setColor(valueArr);
                for (var i = 0; i < response.ContourPlane.length; i++) {
                    var polygon = new esri.geometry.Polygon(response.ContourPlane[i].GeometryPolygon);

                    var symbol = new SimpleFillSymbol();
                    symbol.setColor(getColor(response.ContourPlane[i].ReclassifyMedian, colors));
                    symbol.outline.setColor(getColor(response.ContourPlane[i].ReclassifyMedian, colors));

                    var infoTemplate = new esri.InfoTemplate();

                    infoTemplate.setTitle("积水深度");

                    infoTemplate.setContent("<b>最大降雨: </b>" + response.ContourPlane[i].ReclassifyMaxValue.toFixed(2) + "<br/>"

                        + "<b>最小降雨: </b>" + response.ContourPlane[i].ReclassifyMinValue.toFixed(2) + "<br/>"

                        + "<b>平均降雨: </b>" + response.ContourPlane[i].ReclassifyMedian.toFixed(2) + "<br/>");


                    var graphic = new esri.Graphic(polygon, symbol);
                    graphic.setInfoTemplate(infoTemplate);

                    pGraphicsLayer.add(graphic);
                }
                app.map.addLayer(pGraphicsLayer);
                addLegend(colors);//增加图例
            }
            else {
                alert(response.state);
            }


            //for (var i = 0; i < response.paths.length; i++) {
            //    var pathDuan = JSON.stringify(response.paths[i]);
            //    var line = new esri.geometry.Polyline([[0.481600001,0.118299998],[12958938.7621,4854421.353299998],[12958944.332600001,4854234.986699998],[12958947.4584,4854184.595399998]]);

            //    var symbol = new esri.symbol.SimpleLineSymbol(esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color([255, 255, 0]), 2);
            //    var graphic = new esri.Graphic(line, symbol);
            //    map.graphics.add(graphic);
            //}

        });
}

/**
 * /积水点等值线
 */
function WaterContourLine() {
    require([
        "esri/map", "esri/geometry/Point", "esri/request", "esri/geometry/Polyline", "esri/symbols/CartographicLineSymbol", "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer", "esri/config",
        "dojo/domReady!"
    ], function (esriRequest, Polyline, CartographicLineSymbol, Graphic, esriConfig) {

        var WaterPointArray = new Array();
        var WaterPoint1 = new Object();
        WaterPoint1.x = 387190;
        WaterPoint1.y = 3639090;
        WaterPoint1.value = 0.3;
        WaterPointArray.push(WaterPoint1);

        var WaterPoint2 = new Object();
        WaterPoint2.x = 392234;
        WaterPoint2.y = 3641170;
        WaterPoint2.value = 0.4;
        WaterPointArray.push(WaterPoint2);

        var WaterPoint3 = new Object();
        WaterPoint3.x = 392267;
        WaterPoint3.y = 3644670;
        WaterPoint3.value = 2.3;
        WaterPointArray.push(WaterPoint3);

        //var WaterPoint4 = new Object();
        //WaterPoint4.x = 391920;
        //WaterPoint4.y = 3646210;
        //WaterPoint4.value = 0.3;
        //WaterPointArray.push(WaterPoint4);

        //var WaterPoint5 = new Object();
        //WaterPoint5.x = 388513;
        //WaterPoint5.y = 3637530;
        //WaterPoint5.value = 0.6;
        //WaterPointArray.push(WaterPoint5);

        var para = new Object();
        para.WaterDepth = JSON.stringify(WaterPointArray);
        para.f = "json";
        var content = {
            'WaterDepth': "[" +
            "{'x':387190,'y':3639090,'value':0.3}," +
            "{'x':392234,'y':3641170,'value':0.2}," +
            "{'x':392267,'y':3644670,'value':0.3}," +
            "{'x':391920,'y':3646210,'value':0.3}," +
            "{'x':388513,'y':3637530,'value':0.1}" +
            "]", 'f': "json"
        };
        var soeurl = "http://172.30.16.198:6080/arcgis/rest/services/SuZhouDem/MapServer/exts/ContourRestSOE2/WaterDepthContourLine";
        //var rquest = esriRequest({
        //    url: soeurl,
        //    content: content,
        //    handleAs: "json",
        //    callbackParamName: "callback"
        //});

        StartPoint = "";
        EndPoint = "";

        app.map.graphics.clear();

        esri.request({
            url: soeurl,
            content: para,
            handleAs: "json",
            callbackParamName: "callback",
            load: drawWaterLine,
            error: function (error) {
                alert(error);
            }
        });




    });
}
//画积水点等值线
function drawWaterLine(response) {
    require(["esri/map",
        "esri/layers/GraphicsLayer",
        "esri/geometry/Point",
        "esri/request",
        "esri/geometry/Polyline",
        "esri/symbols/CartographicLineSymbol",
        "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer",
        "esri/config",
        "esri/symbols/SimpleFillSymbol",
        "esri/symbols/CartographicLineSymbol",
        "esri/symbols/SimpleLineSymbol",
        "esri/Color",
        "esri/InfoTemplate",
        "dojo/domReady!"],
        function (esriRequest, GraphicsLayer, Polyline, CartographicLineSymbol, Graphic, SimpleFillSymbol, CartographicLineSymbol, SimpleLineSymbol, Color, InfoTemplate, esriConfig) {

            app.map.graphics.clear();
            var valueArr = [];
            if (response.state == "success") {
                var pGraphicsLayer = new GraphicsLayer();
                for (var i = 0; i < response.WaterDepthContourLineArray.length; i++) {
                    for (var j = 0; j < response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine.length; j++) {
                        if (valueArr.indexOf(response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine[j].Contour) == -1) {  //判断在s数组中是否存在，不存在则push到s数组中
                            valueArr.push(response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine[j].Contour);
                        }
                    }
                }
                var colors = setColor(valueArr);

                for (var i = 0; i < response.WaterDepthContourLineArray.length; i++) {
                    //实现等值线的添加
                    for (var j = 0; j < response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine.length; j++) {
                        var line = new esri.geometry.Polyline(response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine[j].GeometryLine);
                        var symbol = new esri.symbol.SimpleLineSymbol();
                        symbol.setColor(getColor(response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine[j].Contour, colors));

                        var graphic = new esri.Graphic(line, symbol);
                        //var graphicLine = new esri.Graphic(line, symbolLine);
                        graphic.attr("contourValue", response.WaterDepthContourLineArray[i].WaterContourLine.ContourLine[j].Contour);
                        pGraphicsLayer.add(graphic);
                    }



                    //app.map.graphics.add(graphic);
                }
                app.map.addLayer(pGraphicsLayer);
                addLegend(colors);//增加图例
            }
            else {
                alert(response.state);
            }


        });
}

/**
    * /积水点等值面
    */
function WaterContourPlane() {
    require([
        "esri/map", "esri/geometry/Point", "esri/request", "esri/geometry/Polyline", "esri/symbols/CartographicLineSymbol", "esri/graphic",
        "esri/layers/ArcGISDynamicMapServiceLayer", "esri/config",
        "dojo/domReady!"
    ], function (esriRequest, Polyline, CartographicLineSymbol, Graphic, esriConfig) {

        var WaterPointArray = new Array();
        var WaterPoint1 = new Object();
        WaterPoint1.x = 387190;
        WaterPoint1.y = 3639090;
        WaterPoint1.value = 0.3;
        WaterPointArray.push(WaterPoint1);

        var WaterPoint2 = new Object();
        WaterPoint2.x = 392234;
        WaterPoint2.y = 3641170;
        WaterPoint2.value = 0.4;
        WaterPointArray.push(WaterPoint2);

        var WaterPoint3 = new Object();
        WaterPoint3.x = 392267;
        WaterPoint3.y = 3644670;
        WaterPoint3.value = 2.3;
        WaterPointArray.push(WaterPoint3);

        //var WaterPoint4 = new Object();
        //WaterPoint4.x = 391920;
        //WaterPoint4.y = 3646210;
        //WaterPoint4.value = 0.3;
        //WaterPointArray.push(WaterPoint4);

        //var WaterPoint5 = new Object();
        //WaterPoint5.x = 388513;
        //WaterPoint5.y = 3637530;
        //WaterPoint5.value = 0.6;
        //WaterPointArray.push(WaterPoint5);

        var para = new Object();
        para.WaterDepth = JSON.stringify(datArr);
        para.f = "json";
        var content = {
            'WaterDepth': "[" +
            "{'x':387190,'y':3639090,'value':0.3}," +
            "{'x':392234,'y':3641170,'value':0.2}," +
            "{'x':392267,'y':3644670,'value':0.3}," +
            "{'x':391920,'y':3646210,'value':0.3}," +
            "{'x':388513,'y':3637530,'value':0.1}" +
            "]", 'f': "json"
        };
        var soeurl = "http://172.30.16.198:6080/arcgis/rest/services/SuZhouDem/MapServer/exts/ContourRestSOE2/WaterDepthContourPlane";
        //var rquest = esriRequest({
        //    url: soeurl,
        //    content: content,
        //    handleAs: "json",
        //    callbackParamName: "callback"
        //});

        StartPoint = "";
        EndPoint = "";

        app.map.graphics.clear();

        esri.request({
            url: soeurl,
            content: para,
            handleAs: "json",
            callbackParamName: "callback",
            load: drawWaterPlane,
            timeout: 200000,
            error: function (error) {
                alert(error);
            }
        });




    });
}
//画积水点等值面
function drawWaterPlane(response) {
    require(["dojo/dom",
        "dojo/_base/array",
        "dojo/parser",
        "dojo/query",
        "dojo/on",
        "esri/Color",
        "esri/config",
        "esri/map",
        "esri/layers/GraphicsLayer",
        "esri/graphic",
        "esri/geometry/normalizeUtils",
        "esri/tasks/GeometryService",
        "esri/tasks/BufferParameters",
        "esri/toolbars/draw",
        "esri/symbols/SimpleMarkerSymbol",
        "esri/symbols/SimpleLineSymbol",
        "esri/symbols/SimpleFillSymbol",
        "esri/InfoTemplate"

        //"dijit/layout/BorderContainer",
        //"dijit/layout/ContentPane",
        //"dijit/form/Button",
        //"dojo/domReady!"
    ], function (dom, array, parser, query, on, Color, esriConfig, Map, GraphicsLayer, Graphic, normalizeUtils, GeometryService, BufferParameters, Draw, SimpleMarkerSymbol, SimpleLineSymbol, SimpleFillSymbol, InfoTemplate) {

        app.map.graphics.clear();
        var valueArr = [];
        if (response.state == "success") {
            var pGraphicsLayer = new GraphicsLayer();
            for (var i = 0; i < response.WaterDepthContourPlaneArray.length; i++) {
                for (var j = 0; j < response.WaterDepthContourPlaneArray[i].ContourPlane.length; j++) {
                    if (valueArr.indexOf(response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian) == -1) {  //判断在s数组中是否存在，不存在则push到s数组中
                        valueArr.push(response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian);
                    }
                    //valueArr.push(response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian);
                }
            }
            var colors = setColor(valueArr);

            for (var i = 0; i < response.WaterDepthContourPlaneArray.length; i++) {
                for (var j = 0; j < response.WaterDepthContourPlaneArray[i].ContourPlane.length; j++) {
                    var polygon = new esri.geometry.Polygon(response.WaterDepthContourPlaneArray[i].ContourPlane[j].GeometryPolygon);
                    var symbol = new SimpleFillSymbol();
                    symbol.setColor(getColor(response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian, colors));
                    symbol.outline.setColor(getColor(response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian, colors));

                    var infoTemplate = new esri.InfoTemplate();

                    infoTemplate.setTitle("积水深度");

                    infoTemplate.setContent("<b>最大深度: </b>" + response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMaxValue.toFixed(2) + "<br/>"

                        + "<b>最小深度: </b>" + response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMinValue.toFixed(2) + "<br/>"

                        + "<b>平均深度: </b>" + response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMedian.toFixed(2) + "<br/>");


                    var graphic = new esri.Graphic(polygon, symbol);
                    graphic.setInfoTemplate(infoTemplate);
                    graphic.attr("ReclassifyMinValue", response.WaterDepthContourPlaneArray[i].ContourPlane[j].ReclassifyMinValue);
                    pGraphicsLayer.add(graphic);
                }

            }
            app.map.addLayer(pGraphicsLayer);
            addLegend(colors);//增加图例
        }
        else {
            alert(response.state);
        }


    });
}


function setColor(values) {
    values = values.sort(function (a, b) {
        return +a - (+b);
    });
    //var temp = [Math.floor(121 / (values.length )), Math.floor(150 / (values.length )),Math.floor(150 / (values.length ))];
    // startColor [140,192,251]
    // endColor [3,40,82]
    var result = [];
    var startColor = [55, 249, 28];
    var endColor = [23, 0, 236];
    var tempR = Math.floor((endColor[0] - startColor[0]) / (values.length - 1));
    var tempG = Math.floor((endColor[1] - startColor[1]) / (values.length - 1));
    var tempB = Math.floor((endColor[2] - startColor[2]) / (values.length - 1));
    for (var i = 0; i < values.length; i++) {
        result.push({
            value: values[i],
            color: [startColor[0] + i * tempR, startColor[1] + i * tempG, startColor[2] + i * tempB, 0.5]
            //color: [128 + i * temp[0], 9 + i * temp[1], 9 + temp[2]]
        })
    }

    return result;
}
function getColor(value, colors) {
    for (var i = 0; i < colors.length; i++) {
        if (value == colors[i].value) { return colors[i].color };
    }
    return [0, 0, 0];
}
function addLegend(colors) {
    var div = "";
    $("#legend-contour").empty();
    for (var i = 0; i < colors.length; i++) {
        div += "<li style='font-size:16px;color:#fff;margin-bottom:6px;text-align:center;display:block;width:100%;background:" + "rgb(" + colors[i].color[0] + "," + colors[i].color[1] + "," + colors[i].color[2] + ");'>"
            + colors[i].value.toFixed(2); + "mm</li>";
    }

    $("#legend-contour").append(div);
    $("#legendBoard").show();
    $(".legend-close").on("click", function () {
        app.map.graphics.clear();
        $("#legendBoard").hide();
        $("#legend-contour").empty();
        return false;
    })
    return;
}

/*
//dat.gui控制板
var FizzyText = function () {
    this.message = 'dat.gui';
    this.speed = 0.8;
    this.displayOutline = false;
    this.explode = function () { };
};

window.onload = function () {
    var text = new FizzyText();
    var gui = new dat.GUI();
    gui.add(text, 'message');
    gui.add(text, 'speed', -5, 5);
    gui.add(text, 'displayOutline');
    gui.add(text, 'explode');
};*/

