# FormBase<T>

`abstract class`

Fluent base for the three form kinds. Send() hands ownership to the native side (the holder is freed after send) and auto-unregisters callbacks on submit/close. The builder must not be reused afterwards.

**Namespace** `Endstone.Loader`

**Inheritance** `object`

## Methods

### `T OnClose(System.Action<Player> callback)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

Dispatch a submit result. MessageForm/ActionForm pass the button index, ModalForm passes the raw JSON payload.

### `void Send(Player player)`

Sends the form to the player. Callbacks fire on submit/close.

### `T Title(string text)`

