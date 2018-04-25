# 浏览器工作原理

## 浏览器的组成
- 人机交互部分
- 网络请求部分
- JavaScript引擎部分（解析执行javaScript）
- 渲染引擎部分（渲染html、Css等）
- 数据库存储部分（cookie、HTML5中俄的本地存储LocalStorage、SessionStorage）

sqlite

## 主流渲染引擎
### 介绍
1. 渲染引擎 又叫排版引擎 或 浏览器内核
2. 主流的渲染引擎有
- **Chrom浏览器**：Blink引擎（Webkit的一个分支）。
- **Safari浏览器**：Webkit引擎，windows版本2008年3月18日推出正式版，但是苹果已于2012年停止开发windows版的safari
- **Firefox浏览器**：Gecko引擎（早期版使用Presto引擎）
- **IE浏览器**：Tridend引擎
- **Edge浏览器**：EdgeHTML引擎（Trident的一个分支）

### 工作原理
1. 解析HTML构建Dom树（Document Object Model，文档对象模型），DOM是W3C组织推荐的处理可拓展标记语言的标准程序接口。
2. 构建*渲染树*，*渲染树*并不简单的等同于*DOM树*，因为像`head标签 或 display：none` 这样的元素就没有必要放到*渲染树*中了，但是它们在*DOM树*中。
3. 对*渲染树*进行布局，定位坐标和大小，确定是否换行，确定Position。overflow，z-index等等，这个过程叫`layout`或`"reflow"`
4. 绘制*渲染树*，调用操作系统底层API进行绘图操作。


### 渲染引擎工作原理
![渲染引擎工作原理](http://ouewomi2z.bkt.clouddn.com/18-4-20/64708357.jpg)
![webkit工作原理](http://ouewomi2z.bkt.clouddn.com/18-4-20/10792662.jpg)
![Gecko工作原理](http://ouewomi2z.bkt.clouddn.com/18-4-20/32453873.jpg)


