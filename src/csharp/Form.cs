using System.Globalization;

namespace Endstone.Loader;

public enum FormKind
{
    MessageForm = 0,
    ActionForm = 1,
    ModalForm = 2,
}

public enum FormControlKind
{
    Label = 0,
    Header = 1,
    Divider = 2,
    Dropdown = 3,
    Slider = 4,
    StepSlider = 5,
    TextInput = 6,
    Toggle = 7,
}

internal interface IFormHandler
{
    void InvokeSubmit(Player player, int buttonIndex, string payload);
    void InvokeClose(Player player);
}

/// <summary>Fluent base for the three form kinds. Send() hands ownership to the
/// native side (the holder is freed after send) and auto-unregisters callbacks
/// on submit/close. The builder must not be reused afterwards.</summary>
public abstract unsafe class FormBase<T> : IFormHandler where T : FormBase<T>
{
    private void* _holder;
    private bool _sent;
    private readonly long _id;
    private Action<Player>? _onClose;

    protected FormBase(FormKind kind)
    {
        _holder = (void*)Bridge.Raw->FormCreate((int)kind);
        _id = FormRegistry.NextId();
    }

    protected void* Holder => _holder;

    public T Title(string text)
    {
        Bridge.Call1(Bridge.Raw->FormSetTitle, _holder, text);
        return (T)this;
    }

    public T OnClose(Action<Player> callback)
    {
        _onClose = callback;
        return (T)this;
    }

    /// <summary>Sends the form to the player. Callbacks fire on submit/close.</summary>
    public void Send(Player player)
    {
        if (_sent)
        {
            return;
        }
        Bridge.Raw->FormSetCallbacks(_holder, (ulong)_id);
        FormRegistry.Register(_id, this);
        Bridge.Raw->FormSend((void*)player.NativePtr, _holder);
        _sent = true;
        _holder = null;
    }

    void IFormHandler.InvokeClose(Player player) => _onClose?.Invoke(player);

    void IFormHandler.InvokeSubmit(Player player, int buttonIndex, string payload) => OnSubmit(player, buttonIndex, payload);

    /// <summary>Dispatch a submit result. MessageForm/ActionForm pass the button
    /// index, ModalForm passes the raw JSON payload.</summary>
    protected abstract void OnSubmit(Player player, int buttonIndex, string payload);
}

/// <summary>A simple form with a title, content and two buttons.</summary>
public sealed unsafe class MessageForm : FormBase<MessageForm>
{
    private Action<Player, int>? _onSubmit;
    private Action<Player>? _onButton1;
    private Action<Player>? _onButton2;

    public MessageForm() : base(FormKind.MessageForm) { }

    public MessageForm Content(string text)
    {
        Bridge.Call1(Bridge.Raw->FormSetContent, Holder, text);
        return this;
    }

    public MessageForm Button1(string text, Action<Player>? onClick = null)
    {
        Bridge.Call1(Bridge.Raw->FormSetButton1, Holder, text);
        _onButton1 = onClick;
        return this;
    }

    public MessageForm Button2(string text, Action<Player>? onClick = null)
    {
        Bridge.Call1(Bridge.Raw->FormSetButton2, Holder, text);
        _onButton2 = onClick;
        return this;
    }

    /// <summary>Fallback for raw button-index handling (0 = button1, 1 = button2).</summary>
    public MessageForm OnSubmit(Action<Player, int> callback)
    {
        _onSubmit = callback;
        return this;
    }

    protected override void OnSubmit(Player player, int buttonIndex, string payload)
    {
        (buttonIndex == 0 ? _onButton1 : _onButton2)?.Invoke(player);
        _onSubmit?.Invoke(player, buttonIndex);
    }
}

/// <summary>A form with a title, content and any number of buttons.</summary>
public sealed unsafe class ActionForm : FormBase<ActionForm>
{
    private readonly List<Action<Player>?> _buttons = new();
    private Action<Player, int>? _onSubmit;

    public ActionForm() : base(FormKind.ActionForm) { }

    public ActionForm Content(string text)
    {
        Bridge.Call1(Bridge.Raw->FormSetContent, Holder, text);
        return this;
    }

