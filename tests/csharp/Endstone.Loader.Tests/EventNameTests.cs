using System.Reflection;

namespace Endstone.Loader.Tests;

/// <summary>
/// The string-based event registration path only accepts the built-in event
/// classes that this loader can wrap into a typed <see cref="Event"/>.
/// Registering an unknown name must be rejected, because such an event could
/// never expose its payload or its cancellation state. These checks exercise the
/// registration and factory decisions directly; with no plugin handle attached
/// the native bridge is never called, so no host is required.
/// </summary>
[TestClass]
public class EventNameTests
{
    // Event is abstract, so GetTypes() yields the concrete wrappers alone.
    private static readonly Type[] BuiltInEvents = typeof(Event).Assembly
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(Event)) && t.Namespace == "Endstone.Loader")
        .ToArray();

    [TestMethod]
    public void BuiltInEvents_AreDiscovered()
    {
        Assert.IsNotEmpty(BuiltInEvents, "No built-in event classes were found; the test filter is wrong.");
    }

    [TestMethod]
    public void IsKnown_AcceptsEveryBuiltInEventName()
    {
        foreach (var type in BuiltInEvents)
        {
            Assert.IsTrue(EventFactory.IsKnown(type.Name),
                $"Built-in event '{type.Name}' is missing from the factory table.");
        }
    }

    [TestMethod]
    public void IsKnown_RejectsUnknownNames()
    {
        Assert.IsFalse(EventFactory.IsKnown("MyCustomEvent"));
        Assert.IsFalse(EventFactory.IsKnown("playerjoinevent"), "Name matching must be case-sensitive.");
        Assert.IsFalse(EventFactory.IsKnown(""));
    }

    [TestMethod]
    public void Create_ReturnsTheWrapperNamedByTheEvent()
    {
        // Guards against a factory entry wired to the wrong wrapper class.
        foreach (var type in BuiltInEvents)
        {
            var created = EventFactory.Create(type.Name, IntPtr.Zero);

            Assert.AreEqual(type, created.GetType(), $"'{type.Name}' was created as the wrong event type.");
        }
    }

    [TestMethod]
    public void Register_RejectsUnknownName()
    {
        var manager = new EventManager();

        Assert.IsFalse(manager.Register("MyCustomEvent", EventPriority.Normal, false, _ => { }),
            "An unknown event name must not be registered.");
    }

    [TestMethod]
    public void Register_AcceptsBuiltInName()
    {
        var manager = new EventManager();

        Assert.IsTrue(manager.Register(nameof(PlayerJoinEvent), EventPriority.Normal, false, _ => { }),
            "A built-in event name must be registered.");
    }

    [TestMethod]
    public void RegisterEvent_UnknownName_ReportsInsteadOfThrowing()
    {
        // Registration runs inside the native OnEnable callback, so a rejected
        // name must be reported rather than thrown: an exception here would
        // unwind back into native code. This asserts the absence of a throw.
        var plugin = new TestPlugin();

        plugin.RegisterEvent("MyCustomEvent", _ => { });
        plugin.RegisterEvent<NotAnEndstoneEvent>(_ => { });
    }

    private sealed class TestPlugin : PluginBase;

    /// <summary>A plugin-authored event type, i.e. one the loader has no accessors
    /// for. Event has no abstract members, so this only needs the constructor.</summary>
    private sealed class NotAnEndstoneEvent : Event
    {
        internal NotAnEndstoneEvent() : base(IntPtr.Zero) { }
    }
}
