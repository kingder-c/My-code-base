//根据用户不同的请求做出不同的响应（响应现有的HTML）

var http = require("http");
const fs =require("fs");
const path =require('path');

http.createServer(function(req,res){
    res.setHeader
    if(req.url === '/login'){
         fs.readFile(path.join(__dirname,'page','login.html'),function(err,data){
             if(err){
                 throw err;
             }
             res.end(data);
         })
    }
    else if(req.url === '/register'){
        fs.readFile(path.join(__dirname,'page','register.html'),function(err,data){
            if(err){
                throw err;
            }
            res.end(data);
        })
    }
    else if(req.url === '/image/timg'){
        fs.readFile(path.join(__dirname,'page','timg.jpg'),function(err,data){
            res.setHeader("Content-Type","image/jpeg")
            res.end(data)
        });
    }
    else{
        fs.readFile(path.join(__dirname,'page','404.html'),function(err,data){
            if(err){
                throw err;
            }

            res.end(data);
        })
    }
    // fs.readFile(path.join(__dirname,'page',req.url+'.html'),function(err,data){
    //     if(err){
    //         throw err;
    //     }
    //     res.end(data);
    // })
    console.log(req.url);
}).listen(8080, function(){
    console.log("服务开启")
})