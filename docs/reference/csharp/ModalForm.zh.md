# ModalForm

`sealed class`

模态表单:标题 + 内容 + 多个控件(下拉、滑动条、文本输入、开关等),提交结果为 JSON 负载。

**命名空间** `Endstone.Loader`

**继承** `FormBase<ModalForm>` › `object`

## 构造函数

- `ModalForm()`

## 方法

### `ModalForm Divider()`

### `ModalForm Dropdown(string label, string[] options, int? defaultIndex)`

### `ModalForm Header(string text)`

### `ModalForm Icon(string icon)`

### `ModalForm Label(string text)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

### `ModalForm OnSubmit(System.Action<Player, string> callback)`

Receives the raw JSON response array on submit.

### `ModalForm Slider(string label, float min, float max, float step, float? defaultValue)`

### `ModalForm StepSlider(string label, string[] options, int? defaultIndex)`

### `ModalForm SubmitButton(string text)`

### `ModalForm TextInput(string label, string placeholder, string defaultValue)`

### `ModalForm Toggle(string label, bool defaultValue)`

