using System.Collections.Generic;

namespace Groth16.Net
{
    public class RapidSnarkProof
    {
        public List<string> PiA { get; set; }
        public List<List<string>> PiB { get; set; }
        public List<string> PiC { get; set; }

        public string ToJsonString()
        {
            return $"{{\"pi_a\":{PiA.ToJsonString()},\"pi_b\":{PiB.ToJsonString()},\"pi_c\":{PiC.ToJsonString()}, \"protocol\": \"groth16\"}}";
        }
    }
}