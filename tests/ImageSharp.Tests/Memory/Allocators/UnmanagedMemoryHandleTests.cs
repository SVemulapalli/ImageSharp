﻿// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Microsoft.DotNet.RemoteExecutor;
using SixLabors.ImageSharp.Memory.Internals;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Memory.Allocators
{
    public class UnmanagedMemoryHandleTests
    {
        [Fact]
        public unsafe void Allocate_AllocatesReadWriteMemory()
        {
            var h = UnmanagedMemoryHandle.Allocate(128);
            Assert.False(h.IsInvalid);
            Assert.True(h.IsValid);
            byte* ptr = (byte*)h.Handle;
            for (int i = 0; i < 128; i++)
            {
                ptr[i] = (byte)i;
            }

            for (int i = 0; i < 128; i++)
            {
                Assert.Equal((byte)i, ptr[i]);
            }

            h.Free();
        }

        [Fact]
        public void Free_ClosesHandle()
        {
            var h = UnmanagedMemoryHandle.Allocate(128);
            h.Free();
            Assert.True(h.IsInvalid);
            Assert.Equal(IntPtr.Zero, h.Handle);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(13)]
        public void Create_Free_AllocationsAreTracked(int count)
        {
            RemoteExecutor.Invoke(RunTest, count.ToString()).Dispose();

            static void RunTest(string countStr)
            {
                int countInner = int.Parse(countStr);
                var l = new List<UnmanagedMemoryHandle>();
                for (int i = 0; i < countInner; i++)
                {
                    Assert.Equal(i, UnmanagedMemoryHandle.TotalOutstandingHandles);
                    var h = UnmanagedMemoryHandle.Allocate(42);
                    Assert.Equal(i + 1, UnmanagedMemoryHandle.TotalOutstandingHandles);
                    l.Add(h);
                }

                for (int i = 0; i < countInner; i++)
                {
                    Assert.Equal(countInner - i, UnmanagedMemoryHandle.TotalOutstandingHandles);
                    l[i].Free();
                    Assert.Equal(countInner - i - 1, UnmanagedMemoryHandle.TotalOutstandingHandles);
                }
            }
        }

        [Fact]
        public void Equality_WhenTrue()
        {
            var h1 = UnmanagedMemoryHandle.Allocate(10);
            UnmanagedMemoryHandle h2 = h1;

            Assert.True(h1.Equals(h2));
            Assert.True(h2.Equals(h1));
            Assert.True(h1 == h2);
            Assert.False(h1 != h2);
            Assert.True(h1.GetHashCode() == h2.GetHashCode());
            h1.Free();
        }

        [Fact]
        public void Equality_WhenFalse()
        {
            var h1 = UnmanagedMemoryHandle.Allocate(10);
            var h2 = UnmanagedMemoryHandle.Allocate(10);

            Assert.False(h1.Equals(h2));
            Assert.False(h2.Equals(h1));
            Assert.False(h1 == h2);
            Assert.True(h1 != h2);

            h1.Free();
            h2.Free();
        }
    }
}
