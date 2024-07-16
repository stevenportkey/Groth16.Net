using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Groth16.Net
{
    internal class InternalProvingOutput
    {
        public InternalProvingOutput(IList<string> publicInputs, RapidSnarkProof proof)
        {
            PublicInputs = publicInputs;
            Proof = proof;
        }

        private IList<string> PublicInputs { get; set; }

        private RapidSnarkProof Proof { get; set; }

        public string ToJsonString()
        {
            return $"{{\"public_inputs\":{PublicInputs.ToJsonString()},\"proof\":{Proof.ToJsonString()}}}";
        }
    }

    public unsafe class Verifier : Groth16Base
    {
        static readonly Lazy<verify_bn254> verify_bn254
            = LazyDelegate<verify_bn254>(nameof(verify_bn254));

        public static bool VerifyBn254(string verifyingKey, IList<string> publicInputs, RapidSnarkProof proof)
        {
            if (!verifyingKey.IsHex())
            {
                throw new ArgumentException("The verifying key must be in hex format.");
            }

            if (publicInputs.Any(x => !x.IsDecimal()))
            {
                throw new ArgumentException("The public inputs must be in decimal format.");
            }

            proof.Validate();

            var provingOutput = new InternalProvingOutput(publicInputs, proof).ToJsonString();
            Span<byte> inputInBytes = Encoding.ASCII.GetBytes(verifyingKey);
            Span<byte> provingOutputInBytes = Encoding.ASCII.GetBytes(provingOutput);
            var verified = -100;

            fixed (byte* inputPtr = &MemoryMarshal.GetReference(inputInBytes),
                   provingOutputPtr = &MemoryMarshal.GetReference(provingOutputInBytes))
            {
                verified = verify_bn254.Value(inputPtr, provingOutputPtr);
            }

            return verified == 1;
        }
    }
}