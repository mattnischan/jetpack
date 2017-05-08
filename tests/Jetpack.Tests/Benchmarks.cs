using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Jetpack.Core;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Jetpack.Tests
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [ProtoContract]
        public class Poco
        {
            [ProtoMember(1)]
            public string StringProp { get; set; }      //using the text "hello"
            [ProtoMember(2)]
            public int IntProp { get; set; }            //123
            [ProtoMember(3)]
            public Guid GuidProp { get; set; }          //Guid.NewGuid()
            [ProtoMember(4)]
            public DateTime DateProp { get; set; }      //DateTime.Now
        }

        private Poco _poco = new Poco
        {
            StringProp = "hello",
            IntProp = 123,
            GuidProp = Guid.NewGuid(),
            DateProp = DateTime.Now
        };

        private byte[] _buffer;
        private MemoryStream _stream;
        private WritableBuffer _wBuf;
        private Action<WritableBuffer, JetpackSession, Poco> _serializer;

        public Benchmarks()
        {
            _buffer = new byte[1024];
            _stream = new MemoryStream(_buffer);
            _wBuf = new WritableBuffer(_buffer);
            _serializer = SerializerCompiler.BuildAlphaSerializer<Poco>();
        }
        
        [Benchmark(Baseline = true)]
        public void SerializeProtobuf()
        {
            Serializer.Serialize(_stream, _poco);
            _stream.Seek(0, SeekOrigin.Begin);
        }

        [Benchmark]
        public void SerializeJetpack()
        {
            _serializer(_wBuf, new JetpackSession { GetBuffer = () => new byte[1024] }, _poco);
            _wBuf.Reset();
        }

        [Fact]
        public void Benchmark()
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
