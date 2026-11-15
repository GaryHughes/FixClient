using BenchmarkDotNet.Running;

// Run in Release, for example:
//   dotnet run -c Release --project Fix.Benchmarks -- --filter *MessageLookup*
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
