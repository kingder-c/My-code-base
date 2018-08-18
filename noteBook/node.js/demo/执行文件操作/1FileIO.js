  //执行文件操作

 //加载文件操作模块
const fs = require('fs');

var msg='第一次执行文件写入'
console.log('000')
//调用fs.writeFile
fs.writeFile("./helloWorld",msg,'utf-8',function(err){
    console.log('111');
    if(err){
        console.log("文件写入错误，具体错误原因："+err);
    }
    else{
        console.log("写入成功");
    }
})
console.log('222');

