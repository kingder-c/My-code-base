# vue中的MVVM

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
    <title>Document</title>
    <script src="./lib/vue.js"></script>
</head>
<body>
    
    <div id="app">

            <p>{{msg}}</p>
    </div>
    <script>
        var vm=new Vue({
            el:"#app",//当前vue示例要控制页面上的那个区域
            data:{
                //data属性中，存放的是el中要用到的数据
                msg:'欢迎学习Vue'//通过Vue提供的指令很烦改变的就能把数据渲染到页面上，程序员不再手动操作dom元素{前端的Vue之类的狂阿基不提倡我们直接手动的操作DOM元素}
            }
        })
    </script>
    
</body>
</html>
```
M:
```javascript
data:{
                //data属性中，存放的是el中要用到的数据
                msg:'欢迎学习Vue'//通过Vue提供的指令很烦改变的就能把数据渲染到页面上，程序员不再手动操作dom元素{前端的Vue之类的狂阿基不提倡我们直接手动的操作DOM元素}
            }
```

VM:
```javascript
        var vm=new Vue({
            el:"#app",//当前vue示例要控制页面上的那个区域
            data:{
                //data属性中，存放的是el中要用到的数据
                msg:'欢迎学习Vue'//通过Vue提供的指令很烦改变的就能把数据渲染到页面上，程序员不再手动操作dom元素{前端的Vue之类的狂阿基不提倡我们直接手动的操作DOM元素}
            }
        })
        
```
V:
```   
    <div id="app">

            <p>{{msg}}</p>
    </div>
```


**从8开始**

    
