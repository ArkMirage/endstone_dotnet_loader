namespace Endstone.Loader;

/// <summary>Wraps a native endstone::Mob (an Actor with health).</summary>
public unsafe class Mob : Actor
{
    internal Mob(IntPtr ptr) : base(ptr) { }

    private static Bridge.Table* T => Bridge.Raw;
    private void* P => (void*)NativePtr;

    public int Health => T->MobGetHealth(P);
    public int MaxHealth => T->MobGetMaxHealth(P);
    public bool IsGliding => T->MobIsGliding(P);

    public void SetHealth(int value) => T->MobSetHealth(P, value);
    public void SetMaxHealth(int value) => T->MobSetMaxHealth(P, value);
}