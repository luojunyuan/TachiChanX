# TouchChan.SplashScreenGdiPlus

## 简介

基于 System.Drawing.Common (GDI+) 以极少的代码实现了一个展现透明图片的高性能 Splash 窗口。

核心代码只有 `DisplaySplash()` 50 行左右。

在 i7-8650U 的平台上 AOT 编译后，调用 `splash.Show()` 开始到出现 Splash 的耗时约为 30ms 左右。

## 依赖

* System.Drawing.Common (GDI+ 的封装)
* CSWin32 (与 win32 api 交互，包括创建窗口等)

## 项目实现了

* 由 CSWin32 提供的 no marshaling P/Invoke 生成 win32 api 接口
* 支持同步上下文和线程创建俩种方式构建，前者需要传入 UI 线程的同步上下文，后者适用于没有消息循环的控制台程序的情景

## 大小占用

* 39kb .dll release build (包含 CSWin32 生成的方法)
* 475kb System.Drawing.Common

## 备注

- 高 dpi 下自动缩放图片 (请使用以 96px 为倍数的图片，不低于 192*192 像素的图片)
- splash 不参与任何消息循环，事件通知，这里直接 DestoryWindow 等同于收到 WM_CLOSE 事件
- 使用先创建窗口，再设置 WS_EX_LAYERED，指定 #FF00800(Green) 颜色为透明通道的方式建立的透明窗口。实践中发现有出现透明效果有失效的情景的可能性。

> 可能有用的链接作为参考为将来使用 https://gist.github.com/lxfly2000/f31182868679ff6cf84ec505970bc1e4

## ShowAsync() 调用的设计方案

这里出现的模式是 bracket pattern，无论中间执行是否异常，资源都能被正确释放

## ⚠Attention!

`System.Drawing.Common` 依赖了 `Microsoft.Win32.SystemEvents` (10.0.1) 会在宿主可执行项目 AnyCPU 编译时输出一份多余的 runtime 文件夹到输出目录下。
