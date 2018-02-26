        /*获取URL中的参数*/
        function getUrlParam(name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
            var r = location.search.substr(1).match(reg);
            if (r != null) return unescape(decodeURI(r[2]));
            return null;
        }
