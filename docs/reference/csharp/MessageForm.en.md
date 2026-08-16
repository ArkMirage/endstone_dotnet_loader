# MessageForm

`sealed class`

A simple form with a title, content and two buttons.

**Namespace** `Endstone.Loader`

**Inheritance** `FormBase<MessageForm>` › `object`

## Constructors

- `MessageForm()`

## Methods

### `MessageForm Button1(string text, System.Action<Player> onClick)`

### `MessageForm Button2(string text, System.Action<Player> onClick)`

### `MessageForm Content(string text)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

### `MessageForm OnSubmit(System.Action<Player, int> callback)`

Fallback for raw button-index handling (0 = button1, 1 = button2).

