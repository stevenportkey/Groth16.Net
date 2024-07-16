using System;
using System.Collections.Generic;
using System.Linq;

namespace Groth16.Net
{
    public class InvalidProofFormatException : Exception
    {
        public InvalidProofFormatException(string message) : base(message)
        {
        }
    }

    public class RapidSnarkProof
    {
        public List<string> PiA { get; set; }
        public List<List<string>> PiB { get; set; }
        public List<string> PiC { get; set; }

        public string ToJsonString()
        {
            return
                $"{{\"pi_a\":{PiA.ToJsonString()},\"pi_b\":{PiB.ToJsonString()},\"pi_c\":{PiC.ToJsonString()}, \"protocol\": \"groth16\"}}";
        }

        public void Validate()
        {
            if (PiA.Count != 3)
            {
                throw new InvalidProofFormatException("Invalid proof format: PiA must have 3 elements");
            }

            if (PiA[2] != "1")
            {
                throw new InvalidProofFormatException("Invalid proof format: PiA[1] must be 1");
            }

            if (PiA.Any(x => !x.IsDecimal()))
            {
                throw new InvalidProofFormatException("Invalid proof format: PiA must be in decimal format");
            }

            if (PiB.Count != 3)
            {
                throw new InvalidProofFormatException("Invalid proof format: PiB must have 3 elements");
            }

            if (PiB[2][0] != "1")
            {
                throw new InvalidProofFormatException("Invalid proof format: PiB[2][0] must be 1");
            }

            if (PiB[2][1] != "0")
            {
                throw new InvalidProofFormatException("Invalid proof format: PiB[2][1] must be 0");
            }

            if (PiB.SelectMany(x => x).Any(x => !x.IsDecimal()))
            {
                throw new InvalidProofFormatException("Invalid proof format: PiB must be in decimal format");
            }

            if (PiC.Count != 3)
            {
                throw new InvalidProofFormatException("Invalid proof format: PiC must have 3 elements");
            }

            if (PiC[2] != "1")
            {
                throw new InvalidProofFormatException("Invalid proof format: PiC[1] must be 1");
            }

            if (PiC.Any(x => !x.IsDecimal()))
            {
                throw new InvalidProofFormatException("Invalid proof format: PiC must be in decimal format");
            }
        }
    }
}