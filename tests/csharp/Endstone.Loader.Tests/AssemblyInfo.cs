// Tests here only exercise pure managed maths (Location, Vector3Extensions), so they are
// safe to run in parallel.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
