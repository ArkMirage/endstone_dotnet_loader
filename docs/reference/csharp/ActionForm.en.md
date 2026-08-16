# ActionForm

`sealed class`

A form with a title, content and any number of buttons.

**Namespace** `Endstone.Loader`

**Inheritance** `FormBase<ActionForm>` › `object`

## Constructors

- `ActionForm()`

## Methods

### `ActionForm Button(string text, string icon, System.Action<Player> onClick)`

### `ActionForm Content(string text)`

### `void OnSubmit(Player player, int buttonIndex, string payload)`

### `ActionForm OnSubmit(System.Action<Player, int> callback)`

Fallback for raw button-index handling.

