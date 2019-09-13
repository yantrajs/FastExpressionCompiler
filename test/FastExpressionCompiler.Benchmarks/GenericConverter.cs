﻿using System;
using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using L = FastExpressionCompiler.LightExpression.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    public class GenericConverter
    {
        /*

|                     Method |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|--------------------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
|                    Compile | 58.287 us | 0.3045 us | 0.2848 us |  8.51 |    0.06 | 0.9155 | 0.4272 |      - |    4.4 KB |
|                CompileFast |  6.563 us | 0.0345 us | 0.0322 us |  0.96 |    0.01 | 0.4883 | 0.2441 | 0.0305 |   2.25 KB |
| CompileFast_WithoutClosure |  6.852 us | 0.0431 us | 0.0403 us |  1.00 |    0.00 | 0.4807 | 0.2365 | 0.0381 |   2.22 KB |
         */
        [MemoryDiagnoser]
        public class Compilation
        {
            [Benchmark]
            public Func<int, X> Compile() => 
                GetConverter<int, X>();

            //[Benchmark]
            public Func<int, X> CompileFast() =>
                GetConverter_CompiledFast<int, X>();

            [Benchmark]
            public Func<int, X> CompileFast_WithoutClosure() =>
                GetConverter_CompiledFast_WithoutClosure<int, X>();

            [Benchmark(Baseline = true)]
            public Func<int, X> CompileFast_WithoutClosure_FromLightExpression() =>
                GetConverter_CompiledFast_WithoutClosure_FromLightExpression<int, X>();
        }

        [MemoryDiagnoser, IterationCount(50)]
        public class Invocation
        {
            /*
|                            Method |     Mean |     Error |    StdDev |   Median | Ratio | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|----------:|----------:|---------:|------:|------:|------:|------:|----------:|
|                   Invoke_Compiled | 3.048 ns | 0.0024 ns | 0.0048 ns | 3.047 ns |  1.72 |     - |     - |     - |         - |
|                Invoke_CompileFast | 1.773 ns | 0.0013 ns | 0.0023 ns | 1.772 ns |  1.00 |     - |     - |     - |         - |
| Invoke_CompileFast_WithoutClosure | 1.774 ns | 0.0026 ns | 0.0051 ns | 1.772 ns |  1.00 |     - |     - |     - |         - |
             */

            private static readonly Func<int, X> _compiled     = GetConverter<int, X>();
            private static readonly Func<int, X> _compiledFast = GetConverter_CompiledFast<int, X>();
            private static readonly Func<int, X> _compiledFast_WithoutClosure = GetConverter_CompiledFast_WithoutClosure<int, X>();

            [Benchmark]
            public X Invoke_Compiled() =>
                _compiled(1);

            [Benchmark]
            public X Invoke_CompileFast() =>
                _compiledFast(1);

            [Benchmark(Baseline = true)]
            public X Invoke_CompileFast_WithoutClosure() =>
                _compiledFast_WithoutClosure(1);
        }

        public static Func<TFrom, TTo> GetConverter<TFrom, TTo>()
        {
            var fromParam = Expression.Parameter(typeof(TFrom));
            var expr = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(fromParam, typeof(TTo)), fromParam);
            return expr.Compile();
        }

        public static Func<TFrom, TTo> GetConverter_CompiledFast<TFrom, TTo>()
        {
            var fromParam = Expression.Parameter(typeof(TFrom));
            var expr = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(fromParam, typeof(TTo)), fromParam);
            return expr.CompileFast(true);
        }

        public static Func<TFrom, TTo> GetConverter_CompiledFast_WithoutClosure<TFrom, TTo>()
        {
            var fromParam = Expression.Parameter(typeof(TFrom));
            var expr = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(fromParam, typeof(TTo)), fromParam);
            return expr.TryCompileWithoutClosure<Func<TFrom, TTo>>();
        }

        public static Func<TFrom, TTo> GetConverter_CompiledFast_WithoutClosure_FromLightExpression<TFrom, TTo>()
        {
            var fromParam = L.Parameter(typeof(TFrom));
            var expr = L.Lambda<Func<TFrom, TTo>>(L.Convert<TTo>(fromParam), fromParam);
            return LightExpression.ExpressionCompiler.TryCompileWithoutClosure<Func<TFrom, TTo>>(expr);
        }

        public enum X
        {
            A,
            B
        };
    }
}
