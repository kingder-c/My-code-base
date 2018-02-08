/*函数调用*/       
	   Emergency.Map.Route.routePlan(x1, y1, x2, y2, barrierPathArray, true, function (solveResult) {
            //进行路径查询结果的解析
            var routeResults = solveResult.routeResults;
            var res = routeResults.length;
            var extend = null;
            var totalLength = 0;
            if (res > 0) {
                for (var i = 0; i < res; i++) {
                    totalLength += routeResults[i].directions.totalLength;
                    var html = "";
                    if (routeResults[i].directions.totalLength > 0) {

                        var speed = $('#speedAlalysis').val();
                        var perSpeed = routeResults[i].directions.totalLength.toFixed(2) * 0.001 / speed;

                        $("#totalLength").html(routeResults[i].directions.totalLength.toFixed(2) + '米');
                        if (speed =="") {
                            $("#hourAnalysis").html('0h');
                        }else if(isNaN(speed)){
                            noty({ text: "速度输入有误！", type: "warning", layout: "topCenter", timeout: 1000 });
                            return;
                        }
                        else {
                            $("#hourAnalysis").html(perSpeed.toFixed(2) + 'h');
                        }                      
                        var num = 1;
                        for (var j = 0; j < routeResults[i].directions.features.length; j++) {
                            html += '<li class="list-group-item" style="padding:5px;">';                            
                            html += "<span class=''>" + num++ + ".</span>";
                            var text = getChineseByEng(routeResults[i].directions.features[j].attributes.text, transArr);
                            html += "<span class=''>" + text+ "</span></li> ";
                        }
                        $("#routeInfo").html(html);
                    }
                }
            }

        });
//函数主体
routePlan: function (x1, y1, x2, y2, barrierPathArray, isDraw, callBack) {
        require([
                   "esri/symbols/SimpleLineSymbol", "esri/Color",
                   "esri/tasks/RouteTask",
                    "esri/tasks/FeatureSet",
                   "esri/tasks/RouteParameters"
        ], function () {
            var routeServiceUrl = Robin.Setting.MapAnalyse.route.url;
            var routeGraphicLayer = Robin.Map.GetGraphicLayer(top.Robin.Map.Map2DControl, Robin.Setting.MapAnalyse.route.routeLayerName);
            //Robin.Map.ClearLayer(top.Robin.Map.Map2DControl, Robin.Setting.MapAnalyse.route.routeLayerName);
            if (routeGraphicLayer != null) {
                routeGraphicLayer.clear();
            }
            var map = top.Robin.Map.Map2DControl;
            var wkid = Robin.Setting.GlobalSetting.wkid;
            //起点、终点
            var ptStart = new esri.geometry.Point(parseFloat(x1), parseFloat(y1), new esri.SpatialReference({ wkid: wkid }));
            var ptEnd = new esri.geometry.Point(parseFloat(x2), parseFloat(y2), new esri.SpatialReference({ wkid: wkid }));
            var startPtGra = new esri.Graphic(ptStart);
            var endPtGra = new esri.Graphic(ptEnd);

            //设置路径样式
            var routeSymbol = new esri.symbol.SimpleLineSymbol(esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color([82, 152, 255, 1]), 5);
            //起点、终点尾部线
            var routeSymbolDef = new esri.symbol.SimpleLineSymbol(esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color([77, 220, 38, 1]), 5);

            var routeTask = new esri.tasks.RouteTask(routeServiceUrl);
            var routeParams = new esri.tasks.RouteParameters();
            //设置参数
            routeParams.outSpatialReference = { wkid: wkid };
            routeParams.returnRoutes = true;
            routeParams.returnDirections = true;
            routeParams.directionsLengthUnits = esri.Units.METERS;
            routeParams.stops = new esri.tasks.FeatureSet();
            routeParams.stops.features.push(startPtGra);
            routeParams.stops.features.push(endPtGra);

            //route = JSON.stringify(barrierPathArray);
            //var temp = jQuery.parseJSON(route);
            //var array = [];
            //$.each(temp, function (i, item) {
            //    var point = [item[0], item[1]];
            //    array.push(point);
            //});

            ////设置路障参数
            //if (array.length > 0) {
            //    var barrierRoute = new esri.geometry.Polyline(new esri.SpatialReference({ wkid: wkid }));
            //    barrierRoute.addPath(array);
            //    var barriersGraphic = new esri.Graphic(barrierRoute);
            //    routeParams.polylineBarriers = new esri.tasks.FeatureSet();
            //    routeParams.polylineBarriers.features.push(barriersGraphic);
            //}


            if (barrierPathArray) {
                routeParams.polylineBarriers = new esri.tasks.FeatureSet();
                for (var i = 0; i < barrierPathArray.length; i++) {
                    if (barrierPathArray[i].length == 0) {
                        continue;
                    }
                    var barrierRoute = new esri.geometry.Polyline(new esri.SpatialReference({ wkid: wkid }));
                    barrierRoute.addPath(barrierPathArray[i]);
                    var barriersGraphic = new esri.Graphic(barrierRoute);
                    routeParams.polylineBarriers.features.push(barriersGraphic);
                }
            }
            if (routeParams.stops.features.length == 0) {
                noty({ text: "路径规划输入参数不全，无法分析", type: "warning", layout: "topCenter", timeout: 2000 });
                return;
            }

            routeTask.solve(routeParams, function (solveResult) {
                if (callBack) {
                    callBack(solveResult);
                }

                var routeResults = solveResult.routeResults;
                var res = routeResults.length;
                if (isDraw == false) {
                    return;
                }

                if (res > 0) {
                    for (var i = 0; i < res; i++) {
                        var graphicroute = routeResults[i];
                        var graphic = graphicroute.route;
                        graphic.setSymbol(routeSymbol);
                        routeGraphicLayer.add(graphic);

                        //连接路线起点、终点与对应定位起点、终点
                        var paths = graphic.geometry.paths[0];
                        var routeStr = paths[0];
                        var routeEnd = paths[paths.length - 1];

                        var routeStrline = new esri.geometry.Polyline([[x1, y1], routeStr], new esri.SpatialReference({ wkid: wkid }));
                        var routeEndline = new esri.geometry.Polyline([[x2, y2], routeEnd], new esri.SpatialReference({ wkid: wkid }));
                        var strlinegraphic = new esri.Graphic(routeStrline, routeSymbolDef);
                        var endlinegraphic = new esri.Graphic(routeEndline, routeSymbolDef);

                        routeGraphicLayer.add(strlinegraphic);
                        routeGraphicLayer.add(endlinegraphic);
                    }
                }
                else {
                    //alert("路径规划返回结果空");
                }
            }, function (error) {
                noty({ text: "没有找到合适的路线", type: "warning", layout: "topCenter", timeout: 2000 });
            });
        });
    },		