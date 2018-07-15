# 路径问题

``` javascript
fs.readFile("./helloWorld","utf8",function(err,data){
    if(err){
        throw err;

    }
    else{
        //如果读文件的时候传入了字符编码就自动解析成对应的编码格式
        console.log(data);
    }

})
```
其中的相对路径是执行node的路径
如果不是在该文件夹下执行的话会找不到该相对路径
所以要使用__filename和__dirname


## __filename 和 __dirname并不是全局变量
```javascript
(function(__filename,__dirname){

.....

})("C:\123\123\123.js",C:\123\123\123)
```

## 使用PATH进行路径拼接
``` javascript
const path=require('path');

var filename=path.join(__dirname,"hello.js")
console.log(filename)
```