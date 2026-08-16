# ActionForm

`sealed class`

动作表单:标题 + 内容 + 任意数量的带图标按钮。

**命名空间** `Endstone.Loader`

**继承** `FormBase<ActionForm>` › `object`

## 构造函数

- `ActionForm()`

## 方法

### `ActionForm Button(string text, string icon, System.Action<Player> onClick)`

### `ActionForm Content(string text)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

### `ActionForm OnSubmit(System.Action<Player, int> callback)`

Fallback for raw button-index handling.

