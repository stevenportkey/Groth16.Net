using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Groth16.Net
{
    public class ProvingFailedException : Exception
    {
        public ProvingFailedException(string message) : base(message)
        {
        }
    }

    public unsafe class Prover : Groth16Base, IDisposable
    {
        private static void AssertFileExists(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The file {path} does not exist.");
            }
        }

        /// <summary>
        /// Creates a <see cref="Prover"/> by loading the context from the given paths.
        /// </summary>
        /// <param name="wasmPath">The `.wasm` file produced by compiling a circom ciruit.</param>
        /// <param name="r1csPath">The `.r1cs` file produced by compiling a circom ciruit.</param>
        /// <param name="zkeyPath">The `.zkey` file created from a Groth16 trusted setup for the circuit.</param>
        /// <returns>The Prover instance.</returns>
        public static Prover Create(string wasmPath, string r1csPath, string zkeyPath)
        {
            AssertFileExists(wasmPath);
            AssertFileExists(r1csPath);
            AssertFileExists(zkeyPath);
            var ctx = LoadContextBn2546(wasmPath, r1csPath, zkeyPath);
            var prover = new Prover();
            prover._ctx = ctx;
            return prover;
        }

        IntPtr _ctx;
        public IntPtr Context => _ctx;

        static readonly Lazy<load_context_bn254> load_context_bn254
            = Groth16Base.LazyDelegate<load_context_bn254>(nameof(load_context_bn254));

        static readonly Lazy<free_context_bn254> free_context_bn254
            = Groth16Base.LazyDelegate<free_context_bn254>(nameof(free_context_bn254));

        static readonly Lazy<verifying_key_size_bn254> verifying_key_size_bn254
            = Groth16Base.LazyDelegate<verifying_key_size_bn254>(nameof(verifying_key_size_bn254));

        static readonly Lazy<export_verifying_key_bn254> export_verifying_key_bn254
            = Groth16Base.LazyDelegate<export_verifying_key_bn254>(nameof(export_verifying_key_bn254));

        static readonly Lazy<prove_bn254> prove_bn254
            = Groth16Base.LazyDelegate<prove_bn254>(nameof(prove_bn254));


        static IntPtr LoadContextBn2546(string wasmPath, string r1csPath, string zkeyPath)
        {
            var ctx = IntPtr.Zero;
            var wasm = Encoding.UTF8.GetBytes(wasmPath).AsSpan();
            var r1cs = Encoding.UTF8.GetBytes(r1csPath).AsSpan();
            var zkey = Encoding.UTF8.GetBytes(zkeyPath).AsSpan();


            fixed (byte* wasmPathPtr = &MemoryMarshal.GetReference(wasm), r1csPathPtr =
                       &MemoryMarshal.GetReference(r1cs), zkeyPathPtr = &MemoryMarshal.GetReference(zkey))

            {
                ctx = load_context_bn254.Value(wasmPathPtr, r1csPathPtr, zkeyPathPtr);
            }

            return ctx;
        }

        /// <summary>
        /// Exports the verifying key for the circuit.
        /// </summary>
        /// <returns>The hex representation of the serialized verifying key.</returns>
        public string ExportVerifyingKeyBn254()
        {
            var buf = new byte[verifying_key_size_bn254.Value(_ctx) + 1];
            fixed (byte* bufPtr = buf)
            {
                export_verifying_key_bn254.Value(_ctx, bufPtr, buf.Length);
            }

            var charArray = Encoding.UTF8.GetChars(buf);

            return new string(charArray).Trim('\0');
        }

        /// <summary>
        /// Proves the Bn254.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A string representation of the proof.</returns>
        /// <exception cref="ArgumentException">Thrown when any value in the input dictionary is not a decimal number.</exception>
        /// <exception cref="ProvingFailedException">Thrown when the prove_bn254 function returns a negative value.</exception>
        public string ProveBn254(IDictionary<string, IList<string>> input)
        {
            if (input.Values.SelectMany(x => x).Any(s => !s.IsDecimal()))
            {
                throw new ArgumentException("All input values must be decimal numbers.");
            }

            var buffer = new byte[1048576]; // 1MB

            var inputString = input.ToJsonString();
            Span<byte> inputInBytes = Encoding.ASCII.GetBytes(inputString);
            Span<byte> output = buffer;
            var returnedBytes = 0;

            fixed (byte* inputPtr = &MemoryMarshal.GetReference(inputInBytes),
                   buffPtr = &MemoryMarshal.GetReference(output))
            {
                returnedBytes = prove_bn254.Value(_ctx, inputPtr, buffPtr, buffer.Length);
            }

            if (returnedBytes < 0)
            {
                throw new ProvingFailedException($"failed with code {returnedBytes}");
            }

            var charArray = Encoding.UTF8.GetChars(buffer.TakeWhile(v => v != 0).ToArray());
            return new string(charArray);
        }

        /// <summary>
        /// Gets the verifying key size.
        /// </summary>
        /// <returns>The verifying key size.</returns>
        public int VerifyingKeySizeBn254()
        {
            return verifying_key_size_bn254.Value(_ctx);
        }

        public void Dispose()
        {
            if (_ctx == IntPtr.Zero) return;
            free_context_bn254.Value(_ctx);
            _ctx = IntPtr.Zero;
        }
    }
}