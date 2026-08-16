# FormBase<T>

`abstract class`

三种表单(MessageForm/ActionForm/ModalForm)的流式基类。Send() 把所有权交给原生侧,提交/关闭时自动注销回调;发送后不可复用构建器。

**命名空间** `Endstone.Loader`

**继承** `object`

## 方法

### `T OnClose(System.Action<Player> callback)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

Dispatch a submit result. MessageForm/ActionForm pass the button index, ModalForm passes the raw JSON payload.

### `void Send(Player player)`

Sends the form to the player. Callbacks fire on submit/close.

### `T Title(string text)`

