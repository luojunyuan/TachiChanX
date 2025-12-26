# TouchChanX.UWP

Store 版 TachiChanX 的入口点项目

**NOTE**
- `<UseUwp>` 隐式继承了 BuildTools 和 BuildTools.MSIX 但我们使用分离的打包项目构建时并没有使用该隐式继承的生成工具
- core UWP 暂不支持 dotnet cli 构建，即使 PublishAot 也是通过 package 部署