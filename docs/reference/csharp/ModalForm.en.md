# ModalForm

`sealed class`

A custom form with typed controls and a submit button. The submit payload is the raw JSON response array (values in control order).

**Namespace** `Endstone.Loader`

**Inheritance** `FormBase<ModalForm>` › `object`

## Constructors

- `ModalForm()`

## Methods

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

