## チャットアプリ
ブロードキャスト通信(UDP)による、シンプルなチャットアプリ
同一サブネット内でのメッセージ交換を可能とする


### 修正対象ファイル
```
\MVVM\ViewModel\MainViewModel.cs
```

上記ファイルに変更を追加する

任意でファイル追加して問題ないが、
通信機能の追加であれば上記ファイル内の追加で対応可能

### 参考

#### UdpClient
https://learn.microsoft.com/ja-jp/dotnet/api/system.net.sockets.udpclient?view=net-8.0

送受信処理は以下の関数を使用するとよい

- send
- beginSend
- receive
- beginReceive


#### JsonSerializer
https://learn.microsoft.com/ja-jp/dotnet/api/system.text.json.jsonserializer?view=net-8.0
