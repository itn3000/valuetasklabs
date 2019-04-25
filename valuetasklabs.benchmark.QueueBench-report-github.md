``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.706 (1803/April2018Update/Redstone4)
Intel Core i7-4712MQ CPU 2.30GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=2240914 Hz, Resolution=446.2465 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]   : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT
  ShortRun : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|         Method | LoopNum | TaskNum |        Mean |         Error |      StdDev |       Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |-------- |-------- |------------:|--------------:|------------:|------------:|------:|------:|----------:|
| **ValueTaskBench** |  **100000** |       **1** |    **21.05 ms** |     **0.9455 ms** |   **0.0518 ms** |           **-** |     **-** |     **-** |   **2.73 KB** |
|       TcsBench |  100000 |       1 |   220.13 ms |     7.0272 ms |   0.3852 ms |   5000.0000 |     - |     - |    2.6 KB |
| **ValueTaskBench** |  **100000** |      **10** |   **224.37 ms** |    **33.5831 ms** |   **1.8408 ms** |           **-** |     **-** |     **-** |   **4.84 KB** |
|       TcsBench |  100000 |      10 |   592.02 ms |   185.3798 ms |  10.1613 ms |  30000.0000 |     - |     - |   4.91 KB |
| **ValueTaskBench** |  **100000** |     **100** | **2,173.28 ms** |   **444.9744 ms** |  **24.3905 ms** |           **-** |     **-** |     **-** |  **26.14 KB** |
|       TcsBench |  100000 |     100 | 5,038.77 ms | 5,215.3341 ms | 285.8701 ms | 308000.0000 |     - |     - |  28.26 KB |
