using System;
using System.IO;
using System.Text;

namespace NeuralNetworkLibrary.ArchiveSerialization
{
    public class Archive
    {
        private const int MIndex = 0; // actually never changes
        private readonly ArchiveOp _op;
        private readonly BinaryWriter _writer;
        protected readonly BinaryReader Reader;

        public Archive(Stream stream, ArchiveOp op)
        {
            _op = op;
            if (op == ArchiveOp.Load)
                Reader = new BinaryReader(stream);
            else
                _writer = new BinaryWriter(stream);
        }

        public bool IsStoring()
        {
            return _op == ArchiveOp.Store;
        }

        // ReSharper disable once UnusedMember.Global
        public void Serialize(IArchiveSerialization obj)
        {
            obj.Serialize(this);
        }

        //////////////////////////////////////////////////////
        // write functions

        // ReSharper disable once UnusedMember.Global
        public void Write(char ch)
        {
            //writer.Write(ch);
            _writer.Write(Convert.ToInt16(ch));
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(ushort n)
        {
            _writer.Write(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(short n)
        {
            _writer.Write(n);
        }

        public void Write(uint n)
        {
            _writer.Write(n);
        }

        public void Write(int n)
        {
            _writer.Write(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(ulong n)
        {
            _writer.Write(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(long n)
        {
            _writer.Write(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(float d)
        {
            _writer.Write(d);
        }

        public void Write(double d)
        {
            _writer.Write(d);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(decimal d)
        {
            // store decimals as Int64
            var n = decimal.ToOACurrency(d);
            _writer.Write(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(DateTime dt)
        {
            _writer.Write(dt.ToBinary());
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(bool b)
        {
            _writer.Write(b);
        }

        public void Write(string s)
        {
            _writer.Write(Convert.ToInt32(s.Length));
            _writer.Write(s.ToCharArray());
        }

        // ReSharper disable once UnusedMember.Global
        public void Write(Guid guid)
        {
            var bytes = guid.ToByteArray();
            Write(bytes);
        }

        private void Write(byte[] buffer)
        {
            _writer.Write(buffer);
        }

        ///////////////////////////////////////////////////
        // Read functions

        public void Read(out string s)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            int length;
            Read(out length);

            var ch = new char[length];

            Reader.Read(ch, MIndex, length);

            var sb = new StringBuilder();
            sb.Append(ch);
            s = sb.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out ushort n)
        {
            var bytes = new byte[2];
            Reader.Read(bytes, MIndex, 2);
            n = BitConverter.ToUInt16(bytes, 0);
        }

        private void Read(out short n)
        {
            var bytes = new byte[2];
            Reader.Read(bytes, MIndex, 2);
            n = BitConverter.ToInt16(bytes, 0);
        }

        public void Read(out uint n)
        {
            var bytes = new byte[4];
            Reader.Read(bytes, MIndex, 4);
            n = BitConverter.ToUInt32(bytes, 0);
        }

        public void Read(out int n)
        {
            var bytes = new byte[4];
            Reader.Read(bytes, MIndex, 4);
            n = BitConverter.ToInt32(bytes, 0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out ulong n)
        {
            var bytes = new byte[8];
            Reader.Read(bytes, MIndex, 8);
            n = BitConverter.ToUInt64(bytes, 0);
        }

        private void Read(out long n)
        {
            var bytes = new byte[8];
            Reader.Read(bytes, MIndex, 8);
            n = BitConverter.ToInt64(bytes, 0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out char ch)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            short n;
            Read(out n);
            ch = Convert.ToChar(n);

            /* direct reading as char doesn't work for some reason
                Sometimes it works, but sometimes the character
              takes up only one byte in the buffer and it seems
              to depend on what comes before and after the item in the buffer
         
            */

            // byte[] bytes = new byte[2];
            // reader.Read(bytes, m_Index, 2);
            // ch = BitConverter.ToChar(bytes, 0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out float d)
        {
            var bytes = new byte[4];
            Reader.Read(bytes, MIndex, 4);
            d = BitConverter.ToSingle(bytes, 0);
        }

        public void Read(out double d)
        {
            var bytes = new byte[8];
            Reader.Read(bytes, MIndex, 8);
            d = BitConverter.ToDouble(bytes, 0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out decimal d)
        {
            var bytes = new byte[8];
            Reader.Read(bytes, MIndex, 8);

            // BitConverter does not support direct conversion to Decimal so use Int64
            var n = BitConverter.ToInt64(bytes, 0);
            d = decimal.FromOACurrency(n);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out DateTime dt)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            long l;
            Read(out l);
            dt = DateTime.FromBinary(l);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out bool b)
        {
            var bytes = new byte[1];
            Reader.Read(bytes, MIndex, 1);
            b = BitConverter.ToBoolean(bytes, 0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out Guid guid)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            byte[] bytes;
            Read(out bytes, 16);
            guid = new Guid(bytes);
        }

        private void Read(out byte[] buffer, int bufferSize)
        {
            buffer = new byte[bufferSize];
            Reader.Read(buffer, MIndex, bufferSize);
        }
    }
}