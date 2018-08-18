const http = require("http");
var server=http.request();
// 不需要使用var变量接收creatServer创建出的Server对象直接利用返回值在后面listen就可以
http.createServer(function(req,res){
    console.log(req.url);
    res.setHeader('Content-Type','text/plain;charset=utf-8')
    if(req.url === '/index' || req.url==="/"){
        res.end("hello index");
    } else if(req.url === "/login"){
        res.end("hello login");
    } else if(req.url === '/regest'){
        res.end('hello regest');
    }else{
        res.end('404 not found!@');
    }


}).listen(8080,function(){   
    console.log("123hahah");
})
