# PluginAttribute

`sealed class`

插件声明特性:标记插件主类,并提供名称、版本、描述与作者信息。

**命名空间** `Endstone.Loader`

**继承** `System.Attribute` › `object`

## 构造函数

- `PluginAttribute(string name, string version)` Attribute describing plugin metadata. The plugin name must contain only lowercase letters, numbers and underscores (Endstone requirement).

## 属性

### `Authors` : `string[]`

`{ get;set; }`

插件作者列表。

### `Description` : `string`

`{ get;set; }`

插件描述。

### `Name` : `string`

`{ get; }`

插件名称(仅允许小写字母、数字、下划线)。

### `Version` : `string`

`{ get; }`

插件版本号。