    public ActionForm Button(string text, string? icon = null, Action<Player>? onClick = null)
    {
        var textBuf = Bridge.ToUtf8(text);
        fixed (byte* t = textBuf)
        {
            if (icon == null)
            {
                Bridge.Raw->FormAddButton(Holder, t, null);
            }
            else
            {
                var iconBuf = Bridge.ToUtf8(icon);
                fixed (byte* i = iconBuf)
                {
                    Bridge.Raw->FormAddButton(Holder, t, i);
                }
            }
        }
        _buttons.Add(onClick);
        return this;
    }

    /// <summary>Fallback for raw button-index handling.</summary>
    public ActionForm OnSubmit(Action<Player, int> callback)
    {
        _onSubmit = callback;
        return this;
    }

    protected override void OnSubmit(Player player, int buttonIndex, string payload)
    {
        if (buttonIndex >= 0 && buttonIndex < _buttons.Count)
        {
            _buttons[buttonIndex]?.Invoke(player);
        }
        _onSubmit?.Invoke(player, buttonIndex);
    }
}

/// <summary>A custom form with typed controls and a submit button. The submit
/// payload is the raw JSON response array (values in control order).</summary>
public sealed unsafe class ModalForm : FormBase<ModalForm>
{
    private Action<Player, string>? _onSubmit;

    public ModalForm() : base(FormKind.ModalForm) { }

    public ModalForm Label(string text) => AddControl(FormControlKind.Label, text, null, null);

    public ModalForm Header(string text) => AddControl(FormControlKind.Header, text, null, null);

    public ModalForm Divider() => AddControl(FormControlKind.Divider, "", null, null);

    public ModalForm Dropdown(string label, string[] options, int? defaultIndex = null)
        => AddControl(FormControlKind.Dropdown, label, string.Join(';', options), defaultIndex?.ToString());

    public ModalForm Slider(string label, float min, float max, float step, float? defaultValue = null)
        => AddControl(FormControlKind.Slider, label, null,
            $"{defaultValue?.ToString(CultureInfo.InvariantCulture)};{min.ToString(CultureInfo.InvariantCulture)};{max.ToString(CultureInfo.InvariantCulture)};{step.ToString(CultureInfo.InvariantCulture)}");

    public ModalForm StepSlider(string label, string[] options, int? defaultIndex = null)
        => AddControl(FormControlKind.StepSlider, label, string.Join(';', options), defaultIndex?.ToString());

    public ModalForm TextInput(string label, string placeholder = "", string? defaultValue = null)
        => AddControl(FormControlKind.TextInput, label, null, $"{placeholder};{defaultValue}");

    public ModalForm Toggle(string label, bool defaultValue = false)
        => AddControl(FormControlKind.Toggle, label, null, defaultValue ? "1" : "0");

    public ModalForm SubmitButton(string text)
    {
        Bridge.Call1(Bridge.Raw->FormSetSubmitButton, Holder, text);
        return this;
    }

    public ModalForm Icon(string icon)
    {
        Bridge.Call1(Bridge.Raw->FormSetIcon, Holder, icon);
        return this;
    }

    /// <summary>Receives the raw JSON response array on submit.</summary>
    public ModalForm OnSubmit(Action<Player, string> callback)
    {
        _onSubmit = callback;
        return this;
    }

    protected override void OnSubmit(Player player, int buttonIndex, string payload) => _onSubmit?.Invoke(player, payload);

    private ModalForm AddControl(FormControlKind kind, string text, string? options, string? fmt)
    {
        var textBuf = Bridge.ToUtf8(text);
        var optionsBuf = Bridge.ToUtf8(options ?? "");
        var fmtBuf = Bridge.ToUtf8(fmt ?? "");
        fixed (byte* t = textBuf)
        fixed (byte* o = optionsBuf)
        fixed (byte* f = fmtBuf)
        {
            Bridge.Raw->FormAddControl(Holder, (int)kind, t, o, f);
        }
        return this;
    }
}

internal static class FormRegistry
{
    private static long _nextId = 1;
    private static readonly Dictionary<long, IFormHandler> Forms = new();

    internal static long NextId() => Interlocked.Increment(ref _nextId);

    internal static void Register(long id, IFormHandler form)
    {
        lock (Forms)
        {
            Forms[id] = form;
        }
    }

    internal static IFormHandler? Take(long id)
    {
        lock (Forms)
        {
            if (Forms.Remove(id, out var form))
            {
                return form;
            }
            return null;
        }
    }
}