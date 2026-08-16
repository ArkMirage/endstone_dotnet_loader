# MessageForm

`sealed class`

带标题、内容与两个按钮的简单表单。

**命名空间** `Endstone.Loader`

**继承** `FormBase<MessageForm>` › `object`

## 构造函数

- `MessageForm()`

## 方法

### `MessageForm Button1(string text, System.Action<Player> onClick)`

### `MessageForm Button2(string text, System.Action<Player> onClick)`

### `MessageForm Content(string text)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

### `MessageForm OnSubmit(System.Action<Player, int> callback)`

Fallback for raw button-index handling (0 = button1, 1 = button2).

