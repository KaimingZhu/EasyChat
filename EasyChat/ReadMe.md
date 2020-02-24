### EasyChat 客户端
- 该客户端在UWP开发平台下进行，使用语言为C#，对应的 Windows 版本为17763
- 客户端架构为MVVM，但是并没有使用依赖注入机制，包含
    - Model : 提供所有层次所需的类
    - Service : 用服务的形式封装所有的基本操作(数据保存，网络连接)
    - ViewModel : 每个View对应的逻辑实体，主要实现每个View所需的业务逻辑
    - View : 视图，负责交互
- 为了开发方便，并没有使用数据库，所以EntityFrameWork Core也并未涉及，所有数据以txt形式保存在本地
- ~~写的很匆忙，可能会有bug :)。~~ 如果 ~~(硬是)~~ 需要测试，请打开 `EasyChat/Service/WebService/WebService.cs`，并且定位到第22行的构造函数处：

```csharp
    public WebService()
    {
        socket = new StreamSocket();
        socket.Control.KeepAlive = true;
        ip_address = new List<string>();
        ip_address.Add("192.168.1.4"); /**服务器地址**/
        port = new List<string>();
        port.Add("6666"); /**对应端口号**/
        raw_data = "";
        ifConnected = false;
    }
```

并且参考上述注释，将服务器地址更改为您服务端机器的IP地址。

PS : 尽量做到不会管杀不管埋，祝使用愉快 :)
