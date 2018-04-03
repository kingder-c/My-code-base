function MagnifyMap() {
    var _map_drag;
    var divID = "MagnifyMap";
    var mmap;
    var radius = 80;
    var levelMax = 0;

    function searchLayerInfo(layerID) {
        var _layer;
        if (mapLayerObj != undefined) {
            _layer = mapLayerObj.searchLayerByLayerID(layerID);
        }
        return _layer;
    }

    this.InitMagnify = function () {
        var rt = true;
        require(["dojo/on", "esri/map", "esri/geometry/Extent"], function (on, Map, Extent) {
            var sdivID = getSecondLayerID();
            var div = $("#map_" + sdivID);
            var mlayer;
            if (div.length > 0) {
                mlayer = searchLayerInfo(sdivID);

            } else {
                rt = false;
            }
            if (mlayer == undefined || !rt) {
                showError("当前图层不支持透镜操作", true);

                rt = false;
            }
            if (rt) {
                $("#map").append('<div id="' + divID + '" class="magnifying-glass"><div id="mmap"></div></div>');
                mmap = new Map("mmap", {
                    slider: false,
                    logo: false
                });
                $("#mmap").css("width", map.width);
                $("#mmap").css("height", map.height);

                mmap.setLevel(map.getLevel() + levelMax);
                mmap.centerAt(map.extent.getCenter());

                $("#" + divID).css("width", radius * 2);
                $("#" + divID).css("height", radius * 2);
                $("#" + divID).css("border-radius", "50%");
                $("#" + divID).hide();

                mmap.addLayer(mlayer);
                mmap.disablePan();
                mmap.disableDoubleClickZoom();
                mmap.disableScrollWheelZoom();
                mmap.disableSnapping();
                mmap.disableKeyboardNavigation();

                map.disablePan();
                _map_drag = on(map, "mouse-drag", function (e) {
                    e.stopPropagation();
                    map.disablePan();
                    mmap.disablePan();
                    mmap.disableDoubleClickZoom();
                    mmap.disableScrollWheelZoom();
                    mmap.disableSnapping();
                    mmap.disableKeyboardNavigation();

                    $("#" + divID).show();
                    var offsetX = e.screenPoint.x;
                    var offsetY = e.screenPoint.y;
                    var mapx = e.mapPoint.x;
                    var mapy = e.mapPoint.y;
                    refreshDivSize(offsetX, offsetY, mapx, mapy);
                });
            }
        });
        return rt;
    }

    this.disableMagnify = function () {
        clearMagnify();
    }

    function clearMagnify() {
        if (_map_drag != undefined) {
            _map_drag.remove();
            map.enablePan();
        }
        if (mmap != undefined) {
            mmap.removeAllLayers();
            mmap.destroy();
        }
        $("#" + divID).remove();
    }

    function getSecondLayerID() {
        var layerlist = map.layerIds;
        var len = 0;
        if (layerlist != undefined && layerlist.length > 0) {
            len = layerlist.length;
        } else {
            return null;
        }
        var layerID = null;
        var index = 0;
        for (var i = len - 1; i >= 0; i--) {
            var item = map.getLayer(layerlist[i]);
            if (item.visible) {
                if (index == 1) {
                    layerID = item.id;
                    break;
                }
                index++;
            }
        }
        return layerID;
    }

    function refreshDivSize(offsetX, offsetY, mapX, mapY) {
        var t = offsetY - radius;
        var l = offsetX - radius;
        $("#mmap").css("top", -(offsetY - radius) + "px");
        $("#mmap").css("left", -(offsetX - radius) + "px");

        $("#" + divID).css("top", t + "px");
        $("#" + divID).css("left", l + "px");
    }

}
